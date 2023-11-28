using AutoMapper;
using DA_CNPM_VatTu.Models;
using DA_CNPM_VatTu.Models.Entities;
using DA_CNPM_VatTu.Models.MapData;
using DA_CNPM_VatTu.Services;
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
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace DA_CNPM_VatTu.Controllers
{
    [Authorize(Roles = "QL_NhapKho")]
    [Route("QuanLy/[controller]")]
    public class NhapKhoController : Controller
    {
        private DACNPMContext _dACNPMContext;
        private ICompositeViewEngine _viewEngine;
        private readonly ILogger<DonViTinhController> _logger;
        private readonly IConverter _converter;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private List<NhanVien> _nvs;
        private readonly IMemoryCache _memoryCache;
        private readonly IMapper _mapper;
        public NhapKhoController(ILogger<DonViTinhController> logger,
            ICompositeViewEngine viewEngine, IMemoryCache memoryCache, 
            IWebHostEnvironment hostingEnvironment, IMapper mapper,
            IConverter converter)
        {
            _dACNPMContext = new DACNPMContext();
            _logger = logger;
            _memoryCache = memoryCache;
            _viewEngine = viewEngine;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
            _converter = converter;
        }
        [HttpGet]
        public async Task<IActionResult> NhapKho()
        {
            var p = await GetPhanQuyenNhapKho();

            ViewBag.SoPhieuNhap = getSoPhieu().Result;
            ViewBag.phanQuyenNhapKho = p;
            ViewData["title"] = p.IdchucNangNavigation.TenChucNang;
            return View();
        }
        [HttpPost("api/hhs")]
        public async Task<IActionResult> searchHH(bool active = true)
        {
            if (active)
            {
                return Ok(await _dACNPMContext.HangHoas
                .AsNoTracking()
                .Where(x => x.Active == true)
                .Select(x => new
                {
                    id = x.Id,
                    ten = x.TenHh.Trim(),
                    ma = x.MaHh,
                    soLos = x.ChiTietPhieuNhaps.Select(x => x.SoLo).Distinct().ToList(),
                    dvts = x.Hhdvts.Where(y => y.Active == true).Select(y => new
                    {
                        id = y.IddvtNavigation.Id,
                        ten = y.IddvtNavigation.TenDvt,
                        ma = y.IddvtNavigation.MaDvt,
                        slqd = y.SlquyDoi
                    }).ToList(),
                    dvtChinh = new
                    {
                        id = x.Iddvtchinh,
                        ten = x.IddvtchinhNavigation.TenDvt,
                        ma = x.IddvtchinhNavigation.MaDvt,
                        slqd = 1
                    },
                }).OrderBy(x => x.ten).ToListAsync());
            }
            else
            {
                return Ok(await _dACNPMContext.HangHoas
                .AsNoTracking()
                .Select(x => new
                {
                    id = x.Id,
                    ten = x.TenHh.Trim(),
                    ma = x.MaHh,
                    soLos = x.ChiTietPhieuNhaps.Select(x => x.SoLo).Distinct().ToList(),
                    dvts = x.Hhdvts.Where(y => y.Active == true).Select(y => new
                    {
                        id = y.IddvtNavigation.Id,
                        ten = y.IddvtNavigation.TenDvt,
                        ma = y.IddvtNavigation.MaDvt,
                        slqd = y.SlquyDoi
                    }).ToList(),
                    dvtChinh = new
                    {
                        id = x.Iddvtchinh,
                        ten = x.IddvtchinhNavigation.TenDvt,
                        ma = x.IddvtchinhNavigation.MaDvt,
                        slqd = 1
                    },
                }).OrderBy(x => x.ten).ToListAsync());
            }
            
        }
        [HttpPost("api/getSoPhieuNhap")]
        public async Task<IActionResult> getSoPhieuNhap()
        {
            return Ok(await getSoPhieu());
        }
        [HttpPost("api/nccs")]
        public async Task<IActionResult> searchNhaCC()
        {
            var listPQnv = await getListNhaCC();
            return Ok(listPQnv.Select(x => new
            {
                id = x.Id,
                ten = x.TenNcc,
                ma = x.MaNcc
            }).ToList());
        }
        [HttpPost("update-pn")]
        public async Task<IActionResult> addPN([FromBody] PhieuNhapKhoMap phieuNhapKhoMap)
        {
            PhieuNhapKho phieuNhap = _mapper.Map<PhieuNhapKho>(phieuNhapKhoMap);
            var tran = _dACNPMContext.Database.BeginTransaction();
            try
            {
                int _userId = int.Parse(User.Identity.Name);
                int idCn = int.Parse(User.FindFirstValue("IdCn"));
                if (phieuNhap.Id == 0)
                {
                    phieuNhap.SoPn = await getSoPhieu();
                    phieuNhap.Idnv = _userId;
                    phieuNhap.Active = true;
                    phieuNhap.Idcn = idCn;

                    await _dACNPMContext.PhieuNhapKhos.AddAsync(phieuNhap);
                    await _dACNPMContext.SaveChangesAsync();

                    List<HangTonKho> listHt = new List<HangTonKho>();
                    foreach (var t in phieuNhap.ChiTietPhieuNhaps)
                    {
                        var hh = await _dACNPMContext.HangHoas.FirstOrDefaultAsync(x => x.Id == t.Idhh);
                        t.NgayTao = phieuNhap.NgayTao;
                        t.Nvtao = _userId;
                        t.Tgbh = hh.IdbaoHanhNavigation == null ? null : hh.IdbaoHanhNavigation.SoNgay;
                        t.Idbh = hh.IdbaoHanh;
                        t.Active = true;
                        HangTonKho sl = new HangTonKho();
                        sl.Idctpn = t.Id;
                        sl.Slcon = Math.Round((double)t.Sl, 2);
                        sl.Idcn = idCn;
                        sl.NgayNhap = t.NgayTao;
                        sl.Thue = t.Thue ?? 0;
                        sl.Cktm = t.Cktm ?? 0;
                        sl.GiaNhap = t.DonGia;
                        sl.Hsd = t.Hsd;
                        sl.Idhh = t.Idhh;
                        listHt.Add(sl);
                    }
                    await _dACNPMContext.HangTonKhos.AddRangeAsync(listHt);
                    await _dACNPMContext.SaveChangesAsync();

                    var SoTT = await _dACNPMContext.SoThuTus.FirstOrDefaultAsync(x => x.Ngay.Date == DateTime.Now.Date && x.Loai.Equals("NhapKho"));
                    SoTT.Stt += 1;
                    _dACNPMContext.SoThuTus.Update(SoTT);
                    _dACNPMContext.SaveChanges();

                    await tran.CommitAsync();

                    return Ok(new ResponseModel()
                    {
                        statusCode = 200,
                        message = "Thành công! Phiếu nhập của bạn có số phiếu là: " + phieuNhap.SoPn,
                    });
                }
                else
                {
                    var hangTonXoa = await _dACNPMContext.HangTonKhos.Where(x=> phieuNhapKhoMap.DaXoas.Any(y=>y == x.Idctpn)).ToListAsync();

                    _dACNPMContext.HangTonKhos.RemoveRange(hangTonXoa);
                    await _dACNPMContext.SaveChangesAsync();
                    var listDaXoa = phieuNhapKhoMap.DaXoas.Select(x=> new ChiTietPhieuNhap()
                    {
                        Id = x
                    }).ToList();
                    _dACNPMContext.ChiTietPhieuNhaps.RemoveRange(listDaXoa);
                    await _dACNPMContext.SaveChangesAsync();

                    List<ChiTietPhieuNhap> listChiTietphieu = phieuNhap.ChiTietPhieuNhaps.ToList();

                    foreach (var t in listChiTietphieu)
                    {
                        if (t.Id == 0)
                        {
                            ChiTietPhieuNhap ct = new ChiTietPhieuNhap();
                            var hh = await _dACNPMContext.HangHoas.FirstOrDefaultAsync(x => x.Id == t.Idhh);
                            ct.Idpn = phieuNhap.Id;
                            ct.Idhh = hh.Id;
                            ct.Tgbh = hh.IdbaoHanhNavigation == null ? null : hh.IdbaoHanhNavigation.SoNgay;
                            ct.Idbh = hh.IdbaoHanh;
                            ct.Iddvtnhap = t.Iddvtnhap;
                            ct.Slqd = t.Slqd;
                            ct.Sl = t.Sl;
                            ct.DonGia = t.DonGia;
                            ct.Cktm = t.Cktm ?? 0;
                            ct.Thue = t.Thue ?? 0;
                            ct.SoLo = t.SoLo;
                            ct.Nsx = t.Nsx;
                            ct.Hsd = t.Hsd;
                            ct.GhiChu = t.GhiChu;
                            ct.Nvtao = _userId;
                            ct.NgayTao = phieuNhap.NgayTao;
                            ct.Active = true;
                            await _dACNPMContext.ChiTietPhieuNhaps.AddAsync(ct);
                            await _dACNPMContext.SaveChangesAsync();

                            HangTonKho sl = new HangTonKho();
                            sl.Idctpn = ct.Id;
                            sl.Slcon = Math.Round((double)t.Sl, 2);
                            sl.Idcn = idCn;
                            sl.NgayNhap = ct.NgayTao;
                            sl.Thue = ct.Thue ?? 0;
                            sl.Cktm = ct.Cktm ?? 0;
                            sl.GiaNhap = ct.DonGia;
                            sl.Hsd = ct.Hsd;
                            sl.Idhh = ct.Idhh;
                            await _dACNPMContext.HangTonKhos.AddAsync(sl);
                            await _dACNPMContext.SaveChangesAsync();
                        }
                        else
                        {
                            ChiTietPhieuNhap ct = await _dACNPMContext.ChiTietPhieuNhaps.FindAsync(t.Id);
                            HangTonKho sl = await _dACNPMContext.HangTonKhos.FirstOrDefaultAsync(x => x.Idctpn == ct.Id);
                            if (sl.Slcon != ct.Sl)
                            {
                                return Ok(new ResponseModel()
                                {
                                    statusCode = 500,
                                    message = "Phiếu đã được xuất kho, không thể sửa!"
                                });
                            }

                            var hh = await _dACNPMContext.HangHoas.FirstOrDefaultAsync(x => x.Id == t.Idhh);
                            ct.Idhh = hh.Id;
                            ct.Tgbh = hh.IdbaoHanhNavigation == null ? null : hh.IdbaoHanhNavigation.SoNgay;
                            ct.Idbh = hh.IdbaoHanh;
                            ct.Iddvtnhap = t.Iddvtnhap;
                            ct.Slqd = t.Slqd;
                            ct.Sl = t.Sl;
                            ct.DonGia = t.DonGia;
                            ct.Cktm = t.Cktm ?? 0;
                            ct.Thue = t.Thue ?? 0;
                            ct.SoLo = t.SoLo;
                            ct.Nsx = t.Nsx;
                            ct.Hsd = t.Hsd;
                            ct.GhiChu = t.GhiChu;
                            ct.Nvsua = _userId;
                            ct.NgaySua = phieuNhap.NgayTao;
                            _dACNPMContext.ChiTietPhieuNhaps.Update(ct);
                            await _dACNPMContext.SaveChangesAsync();

                            sl.Slcon = Math.Round((double)t.Sl, 2);
                            sl.Thue = ct.Thue ?? 0;
                            sl.Cktm = ct.Cktm ?? 0;
                            sl.GiaNhap = ct.DonGia;
                            sl.Hsd = ct.Hsd;
                            sl.Idhh = ct.Idhh;
                            _dACNPMContext.HangTonKhos.Update(sl);
                            await _dACNPMContext.SaveChangesAsync();
                        }
                    }
                    var p = await _dACNPMContext.PhieuNhapKhos
                        .Include(x => x.ChiTietPhieuNhaps)
                        .ThenInclude(x => x.HangTonKhos)
                        .Include(x => x.IdnccNavigation)
                        .Include(x => x.IdnvNavigation)
                        .FirstOrDefaultAsync(x=>x.Id == phieuNhap.Id);
                    p.Idncc = phieuNhap.Idncc;
                    p.NgayTao = phieuNhap.NgayTao;
                    p.NgayHd = phieuNhap.NgayHd;
                    p.SoHd = phieuNhap.SoHd;
                    p.GhiChu = phieuNhap.GhiChu;
                    p.Nvsua = _userId;
                    p.NgaySua = DateTime.Now;
                    await _dACNPMContext.SaveChangesAsync();

                    await tran.CommitAsync();
                    return Ok(new ResponseModel()
                    {
                        statusCode = 200,
                        message = "Thành công!",
                        result = p
                    });
                }
            }
            catch (Exception e)
            {
                tran.Rollback();
                return Ok(new ResponseModel()
                {
                    statusCode = 500,
                    message = "Thất bại!"
                });
            }

        }
        [HttpPost("showEditPhieuNhap")]
        public async Task<IActionResult> showEditPhieuNhap(int idPN)
        {
            var phieu = await _dACNPMContext.PhieuNhapKhos
                .Include(x => x.ChiTietPhieuNhaps)
                .ThenInclude(x => x.HangTonKhos)
                .Include(x => x.IdnccNavigation)
                .Include(x => x.IdnvNavigation)
                .FirstOrDefaultAsync(x => x.Id == idPN);
            return Ok(phieu);
        }
        [HttpPost("removePhieuNhap")]
        public async Task<IActionResult> removePhieuNhap(int idPN)
        {
            var tran = _dACNPMContext.Database.BeginTransaction();
            try
            {
                var phieu = await _dACNPMContext.PhieuNhapKhos
                    .Include(x => x.ChiTietPhieuNhaps)
                    .ThenInclude(x => x.ChiTietPhieuXuats)
                    .FirstOrDefaultAsync(x => x.Id == idPN);

                if (phieu.ChiTietPhieuNhaps.Any(x => x.ChiTietPhieuXuats.Count() > 0))
                {
                    return Ok(new ResponseModel()
                    {
                        statusCode = 500,
                        message = "Phiếu đã được xuất kho, không thể xoá!"
                    });
                }
                var phieuNhapKhoCts = phieu.ChiTietPhieuNhaps;
                var tonKhos = _dACNPMContext.HangTonKhos.AsEnumerable().Where(x => phieuNhapKhoCts.Any(y => y.Id == x.Idctpn)).ToList();

                _dACNPMContext.HangTonKhos.RemoveRange(tonKhos);
                await _dACNPMContext.SaveChangesAsync();

                _dACNPMContext.ChiTietPhieuNhaps.RemoveRange(phieu.ChiTietPhieuNhaps);
                await _dACNPMContext.SaveChangesAsync();

                _dACNPMContext.PhieuNhapKhos.Remove(phieu);
                await _dACNPMContext.SaveChangesAsync();

                await tran.CommitAsync();
                return Ok(new ResponseModel()
                {
                    statusCode = 200,
                    message = "Thành công!"
                });
            }
            catch (Exception e)
            {
                tran.Rollback();
                return Ok(new ResponseModel()
                {
                    statusCode = 500,
                    message = "Thất bại!"
                });
            }
        }
        [HttpPost("loadTableLichSuNhap")]
        public async Task<IActionResult> loadTableLichSuNhap(string fromDay, string toDay, string soPhieuLS, string soHDLS, int nhaCC, int hhLS)
        {
            int idCn = int.Parse(User.FindFirstValue("IdCn"));
            DateTime FromDay = DateTime.ParseExact(fromDay, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime ToDay = DateTime.ParseExact(toDay, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            List<PhieuNhapKho> listPhieu = _dACNPMContext.PhieuNhapKhos
                .Include(x => x.IdnccNavigation)
                .Include(x => x.IdnvNavigation)
                .Include(x => x.ChiTietPhieuNhaps)
            .AsParallel()
            .Where(x => x.NgayTao.Value.Date >= FromDay
                && x.NgayTao.Value.Date <= ToDay
                && x.Idcn == idCn
                && x.Active == true)
            .OrderByDescending(x => x.Id)
            .ToList();
            if (nhaCC == 0 && hhLS == 0)
            {
                ViewBag.ListPhieuNhap = listPhieu.AsParallel().Where(x => (soHDLS == null ? true : (x.SoHd?.Contains(soHDLS) ?? false))
                && (soPhieuLS == null ? true : x.SoPn.Contains(soPhieuLS.ToUpper())));
            }
            else
            {

                ViewBag.ListPhieuNhap = listPhieu.AsParallel().Where(x => (hhLS == 0 ? true : (x.ChiTietPhieuNhaps.Where(y => y.Idhh == hhLS).Count() > 0 ? true : false))
                && (nhaCC == 0 ? true : x.Idncc == nhaCC)
                && (soPhieuLS == null ? true : x.SoPn.Contains(soPhieuLS.ToUpper()))
                && (soHDLS == null ? true : (x.SoHd?.Contains(soHDLS.ToUpper()) ?? false)));
            }

            return PartialView("tableLichSuNhap");
        }
        [HttpPost("ViewThongTinPhieuNhap")]
        public IActionResult ViewThongTinPhieuNhap(int idPN)
        {
            var phieu = _dACNPMContext.PhieuNhapKhos
                .Include(x => x.ChiTietPhieuNhaps)
                .Include(x => x.IdnccNavigation)
                .Include(x => x.IdnvNavigation)
                .AsParallel()
                .FirstOrDefault(x => x.Id == idPN);
            ViewBag.HangHoas = getListHH().Result;
            return PartialView(phieu);
        }

        [Route("download/phieunhapkho/{id:int}")]
        public async Task<IActionResult> downloadPhieuNhap(int id)
        {
            var phieu = await _dACNPMContext.PhieuNhapKhos
                .Include(x => x.ChiTietPhieuNhaps)
                .ThenInclude(x => x.IdhhNavigation)
                .ThenInclude(x => x.IddvtchinhNavigation)
                .Include(x => x.IdnccNavigation)
                .Include(x => x.IdnvNavigation)
                .FirstOrDefaultAsync(x => x.Id == id);
            ViewBag.PhieuNhap = phieu;
            ViewBag.ttDoanhNghiep = await _dACNPMContext.ThongTinDoanhNghieps.FirstOrDefaultAsync();
            ViewBag.logo = CommonServices.ConvertImageToBase64(_hostingEnvironment, "/assets/images/logo2.png");
            PartialViewResult partialViewResult = PartialView("PhieuNhapKhoPDF");
            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                        ColorMode = ColorMode.Color,
                        Orientation = Orientation.Portrait,
                        PaperSize = PaperKind.A4,
                        Margins = new MarginSettings()
                        {
                            Left = 0.5,
                            Right = 0.5,
                            Unit = Unit.Centimeters
                        },
                    },
                Objects = {
                        new ObjectSettings() {
                            PagesCount = true,
                            HtmlContent = viewContent,
                            WebSettings = {
                                DefaultEncoding = "utf-8",
                            },
                            UseLocalLinks = true,
                            FooterSettings = { FontSize = 9, Right = "Trang [page]", Line = true, Spacing = 2.812 }
                        }
                    }
            };
            var pdfBytes = _converter.Convert(doc);

            return File(pdfBytes, "application/pdf", phieu.SoPn + ".pdf");
        }
        [HttpPost("download/BaoCaoPhieuNhap")]
        public async Task<IActionResult> downloadBaoCaoPhieuNhap(string fromDay, string toDay, string soPhieuLS, string soHDLS, int nhaCCLS, int hangHoaLS)
        {
            DateTime FromDay = DateTime.ParseExact(fromDay, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime ToDay = DateTime.ParseExact(toDay, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            ViewBag.tuNgay = fromDay;
            ViewBag.denNgay = toDay;

            List<PhieuNhapKho> listPhieu = await _dACNPMContext.PhieuNhapKhos
             .Include(x => x.IdnccNavigation)
            .Include(x => x.IdnvNavigation)
            .Include(x => x.ChiTietPhieuNhaps)
            .Where(x => x.Active == true
                && x.NgayTao.Value.Date >= FromDay
                && x.NgayTao.Value.Date <= ToDay)
            .OrderByDescending(x => x.Id)
            .ToListAsync();

            if (nhaCCLS == 0 && hangHoaLS == 0)
            {
                listPhieu = listPhieu.Where(x => (soHDLS == null ? true : (x.SoHd?.Contains(soHDLS) ?? false))
                && (soPhieuLS == null ? true : x.SoPn.Contains(soPhieuLS.ToUpper()))).ToList();
            }
            else
            {
                listPhieu = listPhieu.AsParallel().Where(x => (hangHoaLS == 0 ? true : (x.ChiTietPhieuNhaps.Where(y => y.Idhh == hangHoaLS).Count() > 0 ? true : false))
                && (nhaCCLS == 0 ? true : x.Idncc == nhaCCLS)
                && (soPhieuLS == null ? true : x.SoPn.Contains(soPhieuLS.ToUpper()))
                && (soHDLS == null ? true : (x.SoHd?.Contains(soHDLS.ToUpper()) ?? false))).ToList();
            }

            ViewBag.ttDoanhNghiep = await _dACNPMContext.ThongTinDoanhNghieps.FirstOrDefaultAsync();
            ViewBag.logo = CommonServices.ConvertImageToBase64(_hostingEnvironment, "/assets/images/logo2.png");
            PartialViewResult partialViewResult = PartialView("BaoCaoNhapKhoPDF", listPhieu);
            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                        ColorMode = ColorMode.Color,
                        Orientation = Orientation.Portrait,
                        PaperSize = PaperKind.A4,
                        Margins = new MarginSettings()
                        {
                            Left = 0.5,
                            Right = 0.5,
                            Unit = Unit.Centimeters
                        },
                    },
                Objects = {
                        new ObjectSettings() {
                            PagesCount = true,
                            HtmlContent = viewContent,
                            WebSettings = {
                                DefaultEncoding = "utf-8",
                            },
                            UseLocalLinks = true,
                            FooterSettings = { FontSize = 9, Right = "Trang [page]", Line = true, Spacing = 2.812 }
                        }
                    }
            };
            var pdfBytes = _converter.Convert(doc);

            return File(pdfBytes, "application/pdf", "BaoCaoNhapKho.pdf");
        }
        async Task<string> getSoPhieu()
        {
            var context = new DACNPMContext();
            //ID chi nhánh
            int cn = int.Parse(User.FindFirstValue("IdCn"));

            DateTime d = DateTime.Now;
            string ngayThangNam = d.ToString("yyMMdd");

            QuyDinhMa qd = await context.QuyDinhMas.FindAsync(1);
            string SoPhieu = cn + "_" + qd.TiepDauNgu + ngayThangNam;
            var list = await context.SoThuTus.FirstOrDefaultAsync(x => x.Ngay.Date == DateTime.Now.Date && x.Loai.Equals("NhapKho"));
            int stt;
            if (list == null)
            {
                SoThuTu sttt = new SoThuTu();
                sttt.Ngay = DateTime.ParseExact(DateTime.Now.ToString("dd-MM-yyyy"), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                sttt.Stt = 0;
                sttt.Loai = "NhapKho";
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
        async Task<List<NhaCungCap>> getListNhaCC()
        {
            int _userId = int.Parse(User.Identity.Name);
            return await _memoryCache.GetOrCreateAsync("NhaCC_" + _userId, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return _dACNPMContext.NhaCungCaps.ToList();
            });
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
        async Task<List<ChiTietPhieuNhap>> getListCTPN()
        {
            int _userId = int.Parse(User.Identity.Name);
            return await _memoryCache.GetOrCreateAsync("CTPNs_" + _userId, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return _dACNPMContext.ChiTietPhieuNhaps
                                .ToList();
            });
        }
        async Task<PhanQuyenChucNang> GetPhanQuyenNhapKho()
        {
            int idPq = int.Parse(User.FindFirstValue("IdPq"));

            return await _dACNPMContext.PhanQuyenChucNangs
                .Include(x => x.IdchucNangNavigation)
                .FirstOrDefaultAsync(x => x.Idpq == idPq
                && x.IdchucNangNavigation.MaChucNang.Equals("QL_NhapKho"));
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
        /*[HttpPost("api/solo")]
        public async Task<IActionResult> searchSoLo(int idHh)
        {
            var listPQnv = await getListCTPN();
            var sls = listPQnv.AsParallel()
                .Where(x => x.Idhh == idHh && x.Active == true)
                .DistinctBy(x => x.SoLo)
                .Select(x => new
                {
                    soLo = x.SoLo,
                })
                .ToList();
            var hh = getListHH().Result.FirstOrDefault(x => x.Id == idHh);
            return Ok(new
            {
                soLos = sls,
                dvt = hh.IddvtchinhNavigation.TenDvt,
                Avatar = hh.Avatar,
                alt = hh.TenHh
            });
        }

        [HttpPost("add-ctpnt")]
        public async Task<IActionResult> addCTPNT(int idHH, string SoLo, float ThueXuat, float SL, float DonGia,
            float ChietKhau, string HanDung, string NgaySX, string Dvt)
        {
            int _userId = int.Parse(User.Identity.Name);
            string host = GetLocalIPAddress();
            ChiTietPhieuNhapTam ct = new ChiTietPhieuNhapTam();
            ct.Idhh = idHH;
            ct.SoLo = SoLo;
            ct.Thue = Math.Round(ThueXuat, 2);
            ct.Sl = Math.Round(SL, 2);
            ct.DonGia = Math.Round(DonGia, 2);
            ct.Cktm = Math.Round(ChietKhau, 2);
            ct.Hsd = DateTime.ParseExact(HanDung, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            ct.Nsx = DateTime.ParseExact(NgaySX, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            ct.Host = host;
            ct.Dvt = Dvt;
            _dACNPMContext.ChiTietPhieuNhapTams.Add(ct);
            _dACNPMContext.SaveChanges();

            _memoryCache.Remove("CTPNTs_" + _userId);

            var ListCTPNT = GetListCTPNT();
            var lhh = getListHH();
            var p = GetPhanQuyenNhapKho();
            Task.WaitAll(ListCTPNT, lhh, p);

            ViewBag.CTPNTs = ListCTPNT.Result;
            ViewBag.HHs = lhh.Result;
            ViewBag.phanQuyenNhapKho = p.Result;
            PartialViewResult partialViewResult = PartialView("tableChiTietPhieuNhap");

            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);

            var TienHang = ListCTPNT.Result.Sum(x => x.Sl * x.DonGia);
            var TienCK = ListCTPNT.Result.Sum(x => (x.Sl * x.DonGia * x.Cktm) / 100);
            var TienThue = ListCTPNT.Result.Sum(x => (((x.Sl * x.DonGia) - ((x.Sl * x.DonGia * x.Cktm) / 100)) * x.Thue) / 100);
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
        [HttpPost("edit-ctpnt")]
        public IActionResult editCTPNT(int idHH, string SoLo, float ThueXuat, float SL, float DonGia,
            float ChietKhau, string HanDung, string NgaySX, string Dvt, int id)
        {
            int _userId = int.Parse(User.Identity.Name);
            string host = GetLocalIPAddress();
            ChiTietPhieuNhapTam ct = GetListCTPNT().Result.FirstOrDefault(x => x.Id == id);
            ct.Idhh = idHH;
            ct.SoLo = SoLo;
            ct.Thue = Math.Round(ThueXuat, 2);
            ct.Sl = Math.Round(SL, 2);
            ct.DonGia = Math.Round(DonGia, 2);
            ct.Cktm = Math.Round(ChietKhau, 2);
            ct.Hsd = DateTime.ParseExact(HanDung, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            ct.Nsx = DateTime.ParseExact(NgaySX, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            ct.Host = host;
            ct.Dvt = Dvt;
            _dACNPMContext.ChiTietPhieuNhapTams.Update(ct);
            _dACNPMContext.SaveChanges();

            _memoryCache.Remove("CTPNTs_" + _userId);

            var ListCTPNT = GetListCTPNT();
            var lhh = getListHH();
            var p = GetPhanQuyenNhapKho();
            Task.WaitAll(ListCTPNT, lhh, p);

            ViewBag.CTPNTs = ListCTPNT.Result;
            ViewBag.HHs = lhh.Result;
            ViewBag.phanQuyenNhapKho = p.Result;

            PartialViewResult partialViewResult = PartialView("tableChiTietPhieuNhap");

            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);

            var TienHang = ListCTPNT.Result.Sum(x => x.Sl * x.DonGia);
            var TienCK = ListCTPNT.Result.Sum(x => (x.Sl * x.DonGia * x.Cktm) / 100);
            var TienThue = ListCTPNT.Result.Sum(x => (((x.Sl * x.DonGia) - ((x.Sl * x.DonGia * x.Cktm) / 100)) * x.Thue) / 100);
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
        [HttpPost("delete-ctpnt")]
        public IActionResult deletePhieuNhapTam(int id)
        {
            ChiTietPhieuNhapTam ch = _dACNPMContext.ChiTietPhieuNhapTams.Find(id);
            int _userId = int.Parse(User.Identity.Name);
            _dACNPMContext.ChiTietPhieuNhapTams.Remove(ch);
            _dACNPMContext.SaveChanges();

            _memoryCache.Remove("CTPNTs_" + _userId);

            var ListCTPNT = GetListCTPNT();
            var lhh = getListHH();
            var p = GetPhanQuyenNhapKho();
            Task.WaitAll(ListCTPNT, lhh, p);

            ViewBag.CTPNTs = ListCTPNT.Result;
            ViewBag.HHs = lhh.Result;
            ViewBag.phanQuyenNhapKho = p.Result;

            PartialViewResult partialViewResult = PartialView("tableChiTietPhieuNhap");

            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);

            var TienHang = ListCTPNT.Result.Sum(x => x.Sl * x.DonGia);
            var TienCK = ListCTPNT.Result.Sum(x => (x.Sl * x.DonGia * x.Cktm) / 100);
            var TienThue = ListCTPNT.Result.Sum(x => (((x.Sl * x.DonGia) - ((x.Sl * x.DonGia * x.Cktm) / 100)) * x.Thue) / 100);
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
        [HttpPost("showEditCTPNT")]
        public async Task<IActionResult> editChitietPhieuNhapTam(int id)
        {
            var ctpnt = GetListCTPNT().Result.FirstOrDefault(x => x.Id == id);
            ctpnt.TenHh = getListHH().Result.FirstOrDefault(x => x.Id == ctpnt.Idhh).TenHh;
            var listPQnv = await getListCTPN();
            var sls = listPQnv.AsParallel()
                .Where(x => x.Idhh == ctpnt.Idhh && x.Active == true)
                .DistinctBy(x => x.SoLo)
                .Select(x => x.SoLo)
                .ToList();
            ViewBag.SoLos = sls;
            return PartialView("GroupChitietPhieuNhap", ctpnt);
        }*/
    }
}
