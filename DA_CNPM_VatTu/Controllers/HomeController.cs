using DA_CNPM_VatTu.Models;
using DA_CNPM_VatTu.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Claims;

namespace DA_CNPM_VatTu.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private DACNPMContext _dACNPMContext;
        private readonly IMemoryCache _memoryCache;
        private readonly IDataProtector _dataProtector;
        public HomeController(ILogger<HomeController> logger, IMemoryCache memoryCache, IDataProtectionProvider dataProtector)
        {
            _logger = logger;
            _dACNPMContext = new DACNPMContext();
            _memoryCache = memoryCache; 
            this._dataProtector = dataProtector.CreateProtector("this is my keycode");
        }
        [HttpGet("")]
        [HttpGet("/TrangChu")]
        public async Task<IActionResult> Index()
        {
            string format = "dd-MM-yyyy";
            int idCn = int.Parse(User.FindFirstValue("IdCn"));
            List<string> results = new List<string>();
            List<double> tongGiaNhap = new List<double>();
            List<double> tongGiaXuat = new List<double>();
            var nhapKho = await _dACNPMContext.ChiTietPhieuNhaps
                .Include(x => x.IdpnNavigation)
                .Where(x => x.IdpnNavigation.Idcn == idCn)
                .ToListAsync();
            var xuatKho = await _dACNPMContext.ChiTietPhieuXuats
                .Include(x => x.IdpxNavigation)
                .Include(x => x.IdhhNavigation.IdnhhNavigation)
                .Where(x => x.IdpxNavigation.Idcn == idCn)
                .ToListAsync();
            for (int i = 6; i >= 0; i--)
            {
                DateTime date = DateTime.Now.AddDays(-i);
                string result = date.ToString(format);
                results.Add(result);
                double sumNhap = nhapKho
                    .AsParallel()
                    .Where(x => x.IdpnNavigation.NgayTao.Value.Date == date.Date)
                    .Sum(x => (x.DonGia * x.Sl));
                tongGiaNhap.Add(sumNhap);

                double sumXuat = xuatKho
                    .AsParallel()
                    .Where(x => x.IdpxNavigation.NgayTao.Value.Date == date.Date)
                    .Sum(x => (x.DonGia * x.Sl)).Value;
                tongGiaXuat.Add(sumXuat);
            }
            string label = "['" + string.Join("', '", results) + "']";
            string dataNhap = "[" + string.Join(", ", tongGiaNhap) + "]";
            string dataXuat = "[" + string.Join(", ", tongGiaXuat) + "]";

            ViewBag.label = label;
            ViewBag.dataNhap = dataNhap;
            ViewBag.dataXuat = dataXuat;
            ViewBag.sumNhap = tongGiaNhap.Sum();
            ViewBag.sumXuat = tongGiaXuat.Sum();

            var giaTriNhh = xuatKho.AsParallel()
                                .Where(ctpx => ctpx.Active == true)
                                .GroupBy(ctpx => ctpx.IdhhNavigation.IdnhhNavigation.TenNhh)
                                .Select(g => new
                                {
                                    TenNhom = g.Key,
                                    TongGiaTriXuat = g.Sum(ctpx => ctpx.Sl * ctpx.DonGia ?? 0)
                                })
                                .OrderByDescending(x => x.TongGiaTriXuat)
                                .Take(10)
                                .ToList();
            ViewBag.giaTriNhh = "['" + string.Join("', '", giaTriNhh
                .Select(x => (double)x.GetType().GetProperty("TongGiaTriXuat").GetValue(x)).ToList()) + "']";
            ViewBag.tenNhh = "['" + string.Join("', '", giaTriNhh
                .Select(x => x.GetType().GetProperty("TenNhom").GetValue(x)).ToList()) + "']";

            ViewData["title"] = "Trang chủ";
            return View();
        }
        [HttpPost("/TrangChu/api/data")]
        public async Task<IActionResult> getData(int val)
        {
            int idCn = int.Parse(User.FindFirstValue("IdCn"));
            if (val == 0)
            {
                string format = "dd-MM-yyyy";

                List<string> results = new List<string>();
                List<double> tongGiaNhap = new List<double>();
                List<double> tongGiaXuat = new List<double>();
                var nhapKho = await _dACNPMContext.ChiTietPhieuNhaps
                    .Include(x => x.IdpnNavigation)
                    .Where(x => x.IdpnNavigation.Idcn == idCn)
                    .ToListAsync();
                var xuatKho = await _dACNPMContext.ChiTietPhieuXuats
                    .Include(x => x.IdpxNavigation)
                    .Where(x => x.IdpxNavigation.Idcn == idCn)
                    .ToListAsync();
                for (int i = 6; i >= 0; i--)
                {
                    DateTime date = DateTime.Now.AddDays(-i);
                    string result = date.ToString(format);
                    results.Add(result);
                    double sumNhap = nhapKho
                        .AsParallel()
                        .Where(x => x.IdpnNavigation.NgayTao.Value.Date == date.Date)
                        .Sum(x => (x.DonGia * x.Sl));
                    tongGiaNhap.Add(sumNhap);

                    double sumXuat = xuatKho
                        .AsParallel()
                        .Where(x => x.IdpxNavigation.NgayTao.Value.Date == date.Date)
                        .Sum(x => (x.DonGia * x.Sl)).Value;
                    tongGiaXuat.Add(sumXuat);
                }
                return Ok(new
                {
                    label = results,
                    dataNhap = tongGiaNhap,
                    dataXuat = tongGiaXuat,
                    sumNhap = tongGiaNhap.Sum(),
                    sumXuat = tongGiaXuat.Sum(),
                });

            }
            if (val == 1)
            {
                DateTime currentDate = DateTime.Now.Date;
                DateTime firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
                int daysIsMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);

                List<string> dateList = new List<string>();

                List<double> tongGiaNhap = new List<double>();
                List<double> tongGiaXuat = new List<double>();
                var nhapKho = await _dACNPMContext.ChiTietPhieuNhaps
                    .Include(x => x.IdpnNavigation)
                    .Where(x => x.IdpnNavigation.Idcn == idCn)
                    .ToListAsync();
                var xuatKho = await _dACNPMContext.ChiTietPhieuXuats
                    .Include(x => x.IdpxNavigation)
                    .Where(x => x.IdpxNavigation.Idcn == idCn)
                    .ToListAsync();

                for (int i = 0; i < daysIsMonth; i++)
                {
                    DateTime dateToAdd = firstDayOfMonth.AddDays(i);

                    dateList.Add(dateToAdd.ToString("dd"));
                    double sumNhap = nhapKho
                    .AsParallel()
                    .Where(x => x.IdpnNavigation.NgayTao.Value.Date == dateToAdd.Date)
                    .Sum(x => (x.DonGia * x.Sl));
                    tongGiaNhap.Add(sumNhap);

                    double sumXuat = xuatKho
                        .AsParallel()
                        .Where(x => x.IdpxNavigation.NgayTao.Value.Date == dateToAdd.Date)
                        .Sum(x => (x.DonGia * x.Sl)).Value;
                    tongGiaXuat.Add(sumXuat);

                }
                return Ok(new
                {
                    label = dateList,
                    dataNhap = tongGiaNhap,
                    dataXuat = tongGiaXuat,
                    sumNhap = tongGiaNhap.Sum(),
                    sumXuat = tongGiaXuat.Sum(),
                });
            }
            if (val == 2)
            {
                var nhapKho = await _dACNPMContext.ChiTietPhieuNhaps
                .Include(x => x.IdpnNavigation)
                .Where(x => x.IdpnNavigation.Idcn == idCn)
                .ToListAsync();
                var xuatKho = await _dACNPMContext.ChiTietPhieuXuats
                    .Include(x => x.IdpxNavigation)
                    .Where(x => x.IdpxNavigation.Idcn == idCn)
                    .ToListAsync();
                List<string> dateList = new List<string>();
                List<double> tongGiaNhap = new List<double>();
                List<double> tongGiaXuat = new List<double>();
                int year = DateTime.Now.Year;
                for (int i = 1; i <= 12; i++)
                {
                    dateList.Add("Tháng " + i);
                    double sumNhap = nhapKho
                        .AsParallel()
                        .Where(x => x.IdpnNavigation.NgayTao.Value.Month == i && x.IdpnNavigation.NgayTao.Value.Year == year)
                        .Sum(x => (x.DonGia * x.Sl));
                    tongGiaNhap.Add(sumNhap);

                    double sumXuat = xuatKho
                        .AsParallel()
                        .Where(x => x.IdpxNavigation.NgayTao.Value.Month == i && x.IdpxNavigation.NgayTao.Value.Year == year)
                        .Sum(x => (x.DonGia * x.Sl)).Value;
                    tongGiaXuat.Add(sumXuat);
                }
                return Ok(new
                {
                    label = dateList,
                    dataNhap = tongGiaNhap,
                    dataXuat = tongGiaXuat,
                    sumNhap = tongGiaNhap.Sum(),
                    sumXuat = tongGiaXuat.Sum(),
                });
            }
            return Ok();
        }
        [HttpGet("/DanhMuc")]
        public async Task<IActionResult> danhMuc()
        {
            ViewData["title"] = "Danh mục";

            int Idpq = int.Parse(User.FindFirstValue("IdPq"));

            var phanQuyenChucNangs = await _dACNPMContext.PhanQuyenChucNangs
                    .Include(x => x.IdchucNangNavigation)
                    .Where(x => x.Idpq == Idpq
                                && x.IdchucNangNavigation.MaChucNang.StartsWith("DM_"))
                    .ToListAsync();

            List<ChucNang> chucNangs = new List<ChucNang>();
            foreach (var c in phanQuyenChucNangs)
            {
                chucNangs.Add(c.IdchucNangNavigation);
            }
            ViewBag.danhMuc = chucNangs;
            return View();
        }

        [HttpGet("/CaiDat")]
        public IActionResult caiDat()
        {
            return View();
        }
        [HttpGet("/layout")]
        public IActionResult layout()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public bool IsBase64String(string s)
        {
            try
            {
                // Thử chuyển chuỗi thành mảng byte bằng Convert.FromBase64String
                byte[] data = Convert.FromBase64String(s);
                // Nếu thành công, đây là một Base-64 string hợp lệ
                return true;
            }
            catch (FormatException)
            {
                // Nếu có lỗi FormatException, đây không phải là Base-64 string hợp lệ
                return false;
            }
        }
    }
}