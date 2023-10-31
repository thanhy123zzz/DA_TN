using AutoMapper;
using DA_CNPM_VatTu.Models;
using DA_CNPM_VatTu.Models.Entities;
using DA_CNPM_VatTu.Models.MapData;
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
    [Authorize(Roles = "QL_NhapKho")]
    [Route("QuanLy/[controller]")]
    public class NhapKhoController : Controller
    {
        private DACNPMContext _dACNPMContext;
        private ICompositeViewEngine _viewEngine;
        private readonly ILogger<DonViTinhController> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private List<NhanVien> _nvs;
        private readonly IMemoryCache _memoryCache;
        private readonly IMapper _mapper;
        public NhapKhoController(ILogger<DonViTinhController> logger,
            ICompositeViewEngine viewEngine, IMemoryCache memoryCache, IWebHostEnvironment hostingEnvironment, IMapper mapper)
        {
            _dACNPMContext = new DACNPMContext();
            _logger = logger;
            _viewEngine = viewEngine;
            _memoryCache = memoryCache;
            _hostingEnvironment = hostingEnvironment;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> NhapKho()
        {
            var ncc = getListNhaCC();
            var hh = getListHH();
            var ctpn = getListCTPN();
            var ctpnt = GetListCTPNT();
            var p = GetPhanQuyenNhapKho();
            await Task.WhenAll(ncc, hh, ctpn, p, ctpnt);

            ViewBag.SoPhieuNhap = getSoPhieu().Result;
            ViewBag.CTPNTs = ctpnt.Result;
            ViewBag.HHs = getListHH().Result;
            ViewBag.phanQuyenNhapKho = p.Result;
            ViewData["title"] = p.Result.IdchucNangNavigation.TenChucNang;
            return View();
        }
        [HttpPost("api/hhs")]
        public async Task<IActionResult> searchHH()
        {
            return Ok(await _dACNPMContext.HangHoas.Select(x => new
            {
                Id = x.Id,
                TenHh = x.TenHh.Trim(),
                MaHh = x.MaHh,
                TenDonViTinh = x.IddvtchinhNavigation.TenDvt,
                soLos = x.ChiTietPhieuNhaps.Select(x=>x.SoLo).Distinct().ToList()
            }).OrderBy(x => x.TenHh).ToListAsync());
        }

        [HttpPost("api/nccs")]
        public async Task<IActionResult> searchNhaCC()
        {
            var listPQnv = await getListNhaCC();
            return Ok(listPQnv.Select(x => new
            {
                Id = x.Id,
                TenNcc = x.TenNcc,
                MaNcc = x.MaNcc
            }).ToList());
        }
        [HttpPost("api/solo")]
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
        }
        [HttpPost("add-pn")]
        public async Task<IActionResult> addPN([FromBody] PhieuNhapKhoMap phieuNhapKhoMap)
        {
            PhieuNhapKho phieuNhap = _mapper.Map<PhieuNhapKho>(phieuNhapKhoMap);
            var tran = _dACNPMContext.Database.BeginTransaction();
            try
            {
                int _userId = int.Parse(User.Identity.Name);
                int idCn = int.Parse(User.FindFirstValue("IdCn"));
                phieuNhap.SoPn = await getSoPhieu();
                phieuNhap.Idnv = _userId;
                phieuNhap.Active = true;
                phieuNhap.Idcn = idCn;

                await _dACNPMContext.PhieuNhapKhos.AddAsync(phieuNhap);
                await _dACNPMContext.SaveChangesAsync();

                List<HangTonKho> listHt = new List<HangTonKho>();
                foreach (var t in phieuNhap.ChiTietPhieuNhaps)
                {
                    HangTonKho sl = new HangTonKho();
                    sl.Idctpn = t.Id;
                    sl.Slcon = Math.Round((double)t.Sl, 2);
                    sl.Idcn = idCn;
                    sl.NgayNhap = t.NgayTao;
                    sl.Thue = t.Thue;
                    sl.Cktm = t.Cktm;
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
        public IActionResult downloadPhieuNhap(int id)
        {
            var fullView = new HtmlToPdf();
            fullView.Options.WebPageWidth = 1280;
            fullView.Options.PdfPageSize = PdfPageSize.A4;
            fullView.Options.MarginTop = 20;
            fullView.Options.MarginBottom = 20;
            fullView.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

            var currentUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

            var pdf = fullView.ConvertUrl(currentUrl + "/QuanLy/NhapKho/phieuNhapKhoPDF/" + id);

            var pdfBytes = pdf.Save();
            return File(pdfBytes, "application/pdf", "PhieuNhap.pdf");
        }
        [HttpPost("download/BaoCaoPhieuNhap")]
        public IActionResult downloadBaoCaoPhieuNhap(string fromDay, string toDay, string soPhieuLS, string soHDLS, int nhaCCLS, int hangHoaLS)
        {
            var fullView = new HtmlToPdf();
            fullView.Options.WebPageWidth = 1280;
            fullView.Options.PdfPageSize = PdfPageSize.A4;
            fullView.Options.MarginTop = 20;
            fullView.Options.MarginBottom = 20;
            fullView.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

            var url = Url.Action("viewBaoCaoPhieuNhapPDF", "Common", new { fromDay = fromDay, toDay = toDay, soPhieuLS = soPhieuLS, soHDLS = soHDLS, nhaCC = nhaCCLS, hhLS = hangHoaLS });

            var currentUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}" + url;

            var pdf = fullView.ConvertUrl(currentUrl);

            var pdfBytes = pdf.Save();
            return File(pdfBytes, "application/pdf", "BaoCaoPhieuNhap.pdf");
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
        async Task<List<ChiTietPhieuNhapTam>> GetListCTPNT()
        {
            int _userId = int.Parse(User.Identity.Name);
            return await _memoryCache.GetOrCreateAsync("CTPNTs_" + _userId, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return _dACNPMContext.ChiTietPhieuNhapTams.Where(x => x.Host == GetLocalIPAddress())
                                .ToList();
            });
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
    }
}
