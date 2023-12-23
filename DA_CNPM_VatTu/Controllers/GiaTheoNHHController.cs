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
    [Authorize(Roles = "QD_GiaTheoNHH")]
    [Route("/QuyDinh/[controller]")]
    public class GiaTheoNHHController : Controller
    {
        private DACNPMContext _dACNPMContext;
        private readonly IMemoryCache _memoryCache;
        private ICompositeViewEngine _viewEngine;
        public GiaTheoNHHController(IMemoryCache memoryCache, ICompositeViewEngine viewEngine)
        {
            _dACNPMContext = new DACNPMContext();
            _memoryCache = memoryCache;
            _viewEngine = viewEngine;
        }
        [HttpGet]
        public async Task<IActionResult> GiaTheoNHH()
        {
            var pqcn = await GetPhanQuyenGiaTheoNHH();
            ViewBag.PhanQuyenPQ = pqcn;
            ViewBag.NHHs = getListNhomHH().Result.OrderBy(x=>x.TenNhh).ToList();
            ViewData["title"] = pqcn.IdchucNangNavigation.TenChucNang;
            return View();
        }
        [HttpPost("search-nhh")]
        public async Task<IActionResult> searchNHH(string key)
        {
            var listNhh = await getListNhomHH();
            if (key == null)
            {
                return Ok(new
                {
                    data = listNhh.AsParallel()
                    .Where(x => x.Active == true)
                    .Select(x => new
                    {
                        Id = x.Id,
                        TenNhh = x.TenNhh,
                        MaNhh = x.MaNhh
                    })
                    .ToList(),
                });
            }
            var vts = listNhh.AsParallel()
                .Where(x => (x.TenNhh + " " + x.MaNhh).ToLower().Contains(key.ToLower()) && x.Active == true)
                .ToList();
            return Ok(new
            {
                data = vts.Select(x => new
                {
                    Id = x.Id,
                    TenNhh = x.TenNhh,
                    MaNhh = x.MaNhh
                })
            });
        }

        [HttpPost("load-gtnhh")]
        public async Task<IActionResult> loadTableGTHHDVT(int idNhh)
        {
            int idCn = int.Parse(User.FindFirstValue("IdCn"));
            ViewBag.GTNHHs = _dACNPMContext.GiaTheoNhomHhs.AsParallel()
                .Where(x => x.Idnhh == idNhh && x.Active == true && x.Idcn == idCn).ToList();

            ViewBag.PhanQuyenPQ = await GetPhanQuyenGiaTheoNHH();
            return PartialView("loadTableGTNHH");
        }
        [HttpPost("show-modal-gtnhh")]
        public async Task<IActionResult> updateDVTChinh(int idGtnhh)
        {
            var gtnhh = _dACNPMContext.GiaTheoNhomHhs.AsParallel().FirstOrDefault(x => x.Id == idGtnhh);
            PartialViewResult partialViewResult = PartialView("formGTNHH", gtnhh == null ? new GiaTheoNhomHh() : gtnhh);
            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);

            return Ok(new
            {
                view = viewContent,
                title = gtnhh == null ? "Thêm đơn giá theo nhóm hàng hoá" : "Chỉnh sửa giá theo nhóm hàng hoá"
            });
        }
        [HttpPost("update-gtnhh")]
        public async Task<IActionResult> updateHHDVT([FromBody] GiaTheoNhomHh gtnhh)
        {
            if (gtnhh.Min >= gtnhh.Max)
            {
                return Ok(new
                {
                    statusCode = 500,
                    message = "Min phải bé hơn max!",
                    color = "bg-danger"
                });
            }
            var tran = _dACNPMContext.Database.BeginTransaction();
            int idCn = int.Parse(User.FindFirstValue("IdCn"));
            int _userId = int.Parse(User.Identity.Name);
            try
            {
                if (gtnhh.Id == 0)
                {
                    var ListGiaTheoNhomHh = _dACNPMContext.GiaTheoNhomHhs
                    .Where(x => x.Idnhh == gtnhh.Idnhh && x.Active == true).ToList();
                    foreach (GiaTheoNhomHh g in ListGiaTheoNhomHh)
                    {
                        if ((gtnhh.Min - g.Min >= 0 && gtnhh.Min - g.Max <= 0) || (gtnhh.Max - g.Max <= 0 && gtnhh.Max - g.Min >= 0))
                        {
                            return Ok(new
                            {
                                statusCode = 500,
                                message = "Các giá khoảng giá trị min max không được lặp!",
                                color = "bg-danger"
                            });
                        }
                    }
                    gtnhh.Active = true;
                    gtnhh.Nvtao = int.Parse(User.Identity.Name);
                    gtnhh.NgayTao = DateTime.Now;
                    gtnhh.TiLeSi = gtnhh.TiLeSi == 0 ? null : gtnhh.TiLeSi;
                    gtnhh.TiLeLe = gtnhh.TiLeLe == 0 ? null : gtnhh.TiLeLe;
                    gtnhh.Idcn = idCn;
                    await _dACNPMContext.GiaTheoNhomHhs.AddAsync(gtnhh);
                }
                else
                {
                    var ListGiaTheoNhomHh = _dACNPMContext.GiaTheoNhomHhs
                    .Where(x => x.Idnhh == gtnhh.Idnhh && x.Id != gtnhh.Id && x.Active == true).ToList();
                    ListGiaTheoNhomHh.Remove(gtnhh);
                    foreach (GiaTheoNhomHh g in ListGiaTheoNhomHh)
                    {
                        if ((gtnhh.Min - g.Min >= 0 && gtnhh.Min - g.Max <= 0) || (gtnhh.Max - g.Max <= 0 && gtnhh.Max - g.Min >= 0))
                        {
                            return Ok(new
                            {
                                statusCode = 500,
                                message = "Các giá khoảng giá trị min max không được lặp!",
                                color = "bg-danger"
                            });
                        }
                    }
                    var gtnhhDB = await _dACNPMContext.GiaTheoNhomHhs.FindAsync(gtnhh.Id);

                    gtnhhDB.TiLeLe = gtnhh.TiLeLe == 0 ? null : gtnhh.TiLeLe;
                    gtnhhDB.TiLeSi = gtnhh.TiLeSi == 0 ? null : gtnhh.TiLeSi;
                    gtnhhDB.Min = gtnhh.Min;
                    gtnhhDB.Max = gtnhh.Max;

                    gtnhhDB.Nvsua = int.Parse(User.Identity.Name);
                    gtnhhDB.NgaySua = DateTime.Now;

                    _dACNPMContext.GiaTheoNhomHhs.Update(gtnhhDB);
                }

                await _dACNPMContext.SaveChangesAsync();
                await tran.CommitAsync();

                ViewBag.GTNHHs = _dACNPMContext.GiaTheoNhomHhs.AsParallel()
                .Where(x => x.Idnhh == gtnhh.Idnhh && x.Active == true).ToList();

                ViewBag.PhanQuyenPQ = await GetPhanQuyenGiaTheoNHH();
                PartialViewResult partialViewResult = PartialView("loadTableGTNHH");
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
        public async Task<IActionResult> remove(int idGtnhh)
        {
            int _userId = int.Parse(User.Identity.Name);
            var itemDB = await _dACNPMContext.GiaTheoNhomHhs.FindAsync(idGtnhh);
            itemDB.Active = false;
            itemDB.Nvsua = int.Parse(User.Identity.Name);
            itemDB.NgaySua = DateTime.Now;
            _dACNPMContext.GiaTheoNhomHhs.Update(itemDB);
            await _dACNPMContext.SaveChangesAsync();

            ViewBag.GTNHHs = _dACNPMContext.GiaTheoNhomHhs.AsParallel()
                .Where(x => x.Idnhh == itemDB.Idnhh && x.Active == true).ToList();

            ViewBag.PhanQuyenPQ = await GetPhanQuyenGiaTheoNHH();
            PartialViewResult partialViewResult = PartialView("loadTableGTNHH");
            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);

            return Ok(new
            {
                statusCode = 200,
                message = "Thành công!",
                color = "bg-success",
                viewData = viewContent
            });
        }
        async Task<PhanQuyenChucNang> GetPhanQuyenGiaTheoNHH()
        {
            int idPq = int.Parse(User.FindFirstValue("IdPq"));

            return await _dACNPMContext.PhanQuyenChucNangs
                .Include(x => x.IdchucNangNavigation)
                .FirstOrDefaultAsync(x => x.Idpq == idPq
                && x.IdchucNangNavigation.MaChucNang.Equals("QD_GiaTheoNHH"));
        }
        async Task<List<NhomHangHoa>> getListNhomHH()
        {
            int _userId = int.Parse(User.Identity.Name);
            return await _memoryCache.GetOrCreateAsync("NhomHH_" + _userId, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return _dACNPMContext.NhomHangHoas.ToList();
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
