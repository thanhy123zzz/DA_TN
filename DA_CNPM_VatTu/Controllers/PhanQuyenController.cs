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
    [Authorize(Roles = "HT_PhanQuyen")]
    [Route("/HeThong/[controller]")]
    public class PhanQuyenController : Controller
    {
        private DACNPMContext _dACNPMContext;
        private readonly IMemoryCache _memoryCache;
        private ICompositeViewEngine _viewEngine;
        public PhanQuyenController(IMemoryCache memoryCache, ICompositeViewEngine viewEngine)
        {
            _dACNPMContext = new DACNPMContext();
            _memoryCache = memoryCache;
            _viewEngine = viewEngine;
        }
        [HttpGet]
        public async Task<IActionResult> phanQuyen()
        {
            ViewBag.vaiTros = getListPhanQuyen();
            var pqcn = await GetPhanQuyenPQ();
            ViewBag.PhanQuyenPQ = pqcn;
            ViewData["title"] = pqcn.IdchucNangNavigation.TenChucNang;
            return View();
        }
        [HttpGet("show-modal-vaitro/{id}")]
        public async Task<IActionResult> show_Modal_Vaitro(int id)
        {
            var fullVt = await _dACNPMContext.VaiTros.ToListAsync();
            var vt = fullVt.FirstOrDefault(x => x.Id == id);
            var currVt = getListPhanQuyen().Select(x => x.IdvtNavigation).ToList();

            var excepVt = fullVt.Where(x => !currVt.Any(y => y.Id == x.Id)).ToList();
            if (vt != null)
            {
                excepVt.Insert(0, vt);
            }
            PartialViewResult partialViewResult = PartialView("formThemVaiTroChiNhanh", excepVt);
            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);
            return Ok(new
            {
                view = viewContent,
                title = vt == null ? "Thêm vai trò" : "Chỉnh sửa vai trò"
            });
        }
        [HttpPost("update-vaitro")]
        public async Task<IActionResult> updateVaiTro(int idVT)
        {
            int _idUser = int.Parse(User.Identity.Name);
            int idCn = int.Parse(User.FindFirstValue("IdCn"));
            var phanQuyen = _dACNPMContext.PhanQuyens
                .FirstOrDefault(x => x.Idcn == idCn && x.Idvt == idVT);
            if (phanQuyen == null)
            {
                phanQuyen = new PhanQuyen();
                phanQuyen.Nvtao = int.Parse(User.Identity.Name);
                phanQuyen.NgayTao = DateTime.Now;
                phanQuyen.Idvt = idVT;
                phanQuyen.Idcn = idCn;
                phanQuyen.Active = true;

                _dACNPMContext.PhanQuyens.Add(phanQuyen);
            }
            else
            {
                phanQuyen.Idvt = idVT;
                phanQuyen.Nvsua = int.Parse(User.Identity.Name);
                phanQuyen.NgaySua = DateTime.Now;
                phanQuyen.Active = true;

                _dACNPMContext.PhanQuyens.Update(phanQuyen);
            }
            await _dACNPMContext.SaveChangesAsync();
            _memoryCache.Remove("vaiTros_" + _idUser);
            var vts = getListPhanQuyen();
            var pq = await GetPhanQuyenPQ();
            return Ok(new
            {
                statusCode = 200,
                message = "Thành công!",
                color = "bg-success",
                data = vts,
                xoa = pq.Xoa
            });
        }
        [HttpPost("delete-vaitro")]
        public async Task<IActionResult> deleteVaiTro(int idVT)
        {
            int _idUser = int.Parse(User.Identity.Name);
            var phanQuyen = getListPhanQuyen()
                .FirstOrDefault(x => x.Id == idVT);

            phanQuyen.Nvsua = int.Parse(User.Identity.Name);
            phanQuyen.NgaySua = DateTime.Now;
            phanQuyen.Active = false;

            _dACNPMContext.PhanQuyens.Update(phanQuyen);

            await _dACNPMContext.SaveChangesAsync();
            _memoryCache.Remove("vaiTros_" + _idUser);
            var vts = getListPhanQuyen();
            var pq = await GetPhanQuyenPQ();
            return Ok(new
            {
                statusCode = 200,
                message = "Thành công!",
                color = "bg-success",
                data = vts,
                xoa = pq.Xoa
            });
        }
        [HttpPost("search-vaitro")]
        public async Task<IActionResult> searchVaiTro(string key)
        {
            if (key == null)
            {
                return Ok(new
                {
                    data = getListPhanQuyen()
                });
            }
            var vts = getListPhanQuyen()
                .Where(x => x.IdvtNavigation.TenVaiTro.ToLower().Contains(key.ToLower()))
                .ToList();
            var pq = await GetPhanQuyenPQ();
            return Ok(new
            {
                data = vts,
                xoa = pq.Xoa
            });
        }
        [HttpPost("load-chucNang")]
        public async Task<IActionResult> loadChucNang(int idPQ)
        {
            ViewBag.ChucNangs = await _dACNPMContext.ChucNangs.ToListAsync();
            ViewBag.PhanQuyen = await _dACNPMContext.PhanQuyens.Include(x => x.PhanQuyenChucNangs)
                                .FirstOrDefaultAsync(x => x.Id == idPQ);
            return PartialView("loadTableChucNang");
        }
        [HttpPost("save-pqcn")]
        public async Task<IActionResult> savePqcn([FromBody] List<PhanQuyenChucNang> pqcns)
        {
            var tran = _dACNPMContext.Database.BeginTransaction();
            try
            {
                foreach (var pqcn in pqcns)
                {
                    //nếu PhanQuyenChucNangs không tồn tại
                    if (pqcn.Id == 0)
                    {
                        // nếu trong các chức năng có 1 chức năng sử dụng được
                        // thì tạo 1 PhanQuyenChucNangs mới
                        if (pqcn.Them.Value || pqcn.Sua.Value
                            || pqcn.Xoa.Value || pqcn.TimKiem.Value || pqcn.In.Value
                            || pqcn.Xuat.Value)
                        {

                            pqcn.NgayTao = DateTime.Now;
                            pqcn.Nvtao = int.Parse(User.Identity.Name);

                            await _dACNPMContext.PhanQuyenChucNangs.AddAsync(pqcn);
                            await _dACNPMContext.SaveChangesAsync();
                        }
                    }
                    //nếu PhanQuyenChucNangs có tồn tại
                    else
                    {
                        // lấy PhanQuyenChucNangs đó trong dtb
                        var pq = await _dACNPMContext.PhanQuyenChucNangs.FindAsync(pqcn.Id);
                        // nếu PhanQuyenChucNangs được lấy từ view không có chức năng nào thì xoá
                        if (!(pqcn.Them.Value || pqcn.Sua.Value
                            || pqcn.Xoa.Value || pqcn.TimKiem.Value || pqcn.In.Value
                            || pqcn.Xuat.Value))
                        {
                            _dACNPMContext.PhanQuyenChucNangs.Remove(pq);
                        }
                        // còn nếu có thì áp dụng thay đổi
                        else
                        {
                            pq.NgaySua = DateTime.Now;
                            pq.Nvsua = int.Parse(User.Identity.Name);
                            pq.Them = pqcn.Them;
                            pq.Sua = pqcn.Sua;
                            pq.Xoa = pqcn.Xoa;
                            pq.TimKiem = pqcn.TimKiem;
                            pq.In = pqcn.In;
                            pq.Xuat = pqcn.Xuat;
                            _dACNPMContext.PhanQuyenChucNangs.Update(pq);

                        }
                    }
                    await _dACNPMContext.SaveChangesAsync();
                }
                tran.Commit();
                return Ok(new
                {
                    statusCode = 200,
                    message = "Lưu thành công!",
                    color = "bg-success"
                });
            }
            catch (Exception e)
            {
                tran.Rollback();
                return Ok(new
                {
                    statusCode = 500,
                    message = "Lưu thất bại!",
                    color = "bg-danger"
                });
            }

        }
        List<PhanQuyen> getListPhanQuyen()
        {
            int _idUser = int.Parse(User.Identity.Name);
            return _memoryCache.GetOrCreate("vaiTros_" + _idUser, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                int idCn = int.Parse(User.FindFirstValue("IdCn"));
                return _dACNPMContext.PhanQuyens
                                    .Include(x => x.IdvtNavigation)
                                    .Where(x => x.Idcn == idCn && x.Active == true)
                                    .ToList();
            });
        }
        async Task<PhanQuyenChucNang> GetPhanQuyenPQ()
        {
            int idPq = int.Parse(User.FindFirstValue("IdPq"));

            return await _dACNPMContext.PhanQuyenChucNangs
                .Include(x => x.IdchucNangNavigation)
                .FirstOrDefaultAsync(x => x.Idpq == idPq
                && x.IdchucNangNavigation.MaChucNang.Equals("HT_PhanQuyen"));
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
