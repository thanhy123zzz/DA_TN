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
    [Authorize(Roles = "DM_NhaCC")]
    [Route("DanhMuc/[controller]")]
    public class NhaCCController : Controller
    {
        private DACNPMContext _dACNPMContext;
        private ICompositeViewEngine _viewEngine;
        private readonly ILogger<DonViTinhController> _logger;
        private List<NhanVien> _nvs;
        private readonly IMemoryCache _memoryCache;
        public NhaCCController(ILogger<DonViTinhController> logger, ICompositeViewEngine viewEngine, IMemoryCache memoryCache)
        {
            _dACNPMContext = new DACNPMContext();
            _logger = logger;
            _viewEngine = viewEngine;
            _memoryCache = memoryCache;
        }
        [HttpGet]
        public async Task<IActionResult> NhaCC()
        {
            ViewBag.NhaCCs = getListNhaCC().Result.Where(x => x.Active == true)
            .Take(10)
            .ToList();

            var pqcn = await GetPhanQuyenNhaCC();
            ViewBag.phanQuyenNhaCC = pqcn;
            ViewData["title"] = pqcn.IdchucNangNavigation.TenChucNang;
            ViewBag.NhanViens = await getListNhanVien();
            return View();
        }
        [HttpPost("change-page")]
        public async Task<string> changePage(int page, bool active)
        {
            List<NhaCungCap> nccs;
            if (active)
            {
                nccs = getListNhaCC().Result.AsParallel()
                .Where(x => x.Active == active)
                .Skip(page * 10)
                .Take(10)
                .ToList();
                var r = await RenderDvts(nccs);
                return r;
            }
            else
            {
                nccs = getListNhaCC().Result.AsParallel()
                .Skip(page * 10)
                .Take(10)
                .ToList();
                var r = await RenderDvts(nccs);
                return r;
            }
        }
        [HttpPost("searchTableNhaCC")]
        public async Task<string> searchNhaCC(string key, bool active)
        {
            List<NhaCungCap> nsxs;
            if (active)
            {
                nsxs = getListNhaCC().Result.AsParallel()
                .Where(x => x.Active == active && (
                x.MaNcc.ToLower().Contains(key.ToLower()) ||
                x.TenNcc?.ToLower().Contains(key.ToLower()) == true ||
                x.DiaChi?.ToLower().Contains(key.ToLower()) == true ||
                x.Sdt?.ToLower().Contains(key.ToLower()) == true ||
                x.Email?.ToLower().Contains(key.ToLower()) == true ||
                x.GhiChu?.ToLower().Contains(key.ToLower()) == true))
                .ToList();
                var r = await RenderDvts(nsxs);

                return r;
            }
            else
            {
                nsxs = getListNhaCC().Result.AsParallel()
                .Where(x =>
                x.MaNcc.ToLower().Contains(key.ToLower()) ||
                x.TenNcc?.ToLower().Contains(key.ToLower()) == true ||
                x.DiaChi?.ToLower().Contains(key.ToLower()) == true ||
                x.Sdt?.ToLower().Contains(key.ToLower()) == true ||
                x.Email?.ToLower().Contains(key.ToLower()) == true ||
                x.GhiChu?.ToLower().Contains(key.ToLower()) == true
                )
                .ToList();
                var r = await RenderDvts(nsxs);

                return r;
            }

        }
        [HttpGet("show-modal/{id}")]
        public IActionResult showEdit(int id)
        {
            var nhaCC = getListNhaCC().Result.Find(x => x.Id == id);

            PartialViewResult partialViewResult = PartialView("FormNhaCC", nhaCC == null ? new NhaCungCap() : nhaCC);
            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);
            return Ok(new
            {
                view = viewContent,
                title = nhaCC == null ? "Thêm nhà cung cấp" : "Chỉnh sửa nhà cung cấp"
            });
        }
        [HttpPost("update-nhacc")]
        public async Task<IActionResult> updateNhaCC([FromBody] NhaCungCap ncc)
        {
            var tran = _dACNPMContext.Database.BeginTransaction();
            int _userId = int.Parse(User.Identity.Name);
            try
            {
                if (ncc.Id == 0)
                {
                    ncc.Active = true;
                    ncc.Nvtao = int.Parse(User.Identity.Name);
                    ncc.NgayTao = DateTime.Now;
                    ncc.Idcn = int.Parse(User.FindFirstValue("IdCn"));
                    await _dACNPMContext.NhaCungCaps.AddAsync(ncc);
                }
                else
                {
                    var nhacc = getListNhaCC().Result.Find(x => x.Id == ncc.Id);

                    nhacc.MaNcc = ncc.MaNcc;
                    nhacc.TenNcc = ncc.TenNcc;
                    nhacc.DiaChi = ncc.DiaChi;
                    nhacc.Email = ncc.Email;
                    nhacc.GhiChu = ncc.GhiChu;
                    nhacc.Sdt = ncc.Sdt;

                    nhacc.Nvsua = int.Parse(User.Identity.Name);
                    nhacc.NgaySua = DateTime.Now;

                    _dACNPMContext.NhaCungCaps.Update(nhacc);
                }
                await _dACNPMContext.SaveChangesAsync();
                await tran.CommitAsync();
                _memoryCache.Remove("NhaCC_" + _userId);
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
            var itemDB = getListNhaCC().Result.Find(x => x.Id == Id);
            itemDB.Active = !itemDB.Active;
            itemDB.Nvsua = int.Parse(User.Identity.Name);
            itemDB.NgaySua = DateTime.Now;
            _dACNPMContext.NhaCungCaps.Update(itemDB);

            await _dACNPMContext.SaveChangesAsync();

            _memoryCache.Remove("NhaCC_" + _userId);

            List<NhaCungCap> nsxs;
            string r;
            if (key == null)
            {
                if (active)
                {
                    nsxs = getListNhaCC().Result
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
                    nsxs = getListNhaCC().Result
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
                    nsxs = getListNhaCC().Result
                    .Where(x => x.Active == active && (
                x.MaNcc.ToLower().Contains(key.ToLower()) ||
                x.TenNcc?.ToLower().Contains(key.ToLower()) == true ||
                x.DiaChi?.ToLower().Contains(key.ToLower()) == true ||
                x.Sdt?.ToLower().Contains(key.ToLower()) == true ||
                x.Email?.ToLower().Contains(key.ToLower()) == true ||
                x.GhiChu?.ToLower().Contains(key.ToLower()) == true))
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
                    nsxs = getListNhaCC().Result
                    .Where(x =>
                x.MaNcc.ToLower().Contains(key.ToLower()) ||
                x.TenNcc?.ToLower().Contains(key.ToLower()) == true ||
                x.DiaChi?.ToLower().Contains(key.ToLower()) == true ||
                x.Sdt?.ToLower().Contains(key.ToLower()) == true ||
                x.Email?.ToLower().Contains(key.ToLower()) == true ||
                x.GhiChu?.ToLower().Contains(key.ToLower()) == true
                    )
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
        async Task<string> RenderDvts(List<NhaCungCap> nhaCCs)
        {
            ConcurrentBag<string> str = new ConcurrentBag<string>();
            _nvs = await getListNhanVien();
            var phanQuyenNhaCC = GetPhanQuyenNhaCC().Result;

            string can = "lni lni-trash-can";
            string re = "lni lni-spinner-arrow";

            await Task.Run(() =>
                Parallel.ForEach(nhaCCs, ncc =>
                {
                    string t = ncc.Active.Value ? can : re;
                    string btnSua = phanQuyenNhaCC.Sua.Value
                                    ? $"<button onclick='showModalEdit({ncc.Id})' class='text-primary'  data-bs-toggle='modal' data-bs-target='#staticBackdrop'><i class='lni lni-pencil'></i></button>"
                                    : "";
                    string btnXoa = phanQuyenNhaCC.Xoa.Value
                                    ? $"<button onclick='deleteNhaCC({ncc.Id})' class='text-danger'><i class='{t}'></i></button>"
                                    : "";
                    str.Add($"<tr>" +
                                    $"<td class ='text-center'>{ncc.MaNcc}</td>" +
                                    $"<td>{ncc.TenNcc}</td>" +
                                    $"<td>{ncc.DiaChi}</td>" +
                                    $"<td>{ncc.Sdt}</td>" +
                                    $"<td>{ncc.Email}</td>" +
                                    $"<td>{ncc.GhiChu}</td>" +
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
        async Task<PhanQuyenChucNang> GetPhanQuyenNhaCC()
        {
            int idPq = int.Parse(User.FindFirstValue("IdPq"));

            return await _dACNPMContext.PhanQuyenChucNangs
                .Include(x => x.IdchucNangNavigation)
                .FirstOrDefaultAsync(x => x.Idpq == idPq
                && x.IdchucNangNavigation.MaChucNang.Equals("DM_NhaCC"));
        }
        async Task<List<NhaCungCap>> getListNhaCC()
        {
            int _userId = int.Parse(User.Identity.Name);
            return await _memoryCache.GetOrCreateAsync("NhaCC_" + _userId, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return await _dACNPMContext.NhaCungCaps.ToListAsync();
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
