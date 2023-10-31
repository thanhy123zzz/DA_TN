using DA_CNPM_VatTu.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SelectPdf;
using System.Drawing;
using System.Globalization;
using System.Security.Claims;

namespace DA_CNPM_VatTu.Controllers
{
    [Authorize(Roles = "QL_BaoCaoTon")]
    [Route("QuanLy/[controller]")]
    public class BaoCaoTonController : Controller
    {
        private DACNPMContext _dACNPMContext;
        private ICompositeViewEngine _viewEngine;
        private readonly ILogger<BaoCaoTonController> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private List<NhanVien> _nvs;
        private readonly IMemoryCache _memoryCache;
        public BaoCaoTonController(ILogger<BaoCaoTonController> logger,
            ICompositeViewEngine viewEngine, IMemoryCache memoryCache, IWebHostEnvironment hostingEnvironment)
        {
            _dACNPMContext = new DACNPMContext();
            _logger = logger;
            _viewEngine = viewEngine;
            _memoryCache = memoryCache;
            _hostingEnvironment = hostingEnvironment;
        }
        [HttpGet]
        public IActionResult BaoCaoTon()
        {
            var p = GetPhanQuyenBaoCaoTon();
            ViewBag.phanQuyen = p.Result;
            ViewData["title"] = p.Result.IdchucNangNavigation.TenChucNang;
            return View();
        }
        [HttpPost("search-table-baocaotonghop")]
        public async Task<IActionResult> searchTableBaoCaoTongHop(int idNhh, int idHh, string fromDay, string toDay)
        {
            int idCn = int.Parse(User.FindFirstValue("IdCn"));
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
            return PartialView("tableBaoCaoTongHop");
        }
        [HttpPost("download/BaoCaoTongHopPDF")]
        public IActionResult downloadBaoCaoTongHopPDF(int idNhh, int idHh, string fromDay, string toDay)
        {
            int idCn = int.Parse(User.FindFirstValue("IdCn"));
            var fullView = new HtmlToPdf();
            fullView.Options.WebPageWidth = 1280;
            fullView.Options.PdfPageSize = PdfPageSize.A4;
            fullView.Options.MarginTop = 20;
            fullView.Options.MarginBottom = 20;
            fullView.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

            var currentUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            var url = Url.Action("viewBaoCaoTongHopPDF", "Common", new { fromDay = fromDay, toDay = toDay, idNhh = idNhh, idHh = idHh, idCn = idCn });
            var pdf = fullView.ConvertUrl(currentUrl + url);

            var pdfBytes = pdf.Save();
            return File(pdfBytes, "application/pdf", "BaoCaoTongHopPDF.pdf");
        }
        [HttpPost("download/BaoCaoTongHopExcel")]
        public async Task<IActionResult> downloadBaoCaoTongHopExcel(int idNhh, int idHh, string fromDay, string toDay)
        {
            int idCn = int.Parse(User.FindFirstValue("IdCn"));
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

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage excel = new ExcelPackage();

            // Tạo một worksheet mới với tên "My Worksheet"
            var worksheet = excel.Workbook.Worksheets.Add("Báo Cáo Tổng Hợp Excel");

            // Ghi các thông tin header vào worksheet và merge các ô từ cột A đến cột D
            worksheet.Cells["A1:D1"].Merge = true;
            worksheet.Cells["A1"].Value = "CÔNG TY TNHH MTV THANH Ý";
            worksheet.Cells["A2:D2"].Merge = true;
            worksheet.Cells["A2"].Value = "Địa chỉ: 45 Nguyễn Trọng Lội, P4, Q.Tân Bình, TP HCM";
            worksheet.Cells["A3:B3"].Merge = true;
            worksheet.Cells["A3"].Value = "SĐT: 0329263644";
            worksheet.Cells["C3:D3"].Merge = true;
            worksheet.Cells["C3"].Value = "Email: thanhy123zzz@gmail.com";
            worksheet.Cells["A4:B4"].Merge = true;
            worksheet.Cells["A4"].Value = "MST: 21552663733";
            worksheet.Cells["C4:D4"].Merge = true;
            worksheet.Cells["C4"].Value = "Số TK: 022392726237";
            worksheet.Cells["A5:B5"].Merge = true;
            worksheet.Cells["A5"].Value = "Ngân hàng: TECHCOMBANK";
            worksheet.Cells["C5:D5"].Merge = true;
            worksheet.Cells["C5"].Value = "Chủ TK: Nguyễn Thanh Ý";

            // Thiết lập font chữ, cỡ chữ, in đậm, màu chữ và căn giữa cho header
            using (var range = worksheet.Cells["A1:D5"])
            {
                range.Style.Font.Bold = true;
                range.Style.Font.Color.SetColor(Color.Black);
                range.Style.Font.Size = 14;
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }

            worksheet.Column(1).Width = 30;
            worksheet.Column(2).Width = 30;
            worksheet.Column(3).Width = 50;
            worksheet.Column(4).Width = 30;
            worksheet.Column(5).Width = 30;
            worksheet.Cells["A6:E6"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            worksheet.Cells["A7:E7"].Merge = true;
            worksheet.Cells["A7:E7"].Style.Font.Bold = true;
            worksheet.Cells["A7:E7"].Style.Font.Color.SetColor(Color.Black);
            worksheet.Cells["A7:E7"].Style.Font.Size = 18;
            worksheet.Cells["A7:E7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A7:E7"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells["A7"].Value = "BÁO CÁO HÀNG TỒN TỔNG HỢP";

            worksheet.Cells[9, 1].Value = "STT";
            worksheet.Cells[9, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[9, 2].Value = "Mã hàng";
            worksheet.Cells[9, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[9, 3].Value = "Tên hàng hoá";
            worksheet.Cells[9, 3].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[9, 4].Value = "Số lượng tồn";
            worksheet.Cells[9, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[9, 5].Value = "Tổng giá trị tồn";
            worksheet.Cells[9, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);

            worksheet.Cells[9, 1, 9, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[9, 1, 9, 5].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            worksheet.Cells[9, 1, 9, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[9, 1, 9, 5].Style.Font.Bold = true;
            worksheet.Cells[9, 1, 9, 5].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[9, 1, 9, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            double? tongGiaTri = 0;
            int row = 10;
            foreach (NhomHangHoa n in Nhhs)
            {
                worksheet.Cells[row, 1, row, 5].Merge = true;
                worksheet.Cells[row, 1, row, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 1, row, 5].Style.Fill.BackgroundColor.SetColor(Color.AliceBlue);
                worksheet.Cells[row, 1, row, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[row, 1, row, 5].Style.Font.Bold = true;
                worksheet.Cells[row, 1, row, 5].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells[row, 1, row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                worksheet.Cells[row, 1, row, 5].Value = n.TenNhh;

                var htNhh = hangTons.AsParallel().Where(x => x.IdhhNavigation.Idnhh == n.Id)
                .DistinctBy(x => x.IdhhNavigation)
                .ToList();
                int stt = 1;
                row++;
                foreach (HangTonKho ht in htNhh)
                {
                    var hhHangTons = hangTons.AsParallel()
                    .Where(x => x.Idhh == ht.Idhh)
                    .ToList();
                    var sumSl = hhHangTons.AsParallel().Sum(x => x.Slcon);
                    var sumGia = hhHangTons.AsParallel().Sum(x => (x.Slcon * x.GiaNhap));
                    tongGiaTri += sumGia;


                    worksheet.Cells[row, 1].Value = stt;
                    worksheet.Cells[row, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 2].Value = ht.IdhhNavigation.MaHh;
                    worksheet.Cells[row, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 3].Value = ht.IdhhNavigation.TenHh;
                    worksheet.Cells[row, 3].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    worksheet.Cells[row, 4].Value = toDecimal(sumSl);
                    worksheet.Cells[row, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells[row, 5].Value = toDecimal(sumGia);
                    worksheet.Cells[row, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    worksheet.Cells[row, 1, row, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 1, row, 5].Style.Fill.BackgroundColor.SetColor(Color.White);
                    worksheet.Cells[row, 1, row, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    row++;
                    stt++;
                }
            }
            row++;
            worksheet.Cells[row, 1, row, 4].Merge = true;
            worksheet.Cells[row, 1, row, 4].Value = "Tổng giá trị";
            worksheet.Cells[row, 1, row, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[row, 1, row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[row, 4, row, 5].Value = toDecimal(tongGiaTri) + "(VNĐ)";
            worksheet.Cells[row, 4, row, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[row, 4, row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            worksheet.Cells[row, 1, row, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[row, 1, row, 5].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            worksheet.Cells[row, 1, row, 5].Style.Font.Bold = true;
            return File(excel.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BaoCaoTongHopExcel.xlsx");
        }
        [HttpPost("search-table-baocaochitiet")]
        public async Task<IActionResult> searchTableBaoCaoChiTiet(int idNhh, int idHh, string fromDay, string toDay, int idNcc)
        {
            int idCn = int.Parse(User.FindFirstValue("IdCn"));
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
                    && x.IdpnNavigation.Idcn == idCn
                    && x.HangTonKhos.Count > 0
                    )
                    .OrderBy(x => x.IdhhNavigation.TenHh)
                    .ToList();
            ViewBag.chiTietPhieuNhaps = chiTietPhieuNhaps;
            return PartialView("tableBaoCaoChiTiet");
        }
        [HttpPost("download/BaoCaoChiTietPDF")]
        public IActionResult downloadBaoCaoChiTietPDF(int idNhh, int idHh, string fromDay, string toDay, int idNcc)
        {
            int idCn = int.Parse(User.FindFirstValue("IdCn"));
            var fullView = new HtmlToPdf();
            fullView.Options.WebPageWidth = 1920;
            fullView.Options.PdfPageSize = PdfPageSize.A4;
            fullView.Options.MarginTop = 20;
            fullView.Options.MarginBottom = 20;
            fullView.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

            var currentUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            var url = Url.Action("viewBaoCaoChiTietPDF", "Common", new { fromDay = fromDay, toDay = toDay, idNhh = idNhh, idHh = idHh, idCn = idCn, idNcc = idNcc });
            var pdf = fullView.ConvertUrl(currentUrl + url);

            var pdfBytes = pdf.Save();
            return File(pdfBytes, "application/pdf", "BaoCaoChiTietPDF.pdf");
        }
        [HttpPost("download/BaoCaoChiTietExcel")]
        public async Task<IActionResult> downloadBaoCaoChiTietExcel(int idNhh, int idHh, string fromDay, string toDay, int idNcc)
        {
            int idCn = int.Parse(User.FindFirstValue("IdCn"));
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
                    .ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage excel = new ExcelPackage();

            // Tạo một worksheet mới với tên "My Worksheet"
            var worksheet = excel.Workbook.Worksheets.Add("Báo Cáo Hàng Tồn Chi Tiết Excel");

            // Ghi các thông tin header vào worksheet và merge các ô từ cột A đến cột D
            worksheet.Cells["A1:D1"].Merge = true;
            worksheet.Cells["A1"].Value = "CÔNG TY TNHH MTV THANH Ý";
            worksheet.Cells["A2:D2"].Merge = true;
            worksheet.Cells["A2"].Value = "Địa chỉ: 45 Nguyễn Trọng Lội, P4, Q.Tân Bình, TP HCM";
            worksheet.Cells["A3:B3"].Merge = true;
            worksheet.Cells["A3"].Value = "SĐT: 0329263644";
            worksheet.Cells["C3:D3"].Merge = true;
            worksheet.Cells["C3"].Value = "Email: thanhy123zzz@gmail.com";
            worksheet.Cells["A4:B4"].Merge = true;
            worksheet.Cells["A4"].Value = "MST: 21552663733";
            worksheet.Cells["C4:D4"].Merge = true;
            worksheet.Cells["C4"].Value = "Số TK: 022392726237";
            worksheet.Cells["A5:B5"].Merge = true;
            worksheet.Cells["A5"].Value = "Ngân hàng: TECHCOMBANK";
            worksheet.Cells["C5:D5"].Merge = true;
            worksheet.Cells["C5"].Value = "Chủ TK: Nguyễn Thanh Ý";

            // Thiết lập font chữ, cỡ chữ, in đậm, màu chữ và căn giữa cho header
            using (var range = worksheet.Cells["A1:D5"])
            {
                range.Style.Font.Bold = true;
                range.Style.Font.Color.SetColor(Color.Black);
                range.Style.Font.Size = 14;
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }

            worksheet.Column(1).Width = 20;
            worksheet.Column(2).Width = 30;
            worksheet.Column(3).Width = 60;
            worksheet.Column(4).Width = 30;
            worksheet.Column(5).Width = 50;
            worksheet.Column(6).Width = 30;
            worksheet.Column(7).Width = 30;
            worksheet.Column(8).Width = 30;
            worksheet.Column(9).Width = 30;
            worksheet.Column(10).Width = 30;
            worksheet.Column(11).Width = 30;
            worksheet.Column(12).Width = 30;
            worksheet.Column(13).Width = 30;

            worksheet.Cells["A6:M6"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            worksheet.Cells["A7:M7"].Merge = true;
            worksheet.Cells["A7:M7"].Style.Font.Bold = true;
            worksheet.Cells["A7:M7"].Style.Font.Color.SetColor(Color.Black);
            worksheet.Cells["A7:M7"].Style.Font.Size = 18;
            worksheet.Cells["A7:M7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A7:M7"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells["A7"].Value = "BÁO CÁO HÀNG TỒN CHI TIẾT";

            worksheet.Cells[9, 1].Value = "STT";
            worksheet.Cells[9, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[9, 2].Value = "Ngày nhập";
            worksheet.Cells[9, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[9, 3].Value = "Nhà cung cấp";
            worksheet.Cells[9, 3].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[9, 4].Value = "Mã hàng";
            worksheet.Cells[9, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[9, 5].Value = "Tên hàng";
            worksheet.Cells[9, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[9, 6].Value = "Số lô";
            worksheet.Cells[9, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[9, 7].Value = "Hạn dùng";
            worksheet.Cells[9, 7].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[9, 8].Value = "Số lượng nhập";
            worksheet.Cells[9, 8].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[9, 9].Value = "Số lượng xuất";
            worksheet.Cells[9, 9].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[9, 10].Value = "Số lượng tồn";
            worksheet.Cells[9, 10].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[9, 11].Value = "Đơn vị tính";
            worksheet.Cells[9, 11].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[9, 12].Value = "Đơn giá nhập";
            worksheet.Cells[9, 12].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[9, 13].Value = "Thành tiền";
            worksheet.Cells[9, 13].Style.Border.BorderAround(ExcelBorderStyle.Thin);

            worksheet.Cells[9, 1, 9, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[9, 1, 9, 13].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            worksheet.Cells[9, 1, 9, 13].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[9, 1, 9, 13].Style.Font.Bold = true;
            worksheet.Cells[9, 1, 9, 13].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[9, 1, 9, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            double? tongGiaTri = 0;
            int row = 10;
            int stt = 1;
            foreach (ChiTietPhieuNhap ht in chiTietPhieuNhaps)
            {
                worksheet.Cells[row, 1].Value = stt;
                worksheet.Cells[row, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row, 2].Value = dayToString(ht.NgayTao);
                worksheet.Cells[row, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row, 3].Value = ht.IdpnNavigation.IdnccNavigation.TenNcc;
                worksheet.Cells[row, 3].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells[row, 4].Value = ht.IdhhNavigation.MaHh;
                worksheet.Cells[row, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row, 5].Value = ht.IdhhNavigation.TenHh;
                worksheet.Cells[row, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells[row, 6].Value = ht.SoLo;
                worksheet.Cells[row, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells[row, 7].Value = dayToString(ht.Hsd);
                worksheet.Cells[row, 7].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row, 8].Value = toDecimal(ht.Sl);
                worksheet.Cells[row, 8].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[row, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[row, 9].Value = toDecimal(ht.Sl - (ht.HangTonKhos.Count == 0 ? 0 : ht.HangTonKhos.FirstOrDefault().Slcon));
                worksheet.Cells[row, 9].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[row, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[row, 10].Value = toDecimal(ht.HangTonKhos.Count == 0 ? 0 : ht.HangTonKhos.FirstOrDefault().Slcon);
                worksheet.Cells[row, 10].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[row, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[row, 11].Value = ht.IdhhNavigation.IddvtchinhNavigation.TenDvt;
                worksheet.Cells[row, 11].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[row, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells[row, 12].Value = toDecimal(ht.DonGia);
                worksheet.Cells[row, 12].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[row, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[row, 13].Value = toDecimal(ht.DonGia * (ht.HangTonKhos.Count == 0 ? 0 : ht.HangTonKhos.FirstOrDefault().Slcon));
                worksheet.Cells[row, 13].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                worksheet.Cells[row, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                worksheet.Cells[row, 1, row, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 1, row, 13].Style.Fill.BackgroundColor.SetColor(Color.White);
                worksheet.Cells[row, 1, row, 13].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                tongGiaTri += ht.DonGia * (ht.HangTonKhos.Count == 0 ? 0 : ht.HangTonKhos.FirstOrDefault().Slcon);
                row++;
                stt++;
            }
            worksheet.Cells[row, 1, row, 12].Merge = true;
            worksheet.Cells[row, 1, row, 12].Value = "Tổng giá trị";
            worksheet.Cells[row, 1, row, 12].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[row, 1, row, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[row, 4, row, 13].Value = toDecimal(tongGiaTri) + "(VNĐ)";
            worksheet.Cells[row, 4, row, 13].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            worksheet.Cells[row, 4, row, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            worksheet.Cells[row, 1, row, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[row, 1, row, 13].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            worksheet.Cells[row, 1, row, 13].Style.Font.Bold = true;
            return File(excel.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BaoCaoChoTietExcel.xlsx");
        }
        async Task<PhanQuyenChucNang> GetPhanQuyenBaoCaoTon()
        {
            int idPq = int.Parse(User.FindFirstValue("IdPq"));

            return await _dACNPMContext.PhanQuyenChucNangs
                .Include(x => x.IdchucNangNavigation)
                .FirstOrDefaultAsync(x => x.Idpq == idPq
                && x.IdchucNangNavigation.MaChucNang.Equals("QL_BaoCaoTon"));
        }
        string toDecimal(double? d)
        {
            if (d == null)
            {
                return "";
            }
            else
            {
                return d.Value.ToString("#,##0.00");
            }
        }
        string dayToString(DateTime? a)
        {
            if (a == null)
            {
                return "";
            }
            return a.Value.ToString("dd-MM-yyyy");
        }
    }
}
