using DA_CNPM_VatTu.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;

namespace DA_CNPM_VatTu.Controllers
{
    public class CommonController : Controller
    {
        private DACNPMContext _dACNPMContext;
        private ICompositeViewEngine _viewEngine;
        private readonly ILogger<CommonController> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private List<NhanVien> _nvs;
        private readonly IMemoryCache _memoryCache;
        public CommonController(ILogger<CommonController> logger,
            ICompositeViewEngine viewEngine, IMemoryCache memoryCache, IWebHostEnvironment hostingEnvironment)
        {
            _dACNPMContext = new DACNPMContext();
            _logger = logger;
            _viewEngine = viewEngine;
            _memoryCache = memoryCache;
            _hostingEnvironment = hostingEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        [Route("/QuanLy/NhapKho/phieuNhapKhoPDF/{id:int}")]
        public async Task<IActionResult> viewPDFPhieuNhap(int id)
        {
            var phieu = await _dACNPMContext.PhieuNhapKhos
                .Include(x => x.ChiTietPhieuNhaps)
                .Include(x => x.IdnccNavigation)
                .Include(x => x.IdnvNavigation)
                .FirstOrDefaultAsync(x => x.Id == id);
            var hh = await _dACNPMContext.HangHoas
                                .Include(x => x.IddvtchinhNavigation)
                                .Include(x => x.IdhsxNavigation)
                                .Include(x => x.IdnsxNavigation)
                                .Include(x => x.IdnhhNavigation)
                                .ToListAsync();
            ViewBag.HangHoas = hh;
            return View("PhieuNhapkhoPDF", phieu);
        }
        public IActionResult viewBaoCaoPhieuNhapPDF(string fromDay, string toDay, string soPhieuLS, string soHDLS, int nhaCC, int hhLS)
        {
            DateTime FromDay = DateTime.ParseExact(fromDay, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime ToDay = DateTime.ParseExact(toDay, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            ViewBag.tuNgay = fromDay;
            ViewBag.denNgay = toDay;

            List<PhieuNhapKho> listPhieu = _dACNPMContext.PhieuNhapKhos
             .Include(x => x.IdnccNavigation)
            .Include(x => x.IdnvNavigation)
            .Include(x => x.ChiTietPhieuNhaps)
            .AsParallel()
            .Where(x => x.NgayTao.Value.Date >= FromDay
            && x.NgayTao.Value.Date <= ToDay
            && x.Active == true)
            .OrderByDescending(x => x.Id)
            .ToList();
            if (nhaCC == 0 && hhLS == 0)
            {
                return View("BaocaophieunhapPDF", listPhieu.AsParallel().Where(x => (soHDLS == null ? true : (x.SoHd?.Contains(soHDLS) ?? false))
                && (soPhieuLS == null ? true : x.SoPn.Contains(soPhieuLS.ToUpper()))).ToList());
            }
            else
            {
                return View("BaocaophieunhapPDF", listPhieu.AsParallel().Where(x => (hhLS == 0 ? true : (x.ChiTietPhieuNhaps.Where(y => y.Idhh == hhLS).Count() > 0 ? true : false))
                && (nhaCC == 0 ? true : x.Idncc == nhaCC)
                && (soPhieuLS == null ? true : x.SoPn.Contains(soPhieuLS.ToUpper()))
                && (soHDLS == null ? true : (x.SoHd?.Contains(soHDLS.ToUpper()) ?? false))).ToList());
            }
        }

        [Route("/QuanLy/XuatKho/phieuXuatKhoPDF/{id:int}")]
        public async Task<IActionResult> viewPDFPhieuXuat(int id)
        {
            var phieu = _dACNPMContext.PhieuXuatKhos
                .Include(x => x.ChiTietPhieuXuats)
                .Include(x => x.IdkhNavigation)
                .Include(x => x.IdnvNavigation)
                .FirstOrDefaultAsync(x => x.Id == id);
            var hh = _dACNPMContext.HangHoas
                                .Include(x => x.IddvtchinhNavigation)
                                .Include(x => x.IdhsxNavigation)
                                .Include(x => x.IdnsxNavigation)
                                .Include(x => x.IdnhhNavigation)
                                .ToListAsync();
            ViewBag.HangHoas = hh.Result;
            return View("PhieuXuatKhoPDF", phieu.Result);
        }
        public IActionResult viewBaoCaoPhieuXuatPDF(string fromDay, string toDay, string soPhieuLS, string soHDLS, int KhLs, int hhLS)
        {
            DateTime FromDay = DateTime.ParseExact(fromDay, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime ToDay = DateTime.ParseExact(toDay, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            ViewBag.tuNgay = fromDay;
            ViewBag.denNgay = toDay;

            List<PhieuXuatKho> listPhieu = _dACNPMContext.PhieuXuatKhos
             .Include(x => x.IdkhNavigation)
            .Include(x => x.IdnvNavigation)
            .Include(x => x.ChiTietPhieuXuats)
            .AsParallel()
            .Where(x => x.NgayTao.Value.Date >= FromDay
            && x.NgayTao.Value.Date <= ToDay
            && x.Active == true)
            .OrderByDescending(x => x.Id)
            .ToList();
            if (KhLs == 0 && hhLS == 0)
            {
                return View("BaocaophieuxuatPDF", listPhieu.AsParallel().Where(x => (soHDLS == null ? true : (x.SoHd?.Contains(soHDLS) ?? false))
                && (soPhieuLS == null ? true : x.SoPx.Contains(soPhieuLS.ToUpper()))).ToList());
            }
            else
            {
                return View("BaocaophieuxuatPDF", listPhieu.AsParallel().Where(x => (hhLS == 0 ? true : (x.ChiTietPhieuXuats.Where(y => y.Idhh == hhLS).Count() > 0 ? true : false))
                && (KhLs == 0 ? true : x.Idkh == KhLs)
                && (soPhieuLS == null ? true : x.SoPx.Contains(soPhieuLS.ToUpper()))
                && (soHDLS == null ? true : (x.SoHd?.Contains(soHDLS.ToUpper()) ?? false))).ToList());
            }
        }
        public async Task<IActionResult> viewBaoCaoTongHopPDF(int idNhh, int idHh, string fromDay, string toDay, int idCn)
        {
            var _dACNPMContext = new DACNPMContext();
            DateTime FromDay = DateTime.ParseExact(fromDay, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime ToDay = DateTime.ParseExact(toDay, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var hangTons = await _dACNPMContext.HangTonKhos
                    .Include(x => x.IdhhNavigation)
                    .Include(x => x.IdhhNavigation.IdnhhNavigation)
                    .Where(x => (idNhh == 0 ? true : x.IdhhNavigation.Idnhh == idNhh)
                    && (idHh == 0 ? true : x.Idhh == idHh)
                    && x.NgayNhap.Value.Date >= FromDay
                    && x.NgayNhap.Value.Date <= ToDay
                    && x.Idcn == idCn)
                    .ToListAsync();
            var Nhhs = hangTons.AsParallel()
                    .Select(x => x.IdhhNavigation.IdnhhNavigation)
                    .Distinct()
                    .ToList();
            ViewBag.Nhhs = Nhhs;
            ViewBag.hangTons = hangTons;
            ViewBag.FromDay = FromDay;
            ViewBag.ToDay = ToDay;
            return View("viewBaoCaoTongHopPDF");
        }
        public IActionResult viewBaoCaoChiTietPDF(int idNhh, int idHh, string fromDay, string toDay, int idCn, int idNcc)
        {
            var _dACNPMContext = new DACNPMContext();
            DateTime FromDay = DateTime.ParseExact(fromDay, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime ToDay = DateTime.ParseExact(toDay, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var chiTietPhieuNhaps = _dACNPMContext.ChiTietPhieuNhaps
                    .Include(x => x.IdhhNavigation)
                    .Include(x => x.IdhhNavigation.IddvtchinhNavigation)
                    .Include(x => x.IdhhNavigation.IdnhhNavigation)
                    .Include(x => x.ChiTietPhieuXuats)
                    .Include(x => x.HangTonKhos)
                    .Include(x => x.IdpnNavigation)
                    .Include(x => x.IdpnNavigation.IdnccNavigation)
                    .AsParallel()
                    .Where(x => (idNhh == 0 ? true : x.IdhhNavigation.Idnhh == idNhh)
                    && (idNcc == 0 ? true : x.IdpnNavigation.Idncc == idNcc)
                    && (idHh == 0 ? true : x.Idhh == idHh)
                    && x.NgayTao.Value.Date >= FromDay
                    && x.NgayTao.Value.Date <= ToDay
                    && x.IdpnNavigation.Idcn == idCn)
                    .OrderBy(x => x.IdhhNavigation.TenHh)
                    .ToList();
            ViewBag.chiTietPhieuNhaps = chiTietPhieuNhaps;
            ViewBag.FromDay = FromDay;
            ViewBag.ToDay = ToDay;
            return View("viewBaoCaoChiTietPDF");
        }
    }
}
