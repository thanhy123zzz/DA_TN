using DA_CNPM_VatTu.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace DA_CNPM_VatTu.Controllers
{
    [Authorize(Roles = "QD_GiaTheoKH")]
    [Route("/QuyDinh/[controller]")]
    public class GiaTheoKHController : Controller
    {
        private DACNPMContext _dACNPMContext;
        private readonly IMemoryCache _memoryCache;
        private ICompositeViewEngine _viewEngine;
        public GiaTheoKHController(IMemoryCache memoryCache, ICompositeViewEngine viewEngine)
        {
            _dACNPMContext = new DACNPMContext();
            _memoryCache = memoryCache;
            _viewEngine = viewEngine;
        }
        [HttpGet]
        public async Task<IActionResult> GiaTheoKH()
        {
            var pqcn = await GetPhanQuyenGiaTheoKH();
            ViewBag.PhanQuyenGiaTheoKH = pqcn;
            ViewData["title"] = pqcn.IdchucNangNavigation.TenChucNang;
            return View();
        }
        [HttpPost("load-hhdvt")]
        public async Task<IActionResult> loadHHDVT(int idHh)
        {
            ViewBag.Hhdvts = getListHHdvt().Result.AsParallel()
                .Where(x => x.Idhh == idHh && x.Active == true).ToList();

            ViewBag.Hhdvt = getListHH().Result.AsParallel().FirstOrDefault(x => x.Id == idHh);

            return PartialView("loadTableDVTHH");
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

        [HttpPost("load-gtkh")]
        public async Task<IActionResult> loadGTKH(int idHh, int idKh)
        {
            var pqcn = await GetPhanQuyenGiaTheoKH();
            ViewBag.PhanQuyenGiaTheoKH = pqcn;
            var giaTheoKH = await _dACNPMContext
                .GiaTheoKhachHangs
                .Include(x => x.IddvtNavigation)
                .Where(x => x.Idhh == idHh && x.Idkh == idKh && x.Active == true).ToListAsync();

            ViewBag.GTKHs = giaTheoKH;
            return PartialView("loadTableGiaKH");
        }
        [HttpPost("show-modal")]
        public async Task<IActionResult> show_Modal(int idGTKH, int idHh, int idKh)
        {
            var gtkh = _dACNPMContext.GiaTheoKhachHangs
                .Include(x => x.IddvtNavigation)
                .AsParallel().FirstOrDefault(x => x.Id == idGTKH);
            var dvtChinh = getListHH().Result.AsParallel().FirstOrDefault(x => x.Id == idHh).IddvtchinhNavigation;
            var listDvt = getListHHdvt().Result.AsParallel()
                .Where(x => x.Idhh == idHh && x.Active == true)
                .Select(x => x.IddvtNavigation).ToList();
            listDvt.Add(dvtChinh);

            var listDVTKH = _dACNPMContext.GiaTheoKhachHangs
                .Where(x => x.Idhh == idHh && x.Active == true && x.Idkh == idKh)
                .Select(x => x.IddvtNavigation)
                .ToList();

            var dvts = listDvt.AsParallel()
                .Where(x => !listDVTKH.Any(y => y.Id == x.Id)).ToList();
            ViewBag.Dvts = dvts;
            if (gtkh == null)
            {
                GiaTheoKhachHang g = new GiaTheoKhachHang();
                g.Idhh = idHh;
                g.Idkh = idKh;
                PartialViewResult partialViewResult = PartialView("formGiaTheoKH", g);
                string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);
                return Ok(new
                {
                    view = viewContent,
                    title = "Thêm giá bán theo khách hàng"
                });
            }
            else
            {
                PartialViewResult partialViewResult = PartialView("formGiaTheoKH", gtkh);
                string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);
                return Ok(new
                {
                    view = viewContent,
                    title = "Chỉnh sửa giá bán theo khách hàng"
                });
            }
        }
        [HttpPost("update-gtkh")]
        public async Task<IActionResult> updateGTKH([FromBody] GiaTheoKhachHang gtkh)
        {
            var tran = _dACNPMContext.Database.BeginTransaction();
            int _userId = int.Parse(User.Identity.Name);
            try
            {
                if (gtkh.Id == 0)
                {
                    gtkh.Active = true;
                    gtkh.Nvtao = int.Parse(User.Identity.Name);
                    gtkh.NgayTao = DateTime.Now;
                    gtkh.Idcn = int.Parse(User.FindFirstValue("IdCn"));
                    gtkh.TiLeLe = gtkh.TiLeLe == 0 ? null : gtkh.TiLeLe;
                    gtkh.TiLeSi = gtkh.TiLeSi == 0 ? null : gtkh.TiLeSi;
                    gtkh.GiaBanSi = gtkh.GiaBanSi == 0 ? null : gtkh.GiaBanSi;
                    gtkh.GiaBanLe = gtkh.GiaBanLe == 0 ? null : gtkh.GiaBanLe;

                    await _dACNPMContext.GiaTheoKhachHangs.AddAsync(gtkh);
                }
                else
                {
                    var gtkhDB = await _dACNPMContext.GiaTheoKhachHangs.FindAsync(gtkh.Id);

                    gtkhDB.Iddvt = gtkh.Iddvt;
                    gtkhDB.TiLeLe = gtkh.TiLeLe == 0 ? null : gtkh.TiLeLe;
                    gtkhDB.TiLeSi = gtkh.TiLeSi == 0 ? null : gtkh.TiLeSi;
                    gtkhDB.GiaBanLe = gtkh.GiaBanLe == 0 ? null : gtkh.GiaBanLe;
                    gtkhDB.GiaBanSi = gtkh.GiaBanSi == 0 ? null : gtkh.GiaBanSi;

                    gtkhDB.Nvsua = int.Parse(User.Identity.Name);
                    gtkhDB.NgaySua = DateTime.Now;

                    _dACNPMContext.GiaTheoKhachHangs.Update(gtkhDB);
                }

                await _dACNPMContext.SaveChangesAsync();
                await tran.CommitAsync();

                var pqcn = await GetPhanQuyenGiaTheoKH();
                ViewBag.PhanQuyenGiaTheoKH = pqcn;
                var giaTheoKH = await _dACNPMContext
                    .GiaTheoKhachHangs
                    .Include(x => x.IddvtNavigation)
                    .Where(x => x.Idhh == gtkh.Idhh
                    && x.Idkh == gtkh.Idkh && x.Active == true).ToListAsync();

                ViewBag.GTKHs = giaTheoKH;

                PartialViewResult partialViewResult = PartialView("loadTableGiaKH");
                string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);

                return Ok(new
                {
                    statusCode = 200,
                    message = "Thành công!",
                    color = "bg-success",
                    viewData = viewContent
                });
            }
            catch (Exception e)
            {
                await tran.RollbackAsync();
                return Ok(new
                {
                    statusCode = 500,
                    message = "Thất bại!",
                    color = "bg-danger"
                });
            }
        }
        [HttpPost("remove")]
        public async Task<IActionResult> remove(int idGTKH)
        {
            int _userId = int.Parse(User.Identity.Name);
            var itemDB = await _dACNPMContext.GiaTheoKhachHangs.FindAsync(idGTKH);
            itemDB.Active = !itemDB.Active;
            itemDB.Nvsua = int.Parse(User.Identity.Name);
            itemDB.NgaySua = DateTime.Now;
            _dACNPMContext.GiaTheoKhachHangs.Update(itemDB);

            await _dACNPMContext.SaveChangesAsync();

            var pqcn = await GetPhanQuyenGiaTheoKH();
            ViewBag.PhanQuyenGiaTheoKH = pqcn;
            var giaTheoKH = await _dACNPMContext
                .GiaTheoKhachHangs
                .Include(x => x.IddvtNavigation)
                .Where(x => x.Idhh == itemDB.Idhh
                && x.Idkh == itemDB.Idkh && x.Active == true).ToListAsync();

            ViewBag.GTKHs = giaTheoKH;

            PartialViewResult partialViewResult = PartialView("loadTableGiaKH");
            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);

            return Ok(new
            {
                statusCode = 200,
                message = "Thành công!",
                color = "bg-success",
                viewData = viewContent
            });
        }
        async Task<PhanQuyenChucNang> GetPhanQuyenGiaTheoKH()
        {
            int idPq = int.Parse(User.FindFirstValue("IdPq"));

            return await _dACNPMContext.PhanQuyenChucNangs
                .Include(x => x.IdchucNangNavigation)
                .FirstOrDefaultAsync(x => x.Idpq == idPq
                && x.IdchucNangNavigation.MaChucNang.Equals("QD_GiaTheoKH"));
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
        async Task<List<Hhdvt>> getListHHdvt()
        {
            int idCn = int.Parse(User.FindFirstValue("IdCn"));
            int _userId = int.Parse(User.Identity.Name);
            return await _memoryCache.GetOrCreateAsync("Hhdvts_" + _userId, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return _dACNPMContext.Hhdvts
                                .Include(x => x.IddvtNavigation)
                                .Where(x => x.Idcn == idCn)
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
        async Task<List<DonViTinh>> getListDVT()
        {
            int idcn = int.Parse(User.FindFirstValue("IdCn"));
            return await _memoryCache.GetOrCreateAsync("Dvts_" + idcn, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return await _dACNPMContext.DonViTinhs.ToListAsync();
            });
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
