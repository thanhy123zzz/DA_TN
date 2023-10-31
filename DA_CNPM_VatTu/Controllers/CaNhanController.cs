using DA_CNPM_VatTu.Models.Entities;
using DA_CNPM_VatTu.Models.MapData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using System.Security.Claims;

namespace DA_CNPM_VatTu.Controllers
{
    [Authorize]
    public class CaNhanController : Controller
    {
        private readonly ILogger<CaNhanController> _logger;
        private DACNPMContext _dACNPMContext;
        private readonly IMemoryCache _memoryCache;
        private ICompositeViewEngine _viewEngine;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public CaNhanController(ILogger<CaNhanController> logger, IMemoryCache memoryCache, ICompositeViewEngine viewEngine, IWebHostEnvironment hostingEnvironment)
        {
            _logger = logger;
            _dACNPMContext = new DACNPMContext();
            _memoryCache = memoryCache;
            _viewEngine = viewEngine;
            _hostingEnvironment = hostingEnvironment;
        }
        [HttpGet("/CaNhan")]
        public async Task<IActionResult> CaNhan()
        {
            int _userId = int.Parse(User.Identity.Name);
            var nv = await _dACNPMContext.NhanViens
                .Include(x => x.IdtkNavigation)
                .Include(x => x.IdnnvNavigation)
                .FirstOrDefaultAsync(x => x.Id == _userId);
            var pqcn = await GetPhanQuyenCaNhan();
            if (pqcn != null)
            {
                ViewBag.phanQuyenCaNhan = pqcn;
                ViewData["title"] = pqcn.IdchucNangNavigation.TenChucNang;
            }
            else
            {
                ViewBag.phanQuyenCaNhan = new PhanQuyenChucNang() { Sua = false };
                ViewData["title"] = "Thông tin cá nhân";
            }

            return View(nv);
        }
        [HttpGet("/CaNhan/show-modal/{id}")]
        public async Task<IActionResult> showEdit(int id)
        {
            var nv = await _dACNPMContext.NhanViens
                .Include(x => x.IdtkNavigation)
                .Include(x => x.IdnnvNavigation)
                .FirstOrDefaultAsync(x => x.Id == id);

            PartialViewResult partialViewResult = PartialView("FormNV", nv);
            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);

            return Ok(new
            {
                view = viewContent,
                title = "Chỉnh sửa thông tin cá nhân"
            });
        }
        [HttpPost("/CaNhan/update-tt")]
        public async Task<IActionResult> updateNV(MapNhanVien mapNV)
        {
            var tran = _dACNPMContext.Database.BeginTransaction();
            try
            {
                NhanVien nv = mapNV.NhanVien;
                int _userId = int.Parse(User.Identity.Name);
                var nvDB = await _dACNPMContext.NhanViens
                        .FirstOrDefaultAsync(x => x.Id == nv.Id);
                if (nv != null)
                {
                    string webRootPath = _hostingEnvironment.WebRootPath;
                    string imagesPath = Path.Combine(webRootPath, "assets", "images", "NhanVien");

                    if (mapNV.FormFile != null)
                    {
                        string fileName = nvDB.MaNv + Path.GetExtension(mapNV.FormFile.FileName);
                        string filePath = Path.Combine(imagesPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await mapNV.FormFile.CopyToAsync(stream);
                        }
                        nv.Avatar = "/assets/images/NhanVien/" + fileName;
                    }
                    nvDB.TenNv = nv.TenNv;
                    nvDB.Email = nv.Email;
                    nvDB.DiaChi = nv.DiaChi;
                    nvDB.QueQuan = nv.QueQuan;
                    nvDB.Sdt = nv.Sdt;
                    nvDB.Cccd = nv.Cccd;
                    nvDB.GioiTinh = nv.GioiTinh;
                    nvDB.NgaySinh = DateTime.ParseExact(mapNV.NgaySinh, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    nvDB.Avatar = nv.Avatar == null ? nvDB.Avatar : nv.Avatar;
                    nvDB.Nvsua = _userId;
                    nvDB.NgaySua = DateTime.Now;

                    _dACNPMContext.NhanViens.Update(nvDB);

                }
                _dACNPMContext.SaveChanges();
                tran.Commit();

                _memoryCache.Remove("NhanViens_" + _userId);
                return Ok(new
                {
                    statusCode = 200,
                    message = "Thành công!",
                    color = "bg-success",
                    tt = nvDB
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
        async Task<PhanQuyenChucNang> GetPhanQuyenCaNhan()
        {
            int idPq = int.Parse(User.FindFirstValue("IdPq"));

            return await _dACNPMContext.PhanQuyenChucNangs
                .Include(x => x.IdchucNangNavigation)
                .FirstOrDefaultAsync(x => x.Idpq == idPq
                && x.IdchucNangNavigation.MaChucNang.Equals("TT_CaNhan"));
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
