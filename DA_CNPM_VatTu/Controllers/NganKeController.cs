using DA_CNPM_VatTu.Models.Entities;
using DA_CNPM_VatTu.Models.MapData;
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
    [Authorize(Roles = "DM_NganKe")]
    [Route("DanhMuc/[controller]")]
    public class NganKeController : Controller
    {
        private DACNPMContext _dACNPMContext;
        private ICompositeViewEngine _viewEngine;
        private readonly ILogger<DonViTinhController> _logger;
        private List<NhanVien> _nvs;
        private readonly IMemoryCache _memoryCache;

        public NganKeController(ILogger<DonViTinhController> logger, ICompositeViewEngine viewEngine, IMemoryCache memoryCache)
        {
            _dACNPMContext = new DACNPMContext();
            _logger = logger;
            _viewEngine = viewEngine;
            _memoryCache = memoryCache;
        }
        [HttpGet]
        public async Task<IActionResult> NganKe()
        {
            ViewBag.NganKes = getListNganKe().Result.Where(x => x.Active == true)
            .Take(15)
            .ToList();

            var pqcn = await GetPhanQuyenNuocSX();
            ViewBag.phanQuyenNganKe = pqcn;
            ViewData["title"] = pqcn.IdchucNangNavigation.TenChucNang;
            ViewBag.NhanViens = await getListNhanVien();
            return View();
        }

        [HttpPost("change-page")]
        public async Task<string> changePage(int page, bool active)
        {
            List<NganKe> vts;
            if (active)
            {
                vts = getListNganKe().Result
                .Where(x => x.Active == active)
                .Skip(page * 15)
                .Take(15)
                .ToList();
                var r = await RenderDvts(vts);
                return r;
            }
            else
            {
                vts = getListNganKe().Result
                .Skip(page * 15)
                .Take(15)
                .ToList();
                var r = await RenderDvts(vts);
                return r;
            }
        }

        [HttpPost("searchTableNganKe")]
        public async Task<string> searchNganKe(string key, bool active)
        {
            List<NganKe> vts;
            if (active)
            {
                vts = getListNganKe().Result
                .Where(x => x.Active == active && x.TenNganKe.ToLower().Contains(key.ToLower()))
                .ToList();
                var r = await RenderDvts(vts);

                return r;
            }
            else
            {
                vts = getListNganKe().Result
                .Where(x => x.TenNganKe.ToLower().Contains(key.ToLower()))
                .ToList();
                var r = await RenderDvts(vts);

                return r;
            }

        }

        [HttpGet("show-modal/{id}")]
        public IActionResult showEdit(int id)
        {
            var vt = getListNganKe().Result.FirstOrDefault(x => x.Id == id);

            PartialViewResult partialViewResult = PartialView("FormNganKe", vt == null ? new NganKe() : vt);
            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);
            return Ok(new
            {
                view = viewContent,
                title = vt == null ? "Thêm vai trò" : "Chỉnh sửa vai trò"
            });
        }
        [HttpPost("update-nganke")]
        public async Task<IActionResult> updateNganKe([FromBody] NganKe vt)
        {
            var tran = _dACNPMContext.Database.BeginTransaction();
            int _userId = int.Parse(User.Identity.Name);
            int cn = int.Parse(User.FindFirstValue("IdCn"));
            try
            {
                if (vt.Id == 0)
                {
                    vt.Active = true;
                    vt.Nvtao = int.Parse(User.Identity.Name);
                    vt.NgayTao = DateTime.Now;
                    vt.Idcn = cn;
                    await _dACNPMContext.NganKes.AddAsync(vt);
                }
                else
                {
                    var dvtdb = getListNganKe().Result.FirstOrDefault(x => x.Id == vt.Id);

                    dvtdb.TenNganKe = vt.TenNganKe;

                    dvtdb.Nvsua = int.Parse(User.Identity.Name);
                    dvtdb.NgaySua = DateTime.Now;

                    _dACNPMContext.NganKes.Update(dvtdb);
                }
                await _dACNPMContext.SaveChangesAsync();
                await tran.CommitAsync();
                _memoryCache.Remove("NganKes_" + _userId);
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
        [HttpPost("remove-nganke")]
        public async Task<IActionResult> removeVT(int Id, bool active, int page, string key)
        {
            int _userId = int.Parse(User.Identity.Name);
            var dvtdb = getListNganKe().Result.FirstOrDefault(x => x.Id == Id);
            dvtdb.Active = !dvtdb.Active;
            dvtdb.Nvsua = int.Parse(User.Identity.Name);
            dvtdb.NgaySua = DateTime.Now;
            _dACNPMContext.NganKes.Update(dvtdb);

            await _dACNPMContext.SaveChangesAsync();

            _memoryCache.Remove("NganKes_" + _userId);

            List<NganKe> vts;
            string r;
            if (key == null)
            {
                if (active)
                {
                    vts = getListNganKe().Result
                    .Where(x => x.Active == active)
                    .Skip(page * 15)
                    .Take(15)
                    .ToList();
                    r = await RenderDvts(vts);
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
                    vts = getListNganKe().Result
                    .Skip(page * 15)
                    .Take(15)
                    .ToList();
                    r = await RenderDvts(vts);
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
                    vts = getListNganKe().Result
                    .Where(x => x.Active == active && x.TenNganKe.ToLower().Contains(key.ToLower()))
                    .ToList();
                    r = await RenderDvts(vts);
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
                    vts = getListNganKe().Result
                    .Where(x => x.TenNganKe.ToLower().Contains(key.ToLower()))
                    .ToList();
                    r = await RenderDvts(vts);
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
        async Task<string> RenderDvts(List<NganKe> vts)
        {
            ConcurrentBag<string> str = new ConcurrentBag<string>();
            _nvs = await getListNhanVien();
            var phanQuyenNSX = GetPhanQuyenNuocSX().Result;

            string can = "lni lni-trash-can";
            string re = "lni lni-spinner-arrow";

            await Task.Run(() =>
                Parallel.ForEach(vts, vt =>
                {
                    string t = vt.Active.Value ? can : re;
                    string btnSua = phanQuyenNSX.Sua.Value
                                    ? $"<button onclick='showModalEdit({vt.Id})' class='text-primary'  data-bs-toggle='modal' data-bs-target='#staticBackdrop'><i class='lni lni-pencil'></i></button>"
                                    : "";
                    string btnXoa = phanQuyenNSX.Xoa.Value
                                    ? $"<button onclick='deleteVT({vt.Id})' class='text-danger'><i class='{t}'></i></button>"
                                    : "";
                    str.Add($"<tr>" +
                                    $"<td>{vt.TenNganKe}</td>" +
                                    $"<td>{getNhanVien(vt.Nvtao).TenNv}</td>" +
                                    $"<td class=\"text-center\">{formatDay(vt.NgayTao)}</td>" +
                                    $"<td>{getNhanVien(vt.Nvsua).TenNv}</td>" +
                                    $"<td class=\"text-center\">{formatDay(vt.NgaySua)}</td>" +
                                    $"<td>" +
                                        $"<div class='action justify-content-center'>" +
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
                && x.IdchucNangNavigation.MaChucNang.Equals("DM_NganKe"));
        }
        async Task<List<NganKe>> getListNganKe()
        {
            int cn = int.Parse(User.FindFirstValue("IdCn"));
            int _userId = int.Parse(User.Identity.Name);
            return await _memoryCache.GetOrCreateAsync("NganKes_" + _userId, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return await _dACNPMContext.NganKes.Where(x=>x.Idcn == cn).ToListAsync();
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
