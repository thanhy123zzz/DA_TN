using DA_CNPM_VatTu.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SelectPdf;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Claims;

namespace DA_CNPM_VatTu.Controllers
{
    [Authorize(Roles = "QL_XuatKho")]
    [Route("QuanLy/[controller]")]
    public class XuatKhoController : Controller
    {
        private DACNPMContext _dACNPMContext;
        private ICompositeViewEngine _viewEngine;
        private readonly ILogger<XuatKhoController> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private List<NhanVien> _nvs;
        private readonly IMemoryCache _memoryCache;
        public XuatKhoController(ILogger<XuatKhoController> logger,
            ICompositeViewEngine viewEngine, IMemoryCache memoryCache, IWebHostEnvironment hostingEnvironment)
        {
            _dACNPMContext = new DACNPMContext();
            _logger = logger;
            _viewEngine = viewEngine;
            _memoryCache = memoryCache;
            _hostingEnvironment = hostingEnvironment;
        }
        [HttpGet]
        public async Task<IActionResult> XuatKho()
        {
            var ncc = getListKH();
            var hh = getListHH();
            var ctpn = getListCTPX();
            var ctpnt = GetListCTPXT();
            var p = GetPhanQuyenXuatKho();
            await Task.WhenAll(ncc, hh, ctpn, p, ctpnt);

            var dvt = await getListDVT();
            ViewBag.Dvts = dvt;
            ViewBag.SoPhieuXuat = getSoPhieu().Result;
            ViewBag.CTPXTs = ctpnt.Result;
            ViewBag.HHs = getListHH().Result;
            ViewBag.phanQuyenXuatKho = p.Result;
            ViewData["title"] = p.Result.IdchucNangNavigation.TenChucNang;
            return View();
        }
        [HttpPost("api/khs")]
        public async Task<IActionResult> optionsKH(string key)
        {
            var dvts = getListKH().Result.AsParallel()
                .Where(x => x.Active == true && (x.MaKh + " " + x.TenKh).ToLower().Contains(key.ToLower()))
                .ToList();
            return Ok(dvts.Select(x => new
            {
                ID = x.Id,
                MaKh = x.MaKh,
                TenKh = x.TenKh,
                loai = x.LoaiKh.Value ? "Sỉ" : "Lẻ"
            }).ToList());
        }
        [HttpPost("api/hhs")]
        public async Task<IActionResult> searchHH(string key)
        {
            var hhs = await getListHH();
            var idhhs = GetListCTPXT().Result.ToList();
            var vts = hhs.AsParallel()
                .Where(x =>
                (x.TenHh + " " + x.MaHh).ToLower().Contains(key.ToLower())
                && x.Active == true
                && !idhhs.Any(y => y.Idhh == x.Id)
                )
                .ToList();
            return Ok(vts.Select(x => new
            {
                Id = x.Id,
                TenHh = x.TenHh,
                MaHh = x.MaHh
            }).ToList());
        }
        [HttpPost("load-tthh")]
        public async Task<IActionResult> loadTTHH(int idHh, int idKh, int idDvt)
        {
            var kh = getListKH().Result.FirstOrDefault(x => x.Id == idKh);
            int idCn = int.Parse(User.FindFirstValue("IdCn"));
            var loaiKh = kh.LoaiKh.Value;
            // các loại đơn vị tính của hàng hoá
            var hhdvts = await _dACNPMContext
                .Hhdvts.Include(x => x.IddvtNavigation)
                .Where(x => x.Idhh == idHh && x.Active == true)
                .ToListAsync();
            // đơn vị tính chính của hàng hoá
            var hh = await _dACNPMContext
                .HangHoas.Include(x => x.IddvtchinhNavigation)
                .FirstOrDefaultAsync(x => x.Id == idHh && x.Active == true);
            hhdvts.Insert(0, new Hhdvt() { IddvtNavigation = hh.IddvtchinhNavigation, SlquyDoi = 1 });
            // giá trị dvt trả về
            var resultDvt = hhdvts.Select(x => new
            {
                Id = x.IddvtNavigation.Id,
                tenDvt = x.IddvtNavigation.TenDvt,
                slQuyDoi = x.SlquyDoi
            });
            // lấy hàng tồn
            var hangTons = _dACNPMContext.HangTonKhos
                .Include(x => x.IdctpnNavigation)
                .Where(x => x.Idhh == idHh && x.Idcn == idCn).ToListAsync();
            // nếu không còn hàng tồn trong kho
            await hangTons;
            if (hangTons.Result.Count == 0)
            {
                return Ok(new
                {
                    dvts = resultDvt,
                    slCon = 0,
                    donGia = 0,
                    messages = new
                    {
                        message = "Hết hàng trong kho!",
                        color = "bg-danger"
                    },
                    avatar = hh.Avatar
                });
            }
            // cách xuất
            var cachXuat = _dACNPMContext.CachXuats.Find(1);
            HangTonKho hangTon = await getDonGia(hangTons.Result);
            var giaNhap = hangTon.GiaNhap;
            var thue = hangTon.Thue;
            var slCon = hangTons.Result
            .AsParallel().Sum(x => x.Slcon);

            // chọn hàng hoá
            if (idDvt == 0 || idDvt == hh.Iddvtchinh)
            {
                //giá theo khách hàng 
                var gtkh = _dACNPMContext.GiaTheoKhachHangs
                    .FirstOrDefaultAsync(x => x.Idhh == idHh && x.Idkh == idKh && x.Iddvt == hh.Iddvtchinh && x.Active == true);
                var giaTheoKH = gtkh.Result;

                // nếu giá theo khách hàng có tồn tại
                if (giaTheoKH != null && (((giaTheoKH.TiLeSi != null) || (giaTheoKH.GiaBanSi != null)) && loaiKh || ((giaTheoKH.TiLeLe != null) || (giaTheoKH.GiaBanLe != null)) && !loaiKh))
                {
                    // sỉ
                    if (loaiKh)
                    {
                        return Ok(new
                        {
                            dvts = resultDvt,
                            slCon = slCon,
                            donGia = giaTheoKH.GiaBanSi == null
                            ? getDonGia(giaTheoKH.TiLeSi, giaNhap, thue)
                            : giaTheoKH.GiaBanSi,
                            avatar = hh.Avatar
                        });
                    }
                    // lẻ
                    else
                    {
                        return Ok(new
                        {
                            dvts = resultDvt,
                            slCon = slCon,
                            donGia = giaTheoKH.GiaBanLe == null
                            ? getDonGia(giaTheoKH.TiLeLe, giaNhap, thue)
                            : giaTheoKH.GiaBanLe,
                            avatar = hh.Avatar
                        });
                    }
                }
                //nếu không tồn tại thì sét giá theo nhóm hàng hoá
                else
                {
                    var gtnhhs = _dACNPMContext.GiaTheoNhomHhs
                        .Where(x => x.Idnhh == hh.Idnhh && x.Active == true)
                        .ToList();
                    // nếu có gtnhh
                    if (gtnhhs.Count > 0)
                    {
                        //xét nhiều khoản min max khác nhau
                        foreach (var h in gtnhhs)
                        {
                            if (giaNhap >= h.Min && giaNhap <= h.Max)
                            {
                                // sỉ
                                if (loaiKh)
                                {
                                    return Ok(new
                                    {
                                        dvts = resultDvt,
                                        slCon = slCon,
                                        donGia = getDonGia(h.TiLeSi, giaNhap, thue),
                                        avatar = hh.Avatar
                                    });
                                }
                                // lẻ
                                else
                                {
                                    return Ok(new
                                    {
                                        dvts = resultDvt,
                                        slCon = slCon,
                                        donGia = getDonGia(h.TiLeLe, giaNhap, thue),
                                        avatar = hh.Avatar
                                    });
                                }
                            }
                        }
                        // sau khi duyện qua mà vẫn không có thì xét giá bán chung
                        // sỉ
                        if (loaiKh)
                        {
                            return Ok(new
                            {
                                dvts = resultDvt,
                                slCon = slCon,
                                donGia = hh.GiaBanSi == null
                                ? getDonGia(hh.TiLeSi, giaNhap, thue)
                                : hh.GiaBanSi,
                                avatar = hh.Avatar
                            });
                        }
                        // lẻ
                        else
                        {
                            return Ok(new
                            {
                                dvts = resultDvt,
                                slCon = slCon,
                                donGia = hh.GiaBanLe == null
                                ? getDonGia(hh.TiLeLe, giaNhap, thue)
                                : hh.GiaBanLe,
                                avatar = hh.Avatar
                            });
                        }
                    }
                    // giá tnhh không có thì xét giá bán chung
                    else
                    {
                        // sỉ
                        if (loaiKh)
                        {
                            return Ok(new
                            {
                                dvts = resultDvt,
                                slCon = slCon,
                                donGia = hh.GiaBanSi == null
                                ? getDonGia(hh.TiLeSi, giaNhap, thue)
                                : hh.GiaBanSi,
                                avatar = hh.Avatar
                            });
                        }
                        // lẻ
                        else
                        {
                            return Ok(new
                            {
                                dvts = resultDvt,
                                slCon = slCon,
                                donGia = hh.GiaBanLe == null
                                ? getDonGia(hh.TiLeLe, giaNhap, thue)
                                : hh.GiaBanLe,
                                avatar = hh.Avatar
                            });
                        }
                    }
                }
            }
            // chọn đơn vị tính
            else
            {
                // số lượng quy đổi
                var slQuyDoi = hhdvts.FirstOrDefault(x => x.Iddvt == idDvt).SlquyDoi;
                var slConQuyDoi = Math.Round((slCon.Value / slQuyDoi.Value), 2);
                //giá theo khách hàng 
                var gtkh = _dACNPMContext.GiaTheoKhachHangs
                    .FirstOrDefaultAsync(x => x.Idhh == idHh && x.Idkh == idKh && x.Iddvt == idDvt && x.Active == true);
                var giaTheoKH = gtkh.Result;

                // nếu giá theo khách hàng có tồn tại
                if (giaTheoKH != null && (((giaTheoKH.TiLeSi != null) || (giaTheoKH.GiaBanSi != null)) && loaiKh || ((giaTheoKH.TiLeLe != null) || (giaTheoKH.GiaBanLe != null)) && !loaiKh))
                {
                    // sỉ
                    if (loaiKh)
                    {
                        return Ok(new
                        {
                            slCon = slConQuyDoi,
                            donGia = giaTheoKH.GiaBanSi == null
                            ? getDonGia(giaTheoKH.TiLeSi, giaNhap * slQuyDoi, thue)
                            : giaTheoKH.GiaBanSi,
                        });
                    }
                    // lẻ
                    else
                    {
                        return Ok(new
                        {
                            slCon = slConQuyDoi,
                            donGia = giaTheoKH.GiaBanLe == null
                            ? getDonGia(giaTheoKH.TiLeLe, giaNhap * slQuyDoi, thue)
                            : giaTheoKH.GiaBanLe,
                        });
                    }
                }
                // xét tới giá theo nhóm hàng hoá
                else
                {
                    var gtnhhs = _dACNPMContext.GiaTheoNhomHhs
                        .Where(x => x.Idnhh == hh.Idnhh && x.Active == true)
                        .ToList();
                    if (gtnhhs.Count > 0)
                    {
                        //xét nhiều khoản min max khác nhau
                        foreach (var h in gtnhhs)
                        {
                            if (giaNhap >= h.Min && giaNhap <= h.Max)
                            {
                                // sỉ
                                if (loaiKh)
                                {
                                    return Ok(new
                                    {
                                        slCon = slConQuyDoi,
                                        donGia = getDonGia(h.TiLeSi, giaNhap * slQuyDoi, thue),
                                    });
                                }
                                // lẻ
                                else
                                {
                                    return Ok(new
                                    {
                                        slCon = slConQuyDoi,
                                        donGia = getDonGia(h.TiLeLe, giaNhap * slQuyDoi, thue),
                                    });
                                }
                            }
                        }
                        // sau khi duyện qua mà vẫn không có thì xét giá bán chung
                        var hhdvt = _dACNPMContext.Hhdvts.FirstOrDefault(x => x.Idhh == idHh && x.Iddvt == idDvt);
                        // sỉ
                        if (loaiKh)
                        {
                            return Ok(new
                            {
                                slCon = slConQuyDoi,
                                donGia = hhdvt.GiaBanSi == null
                                ? getDonGia(hhdvt.TiLeSi, giaNhap * slQuyDoi, thue)
                                : hhdvt.GiaBanSi,
                            });
                        }
                        // lẻ
                        else
                        {
                            return Ok(new
                            {
                                slCon = slConQuyDoi,
                                donGia = hhdvt.GiaBanLe == null
                                ? getDonGia(hhdvt.TiLeLe, giaNhap * slQuyDoi, thue)
                                : hhdvt.GiaBanLe,
                            });
                        }
                    }
                    // giá tnhh không có thì xét giá bán chung
                    else
                    {
                        var hhdvt = _dACNPMContext.Hhdvts.FirstOrDefault(x => x.Idhh == idHh && x.Iddvt == idDvt);
                        // sỉ
                        if (loaiKh)
                        {
                            return Ok(new
                            {
                                slCon = slConQuyDoi,
                                donGia = hhdvt.GiaBanSi == null
                                ? getDonGia(hhdvt.TiLeSi, giaNhap * slQuyDoi, thue)
                                : hhdvt.GiaBanSi,
                            });
                        }
                        // lẻ
                        else
                        {
                            var r = new
                            {
                                slCon = slConQuyDoi,
                                donGia = hhdvt.GiaBanLe == null
                                ? getDonGia(hhdvt.TiLeLe, giaNhap * slQuyDoi, thue)
                                : hhdvt.GiaBanLe,
                            };
                            return Ok(r);
                        }
                    }
                }
            }
        }
        [HttpPost("add-ctpxt")]
        public async Task<IActionResult> addCTPXT(int idHH, float ThueXuat, float SL, float DonGia,
        float ChietKhau, int idDvt)
        {
            int _userId = int.Parse(User.Identity.Name);
            string host = GetLocalIPAddress();
            ChiTietPhieuXuatTam ct = new ChiTietPhieuXuatTam();
            ct.Idhh = idHH;
            ct.Thue = Math.Round(ThueXuat, 2);
            ct.Sl = Math.Round(SL, 2);
            ct.DonGia = Math.Round(DonGia, 2);
            ct.Cktm = Math.Round(ChietKhau, 2);
            ct.Host = host;
            ct.Iddvt = idDvt;
            _dACNPMContext.ChiTietPhieuXuatTams.Add(ct);
            _dACNPMContext.SaveChanges();

            _memoryCache.Remove("CTPXTs_" + _userId);

            var ListCTPXT = GetListCTPXT();
            var lhh = getListHH();
            var p = GetPhanQuyenXuatKho();
            var ldvt = await getListDVT();
            Task.WaitAll(ListCTPXT, lhh, p);

            ViewBag.CTPXTs = ListCTPXT.Result;
            ViewBag.Dvts = ldvt;
            ViewBag.HHs = lhh.Result;
            ViewBag.phanQuyenXuatKho = p.Result;
            PartialViewResult partialViewResult = PartialView("tableChiTietPhieuXuat");

            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);

            var TienHang = ListCTPXT.Result.Sum(x => x.Sl * x.DonGia);
            var TienCK = ListCTPXT.Result.Sum(x => (x.Sl * x.DonGia * x.Cktm) / 100);
            var TienThue = ListCTPXT.Result.Sum(x => (((x.Sl * x.DonGia) - ((x.Sl * x.DonGia * x.Cktm) / 100)) * x.Thue) / 100);
            var TienThanhToan = TienHang - TienCK + TienThue;
            return Ok(new
            {
                table = viewContent,
                tienHang = TienHang,
                tienCK = TienCK,
                tienThue = TienThue,
                tienThanhToan = TienThanhToan,
                statusCode = 200,
                message = "Thành công!",
                color = "bg-success"
            });
        }
        [HttpPost("showEditCTPXT")]
        public async Task<IActionResult> editChitietPhieuXuatTam(int id)
        {
            int idCn = int.Parse(User.FindFirstValue("IdCn"));
            var ctpxt = GetListCTPXT().Result.FirstOrDefault(x => x.Id == id);
            ctpxt.TenHh = getListHH().Result.FirstOrDefault(x => x.Id == ctpxt.Idhh).TenHh;

            // các loại đơn vị tính của hàng hoá
            var hhdvts = await _dACNPMContext
                .Hhdvts.Include(x => x.IddvtNavigation)
                .Where(x => x.Idhh == ctpxt.Idhh && x.Active == true)
                .ToListAsync();
            // đơn vị tính chính của hàng hoá
            var hh = getListHH().Result
                .FirstOrDefault(x => x.Id == ctpxt.Idhh && x.Active == true);
            hhdvts.Insert(0, new Hhdvt() { IddvtNavigation = hh.IddvtchinhNavigation, SlquyDoi = 1 });
            // lấy hàng tồn
            var hangTons = await _dACNPMContext.HangTonKhos
                .Include(x => x.IdctpnNavigation)
                .Where(x => x.Idhh == ctpxt.Idhh && x.Idcn == idCn).ToListAsync();
            // nếu không còn hàng tồn trong kho
            var slCon = hangTons
            .AsParallel().Sum(x => x.Slcon);

            ViewBag.SlCon = slCon / (hhdvts.FirstOrDefault(x => x.IddvtNavigation.Id == ctpxt.Iddvt).SlquyDoi);
            ViewBag.HhDvts = hhdvts;

            return PartialView("GroupChitietPhieuXuat", ctpxt);
        }
        [HttpPost("edit-ctpxt")]
        public async Task<IActionResult> editCTPXT(int idHH, float ThueXuat, float SL, float DonGia,
        float ChietKhau, int idDvt, int id)
        {
            int _userId = int.Parse(User.Identity.Name);
            string host = GetLocalIPAddress();
            ChiTietPhieuXuatTam ct = GetListCTPXT().Result.FirstOrDefault(x => x.Id == id);
            ct.Idhh = idHH;
            ct.Thue = Math.Round(ThueXuat, 2);
            ct.Sl = Math.Round(SL, 2);
            ct.DonGia = Math.Round(DonGia, 2);
            ct.Cktm = Math.Round(ChietKhau, 2);
            ct.Host = host;
            ct.Iddvt = idDvt;
            _dACNPMContext.ChiTietPhieuXuatTams.Update(ct);
            _dACNPMContext.SaveChanges();

            _memoryCache.Remove("CTPXTs_" + _userId);

            var ListCTPXT = GetListCTPXT();
            var lhh = getListHH();
            var p = GetPhanQuyenXuatKho();
            var ldvt = await getListDVT();
            Task.WaitAll(ListCTPXT, lhh, p);

            ViewBag.CTPXTs = ListCTPXT.Result;
            ViewBag.Dvts = ldvt;
            ViewBag.HHs = lhh.Result;
            ViewBag.phanQuyenXuatKho = p.Result;
            PartialViewResult partialViewResult = PartialView("tableChiTietPhieuXuat");

            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);

            var TienHang = ListCTPXT.Result.Sum(x => x.Sl * x.DonGia);
            var TienCK = ListCTPXT.Result.Sum(x => (x.Sl * x.DonGia * x.Cktm) / 100);
            var TienThue = ListCTPXT.Result.Sum(x => (((x.Sl * x.DonGia) - ((x.Sl * x.DonGia * x.Cktm) / 100)) * x.Thue) / 100);
            var TienThanhToan = TienHang - TienCK + TienThue;
            return Ok(new
            {
                table = viewContent,
                tienHang = TienHang,
                tienCK = TienCK,
                tienThue = TienThue,
                tienThanhToan = TienThanhToan,
                statusCode = 200,
                message = "Thành công!",
                color = "bg-success"
            });
        }
        [HttpPost("delete-ctpxt")]
        public async Task<IActionResult> deletePhieuXuatTam(int id)
        {
            ChiTietPhieuXuatTam ch = _dACNPMContext.ChiTietPhieuXuatTams.Find(id);
            int _userId = int.Parse(User.Identity.Name);
            _dACNPMContext.ChiTietPhieuXuatTams.Remove(ch);
            _dACNPMContext.SaveChanges();

            _memoryCache.Remove("CTPXTs_" + _userId);

            var ListCTPXT = GetListCTPXT();
            var lhh = getListHH();
            var p = GetPhanQuyenXuatKho();
            var ldvt = await getListDVT();
            Task.WaitAll(ListCTPXT, lhh, p);

            ViewBag.CTPXTs = await ListCTPXT;
            ViewBag.Dvts = ldvt;
            ViewBag.HHs = await lhh;
            ViewBag.phanQuyenXuatKho = await p;
            PartialViewResult partialViewResult = PartialView("tableChiTietPhieuXuat");

            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);

            var TienHang = ListCTPXT.Result.Sum(x => x.Sl * x.DonGia);
            var TienCK = ListCTPXT.Result.Sum(x => (x.Sl * x.DonGia * x.Cktm) / 100);
            var TienThue = ListCTPXT.Result.Sum(x => (((x.Sl * x.DonGia) - ((x.Sl * x.DonGia * x.Cktm) / 100)) * x.Thue) / 100);
            var TienThanhToan = TienHang - TienCK + TienThue;
            return Ok(new
            {
                table = viewContent,
                tienHang = TienHang,
                tienCK = TienCK,
                tienThue = TienThue,
                tienThanhToan = TienThanhToan,
                statusCode = 200,
                message = "Thành công!",
                color = "bg-success"
            });
        }
        [HttpPost("add-px")]
        public async Task<IActionResult> addPN(string NgayHD, string NgayXuat,
            string GhiChu, int IdKh, string SoHD)
        {
            int _userId = int.Parse(User.Identity.Name);
            int idCn = int.Parse(User.FindFirstValue("IdCn"));

            // lấy hàng tồn theo cách xuất
            var cachXuat = _dACNPMContext.CachXuats.Find(1);
            List<HangTonKho> hangTons;
            if (cachXuat.TheoTgnhap.Value)
            {
                hangTons = _dACNPMContext.HangTonKhos
                .Include(x => x.IdctpnNavigation)
                .OrderBy(x => x.NgayNhap).ToList();
            }
            else
            {
                hangTons = _dACNPMContext.HangTonKhos
                .Include(x => x.IdctpnNavigation)
                .OrderBy(x => x.Hsd).ToList();
            }
            // lấy danh sách chi tiết phiếu xuất tạm
            var ctpxts = await GetListCTPXT();
            var tran = _dACNPMContext.Database.BeginTransaction();
            try
            {
                PhieuXuatKho px = new PhieuXuatKho();
                px.NgayHd = DateTime.ParseExact(NgayHD, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                px.NgayTao = DateTime.ParseExact(NgayXuat, "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture);
                px.Active = true;
                px.Idcn = idCn;
                px.Idnv = _userId;
                px.SoPx = await getSoPhieu();
                px.GhiChu = GhiChu;
                px.Idkh = IdKh;
                px.SoHd = SoHD;
                _dACNPMContext.PhieuXuatKhos.Add(px);
                _dACNPMContext.SaveChanges();

                foreach (ChiTietPhieuXuatTam t in ctpxts)
                {
                    double slq = 0;
                    foreach (HangTonKho slhhc in hangTons.Where(x => x.Idhh == t.Idhh))
                    {
                        ChiTietPhieuXuat ct = new ChiTietPhieuXuat();
                        ct.Idhh = t.Idhh;
                        ct.Idpx = px.Id;
                        ct.DonGia = t.DonGia;
                        ct.Iddvt = t.Iddvt;
                        ct.Idctpn = slhhc.Idctpn;
                        ct.Cktm = t.Cktm;
                        ct.Thue = t.Thue;
                        ct.Nvtao = px.Idnv;
                        ct.NgayTao = px.NgayTao;
                        ct.Active = true;
                        HangHoa hangHoa = getListHH().Result.FirstOrDefault(x => x.Id == t.Idhh);
                        double? slquydoi = 0;
                        //nếu mà đơn vị tính là đơn vị tính chính
                        if (t.Iddvt == hangHoa.Iddvtchinh)
                        {
                            slquydoi = t.Sl;
                        }
                        else
                        {
                            slquydoi = t.Sl * _dACNPMContext.Hhdvts.Where(x => x.Idhh == t.Idhh && x.Iddvt == t.Iddvt).FirstOrDefault().SlquyDoi;
                        }
                        if (slq == 0)
                        {
                            slq = Math.Round((double)slquydoi, 2);
                        }
                        //nếu mà trong kho còn nhiều hơn số xuất
                        if (slhhc.Slcon > slq)
                        {
                            ct.Sl = t.Sl;
                            slhhc.Slcon -= slq;
                            _dACNPMContext.HangTonKhos.Update(slhhc);
                            _dACNPMContext.ChiTietPhieuXuats.Add(ct);
                            _dACNPMContext.SaveChanges();
                            break;
                        }
                        //nếu mà trong kho ngang với số cần xuất
                        if (slhhc.Slcon == slq)
                        {
                            ct.Sl = t.Sl;
                            _dACNPMContext.HangTonKhos.Remove(slhhc);
                            _dACNPMContext.ChiTietPhieuXuats.Add(ct);
                            _dACNPMContext.SaveChanges();
                            break;
                        }
                        //nếu trong kho còn ít hơn số cần xuất
                        if (slhhc.Slcon < slq)
                        {
                            ct.Sl = slhhc.Slcon;
                            slq = (double)(slq - slhhc.Slcon);
                            t.Sl = slq;
                            _dACNPMContext.HangTonKhos.Remove(slhhc);
                            _dACNPMContext.ChiTietPhieuXuats.Add(ct);
                            _dACNPMContext.SaveChanges();
                        }
                    }
                    _dACNPMContext.ChiTietPhieuXuatTams.Remove(t);
                    _dACNPMContext.SaveChanges();
                }
                var SoTT = await _dACNPMContext.SoThuTus.FirstOrDefaultAsync(x => x.Ngay.Date == DateTime.Now.Date && x.Loai.Equals("XuatKho"));
                SoTT.Stt += 1;
                _dACNPMContext.SoThuTus.Update(SoTT);
                _dACNPMContext.SaveChanges();
                tran.Commit();
                _memoryCache.Remove("CTPXTs_" + _userId);
                _memoryCache.Remove("CTPXs_" + _userId);
                return Ok(new
                {
                    statusCode = 200,
                    message = "Thành công! Phiếu xuất của bạn có số phiếu là: " + px.SoPx,
                    color = "bg-success"
                });
            }
            catch (Exception e)
            {
                tran.Rollback();
                return Ok(new
                {
                    statusCode = 500,
                    message = "Thất bại!",
                    color = "bg-danger"
                });
            }

        }
        [HttpPost("loadTableLichSuXuat")]
        public async Task<IActionResult> loadTableLichSuXuat(string fromDay, string toDay, string soPhieuLS, string soHDLS, int KhLs, int hhLS)
        {
            int idCn = int.Parse(User.FindFirstValue("IdCn"));
            DateTime FromDay = DateTime.ParseExact(fromDay, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime ToDay = DateTime.ParseExact(toDay, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            List<PhieuXuatKho> listPhieu = _dACNPMContext.PhieuXuatKhos
                .Include(x => x.IdkhNavigation)
                .Include(x => x.IdnvNavigation)
                .Include(x => x.ChiTietPhieuXuats)
            .AsParallel()
            .Where(x => x.NgayTao.Value.Date >= FromDay
                && x.NgayTao.Value.Date <= ToDay
                && x.Idcn == idCn
                && x.Active == true)
            .OrderByDescending(x => x.Id)
            .ToList();
            if (KhLs == 0 && hhLS == 0)
            {
                ViewBag.ListPhieuXuat = listPhieu.AsParallel().Where(x => (soHDLS == null ? true : (x.SoHd?.Contains(soHDLS) ?? false))
                && (soPhieuLS == null ? true : x.SoPx.Contains(soPhieuLS.ToUpper())));
            }
            else
            {
                ViewBag.ListPhieuXuat = listPhieu.AsParallel().Where(x => (hhLS == 0 ? true : (x.ChiTietPhieuXuats.Where(y => y.Idhh == hhLS).Count() > 0 ? true : false))
                && (KhLs == 0 ? true : x.Idkh == KhLs)
                && (soPhieuLS == null ? true : x.SoPx.Contains(soPhieuLS.ToUpper()))
                && (soHDLS == null ? true : (x.SoHd?.Contains(soHDLS.ToUpper()) ?? false)));
            }

            return PartialView("tableLichSuXuat");
        }
        [HttpPost("ViewThongTinPhieuXuat")]
        public IActionResult ViewThongTinPhieuXuat(int idPX)
        {
            var phieu = _dACNPMContext.PhieuXuatKhos
                .Include(x => x.ChiTietPhieuXuats)
                .Include(x => x.IdkhNavigation)
                .Include(x => x.IdnvNavigation)
                .AsParallel()
                .FirstOrDefault(x => x.Id == idPX);
            ViewBag.HangHoas = getListHH().Result;
            return PartialView(phieu);
        }
        [Route("download/phieuxuatkho/{id:int}")]
        public IActionResult downloadPhieuXuat(int id)
        {
            var fullView = new HtmlToPdf();
            fullView.Options.WebPageWidth = 1280;
            fullView.Options.PdfPageSize = PdfPageSize.A4;
            fullView.Options.MarginTop = 20;
            fullView.Options.MarginBottom = 20;
            fullView.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

            var currentUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

            var pdf = fullView.ConvertUrl(currentUrl + "/QuanLy/XuatKho/phieuXuatKhoPDF/" + id);

            var pdfBytes = pdf.Save();
            return File(pdfBytes, "application/pdf", "PhieuXuat.pdf");
        }
        [HttpPost("download/BaoCaoPhieuXuat")]
        public IActionResult downloadBaoCaoPhieuXuat(string fromDay, string toDay, string soPhieuLS, string soHDLS, int KhachHangLS, int hangHoaLS)
        {
            var fullView = new HtmlToPdf();
            fullView.Options.WebPageWidth = 1280;
            fullView.Options.PdfPageSize = PdfPageSize.A4;
            fullView.Options.MarginTop = 20;
            fullView.Options.MarginBottom = 20;
            fullView.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

            var url = Url.Action("viewBaoCaoPhieuXuatPDF", "Common", new { fromDay = fromDay, toDay = toDay, soPhieuLS = soPhieuLS, soHDLS = soHDLS, KhLs = KhachHangLS, hhLS = hangHoaLS });

            var currentUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}" + url;

            var pdf = fullView.ConvertUrl(currentUrl);

            var pdfBytes = pdf.Save();
            return File(pdfBytes, "application/pdf", "BaoCaoPhieuXuat.pdf");
        }
        double? getDonGia(double? tiLe, double? GiaNhap, double? Vat)
        {
            tiLe ??= 0;
            double tl = Math.Round((tiLe.Value / 100) + 1, 2);
            double thue = Math.Round(Vat.Value / 100, 2);
            return Math.Round(tl * (GiaNhap.Value + (GiaNhap.Value * thue)), 2);
        }
        string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        public string ConvertViewToString(ControllerContext controllerContext, PartialViewResult pvr, ICompositeViewEngine _viewEngine)
        {
            using (StringWriter writer = new StringWriter())
            {
                ViewEngineResult vResult = _viewEngine.FindView(controllerContext, pvr.ViewName, false);
                ViewContext viewContext = new ViewContext(controllerContext, vResult.View, pvr.ViewData, pvr.TempData, writer, new HtmlHelperOptions());

                vResult.View.RenderAsync(viewContext);

                return writer.GetStringBuilder().ToString();
            }
        }
        async Task<PhanQuyenChucNang> GetPhanQuyenXuatKho()
        {
            int idPq = int.Parse(User.FindFirstValue("IdPq"));

            return await _dACNPMContext.PhanQuyenChucNangs
                .Include(x => x.IdchucNangNavigation)
                .FirstOrDefaultAsync(x => x.Idpq == idPq
                && x.IdchucNangNavigation.MaChucNang.Equals("QL_XuatKho"));
        }
        async Task<List<HangHoa>> getListHH()
        {
            int _userId = int.Parse(User.Identity.Name);
            return await _memoryCache.GetOrCreateAsync("HangHoas_" + _userId, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return _dACNPMContext.HangHoas
                                .Include(x => x.IddvtchinhNavigation)
                                .Include(x => x.IdhsxNavigation)
                                .Include(x => x.IdnsxNavigation)
                                .Include(x => x.IdnhhNavigation)
                                .ToList();
            });
        }
        async Task<List<KhachHang>> getListKH()
        {
            int _userId = int.Parse(User.Identity.Name);
            return await _memoryCache.GetOrCreate("KhachHangs_" + _userId, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return _dACNPMContext.KhachHangs.ToList();
            });
        }
        async Task<List<ChiTietPhieuXuat>> getListCTPX()
        {
            int _userId = int.Parse(User.Identity.Name);
            return await _memoryCache.GetOrCreateAsync("CTPXs_" + _userId, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return _dACNPMContext.ChiTietPhieuXuats
                                .ToList();
            });
        }
        async Task<List<ChiTietPhieuXuatTam>> GetListCTPXT()
        {
            int _userId = int.Parse(User.Identity.Name);
            return await _memoryCache.GetOrCreateAsync("CTPXTs_" + _userId, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return _dACNPMContext.ChiTietPhieuXuatTams.Where(x => x.Host == GetLocalIPAddress())
                                .ToList();
            });
        }
        async Task<List<DonViTinh>> getListDVT()
        {
            int idcn = int.Parse(User.FindFirstValue("IdCn"));
            return await _memoryCache.GetOrCreateAsync("Dvts_" + idcn, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return _dACNPMContext.DonViTinhs.ToList();
            });
        }
        async Task<string> getSoPhieu()
        {
            var context = new DACNPMContext();
            //ID chi nhánh
            int cn = int.Parse(User.FindFirstValue("IdCn"));

            DateTime d = DateTime.Now;
            string ngayThangNam = d.ToString("yyMMdd");

            QuyDinhMa qd = await context.QuyDinhMas.FindAsync(2);
            string SoPhieu = cn + "_" + qd.TiepDauNgu + ngayThangNam;
            var list = await context.SoThuTus.FirstOrDefaultAsync(x => x.Ngay.Date == DateTime.Now.Date && x.Loai.Equals("XuatKho"));
            int stt;
            if (list == null)
            {
                SoThuTu sttt = new SoThuTu();
                sttt.Ngay = DateTime.ParseExact(DateTime.Now.ToString("dd-MM-yyyy"), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                sttt.Stt = 0;
                sttt.Loai = "XuatKho";
                await context.SoThuTus.AddAsync(sttt);
                context.SaveChanges();
                stt = 1;
            }
            else
            {
                stt = list.Stt + 1;
            }
            SoPhieu += stt;
            while (true)
            {
                if (qd.DoDai == SoPhieu.Length) break;
                SoPhieu = SoPhieu.Insert(SoPhieu.Length - stt.ToString().Length, "0");
            }
            return SoPhieu;
        }

        async Task<HangTonKho> getDonGia(List<HangTonKho> hangTons)
        {
            var tinhGiaXuats = await _dACNPMContext.TinhGiaXuats.ToListAsync();
            HangTonKho ht = new HangTonKho();
            if (tinhGiaXuats.FirstOrDefault(x => x.MaCach.Equals("CaoNhat")).GiaTri)
            {
                ht = hangTons.OrderByDescending(x => x.GiaNhap).FirstOrDefault();
            }
            if (tinhGiaXuats.FirstOrDefault(x => x.MaCach.Equals("TrungBinh")).GiaTri)
            {
                ht = new HangTonKho()
                {
                    GiaNhap = hangTons.Average(x => x.GiaNhap).Value,
                    Thue = hangTons.Average(x => x.Thue).Value
                };
            }
            if (tinhGiaXuats.FirstOrDefault(x => x.MaCach.Equals("MoiNhat")).GiaTri)
            {
                ht = hangTons.OrderByDescending(x => x.NgayNhap).FirstOrDefault();
            }
            return ht;
        }
    }
}
