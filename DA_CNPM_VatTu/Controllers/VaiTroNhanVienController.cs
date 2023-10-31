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
    [Authorize(Roles = "HT_VaiTroNhanVien")]
    [Route("/HeThong/[controller]")]
    public class VaiTroNhanVienController : Controller
    {
        private DACNPMContext _dACNPMContext;
        private readonly IMemoryCache _memoryCache;
        private ICompositeViewEngine _viewEngine;
        public VaiTroNhanVienController(IMemoryCache memoryCache, ICompositeViewEngine viewEngine)
        {
            _dACNPMContext = new DACNPMContext();
            _memoryCache = memoryCache;
            _viewEngine = viewEngine;
        }
        [HttpGet]
        public async Task<IActionResult> VaiTroNhanVien()
        {
            var pqcn = await GetPhanQuyenVaiTroNhanVien();
            ViewBag.PhanQuyenPQ = pqcn;
            ViewData["title"] = pqcn.IdchucNangNavigation.TenChucNang;
            var nvs = await getListNhanVien();
            ViewBag.nhanViens = nvs.Where(x => x.Active == true).ToList();
            return View();
        }
        [HttpPost("load-vaitro")]
        public async Task<IActionResult> loadVaiTro(int idNv)
        {
            var phanQuyenNhanViens = await getListPhanQuyenNhanVien();
            ViewBag.PhanQuyenNhanViens = phanQuyenNhanViens
                .Where(x => x.Idnv == idNv && x.Active == true).ToList();
            ViewBag.PhanQuyenPQ = await GetPhanQuyenVaiTroNhanVien();
            return PartialView("loadTableVaiTro");
        }
        [HttpPost("search-nv")]
        public async Task<IActionResult> searchNV(string key)
        {
            var listPQnv = await getListNhanVien();
            if (key == null)
            {
                return Ok(new
                {
                    data = listPQnv.AsParallel().Where(x => x.Active == true).ToList(),
                });
            }
            var vts = listPQnv.AsParallel()
                .Where(x => (x.MaNv + " " + x.TenNv).ToLower().Contains(key.ToLower()) && x.Active == true)
                .ToList();
            return Ok(new
            {
                data = vts
            });
        }
        [HttpPost("show-modal-vaitro")]
        public async Task<IActionResult> show_Modal_Vaitro(int idNv, int idVt)
        {
            var vaiTroNhanVienCN = await getListPhanQuyenNhanVien();

            var fullVTCN = getListPhanQuyenCN().Result.Select(x => x.IdvtNavigation).ToList();

            var vtNv = vaiTroNhanVienCN.Where(x => x.Idnv == idNv && x.Active == true).Select(x => x.IdpqNavigation.IdvtNavigation).ToList();

            var excepVt = fullVTCN.Where(x => !vtNv.Any(y => y.Id == x.Id)).ToList();

            var vt = fullVTCN.FirstOrDefault(x => x.Id == idVt);
            if (vt != null)
            {
                excepVt.Insert(0, vt);
            }
            PartialViewResult partialViewResult = PartialView("formThemVaiTroNhanVien", excepVt);
            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);
            return Ok(new
            {
                view = viewContent,
                title = vt == null ? "Thêm vai trò nhân viên" : "Chỉnh sửa vai trò nhân viên"
            });
        }
        [HttpPost("update-vaitro-nhanvien")]
        public async Task<IActionResult> updateVaiTroNhanVine(int idVt, int idNv)
        {
            int idCn = int.Parse(User.FindFirstValue("IdCn"));

            int _userId = int.Parse(User.Identity.Name);
            var phanQuyen = getListPhanQuyenCN().Result
                .FirstOrDefault(x => x.Idcn == idCn && x.Idvt == idVt);

            var phanQuyenNVofNV = await _dACNPMContext.PhanQuyenNhanViens
                .FirstOrDefaultAsync(x => x.Idpq == phanQuyen.Id && x.Idnv == idNv);

            if (phanQuyenNVofNV == null)
            {
                var phanQuyenNhanVien = new PhanQuyenNhanVien();
                phanQuyenNhanVien.Nvtao = int.Parse(User.Identity.Name);
                phanQuyenNhanVien.NgayTao = DateTime.Now;
                phanQuyenNhanVien.Idnv = idNv;
                phanQuyenNhanVien.Idpq = phanQuyen.Id;
                phanQuyenNhanVien.Active = true;
                await _dACNPMContext.PhanQuyenNhanViens.AddAsync(phanQuyenNhanVien);
            }
            else
            {
                phanQuyenNVofNV.Active = true;
                phanQuyenNVofNV.Nvsua = int.Parse(User.Identity.Name);
                phanQuyenNVofNV.NgaySua = DateTime.Now;
                _dACNPMContext.PhanQuyenNhanViens.Update(phanQuyenNVofNV);
            }
            _memoryCache.Remove("phanQuyenNhanVienCNs_" + _userId);

            await _dACNPMContext.SaveChangesAsync();

            var phanQuyenNhanViens = await getListPhanQuyenNhanVien();
            ViewBag.PhanQuyenNhanViens = phanQuyenNhanViens.Where(x => x.Idnv == idNv && x.Active == true).ToList();
            ViewBag.PhanQuyenPQ = await GetPhanQuyenVaiTroNhanVien();
            PartialViewResult partialViewResult = PartialView("loadTableVaiTro");
            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);

            return Ok(new
            {
                statusCode = 200,
                message = "Thành công!",
                color = "bg-success",
                viewData = viewContent
            });
        }

        [HttpPost("delete-vaitro-nhanvien")]
        public async Task<IActionResult> deleteVaiTro(int idPqnv)
        {
            int _userId = int.Parse(User.Identity.Name);
            var phanQuyenNV = await _dACNPMContext.PhanQuyenNhanViens
                .FirstOrDefaultAsync(x => x.Id == idPqnv);

            phanQuyenNV.Nvsua = int.Parse(User.Identity.Name);
            phanQuyenNV.NgaySua = DateTime.Now;
            phanQuyenNV.Active = false;

            _dACNPMContext.PhanQuyenNhanViens.Update(phanQuyenNV);

            await _dACNPMContext.SaveChangesAsync();

            _memoryCache.Remove("phanQuyenNhanVienCNs_" + _userId);

            var phanQuyenNhanViens = await getListPhanQuyenNhanVien();
            ViewBag.PhanQuyenNhanViens = phanQuyenNhanViens.Where(x => x.Idnv == phanQuyenNV.Idnv && x.Active == true).ToList();
            ViewBag.PhanQuyenPQ = await GetPhanQuyenVaiTroNhanVien();
            PartialViewResult partialViewResult = PartialView("loadTableVaiTro");
            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);

            return Ok(new
            {
                statusCode = 200,
                message = "Thành công!",
                color = "bg-success",
                viewData = viewContent
            });
        }
        async Task<List<PhanQuyenNhanVien>> getListPhanQuyenNhanVien()
        {
            int _userId = int.Parse(User.Identity.Name);
            return await _memoryCache.GetOrCreateAsync("phanQuyenNhanVienCNs_" + _userId, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                int idCn = int.Parse(User.FindFirstValue("IdCn"));
                return await _dACNPMContext.PhanQuyenNhanViens
                                    .Include(x => x.IdnvNavigation)
                                    .Include(x => x.IdpqNavigation)
                                    .Include(x => x.IdpqNavigation.IdvtNavigation)
                                    .Where(x => x.IdpqNavigation.Idcn == idCn)
                                    .ToListAsync();
            });
        }
        async Task<PhanQuyenChucNang> GetPhanQuyenVaiTroNhanVien()
        {
            int idPq = int.Parse(User.FindFirstValue("IdPq"));

            return await _dACNPMContext.PhanQuyenChucNangs
                .Include(x => x.IdchucNangNavigation)
                .FirstOrDefaultAsync(x => x.Idpq == idPq
                && x.IdchucNangNavigation.MaChucNang.Equals("HT_VaiTroNhanVien"));
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
        async Task<List<NhanVien>> getListNhanVien()
        {
            int _userId = int.Parse(User.Identity.Name);
            return await _memoryCache.GetOrCreateAsync("nhanViens_" + _userId, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return await _dACNPMContext.NhanViens.ToListAsync();
            });
        }
        async Task<List<PhanQuyen>> getListPhanQuyenCN()
        {
            int _userId = int.Parse(User.Identity.Name);
            return await _memoryCache.GetOrCreateAsync("vaiTros_" + _userId, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                int idCn = int.Parse(User.FindFirstValue("IdCn"));
                return await _dACNPMContext.PhanQuyens
                                    .Include(x => x.IdvtNavigation)
                                    .Where(x => x.Idcn == idCn && x.Active == true)
                                    .ToListAsync();
            });
        }
    }
}
