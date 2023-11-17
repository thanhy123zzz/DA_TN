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
    [Authorize(Roles = "DM_DonViTinh")]
    [Route("DanhMuc/[controller]")]
    public class DonViTinhController : Controller
    {
        private DACNPMContext _dACNPMContext;
        private ICompositeViewEngine _viewEngine;
        private readonly ILogger<DonViTinhController> _logger;
        private List<NhanVien> _nvs;
        private readonly IMemoryCache _memoryCache;
        public DonViTinhController(ILogger<DonViTinhController> logger, ICompositeViewEngine viewEngine, IMemoryCache memoryCache)
        {
            _dACNPMContext = new DACNPMContext();
            _logger = logger;
            _viewEngine = viewEngine;
            _memoryCache = memoryCache;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.Dvts = getListDVT().Result.Where(x => x.Active == true)
            .Take(10)
            .ToList();
            var pqcn = await GetPhanQuyenDVT();
            ViewBag.phanQuyenDVT = pqcn;
            ViewData["title"] = pqcn.IdchucNangNavigation.TenChucNang;
            return View();
        }
        [HttpPost("searchTableDVT")]
        public async Task<string> searchDVT(string key, bool active)
        {
            List<DonViTinh> donViTinhs;
            if (active)
            {
                donViTinhs = getListDVT().Result.AsParallel()
                .Where(x => x.Active == active && (x.MaDvt + " " + x.TenDvt).ToLower().Contains(key.ToLower()))

                .ToList();
                var r = await RenderDvts(donViTinhs);

                return r;
            }
            else
            {
                donViTinhs = getListDVT().Result.AsParallel()
                .Where(x => (x.MaDvt + " " + x.TenDvt).ToLower().Contains(key.ToLower()))
                .ToList();
                var r = await RenderDvts(donViTinhs);

                return r;
            }

        }
        [HttpPost("change-page")]
        public async Task<string> changePage(int page, bool active)
        {
            List<DonViTinh> donViTinhs;
            if (active)
            {
                donViTinhs = getListDVT().Result
                .Where(x => x.Active == active)
                .Skip(page * 10)
                .Take(10)
                .ToList();
                var r = await RenderDvts(donViTinhs);
                return r;
            }
            else
            {
                donViTinhs = getListDVT().Result
                .Skip(page * 10)
                .Take(10)
                .ToList();
                var r = await RenderDvts(donViTinhs);
                return r;
            }
        }

        [HttpGet("show-modal/{id}")]
        public IActionResult showEdit(int id)
        {
            var dvt = getListDVT().Result.Find(x => x.Id == id);

            PartialViewResult partialViewResult = PartialView("FormDonViTinh", dvt == null ? new DonViTinh() : dvt);
            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);
            return Ok(new
            {
                view = viewContent,
                title = dvt == null ? "Thêm đơn vị tính" : "Chỉnh sửa đơn vị tính"
            });
        }
        [HttpPost("update-dvt")]
        public async Task<IActionResult> updateDVT([FromBody] DonViTinh dvt)
        {
            var tran = _dACNPMContext.Database.BeginTransaction();

            try
            {
                if (dvt.Id == 0)
                {
                    dvt.Active = true;
                    dvt.Nvtao = int.Parse(User.Identity.Name);
                    dvt.NgayTao = DateTime.Now;
                    dvt.Idcn = int.Parse(User.FindFirstValue("IdCn"));
                    await _dACNPMContext.DonViTinhs.AddAsync(dvt);
                }
                else
                {
                    var dvtdb = getListDVT().Result.Find(x => x.Id == dvt.Id);

                    dvtdb.MaDvt = dvt.MaDvt;
                    dvtdb.TenDvt = dvt.TenDvt;

                    dvtdb.Nvsua = int.Parse(User.Identity.Name);
                    dvtdb.NgaySua = DateTime.Now;

                    _dACNPMContext.DonViTinhs.Update(dvtdb);
                }
                await _dACNPMContext.SaveChangesAsync();
                await tran.CommitAsync();
                int idcn = int.Parse(User.FindFirstValue("IdCn"));
                _memoryCache.Remove("Dvts_" + idcn);
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

        [HttpPost("remove-dvt")]
        public async Task<IActionResult> removeDVT(int Id, bool active, int page, string key)
        {
            var dvtdb = getListDVT().Result.Find(x => x.Id == Id);
            dvtdb.Active = !dvtdb.Active;
            dvtdb.Nvsua = int.Parse(User.Identity.Name);
            dvtdb.NgaySua = DateTime.Now;
            _dACNPMContext.DonViTinhs.Update(dvtdb);

            await _dACNPMContext.SaveChangesAsync();
            int idcn = int.Parse(User.FindFirstValue("IdCn"));
            _memoryCache.Remove("Dvts_" + idcn);

            List<DonViTinh> donViTinhs;
            string r;
            if (key == null)
            {
                if (active)
                {
                    donViTinhs = getListDVT().Result
                    .Where(x => x.Active == active)
                    .Skip(page * 10)
                    .Take(10)
                    .ToList();
                    r = await RenderDvts(donViTinhs);
                    return Ok(new
                    {
                        statusCode = 200,
                        message = "Thành công!",
                        color = "bg-success",
                        donViTinhs = r
                    });
                }
                else
                {
                    donViTinhs = getListDVT().Result
                    .Skip(page * 10)
                    .Take(10)
                    .ToList();
                    r = await RenderDvts(donViTinhs);
                    return Ok(new
                    {
                        statusCode = 200,
                        message = "Thành công!",
                        color = "bg-success",
                        donViTinhs = r
                    });
                }
            }
            else
            {
                if (active)
                {
                    donViTinhs = getListDVT().Result
                    .Where(x => x.Active == active && (x.MaDvt + " " + x.TenDvt).ToLower().Contains(key.ToLower()))
                    .ToList();
                    r = await RenderDvts(donViTinhs);
                    return Ok(new
                    {
                        statusCode = 200,
                        message = "Thành công!",
                        color = "bg-success",
                        donViTinhs = r
                    });
                }
                else
                {
                    donViTinhs = getListDVT().Result
                    .Where(x => (x.MaDvt + " " + x.TenDvt).ToLower().Contains(key.ToLower()))
                    .ToList();
                    r = await RenderDvts(donViTinhs);
                    return Ok(new
                    {
                        statusCode = 200,
                        message = "Thành công!",
                        color = "bg-success",
                        donViTinhs = r
                    });
                }
            }
        }
        async Task<string> RenderDvts(List<DonViTinh> dvts)
        {
            ConcurrentBag<string> str = new ConcurrentBag<string>();
            _nvs = _dACNPMContext.NhanViens.ToList();
            var phanQuyenDVT = GetPhanQuyenDVT().Result;

            string can = "lni lni-trash-can";
            string re = "lni lni-spinner-arrow";

            await Task.Run(() =>
                Parallel.ForEach(dvts, dvt =>
                {
                    string t = dvt.Active.Value ? can : re;
                    string btnSua = phanQuyenDVT.Sua.Value
                                    ? $"<button onclick='showModalEdit({dvt.Id})' class='text-primary'  data-bs-toggle='modal' data-bs-target='#staticBackdrop'><i class='lni lni-pencil'></i></button>"
                                    : "";
                    string btnXoa = phanQuyenDVT.Xoa.Value
                                    ? $"<button onclick='deleteDVT({dvt.Id})' class='text-danger'><i class='{t}'></i></button>"
                                    : "";
                    str.Add($"<tr>" +
                                    $"<td class='text-center'>{dvt.MaDvt}</td>" +
                                    $"<td>{dvt.TenDvt}</td>" +
                                    $"<td>{getNhanVien(@dvt.Nvtao).TenNv}</td>" +
                                    $"<td class='text-center'>{formatDay(@dvt.NgayTao)}</td>" +
                                    $"<td>{getNhanVien(@dvt.Nvsua).TenNv}</td>" +
                                    $"<td class='text-center'>{formatDay(@dvt.NgaySua)}</td>" +
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
            _nvs.Clear();
            return result;
        }
        NhanVien getNhanVien(int? id)
        {
            var nv = _nvs.Find(x => x.Id == id);
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

        async Task<List<DonViTinh>> getListDVT()
        {
            int idcn = int.Parse(User.FindFirstValue("IdCn"));
            return await _memoryCache.GetOrCreateAsync("Dvts_" + idcn, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return await _dACNPMContext.DonViTinhs.ToListAsync();
            });
        }

        async Task<PhanQuyenChucNang> GetPhanQuyenDVT()
        {
            int idPq = int.Parse(User.FindFirstValue("IdPq"));

            return await _dACNPMContext.PhanQuyenChucNangs
                .Include(x => x.IdchucNangNavigation)
                .FirstOrDefaultAsync(x => x.Idpq == idPq
                && x.IdchucNangNavigation.MaChucNang.Equals("DM_DonViTinh"));
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
