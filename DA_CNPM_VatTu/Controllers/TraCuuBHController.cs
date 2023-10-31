﻿using DA_CNPM_VatTu.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using System.Security.Claims;

namespace DA_CNPM_VatTu.Controllers
{
    [Authorize(Roles = "QL_TraCuuBH")]
    [Route("QuanLy/[controller]")]
    public class TraCuuBHController : Controller
    {
        private DACNPMContext _dACNPMContext;
        private ICompositeViewEngine _viewEngine;
        private readonly ILogger<TraCuuBHController> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private List<NhanVien> _nvs;
        private readonly IMemoryCache _memoryCache;
        public TraCuuBHController(ILogger<TraCuuBHController> logger,
    ICompositeViewEngine viewEngine, IMemoryCache memoryCache, IWebHostEnvironment hostingEnvironment)
        {
            _dACNPMContext = new DACNPMContext();
            _logger = logger;
            _viewEngine = viewEngine;
            _memoryCache = memoryCache;
            _hostingEnvironment = hostingEnvironment;
        }
        [HttpGet]
        public async Task<IActionResult> TraCuuBH()
        {
            var p = GetPhanQuyenTraCuuBH();
            ViewBag.phanQuyen = p.Result;
            ViewData["title"] = p.Result.IdchucNangNavigation.TenChucNang;
            return View();
        }
        [HttpPost("search-bh")]
        public async Task<IActionResult> searchBh(int idHh, string fromDay, string toDay, string Sdt, int idKh)
        {
            int idCn = int.Parse(User.FindFirstValue("IdCn"));
            DateTime FromDay = DateTime.ParseExact(fromDay, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            DateTime ToDay = DateTime.ParseExact(toDay, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            ViewBag.BHCT = await _dACNPMContext.ChiTietPhieuXuats
                .Include(x => x.IdhhNavigation)
                .Include(x => x.IdhhNavigation.IdbaoHanhNavigation)
                .Include(x => x.IddvtNavigation)
                .Include(x => x.IdpxNavigation)
                .Include(x => x.IdpxNavigation.IdkhNavigation)
                .Where(x => (idKh == 0 ? true : idKh == x.IdpxNavigation.Idkh)
                    && (idHh == 0 ? true : x.Idhh == idHh)
                    && x.IdpxNavigation.NgayTao.Value.Date >= FromDay
                    && x.IdpxNavigation.NgayTao.Value.Date <= ToDay
                    && (Sdt == null ? true : x.IdpxNavigation.IdkhNavigation.Sdt.ToLower().Contains(Sdt.ToLower()))
                ).ToListAsync();

            return PartialView("tableTraCuuBH");
        }
        async Task<PhanQuyenChucNang> GetPhanQuyenTraCuuBH()
        {
            int idPq = int.Parse(User.FindFirstValue("IdPq"));

            return await _dACNPMContext.PhanQuyenChucNangs
                .Include(x => x.IdchucNangNavigation)
                .FirstOrDefaultAsync(x => x.Idpq == idPq
                && x.IdchucNangNavigation.MaChucNang.Equals("QL_TraCuuBH"));
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
