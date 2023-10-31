using DA_CNPM_VatTu.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace DA_CNPM_VatTu.Controllers
{
    [Authorize(Roles = "DM_NuocSX")]
    [Route("DanhMuc/[controller]")]
    public class NuocSXController : Controller
    {
        private DACNPMContext _dACNPMContext;
        private ICompositeViewEngine _viewEngine;
        private readonly ILogger<DonViTinhController> _logger;
        private List<NhanVien> _nvs;
        private readonly IMemoryCache _memoryCache;

        public NuocSXController(ILogger<DonViTinhController> logger, ICompositeViewEngine viewEngine, IMemoryCache memoryCache)
        {
            _dACNPMContext = new DACNPMContext();
            _logger = logger;
            _viewEngine = viewEngine;
            _memoryCache = memoryCache;
        }
        [HttpGet]
        public async Task<IActionResult> NuocSX()
        {
            ViewBag.NuocSXs = getListNuocSX().Where(x => x.Active == true)
            .Take(10)
            .ToList();

            var pqcn = await GetPhanQuyenNuocSX();
            ViewBag.phanQuyenNuocSX = pqcn;
            ViewData["title"] = pqcn.IdchucNangNavigation.TenChucNang;
            ViewBag.NhanViens = await getListNhanVien();
            return View();
        }
        [HttpPost("change-page")]
        public async Task<string> changePage(int page, bool active)
        {
            List<NuocSanXuat> donViTinhs;
            if (active)
            {
                donViTinhs = getListNuocSX()
                .Where(x => x.Active == active)
                .Skip(page * 10)
                .Take(10)
                .ToList();
                var r = await RenderDvts(donViTinhs);
                return r;
            }
            else
            {
                donViTinhs = getListNuocSX()
                .Skip(page * 10)
                .Take(10)
                .ToList();
                var r = await RenderDvts(donViTinhs);
                return r;
            }
        }

        [HttpPost("searchTableNuocSX")]
        public async Task<string> searchNuocSX(string key, bool active)
        {
            List<NuocSanXuat> nsxs;
            if (active)
            {
                nsxs = getListNuocSX()
                .Where(x => x.Active == active && (x.MaNsx + " " + x.TenNsx).ToLower().Contains(key.ToLower()))
                .ToList();
                var r = await RenderDvts(nsxs);

                return r;
            }
            else
            {
                nsxs = getListNuocSX()
                .Where(x => (x.MaNsx + " " + x.TenNsx).ToLower().Contains(key.ToLower()))
                .ToList();
                var r = await RenderDvts(nsxs);

                return r;
            }

        }

        [HttpGet("show-modal/{id}")]
        public IActionResult showEdit(int id)
        {
            var dvt = getListNuocSX().Find(x => x.Id == id);

            PartialViewResult partialViewResult = PartialView("FormNuocSX", dvt == null ? new NuocSanXuat() : dvt);
            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);
            return Ok(new
            {
                view = viewContent,
                title = dvt == null ? "Thêm nước sản xuất" : "Chỉnh sửa nước sản xuất"
            });
        }

        [HttpPost("update-nuocsx")]
        public async Task<IActionResult> updateNuocSX([FromBody] NuocSanXuat nsx)
        {
            var tran = _dACNPMContext.Database.BeginTransaction();
            int _userId = int.Parse(User.Identity.Name);
            try
            {
                if (nsx.Id == 0)
                {
                    nsx.Active = true;
                    nsx.Nvtao = int.Parse(User.Identity.Name);
                    nsx.NgayTao = DateTime.Now;
                    nsx.Idcn = int.Parse(User.FindFirstValue("IdCn"));
                    await _dACNPMContext.NuocSanXuats.AddAsync(nsx);
                }
                else
                {
                    var dvtdb = getListNuocSX().Find(x => x.Id == nsx.Id);

                    dvtdb.MaNsx = nsx.MaNsx;
                    dvtdb.TenNsx = nsx.TenNsx;

                    dvtdb.Nvsua = int.Parse(User.Identity.Name);
                    dvtdb.NgaySua = DateTime.Now;

                    _dACNPMContext.NuocSanXuats.Update(dvtdb);
                }
                await _dACNPMContext.SaveChangesAsync();
                await tran.CommitAsync();
                _memoryCache.Remove("NuocSx_" + _userId);
                return Ok(new
                {
                    statusCode = 200,
                    message = "Thành công!",
                    color = "bg-success"
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
        [HttpPost("remove-nsx")]
        public async Task<IActionResult> removeNSX(int Id, bool active, int page, string key)
        {
            int _userId = int.Parse(User.Identity.Name);
            var dvtdb = getListNuocSX().Find(x => x.Id == Id);
            dvtdb.Active = !dvtdb.Active;
            dvtdb.Nvsua = int.Parse(User.Identity.Name);
            dvtdb.NgaySua = DateTime.Now;
            _dACNPMContext.NuocSanXuats.Update(dvtdb);

            await _dACNPMContext.SaveChangesAsync();

            _memoryCache.Remove("NuocSx_" + _userId);

            List<NuocSanXuat> nsxs;
            string r;
            if (key == null)
            {
                if (active)
                {
                    nsxs = getListNuocSX()
                    .Where(x => x.Active == active)
                    .Skip(page * 10)
                    .Take(10)
                    .ToList();
                    r = await RenderDvts(nsxs);
                    return Ok(new
                    {
                        statusCode = 200,
                        message = "Thành công!",
                        color = "bg-success",
                        nsxs = r
                    });
                }
                else
                {
                    nsxs = getListNuocSX()
                    .Skip(page * 10)
                    .Take(10)
                    .ToList();
                    r = await RenderDvts(nsxs);
                    return Ok(new
                    {
                        statusCode = 200,
                        message = "Thành công!",
                        color = "bg-success",
                        nsxs = r
                    });
                }
            }
            else
            {
                if (active)
                {
                    nsxs = getListNuocSX()
                    .Where(x => x.Active == active && (x.MaNsx + " " + x.TenNsx).ToLower().Contains(key.ToLower()))
                    .ToList();
                    r = await RenderDvts(nsxs);
                    return Ok(new
                    {
                        statusCode = 200,
                        message = "Thành công!",
                        color = "bg-success",
                        nsxs = r
                    });
                }
                else
                {
                    nsxs = getListNuocSX()
                    .Where(x => (x.MaNsx + " " + x.TenNsx).ToLower().Contains(key.ToLower()))
                    .ToList();
                    r = await RenderDvts(nsxs);
                    return Ok(new
                    {
                        statusCode = 200,
                        message = "Thành công!",
                        color = "bg-success",
                        nsxs = r
                    });
                }
            }
        }
        async Task<string> RenderDvts(List<NuocSanXuat> dvts)
        {
            ConcurrentBag<string> str = new ConcurrentBag<string>();
            _nvs = await getListNhanVien();
            var phanQuyenNSX = GetPhanQuyenNuocSX().Result;

            string can = "lni lni-trash-can";
            string re = "lni lni-spinner-arrow";

            await Task.Run(() =>
                Parallel.ForEach(dvts, nsx =>
                {
                    string t = nsx.Active.Value ? can : re;
                    string btnSua = phanQuyenNSX.Sua.Value
                                    ? $"<button onclick='showModalEdit({nsx.Id})' class='text-primary'  data-bs-toggle='modal' data-bs-target='#staticBackdrop'><i class='lni lni-pencil'></i></button>"
                                    : "";
                    string btnXoa = phanQuyenNSX.Xoa.Value
                                    ? $"<button onclick='deleteNSX({nsx.Id})' class='text-danger'><i class='{t}'></i></button>"
                                    : "";
                    str.Add($"<tr>" +
                                    $"<td>{nsx.MaNsx}</td>" +
                                    $"<td>{nsx.TenNsx}</td>" +
                                    $"<td>{getNhanVien(nsx.Nvtao).TenNv}</td>" +
                                    $"<td>{formatDay(nsx.NgayTao)}</td>" +
                                    $"<td>{getNhanVien(nsx.Nvsua).TenNv}</td>" +
                                    $"<td>{formatDay(nsx.NgaySua)}</td>" +
                                    $"<td>" +
                                        $"<div class='action justify-content-end'>" +
                                            $"{btnSua}" +
                                            $"{btnXoa}" +
                                        $"</div>" +
                                    $"</td>" +
                              $"</tr>");
                }
            ));
            string result = string.Join("", str);
            return result;
        }
        async Task<PhanQuyenChucNang> GetPhanQuyenNuocSX()
        {
            int idPq = int.Parse(User.FindFirstValue("IdPq"));

            return await _dACNPMContext.PhanQuyenChucNangs
                .Include(x => x.IdchucNangNavigation)
                .FirstOrDefaultAsync(x => x.Idpq == idPq
                && x.IdchucNangNavigation.MaChucNang.Equals("DM_NuocSX"));
        }
        List<NuocSanXuat> getListNuocSX()
        {
            int _userId = int.Parse(User.Identity.Name);
            return _memoryCache.GetOrCreate("NuocSx_" + _userId, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return _dACNPMContext.NuocSanXuats.ToList();
            });
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
        NhanVien getNhanVien(int? id)
        {
            var nv = _nvs.FirstOrDefault(x => x.Id == id);
            if (nv != null) return nv;
            else return new NhanVien();
        }
        string formatDay(DateTime? date)
        {
            if (date != null)
            {
                return date.Value.ToString("dd-MM-yyyy HH:mm");
            }
            else
            {
                return "";
            }
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
