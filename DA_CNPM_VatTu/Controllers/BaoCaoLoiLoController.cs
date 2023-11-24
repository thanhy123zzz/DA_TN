using AutoMapper;
using DA_CNPM_VatTu.Models.Entities;
using DA_CNPM_VatTu.Models.MapData;
using DA_CNPM_VatTu.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace DA_CNPM_VatTu.Controllers
{
    [Authorize(Roles = "QL_BaoCaoLoiLo")]
    [Route("QuanLy/[controller]")]
    public class BaoCaoLoiLoController : Controller
    {
        private DACNPMContext _dACNPMContext;
        private readonly IConverter _converter;
        private ICompositeViewEngine _viewEngine;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public BaoCaoLoiLoController(IConverter converter, ICompositeViewEngine viewEngine, IWebHostEnvironment hostingEnvironment)
        {
            _dACNPMContext = new DACNPMContext();
            _converter = converter;
            _viewEngine = viewEngine;
            _hostingEnvironment = hostingEnvironment;
        }
        [HttpGet]
        public async Task<IActionResult> BaoCaoLoiLo()
        {
            var p = await GetPhanQuyenBaoCaoLoiLo();

            ViewBag.phanQuyenBaoCaoLoiLo = p;
            ViewData["title"] = p.IdchucNangNavigation.TenChucNang;

            var now = DateTime.Now;
            ViewBag.tongPhieuXuat = _dACNPMContext.PhieuXuatKhos.Where(x => x.NgayTao.Value.Date == now.Date).Count();
            ViewBag.tongPhieuNhap = _dACNPMContext.PhieuNhapKhos.Where(x => x.NgayTao.Value.Date == now.Date).Count();
            ViewBag.tongSoLanBaoHanh = _dACNPMContext.ThongTinBaoHanhs.Where(x => x.NgayBaoHanh.Value.Date == now.Date).Count();
            ViewBag.khachHangMoi = _dACNPMContext.KhachHangs.Where(x => x.NgayTao.Value.Date == now.Date).Count();

            /*ViewBag.top10NhomHangBanChay = await _dACNPMContext.ChiTietPhieuXuats
                .Include(x=>x.IdhhNavigation)
                .ThenInclude(x=>x.IdnhhNavigation)
                .Where(x => x.NgayTao.Value.Date == now.Date).ToListAsync();*/
            return View();
        }
        [HttpPost("getTop10NhomHangBanChay")]
        public async Task<IActionResult> getTop10NhomHangBanChay(int tg)
        {
            int idCn = int.Parse(User.FindFirstValue("IdCn"));
            var now = DateTime.Now;
            var ctPhieuXuats = _dACNPMContext.ChiTietPhieuXuats
                .Include(x=>x.IdpxNavigation)
                        .Include(x => x.IdhhNavigation)
                        .ThenInclude(x => x.IdnhhNavigation).Where(x=>x.IdpxNavigation.Idcn == idCn);
            // ngày
            if (tg == 1)
            {
                var cts = await ctPhieuXuats.Where(x => x.NgayTao.Value.Date == now.Date).ToListAsync();
                var gr = cts.GroupBy(x => x.IdhhNavigation.IdnhhNavigation).ToList();
                return Ok(gr.Select(x => new
                {
                    ma = x.Key.MaNhh,
                    ten = x.Key.TenNhh,
                    slXuat = x.Sum(y => y.Sl),
                    giaTriXuat = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100))
                }).OrderByDescending(x => x.slXuat).Take(10).ToList());
            }
            // tuần
            if (tg == 2)
            {
                // Lấy ngày đầu tiên của tuần
                DateTime ngayDauTuan = now.Date.AddDays(-(int)now.DayOfWeek);

                // Lấy ngày cuối cùng của tuần
                DateTime ngayCuoiTuan = ngayDauTuan.AddDays(6);

                var cts = await ctPhieuXuats.Where(x => x.NgayTao.Value.Date <= ngayCuoiTuan.Date && x.NgayTao.Value.Date >= ngayDauTuan.Date).ToListAsync();
                var gr = cts.GroupBy(x => x.IdhhNavigation.IdnhhNavigation).ToList();
                return Ok(gr.Select(x => new
                {
                    ma = x.Key.MaNhh,
                    ten = x.Key.TenNhh,
                    slXuat = x.Sum(y => y.Sl),
                    giaTriXuat = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100))
                }).OrderByDescending(x => x.slXuat).Take(10).ToList());
            }
            // tháng
            if (tg == 3)
            {
                // Lấy ngày đầu tiên của tháng
                DateTime ngayDauThang = new DateTime(now.Year, now.Month, 1);

                // Lấy ngày cuối cùng của tháng
                DateTime ngayCuoiThang = ngayDauThang.AddMonths(1).AddDays(-1);

                var cts = await ctPhieuXuats.Where(x => x.NgayTao.Value.Date <= ngayCuoiThang.Date && x.NgayTao.Value.Date >= ngayDauThang.Date).ToListAsync();
                var gr = cts.GroupBy(x => x.IdhhNavigation.IdnhhNavigation).ToList();
                return Ok(gr.Select(x => new
                {
                    ma = x.Key.MaNhh,
                    ten = x.Key.TenNhh,
                    slXuat = x.Sum(y => y.Sl),
                    giaTriXuat = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100))
                }).OrderByDescending(x => x.slXuat).Take(10).ToList());
            }
            // quý
            if (tg == 4)
            {
                // Xác định quý của ngày hiện tại
                int quy = (now.Month - 1) / 3 + 1;

                // Lấy ngày đầu tiên của quý
                DateTime ngayDauQuy = new DateTime(now.Year, (quy - 1) * 3 + 1, 1);

                // Lấy ngày cuối cùng của quý
                DateTime ngayCuoiQuy = ngayDauQuy.AddMonths(3).AddDays(-1);

                var cts = await ctPhieuXuats.Where(x => x.NgayTao.Value.Date <= ngayCuoiQuy.Date && x.NgayTao.Value.Date >= ngayDauQuy.Date).ToListAsync();
                var gr = cts.GroupBy(x => x.IdhhNavigation.IdnhhNavigation).ToList();
                return Ok(gr.Select(x => new
                {
                    ma = x.Key.MaNhh,
                    ten = x.Key.TenNhh,
                    slXuat = x.Sum(y => y.Sl),
                    giaTriXuat = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100))
                }).OrderByDescending(x => x.slXuat).Take(10).ToList());
            }
            // năm
            if (tg == 5)
            {
                // Lấy ngày đầu tiên của năm
                DateTime ngayDauNam = new DateTime(now.Year, 1, 1);

                // Lấy ngày cuối cùng của năm
                DateTime ngayCuoiNam = new DateTime(now.Year, 12, 31);

                var cts = await ctPhieuXuats.Where(x => x.NgayTao.Value.Date <= ngayCuoiNam.Date && x.NgayTao.Value.Date >= ngayDauNam.Date).ToListAsync();
                var gr = cts.GroupBy(x => x.IdhhNavigation.IdnhhNavigation).ToList();
                return Ok(gr.Select(x => new
                {
                    ma = x.Key.MaNhh,
                    ten = x.Key.TenNhh,
                    slXuat = x.Sum(y => y.Sl),
                    giaTriXuat = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100))
                }).OrderByDescending(x => x.slXuat).Take(10).ToList());
            }
            return Ok();
        }
        [HttpPost("getGiaTriNhapXuat")]
        public async Task<IActionResult> getGiaTriNhapXuat(int tg)
        {
            int idCn = int.Parse(User.FindFirstValue("IdCn"));
            var now = DateTime.Now;
            var phieuNhapKhos = _dACNPMContext.PhieuNhapKhos
                        .Include(x => x.ChiTietPhieuNhaps).Where(x=>x.Idcn == idCn);
            var phieuXuatKhos = _dACNPMContext.PhieuXuatKhos
                        .Include(x => x.ChiTietPhieuXuats).Where(x => x.Idcn == idCn);
            List<dynamic> result = new List<dynamic>();
            // tuần
            if (tg == 1)
            {
                // Đặt ngày đầu tuần là ngày đầu tiên của tuần hiện tại
                DateTime startOfWeek = now.AddDays(-(int)now.DayOfWeek);

                // Đặt ngày cuối tuần là ngày cuối cùng của tuần hiện tại
                DateTime endOfWeek = startOfWeek.AddDays(6);

                // Lặp qua từ ngày đầu tuần đến ngày cuối tuần
                for (DateTime date = startOfWeek; date <= endOfWeek; date = date.AddDays(1))
                {
                    var pn = phieuNhapKhos.Where(x => x.NgayTao.Value.Date == date.Date);
                    var tongPn = pn.Count();
                    double? tongGtPn = 0;
                    await pn.ForEachAsync(Model =>
                    {
                        var ListCTPNT = Model.ChiTietPhieuNhaps.ToList();
                        var TienHang = ListCTPNT.Sum(x => x.Sl * x.DonGia);
                        var TienCK = ListCTPNT.Sum(x => (x.Sl * x.DonGia * x.Cktm) / 100);
                        var TienThue = ListCTPNT.Sum(x => (((x.Sl * x.DonGia) - ((x.Sl * x.DonGia * x.Cktm) / 100)) * x.Thue) / 100);
                        tongGtPn += TienHang - TienCK + TienThue;
                    });

                    var px = phieuXuatKhos.Where(x => x.NgayTao.Value.Date == date.Date);
                    var tongPx = px.Count();
                    double? tongGtPx = 0;
                    await px.ForEachAsync(Model =>
                    {
                        var ListCTPNT = Model.ChiTietPhieuXuats.ToList();
                        var TienHang = ListCTPNT.Sum(x => x.Sl * x.DonGia);
                        var TienCK = ListCTPNT.Sum(x => (x.Sl * x.DonGia * x.Cktm) / 100);
                        var TienThue = ListCTPNT.Sum(x => (((x.Sl * x.DonGia) - ((x.Sl * x.DonGia * x.Cktm) / 100)) * x.Thue) / 100);
                        tongGtPx += TienHang - TienCK + TienThue;
                    });
                    result.Add(new
                    {
                        ngay = date.ToString("dd-MM-yyyy"),
                        tongPn,
                        tongGtPn,
                        tongPx,
                        tongGtPx,
                        loiNhuan = tongGtPx - tongGtPn
                    });
                }
                return Ok(result);
            }
            // tháng
            if (tg == 2)
            {
                // Đặt ngày đầu tuần là ngày đầu tiên của tuần hiện tại
                DateTime startOfMonth = new DateTime(now.Year, now.Month, 1);

                // Đặt ngày cuối tuần là ngày cuối cùng của tuần hiện tại
                DateTime endOfMonth = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));

                // Lặp qua từ ngày đầu tuần đến ngày cuối tuần
                for (DateTime date = startOfMonth; date <= endOfMonth; date = date.AddDays(1))
                {
                    var pn = phieuNhapKhos.Where(x => x.NgayTao.Value.Date == date.Date);
                    var tongPn = pn.Count();
                    double? tongGtPn = 0;
                    await pn.ForEachAsync(Model =>
                    {
                        var ListCTPNT = Model.ChiTietPhieuNhaps.ToList();
                        var TienHang = ListCTPNT.Sum(x => x.Sl * x.DonGia);
                        var TienCK = ListCTPNT.Sum(x => (x.Sl * x.DonGia * x.Cktm) / 100);
                        var TienThue = ListCTPNT.Sum(x => (((x.Sl * x.DonGia) - ((x.Sl * x.DonGia * x.Cktm) / 100)) * x.Thue) / 100);
                        tongGtPn += TienHang - TienCK + TienThue;
                    });

                    var px = phieuXuatKhos.Where(x => x.NgayTao.Value.Date == date.Date);
                    var tongPx = px.Count();
                    double? tongGtPx = 0;
                    await px.ForEachAsync(Model =>
                    {
                        var ListCTPNT = Model.ChiTietPhieuXuats.ToList();
                        var TienHang = ListCTPNT.Sum(x => x.Sl * x.DonGia);
                        var TienCK = ListCTPNT.Sum(x => (x.Sl * x.DonGia * x.Cktm) / 100);
                        var TienThue = ListCTPNT.Sum(x => (((x.Sl * x.DonGia) - ((x.Sl * x.DonGia * x.Cktm) / 100)) * x.Thue) / 100);
                        tongGtPx += TienHang - TienCK + TienThue;
                    });
                    result.Add(new
                    {
                        ngay = date.ToString("dd"),
                        tongPn,
                        tongGtPn,
                        tongPx,
                        tongGtPx,
                        loiNhuan = tongGtPx - tongGtPn
                    });
                }
                return Ok(result);
            }
            // quý
            if (tg == 3)
            {
                // Lấy số quý của năm hiện tại
                int quarter = (now.Month - 1) / 3 + 1;
                var tuan = 1;
                // Lặp qua từng tháng trong quý
                for (int month = (quarter - 1) * 3 + 1; month <= quarter * 3; month++)
                {
                    DateTime startOfMonth = new DateTime(now.Year, month, 1);

                    // Lặp qua từng tuần trong tháng
                    for (DateTime date = startOfMonth; date.Month == month; date = date.AddDays(7))
                    {
                        DateTime startOfWeek = date;
                        DateTime endOfWeek = date.AddDays(6) < startOfMonth.AddMonths(1) ? date.AddDays(6) : startOfMonth.AddMonths(1).AddDays(-1);

                        var pn = phieuNhapKhos.Where(x => x.NgayTao.Value.Date <= endOfWeek.Date && x.NgayTao.Value.Date >= startOfWeek.Date);
                        var tongPn = pn.Count();
                        double? tongGtPn = 0;
                        await pn.ForEachAsync(Model =>
                        {
                            var ListCTPNT = Model.ChiTietPhieuNhaps.ToList();
                            var TienHang = ListCTPNT.Sum(x => x.Sl * x.DonGia);
                            var TienCK = ListCTPNT.Sum(x => (x.Sl * x.DonGia * x.Cktm) / 100);
                            var TienThue = ListCTPNT.Sum(x => (((x.Sl * x.DonGia) - ((x.Sl * x.DonGia * x.Cktm) / 100)) * x.Thue) / 100);
                            tongGtPn += TienHang - TienCK + TienThue;
                        });

                        var px = phieuXuatKhos.Where(x => x.NgayTao.Value.Date <= endOfWeek.Date && x.NgayTao.Value.Date >= startOfWeek.Date);
                        var tongPx = px.Count();
                        double? tongGtPx = 0;
                        await px.ForEachAsync(Model =>
                        {
                            var ListCTPNT = Model.ChiTietPhieuXuats.ToList();
                            var TienHang = ListCTPNT.Sum(x => x.Sl * x.DonGia);
                            var TienCK = ListCTPNT.Sum(x => (x.Sl * x.DonGia * x.Cktm) / 100);
                            var TienThue = ListCTPNT.Sum(x => (((x.Sl * x.DonGia) - ((x.Sl * x.DonGia * x.Cktm) / 100)) * x.Thue) / 100);
                            tongGtPx += TienHang - TienCK + TienThue;
                        });
                        result.Add(new
                        {
                            ngay = "Tuần " + tuan.ToString("D2"),
                            tongPn,
                            tongGtPn,
                            tongPx,
                            tongGtPx,
                            loiNhuan = tongGtPx - tongGtPn
                        });
                        tuan++;
                    }
                }
                return Ok(result);
            }
            // năm
            if (tg == 4)
            {
                // Lấy năm hiện tại
                int currentYear = DateTime.Now.Year;

                // Lặp qua từ tháng 1 đến tháng 12
                for (int month = 1; month <= 12; month++)
                {
                    // Lấy ngày đầu tháng
                    DateTime startOfMonth = new DateTime(currentYear, month, 1);

                    // Lấy ngày cuối tháng
                    DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                    var pn = phieuNhapKhos.Where(x => x.NgayTao.Value.Date <= endOfMonth.Date && x.NgayTao.Value.Date >= startOfMonth.Date);
                    var tongPn = pn.Count();
                    double? tongGtPn = 0;
                    await pn.ForEachAsync(Model =>
                    {
                        var ListCTPNT = Model.ChiTietPhieuNhaps.ToList();
                        var TienHang = ListCTPNT.Sum(x => x.Sl * x.DonGia);
                        var TienCK = ListCTPNT.Sum(x => (x.Sl * x.DonGia * x.Cktm) / 100);
                        var TienThue = ListCTPNT.Sum(x => (((x.Sl * x.DonGia) - ((x.Sl * x.DonGia * x.Cktm) / 100)) * x.Thue) / 100);
                        tongGtPn += TienHang - TienCK + TienThue;
                    });

                    var px = phieuXuatKhos.Where(x => x.NgayTao.Value.Date <= endOfMonth.Date && x.NgayTao.Value.Date >= startOfMonth.Date);
                    var tongPx = px.Count();
                    double? tongGtPx = 0;
                    await px.ForEachAsync(Model =>
                    {
                        var ListCTPNT = Model.ChiTietPhieuXuats.ToList();
                        var TienHang = ListCTPNT.Sum(x => x.Sl * x.DonGia);
                        var TienCK = ListCTPNT.Sum(x => (x.Sl * x.DonGia * x.Cktm) / 100);
                        var TienThue = ListCTPNT.Sum(x => (((x.Sl * x.DonGia) - ((x.Sl * x.DonGia * x.Cktm) / 100)) * x.Thue) / 100);
                        tongGtPx += TienHang - TienCK + TienThue;
                    });
                    result.Add(new
                    {
                        ngay = "Tháng " + month.ToString("D2"),
                        tongPn,
                        tongGtPn,
                        tongPx,
                        tongGtPx,
                        loiNhuan = tongGtPx - tongGtPn
                    });
                }
                return Ok(result);
            }
            return Ok();
        }
        [HttpPost("getTop5HangHoaDoanhThu")]
        public async Task<IActionResult> getTop5HangHoaDoanhThu(int tg)
        {
            int idCn = int.Parse(User.FindFirstValue("IdCn"));
            var now = DateTime.Now;
            var ctPhieuXuats = _dACNPMContext.ChiTietPhieuXuats
                        .Include(x => x.IdhhNavigation)
                        .Include(x=>x.IdpxNavigation)
                        .Include(x => x.IdctpnNavigation).Where(x=>x.IdpxNavigation.Idcn == idCn);
            // ngày
            if (tg == 1)
            {
                var cts = await ctPhieuXuats.Where(x => x.NgayTao.Value.Date == now.Date).ToListAsync();
                var gr = cts.GroupBy(x => x.IdhhNavigation).ToList();
                return Ok(gr.Select(x => new
                {
                    ma = x.Key.MaHh,
                    ten = x.Key.TenHh,
                    slXuat = x.Sum(y => y.Sl),
                    giaTriXuat = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100)),
                    giaTriNhap = x.Sum(y => y.IdctpnNavigation.DonGia * (1 - (y.IdctpnNavigation.Cktm ?? 0) / 100) * (1 + (y.IdctpnNavigation.Thue ?? 0) / 100)),
                    loiNhuan = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100)) - x.Sum(y => y.IdctpnNavigation.DonGia * (1 - (y.IdctpnNavigation.Cktm ?? 0) / 100) * (1 + (y.IdctpnNavigation.Thue ?? 0) / 100))
                }).OrderByDescending(x=>x.loiNhuan).Take(5).ToList());
            }
            // tuần
            if (tg == 2)
            {
                // Lấy ngày đầu tiên của tuần
                DateTime ngayDauTuan = now.Date.AddDays(-(int)now.DayOfWeek);

                // Lấy ngày cuối cùng của tuần
                DateTime ngayCuoiTuan = ngayDauTuan.AddDays(6);

                var cts = await ctPhieuXuats.Where(x => x.NgayTao.Value.Date <= ngayCuoiTuan.Date && x.NgayTao.Value.Date >= ngayDauTuan.Date).ToListAsync();
                var gr = cts.GroupBy(x => x.IdhhNavigation).ToList();
                return Ok(gr.Select(x => new
                {
                    ma = x.Key.MaHh,
                    ten = x.Key.TenHh,
                    slXuat = x.Sum(y => y.Sl),
                    giaTriXuat = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100)),
                    giaTriNhap = x.Sum(y => y.IdctpnNavigation.DonGia * (1 - (y.IdctpnNavigation.Cktm ?? 0) / 100) * (1 + (y.IdctpnNavigation.Thue ?? 0) / 100)),
                    loiNhuan = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100)) - x.Sum(y => y.IdctpnNavigation.DonGia * (1 - (y.IdctpnNavigation.Cktm ?? 0) / 100) * (1 + (y.IdctpnNavigation.Thue ?? 0) / 100))
                }).OrderByDescending(x => x.loiNhuan).Take(5).ToList());
            }
            // tháng
            if (tg == 3)
            {
                // Lấy ngày đầu tiên của tháng
                DateTime ngayDauThang = new DateTime(now.Year, now.Month, 1);

                // Lấy ngày cuối cùng của tháng
                DateTime ngayCuoiThang = ngayDauThang.AddMonths(1).AddDays(-1);

                var cts = await ctPhieuXuats.Where(x => x.NgayTao.Value.Date <= ngayCuoiThang.Date && x.NgayTao.Value.Date >= ngayDauThang.Date).ToListAsync();
                var gr = cts.GroupBy(x => x.IdhhNavigation).ToList();
                return Ok(gr.Select(x => new
                {
                    ma = x.Key.MaHh,
                    ten = x.Key.TenHh,
                    slXuat = x.Sum(y => y.Sl),
                    giaTriXuat = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100)),
                    giaTriNhap = x.Sum(y => y.IdctpnNavigation.DonGia * (1 - (y.IdctpnNavigation.Cktm ?? 0) / 100) * (1 + (y.IdctpnNavigation.Thue ?? 0) / 100)),
                    loiNhuan = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100)) - x.Sum(y => y.IdctpnNavigation.DonGia * (1 - (y.IdctpnNavigation.Cktm ?? 0) / 100) * (1 + (y.IdctpnNavigation.Thue ?? 0) / 100))
                }).OrderByDescending(x => x.loiNhuan).Take(5).ToList());
            }
            // quý
            if (tg == 4)
            {
                // Xác định quý của ngày hiện tại
                int quy = (now.Month - 1) / 3 + 1;

                // Lấy ngày đầu tiên của quý
                DateTime ngayDauQuy = new DateTime(now.Year, (quy - 1) * 3 + 1, 1);

                // Lấy ngày cuối cùng của quý
                DateTime ngayCuoiQuy = ngayDauQuy.AddMonths(3).AddDays(-1);

                var cts = await ctPhieuXuats.Where(x => x.NgayTao.Value.Date <= ngayCuoiQuy.Date && x.NgayTao.Value.Date >= ngayDauQuy.Date).ToListAsync();
                var gr = cts.GroupBy(x => x.IdhhNavigation).ToList();
                return Ok(gr.Select(x => new
                {
                    ma = x.Key.MaHh,
                    ten = x.Key.TenHh,
                    slXuat = x.Sum(y => y.Sl),
                    giaTriXuat = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100)),
                    giaTriNhap = x.Sum(y => y.IdctpnNavigation.DonGia * (1 - (y.IdctpnNavigation.Cktm ?? 0) / 100) * (1 + (y.IdctpnNavigation.Thue ?? 0) / 100)),
                    loiNhuan = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100)) - x.Sum(y => y.IdctpnNavigation.DonGia * (1 - (y.IdctpnNavigation.Cktm ?? 0) / 100) * (1 + (y.IdctpnNavigation.Thue ?? 0) / 100))
                }).OrderByDescending(x => x.loiNhuan).Take(5).ToList());
            }
            // năm
            if (tg == 5)
            {
                // Lấy ngày đầu tiên của năm
                DateTime ngayDauNam = new DateTime(now.Year, 1, 1);

                // Lấy ngày cuối cùng của năm
                DateTime ngayCuoiNam = new DateTime(now.Year, 12, 31);

                var cts = await ctPhieuXuats.Where(x => x.NgayTao.Value.Date <= ngayCuoiNam.Date && x.NgayTao.Value.Date >= ngayDauNam.Date).ToListAsync();
                var gr = cts.GroupBy(x => x.IdhhNavigation).ToList();
                return Ok(gr.Select(x => new
                {
                    ma = x.Key.MaHh,
                    ten = x.Key.TenHh,
                    slXuat = x.Sum(y => y.Sl),
                    giaTriXuat = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100)),
                    giaTriNhap = x.Sum(y => y.IdctpnNavigation.DonGia * (1 - (y.IdctpnNavigation.Cktm ?? 0) / 100) * (1 + (y.IdctpnNavigation.Thue ?? 0) / 100)),
                    loiNhuan = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100)) - x.Sum(y => y.IdctpnNavigation.DonGia * (1 - (y.IdctpnNavigation.Cktm ?? 0) / 100) * (1 + (y.IdctpnNavigation.Thue ?? 0) / 100))
                }).OrderByDescending(x => x.loiNhuan).Take(5).ToList());
            }
            return Ok();
        }
        [HttpPost("download/nhapXuat")]
        public async Task<IActionResult> downLoadNhapXuat(int tg)
        {
            int idCn = int.Parse(User.FindFirstValue("IdCn"));
            var now = DateTime.Now;
            var phieuNhapKhos = _dACNPMContext.PhieuNhapKhos
                        .Include(x => x.ChiTietPhieuNhaps).Where(x=>x.Idcn == idCn);
            var phieuXuatKhos = _dACNPMContext.PhieuXuatKhos
                        .Include(x => x.ChiTietPhieuXuats).Where(x => x.Idcn == idCn);
            List<dynamic> result = new List<dynamic>();
            // tuần
            if (tg == 1)
            {
                // Đặt ngày đầu tuần là ngày đầu tiên của tuần hiện tại
                DateTime startOfWeek = now.AddDays(-(int)now.DayOfWeek);

                // Đặt ngày cuối tuần là ngày cuối cùng của tuần hiện tại
                DateTime endOfWeek = startOfWeek.AddDays(6);
                ViewBag.TuNgay = startOfWeek.ToString("dd-MM-yyyy");
                ViewBag.DenNgay = endOfWeek.ToString("dd-MM-yyyy");
                // Lặp qua từ ngày đầu tuần đến ngày cuối tuần
                for (DateTime date = startOfWeek; date <= endOfWeek; date = date.AddDays(1))
                {
                    var pn = phieuNhapKhos.Where(x => x.NgayTao.Value.Date == date.Date);
                    var tongPn = pn.Count();
                    double? tongGtPn = 0;
                    await pn.ForEachAsync(Model =>
                    {
                        var ListCTPNT = Model.ChiTietPhieuNhaps.ToList();
                        var TienHang = ListCTPNT.Sum(x => x.Sl * x.DonGia);
                        var TienCK = ListCTPNT.Sum(x => (x.Sl * x.DonGia * x.Cktm) / 100);
                        var TienThue = ListCTPNT.Sum(x => (((x.Sl * x.DonGia) - ((x.Sl * x.DonGia * x.Cktm) / 100)) * x.Thue) / 100);
                        tongGtPn += TienHang - TienCK + TienThue;
                    });

                    var px = phieuXuatKhos.Where(x => x.NgayTao.Value.Date == date.Date);
                    var tongPx = px.Count();
                    double? tongGtPx = 0;
                    await px.ForEachAsync(Model =>
                    {
                        var ListCTPNT = Model.ChiTietPhieuXuats.ToList();
                        var TienHang = ListCTPNT.Sum(x => x.Sl * x.DonGia);
                        var TienCK = ListCTPNT.Sum(x => (x.Sl * x.DonGia * x.Cktm) / 100);
                        var TienThue = ListCTPNT.Sum(x => (((x.Sl * x.DonGia) - ((x.Sl * x.DonGia * x.Cktm) / 100)) * x.Thue) / 100);
                        tongGtPx += TienHang - TienCK + TienThue;
                    });
                    result.Add(new
                    {
                        ngay = date.ToString("dd-MM-yyyy"),
                        tongPn,
                        tongGtPn,
                        tongPx,
                        tongGtPx,
                        loiNhuan = tongGtPx - tongGtPn
                    });
                }
            }
            // tháng
            if (tg == 2)
            {
                // Đặt ngày đầu tuần là ngày đầu tiên của tuần hiện tại
                DateTime startOfMonth = new DateTime(now.Year, now.Month, 1);

                // Đặt ngày cuối tuần là ngày cuối cùng của tuần hiện tại
                DateTime endOfMonth = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
                ViewBag.TuNgay = startOfMonth.ToString("dd-MM-yyyy");
                ViewBag.DenNgay = endOfMonth.ToString("dd-MM-yyyy");
                // Lặp qua từ ngày đầu tuần đến ngày cuối tuần
                for (DateTime date = startOfMonth; date <= endOfMonth; date = date.AddDays(1))
                {
                    var pn = phieuNhapKhos.Where(x => x.NgayTao.Value.Date == date.Date);
                    var tongPn = pn.Count();
                    double? tongGtPn = 0;
                    await pn.ForEachAsync(Model =>
                    {
                        var ListCTPNT = Model.ChiTietPhieuNhaps.ToList();
                        var TienHang = ListCTPNT.Sum(x => x.Sl * x.DonGia);
                        var TienCK = ListCTPNT.Sum(x => (x.Sl * x.DonGia * x.Cktm) / 100);
                        var TienThue = ListCTPNT.Sum(x => (((x.Sl * x.DonGia) - ((x.Sl * x.DonGia * x.Cktm) / 100)) * x.Thue) / 100);
                        tongGtPn += TienHang - TienCK + TienThue;
                    });

                    var px = phieuXuatKhos.Where(x => x.NgayTao.Value.Date == date.Date);
                    var tongPx = px.Count();
                    double? tongGtPx = 0;
                    await px.ForEachAsync(Model =>
                    {
                        var ListCTPNT = Model.ChiTietPhieuXuats.ToList();
                        var TienHang = ListCTPNT.Sum(x => x.Sl * x.DonGia);
                        var TienCK = ListCTPNT.Sum(x => (x.Sl * x.DonGia * x.Cktm) / 100);
                        var TienThue = ListCTPNT.Sum(x => (((x.Sl * x.DonGia) - ((x.Sl * x.DonGia * x.Cktm) / 100)) * x.Thue) / 100);
                        tongGtPx += TienHang - TienCK + TienThue;
                    });
                    result.Add(new
                    {
                        ngay = date.ToString("dd-MM-yyyy"),
                        tongPn,
                        tongGtPn,
                        tongPx,
                        tongGtPx,
                        loiNhuan = tongGtPx - tongGtPn
                    });
                }
            }
            // quý
            if (tg == 3)
            {
                // Lấy số quý của năm hiện tại
                int quarter = (now.Month - 1) / 3 + 1;
                var tuan = 1;
                // Lặp qua từng tháng trong quý
                for (int month = (quarter - 1) * 3 + 1; month <= quarter * 3; month++)
                {
                    DateTime startOfMonth = new DateTime(now.Year, month, 1);

                    if (month == quarter * 3)
                    {
                        DateTime ngayCuoiCung = new DateTime(now.Year, month, 1).AddMonths(1).AddDays(-1);
                        ViewBag.DenNgay = ngayCuoiCung.ToString("dd-MM-yyyy");
                    }
                    if (month == (quarter - 1) * 3 + 1)
                    {
                        ViewBag.TuNgay = startOfMonth.ToString("dd-MM-yyyy");
                    }
                    // Lặp qua từng tuần trong tháng
                    for (DateTime date = startOfMonth; date.Month == month; date = date.AddDays(7))
                    {
                        DateTime startOfWeek = date;
                        DateTime endOfWeek = date.AddDays(6) < startOfMonth.AddMonths(1) ? date.AddDays(6) : startOfMonth.AddMonths(1).AddDays(-1);

                        var pn = phieuNhapKhos.Where(x => x.NgayTao.Value.Date <= endOfWeek.Date && x.NgayTao.Value.Date >= startOfWeek.Date);
                        var tongPn = pn.Count();
                        double? tongGtPn = 0;
                        await pn.ForEachAsync(Model =>
                        {
                            var ListCTPNT = Model.ChiTietPhieuNhaps.ToList();
                            var TienHang = ListCTPNT.Sum(x => x.Sl * x.DonGia);
                            var TienCK = ListCTPNT.Sum(x => (x.Sl * x.DonGia * x.Cktm) / 100);
                            var TienThue = ListCTPNT.Sum(x => (((x.Sl * x.DonGia) - ((x.Sl * x.DonGia * x.Cktm) / 100)) * x.Thue) / 100);
                            tongGtPn += TienHang - TienCK + TienThue;
                        });

                        var px = phieuXuatKhos.Where(x => x.NgayTao.Value.Date <= endOfWeek.Date && x.NgayTao.Value.Date >= startOfWeek.Date);
                        var tongPx = px.Count();
                        double? tongGtPx = 0;
                        await px.ForEachAsync(Model =>
                        {
                            var ListCTPNT = Model.ChiTietPhieuXuats.ToList();
                            var TienHang = ListCTPNT.Sum(x => x.Sl * x.DonGia);
                            var TienCK = ListCTPNT.Sum(x => (x.Sl * x.DonGia * x.Cktm) / 100);
                            var TienThue = ListCTPNT.Sum(x => (((x.Sl * x.DonGia) - ((x.Sl * x.DonGia * x.Cktm) / 100)) * x.Thue) / 100);
                            tongGtPx += TienHang - TienCK + TienThue;
                        });
                        result.Add(new
                        {
                            ngay = "Tuần " + tuan.ToString("D2"),
                            tongPn,
                            tongGtPn,
                            tongPx,
                            tongGtPx,
                            loiNhuan = tongGtPx - tongGtPn
                        });
                        tuan++;
                    }
                }
            }
            // năm
            if (tg == 4)
            {
                // Lấy năm hiện tại
                int currentYear = DateTime.Now.Year;
                ViewBag.TuNgay = "01-01-" + currentYear;
                ViewBag.DenNgay = "31-12-" + currentYear;
                // Lặp qua từ tháng 1 đến tháng 12
                for (int month = 1; month <= 12; month++)
                {
                    // Lấy ngày đầu tháng
                    DateTime startOfMonth = new DateTime(currentYear, month, 1);

                    // Lấy ngày cuối tháng
                    DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                    var pn = phieuNhapKhos.Where(x => x.NgayTao.Value.Date <= endOfMonth.Date && x.NgayTao.Value.Date >= startOfMonth.Date);
                    var tongPn = pn.Count();
                    double? tongGtPn = 0;
                    await pn.ForEachAsync(Model =>
                    {
                        var ListCTPNT = Model.ChiTietPhieuNhaps.ToList();
                        var TienHang = ListCTPNT.Sum(x => x.Sl * x.DonGia);
                        var TienCK = ListCTPNT.Sum(x => (x.Sl * x.DonGia * x.Cktm) / 100);
                        var TienThue = ListCTPNT.Sum(x => (((x.Sl * x.DonGia) - ((x.Sl * x.DonGia * x.Cktm) / 100)) * x.Thue) / 100);
                        tongGtPn += TienHang - TienCK + TienThue;
                    });

                    var px = phieuXuatKhos.Where(x => x.NgayTao.Value.Date <= endOfMonth.Date && x.NgayTao.Value.Date >= startOfMonth.Date);
                    var tongPx = px.Count();
                    double? tongGtPx = 0;
                    await px.ForEachAsync(Model =>
                    {
                        var ListCTPNT = Model.ChiTietPhieuXuats.ToList();
                        var TienHang = ListCTPNT.Sum(x => x.Sl * x.DonGia);
                        var TienCK = ListCTPNT.Sum(x => (x.Sl * x.DonGia * x.Cktm) / 100);
                        var TienThue = ListCTPNT.Sum(x => (((x.Sl * x.DonGia) - ((x.Sl * x.DonGia * x.Cktm) / 100)) * x.Thue) / 100);
                        tongGtPx += TienHang - TienCK + TienThue;
                    });
                    result.Add(new
                    {
                        ngay = "Tháng " + month.ToString("D2"),
                        tongPn,
                        tongGtPn,
                        tongPx,
                        tongGtPx,
                        loiNhuan = tongGtPx - tongGtPn
                    });
                }
            }
            ViewBag.Datas = result;
            ViewBag.ttDoanhNghiep = await _dACNPMContext.ThongTinDoanhNghieps.FirstOrDefaultAsync();
            ViewBag.logo = CommonServices.ConvertImageToBase64(_hostingEnvironment, "/assets/images/logo2.png");
            PartialViewResult partialViewResult = PartialView("nhapXuatPdf");
            string viewContent = CommonServices.ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);

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

            return File(pdfBytes, "application/pdf", "file.pdf");
        }
        [HttpPost("download/top10")]
        public async Task<IActionResult> downLoadTop10(int tg)
        {
            int idCn = int.Parse(User.FindFirstValue("IdCn"));
            var now = DateTime.Now;
            var ctPhieuXuats = _dACNPMContext.ChiTietPhieuXuats
                .Include(x => x.IdpxNavigation)
                        .Include(x => x.IdhhNavigation)
                        .ThenInclude(x => x.IdnhhNavigation).Where(x => x.IdpxNavigation.Idcn == idCn);
            dynamic datas = new object();
            // ngày
            if (tg == 1)
            {
                ViewBag.TuNgay = now.ToString("dd-MM-yyyy");
                ViewBag.DenNgay = now.ToString("dd-MM-yyyy");
                var cts = await ctPhieuXuats.Where(x => x.NgayTao.Value.Date == now.Date).ToListAsync();
                var gr = cts.GroupBy(x => x.IdhhNavigation.IdnhhNavigation).ToList();
                datas = gr.Select(x => new
                {
                    ma = x.Key.MaNhh,
                    ten = x.Key.TenNhh,
                    slXuat = x.Sum(y => y.Sl),
                    giaTriXuat = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100))
                }).OrderByDescending(x => x.slXuat).Take(10).ToList();
            }
            // tuần
            if (tg == 2)
            {
                // Lấy ngày đầu tiên của tuần
                DateTime ngayDauTuan = now.Date.AddDays(-(int)now.DayOfWeek);

                // Lấy ngày cuối cùng của tuần
                DateTime ngayCuoiTuan = ngayDauTuan.AddDays(6);
                ViewBag.TuNgay = ngayDauTuan.ToString("dd-MM-yyyy");
                ViewBag.DenNgay = ngayCuoiTuan.ToString("dd-MM-yyyy");
                var cts = await ctPhieuXuats.Where(x => x.NgayTao.Value.Date <= ngayCuoiTuan.Date && x.NgayTao.Value.Date >= ngayDauTuan.Date).ToListAsync();
                var gr = cts.GroupBy(x => x.IdhhNavigation.IdnhhNavigation).ToList();
                datas = gr.Select(x => new
                {
                    ma = x.Key.MaNhh,
                    ten = x.Key.TenNhh,
                    slXuat = x.Sum(y => y.Sl),
                    giaTriXuat = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100))
                }).OrderByDescending(x => x.slXuat).Take(10).ToList();
            }
            // tháng
            if (tg == 3)
            {
                // Lấy ngày đầu tiên của tháng
                DateTime ngayDauThang = new DateTime(now.Year, now.Month, 1);

                // Lấy ngày cuối cùng của tháng
                DateTime ngayCuoiThang = ngayDauThang.AddMonths(1).AddDays(-1);
                ViewBag.TuNgay = ngayDauThang.ToString("dd-MM-yyyy");
                ViewBag.DenNgay = ngayCuoiThang.ToString("dd-MM-yyyy");
                var cts = await ctPhieuXuats.Where(x => x.NgayTao.Value.Date <= ngayCuoiThang.Date && x.NgayTao.Value.Date >= ngayDauThang.Date).ToListAsync();
                var gr = cts.GroupBy(x => x.IdhhNavigation.IdnhhNavigation).ToList();
                datas = gr.Select(x => new
                {
                    ma = x.Key.MaNhh,
                    ten = x.Key.TenNhh,
                    slXuat = x.Sum(y => y.Sl),
                    giaTriXuat = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100))
                }).OrderByDescending(x => x.slXuat).Take(10).ToList();
            }
            // quý
            if (tg == 4)
            {
                // Xác định quý của ngày hiện tại
                int quy = (now.Month - 1) / 3 + 1;

                // Lấy ngày đầu tiên của quý
                DateTime ngayDauQuy = new DateTime(now.Year, (quy - 1) * 3 + 1, 1);

                // Lấy ngày cuối cùng của quý
                DateTime ngayCuoiQuy = ngayDauQuy.AddMonths(3).AddDays(-1);
                ViewBag.TuNgay = ngayDauQuy.ToString("dd-MM-yyyy");
                ViewBag.DenNgay = ngayCuoiQuy.ToString("dd-MM-yyyy");
                var cts = await ctPhieuXuats.Where(x => x.NgayTao.Value.Date <= ngayCuoiQuy.Date && x.NgayTao.Value.Date >= ngayDauQuy.Date).ToListAsync();
                var gr = cts.GroupBy(x => x.IdhhNavigation.IdnhhNavigation).ToList();
                datas = gr.Select(x => new
                {
                    ma = x.Key.MaNhh,
                    ten = x.Key.TenNhh,
                    slXuat = x.Sum(y => y.Sl),
                    giaTriXuat = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100))
                }).OrderByDescending(x => x.slXuat).Take(10).ToList();
            }
            // năm
            if (tg == 5)
            {
                // Lấy ngày đầu tiên của năm
                DateTime ngayDauNam = new DateTime(now.Year, 1, 1);

                // Lấy ngày cuối cùng của năm
                DateTime ngayCuoiNam = new DateTime(now.Year, 12, 31);
                ViewBag.TuNgay = ngayDauNam.ToString("dd-MM-yyyy");
                ViewBag.DenNgay = ngayCuoiNam.ToString("dd-MM-yyyy");
                var cts = await ctPhieuXuats.Where(x => x.NgayTao.Value.Date <= ngayCuoiNam.Date && x.NgayTao.Value.Date >= ngayDauNam.Date).ToListAsync();
                var gr = cts.GroupBy(x => x.IdhhNavigation.IdnhhNavigation).ToList();
                datas = gr.Select(x => new
                {
                    ma = x.Key.MaNhh,
                    ten = x.Key.TenNhh,
                    slXuat = x.Sum(y => y.Sl),
                    giaTriXuat = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100))
                }).OrderByDescending(x => x.slXuat).Take(10).ToList();
            }
            ViewBag.Datas = datas;
            ViewBag.ttDoanhNghiep = await _dACNPMContext.ThongTinDoanhNghieps.FirstOrDefaultAsync();
            ViewBag.logo = CommonServices.ConvertImageToBase64(_hostingEnvironment, "/assets/images/logo2.png");
            PartialViewResult partialViewResult = PartialView("Top10Pdf");
            string viewContent = CommonServices.ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);

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

            return File(pdfBytes, "application/pdf", "file.pdf");
        }
        [HttpPost("download/top5")]
        public async Task<IActionResult> downLoadTop5(int tg)
        {
            int idCn = int.Parse(User.FindFirstValue("IdCn"));
            var now = DateTime.Now;
            var ctPhieuXuats = _dACNPMContext.ChiTietPhieuXuats
                        .Include(x => x.IdhhNavigation)
                        .Include(x => x.IdpxNavigation)
                        .Include(x => x.IdctpnNavigation).Where(x => x.IdpxNavigation.Idcn == idCn);
            dynamic datas = new object();
            // ngày
            if (tg == 1)
            {
                ViewBag.TuNgay = now.ToString("dd-MM-yyyy");
                ViewBag.DenNgay = now.ToString("dd-MM-yyyy");
                var cts = await ctPhieuXuats.Where(x => x.NgayTao.Value.Date == now.Date).ToListAsync();
                var gr = cts.GroupBy(x => x.IdhhNavigation).ToList();
                datas = gr.Select(x => new
                {
                    ma = x.Key.MaHh,
                    ten = x.Key.TenHh,
                    slXuat = x.Sum(y => y.Sl),
                    giaTriXuat = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100)),
                    giaTriNhap = x.Sum(y => y.IdctpnNavigation.DonGia * (1 - (y.IdctpnNavigation.Cktm ?? 0) / 100) * (1 + (y.IdctpnNavigation.Thue ?? 0) / 100)),
                    loiNhuan = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100)) - x.Sum(y => y.IdctpnNavigation.DonGia * (1 - (y.IdctpnNavigation.Cktm ?? 0) / 100) * (1 + (y.IdctpnNavigation.Thue ?? 0) / 100))
                }).OrderByDescending(x => x.loiNhuan).Take(5).ToList();
            }
            // tuần
            if (tg == 2)
            {
                // Lấy ngày đầu tiên của tuần
                DateTime ngayDauTuan = now.Date.AddDays(-(int)now.DayOfWeek);

                // Lấy ngày cuối cùng của tuần
                DateTime ngayCuoiTuan = ngayDauTuan.AddDays(6);
                ViewBag.TuNgay = ngayDauTuan.ToString("dd-MM-yyyy");
                ViewBag.DenNgay = ngayCuoiTuan.ToString("dd-MM-yyyy");
                var cts = await ctPhieuXuats.Where(x => x.NgayTao.Value.Date <= ngayCuoiTuan.Date && x.NgayTao.Value.Date >= ngayDauTuan.Date).ToListAsync();
                var gr = cts.GroupBy(x => x.IdhhNavigation).ToList();
                datas = gr.Select(x => new
                {
                    ma = x.Key.MaHh,
                    ten = x.Key.TenHh,
                    slXuat = x.Sum(y => y.Sl),
                    giaTriXuat = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100)),
                    giaTriNhap = x.Sum(y => y.IdctpnNavigation.DonGia * (1 - (y.IdctpnNavigation.Cktm ?? 0) / 100) * (1 + (y.IdctpnNavigation.Thue ?? 0) / 100)),
                    loiNhuan = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100)) - x.Sum(y => y.IdctpnNavigation.DonGia * (1 - (y.IdctpnNavigation.Cktm ?? 0) / 100) * (1 + (y.IdctpnNavigation.Thue ?? 0) / 100))
                }).OrderByDescending(x => x.loiNhuan).Take(5).ToList();
            }
            // tháng
            if (tg == 3)
            {
                // Lấy ngày đầu tiên của tháng
                DateTime ngayDauThang = new DateTime(now.Year, now.Month, 1);

                // Lấy ngày cuối cùng của tháng
                DateTime ngayCuoiThang = ngayDauThang.AddMonths(1).AddDays(-1);
                ViewBag.TuNgay = ngayDauThang.ToString("dd-MM-yyyy");
                ViewBag.DenNgay = ngayCuoiThang.ToString("dd-MM-yyyy");
                var cts = await ctPhieuXuats.Where(x => x.NgayTao.Value.Date <= ngayCuoiThang.Date && x.NgayTao.Value.Date >= ngayDauThang.Date).ToListAsync();
                var gr = cts.GroupBy(x => x.IdhhNavigation).ToList();
                datas = gr.Select(x => new
                {
                    ma = x.Key.MaHh,
                    ten = x.Key.TenHh,
                    slXuat = x.Sum(y => y.Sl),
                    giaTriXuat = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100)),
                    giaTriNhap = x.Sum(y => y.IdctpnNavigation.DonGia * (1 - (y.IdctpnNavigation.Cktm ?? 0) / 100) * (1 + (y.IdctpnNavigation.Thue ?? 0) / 100)),
                    loiNhuan = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100)) - x.Sum(y => y.IdctpnNavigation.DonGia * (1 - (y.IdctpnNavigation.Cktm ?? 0) / 100) * (1 + (y.IdctpnNavigation.Thue ?? 0) / 100))
                }).OrderByDescending(x => x.loiNhuan).Take(5).ToList();

            }
            // quý
            if (tg == 4)
            {
                // Xác định quý của ngày hiện tại
                int quy = (now.Month - 1) / 3 + 1;

                // Lấy ngày đầu tiên của quý
                DateTime ngayDauQuy = new DateTime(now.Year, (quy - 1) * 3 + 1, 1);

                // Lấy ngày cuối cùng của quý
                DateTime ngayCuoiQuy = ngayDauQuy.AddMonths(3).AddDays(-1);
                ViewBag.TuNgay = ngayDauQuy.ToString("dd-MM-yyyy");
                ViewBag.DenNgay = ngayCuoiQuy.ToString("dd-MM-yyyy");
                var cts = await ctPhieuXuats.Where(x => x.NgayTao.Value.Date <= ngayCuoiQuy.Date && x.NgayTao.Value.Date >= ngayDauQuy.Date).ToListAsync();
                var gr = cts.GroupBy(x => x.IdhhNavigation).ToList();
                datas = gr.Select(x => new
                {
                    ma = x.Key.MaHh,
                    ten = x.Key.TenHh,
                    slXuat = x.Sum(y => y.Sl),
                    giaTriXuat = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100)),
                    giaTriNhap = x.Sum(y => y.IdctpnNavigation.DonGia * (1 - (y.IdctpnNavigation.Cktm ?? 0) / 100) * (1 + (y.IdctpnNavigation.Thue ?? 0) / 100)),
                    loiNhuan = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100)) - x.Sum(y => y.IdctpnNavigation.DonGia * (1 - (y.IdctpnNavigation.Cktm ?? 0) / 100) * (1 + (y.IdctpnNavigation.Thue ?? 0) / 100))
                }).OrderByDescending(x => x.loiNhuan).Take(5).ToList();
            }
            // năm
            if (tg == 5)
            {
                // Lấy ngày đầu tiên của năm
                DateTime ngayDauNam = new DateTime(now.Year, 1, 1);

                // Lấy ngày cuối cùng của năm
                DateTime ngayCuoiNam = new DateTime(now.Year, 12, 31);
                ViewBag.TuNgay = ngayDauNam.ToString("dd-MM-yyyy");
                ViewBag.DenNgay = ngayCuoiNam.ToString("dd-MM-yyyy");
                var cts = await ctPhieuXuats.Where(x => x.NgayTao.Value.Date <= ngayCuoiNam.Date && x.NgayTao.Value.Date >= ngayDauNam.Date).ToListAsync();
                var gr = cts.GroupBy(x => x.IdhhNavigation).ToList();
                datas = gr.Select(x => new
                {
                    ma = x.Key.MaHh,
                    ten = x.Key.TenHh,
                    slXuat = x.Sum(y => y.Sl),
                    giaTriXuat = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100)),
                    giaTriNhap = x.Sum(y => y.IdctpnNavigation.DonGia * (1 - (y.IdctpnNavigation.Cktm ?? 0) / 100) * (1 + (y.IdctpnNavigation.Thue ?? 0) / 100)),
                    loiNhuan = x.Sum(y => y.DonGia * (1 - (y.Cktm ?? 0) / 100) * (1 + (y.Thue ?? 0) / 100)) - x.Sum(y => y.IdctpnNavigation.DonGia * (1 - (y.IdctpnNavigation.Cktm ?? 0) / 100) * (1 + (y.IdctpnNavigation.Thue ?? 0) / 100))
                }).OrderByDescending(x => x.loiNhuan).Take(5).ToList();
            }
            ViewBag.Datas = datas;
            ViewBag.ttDoanhNghiep = await _dACNPMContext.ThongTinDoanhNghieps.FirstOrDefaultAsync();
            ViewBag.logo = CommonServices.ConvertImageToBase64(_hostingEnvironment, "/assets/images/logo2.png");
            PartialViewResult partialViewResult = PartialView("Top5Pdf");
            string viewContent = CommonServices.ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);

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

            return File(pdfBytes, "application/pdf", "file.pdf");
        }
        async Task<PhanQuyenChucNang> GetPhanQuyenBaoCaoLoiLo()
        {
            int idPq = int.Parse(User.FindFirstValue("IdPq"));

            return await _dACNPMContext.PhanQuyenChucNangs
                .Include(x => x.IdchucNangNavigation)
                .FirstOrDefaultAsync(x => x.Idpq == idPq
                && x.IdchucNangNavigation.MaChucNang.Equals("QL_BaoCaoLoiLo"));
        }
    }
}
