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
    [Authorize(Roles = "DM_NhomNV")]
    [Route("DanhMuc/[controller]")]
    public class NhomNVController : Controller
    {
        private DACNPMContext _dACNPMContext;
        private ICompositeViewEngine _viewEngine;
        private readonly ILogger<DonViTinhController> _logger;
        private List<NhanVien> _nvs;
        private readonly IMemoryCache _memoryCache;
        public NhomNVController(ILogger<DonViTinhController> logger, ICompositeViewEngine viewEngine, IMemoryCache memoryCache)
        {
            _dACNPMContext = new DACNPMContext();
            _logger = logger;
            _viewEngine = viewEngine;
            _memoryCache = memoryCache;
        }
        [HttpGet]
        public async Task<IActionResult> NhomNhanVien()
        {
            ViewBag.NhomNVs = getListNhomNV().Result.AsParallel().Where(x => x.Active == true)
            .Take(15)
            .ToList();

            var pqcn = await GetPhanQuyenNhomNV();
            ViewBag.phanQuyenNhomNV = pqcn;
            ViewData["title"] = pqcn.IdchucNangNavigation.TenChucNang;
            ViewBag.NhanViens = await getListNhanVien();
            return View();
        }
        [HttpPost("change-page")]
        public async Task<string> changePage(int page, bool active)
        {
            List<NhomNhanVien> nnvs;
            if (active)
            {
                nnvs = getListNhomNV().Result.AsParallel()
                .Where(x => x.Active == active)
                .Skip(page * 15)
                .Take(15)
                .ToList();
                var r = await RenderDvts(nnvs);
                return r;
            }
            else
            {
                nnvs = getListNhomNV().Result.AsParallel()
                .Skip(page * 15)
                .Take(15)
                .ToList();
                var r = await RenderDvts(nnvs);
                return r;
            }
        }
        [HttpPost("searchTableNhomNV")]
        public async Task<string> searchNhomNV(string key, bool active)
        {
            List<NhomNhanVien> nnvs;
            if (active)
            {
                nnvs = getListNhomNV().Result.AsParallel()
                .Where(x => x.Active == active && (x.MaNnv + " " + x.TenNnv).ToLower().Contains(key.ToLower()))
                .ToList();
                var r = await RenderDvts(nnvs);

                return r;
            }
            else
            {
                nnvs = getListNhomNV().Result.AsParallel()
                .Where(x => (x.MaNnv + " " + x.TenNnv).ToLower().Contains(key.ToLower()))
                .ToList();
                var r = await RenderDvts(nnvs);

                return r;
            }

        }
        [HttpGet("show-modal/{id}")]
        public IActionResult showEdit(int id)
        {
            var nhomNV = getListNhomNV().Result.AsParallel().FirstOrDefault(x => x.Id == id);

            PartialViewResult partialViewResult = PartialView("FormNhomNV", nhomNV == null ? new NhomNhanVien() : nhomNV);
            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);
            return Ok(new
            {
                view = viewContent,
                title = nhomNV == null ? "Thêm nhóm nhân viên" : "Chỉnh sửa nhóm nhân viên"
            });
        }
        [HttpPost("update-nhomnv")]
        public async Task<IActionResult> updateNhomNV([FromBody] NhomNhanVien nnv)
        {
            var tran = _dACNPMContext.Database.BeginTransaction();
            int _userId = int.Parse(User.Identity.Name);
            try
            {
                if (nnv.Id == 0)
                {
                    nnv.Active = true;
                    nnv.Nvtao = int.Parse(User.Identity.Name);
                    nnv.NgayTao = DateTime.Now;
                    nnv.Idcn = int.Parse(User.FindFirstValue("IdCn"));
                    await _dACNPMContext.NhomNhanViens.AddAsync(nnv);
                }
                else
                {
                    var dvtdb = getListNhomNV().Result.AsParallel().FirstOrDefault(x => x.Id == nnv.Id);

                    dvtdb.MaNnv = nnv.MaNnv;
                    dvtdb.TenNnv = nnv.TenNnv;

                    dvtdb.Nvsua = int.Parse(User.Identity.Name);
                    dvtdb.NgaySua = DateTime.Now;

                    _dACNPMContext.NhomNhanViens.Update(dvtdb);
                }
                await _dACNPMContext.SaveChangesAsync();
                await tran.CommitAsync();
                _memoryCache.Remove("NhomNV_" + _userId);
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
        [HttpPost("remove")]
        public async Task<IActionResult> remove(int Id, bool active, int page, string key)
        {
            int _userId = int.Parse(User.Identity.Name);
            var itemDB = getListNhomNV().Result.AsParallel().FirstOrDefault(x => x.Id == Id);
            itemDB.Active = !itemDB.Active;
            itemDB.Nvsua = int.Parse(User.Identity.Name);
            itemDB.NgaySua = DateTime.Now;
            _dACNPMContext.NhomNhanViens.Update(itemDB);

            await _dACNPMContext.SaveChangesAsync();

            _memoryCache.Remove("NhomNV_" + _userId);

            List<NhomNhanVien> nnvs;
            string r;
            if (key == null)
            {
                if (active)
                {
                    nnvs = getListNhomNV().Result.AsParallel()
                    .Where(x => x.Active == active)
                    .Skip(page * 15)
                    .Take(15)
                    .ToList();
                    r = await RenderDvts(nnvs);
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
                    nnvs = getListNhomNV().Result.AsParallel()
                    .Skip(page * 15)
                    .Take(15)
                    .ToList();
                    r = await RenderDvts(nnvs);
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
                    nnvs = getListNhomNV().Result.AsParallel()
                    .Where(x => x.Active == active && (x.MaNnv + " " + x.TenNnv).ToLower().Contains(key.ToLower()))
                    .ToList();
                    r = await RenderDvts(nnvs);
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
                    nnvs = getListNhomNV().Result.AsParallel()
                    .Where(x => (x.MaNnv + " " + x.TenNnv).ToLower().Contains(key.ToLower()))
                    .ToList();
                    r = await RenderDvts(nnvs);
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
        async Task<string> RenderDvts(List<NhomNhanVien> nhomNvs)
        {
            ConcurrentBag<string> str = new ConcurrentBag<string>();
            _nvs = await getListNhanVien();
            var phanQuyenNhomHH = GetPhanQuyenNhomNV().Result;

            string can = "lni lni-trash-can";
            string re = "lni lni-spinner-arrow";

            await Task.Run(() =>
                Parallel.ForEach(nhomNvs, nnv =>
                {
                    string t = nnv.Active.Value ? can : re;
                    string btnSua = phanQuyenNhomHH.Sua.Value
                                    ? $"<button onclick='showModalEdit({nnv.Id})' class='text-primary'  data-bs-toggle='modal' data-bs-target='#staticBackdrop'><i class='lni lni-pencil'></i></button>"
                                    : "";
                    string btnXoa = phanQuyenNhomHH.Xoa.Value
                                    ? $"<button onclick='deleteNhomNV({nnv.Id})' class='text-danger'><i class='{t}'></i></button>"
                                    : "";
                    str.Add($"<tr>" +
                                    $"<td>{nnv.MaNnv}</td>" +
                                    $"<td>{nnv.TenNnv}</td>" +
                                    $"<td>{getNhanVien(nnv.Nvtao).TenNv}</td>" +
                                    $"<td>{formatDay(nnv.NgayTao)}</td>" +
                                    $"<td>{getNhanVien(nnv.Nvsua).TenNv}</td>" +
                                    $"<td>{formatDay(nnv.NgaySua)}</td>" +
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
        async Task<PhanQuyenChucNang> GetPhanQuyenNhomNV()
        {
            int idPq = int.Parse(User.FindFirstValue("IdPq"));

            return await _dACNPMContext.PhanQuyenChucNangs
                .Include(x => x.IdchucNangNavigation)
                .FirstOrDefaultAsync(x => x.Idpq == idPq
                && x.IdchucNangNavigation.MaChucNang.Equals("DM_NhomNV"));
        }
        async Task<List<NhomNhanVien>> getListNhomNV()
        {
            int _userId = int.Parse(User.Identity.Name);
            return await _memoryCache.GetOrCreateAsync("NhomNV_" + _userId, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return await _dACNPMContext.NhomNhanViens.ToListAsync();
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
