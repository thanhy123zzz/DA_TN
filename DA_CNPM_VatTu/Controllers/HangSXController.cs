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
    [Authorize(Roles = "DM_HangSX")]
    [Route("DanhMuc/[controller]")]
    public class HangSXController : Controller
    {
        private DACNPMContext _dACNPMContext;
        private ICompositeViewEngine _viewEngine;
        private readonly ILogger<DonViTinhController> _logger;
        private List<NhanVien> _nvs;
        private readonly IMemoryCache _memoryCache;
        public HangSXController(ILogger<DonViTinhController> logger, ICompositeViewEngine viewEngine, IMemoryCache memoryCache)
        {
            _dACNPMContext = new DACNPMContext();
            _logger = logger;
            _viewEngine = viewEngine;
            _memoryCache = memoryCache;
        }
        [HttpGet]
        public async Task<IActionResult> HangSX()
        {
            ViewBag.HangSXs = getListHangSX().Where(x => x.Active == true)
            .Take(15)
            .ToList();

            var pqcn = await GetPhanQuyenHangSX();
            ViewBag.phanQuyenHangSX = pqcn;
            ViewData["title"] = pqcn.IdchucNangNavigation.TenChucNang;
            ViewBag.NhanViens = await getListNhanVien();
            return View();
        }

        [HttpPost("change-page")]
        public async Task<string> changePage(int page, bool active)
        {
            List<HangSanXuat> hangSXs;
            if (active)
            {
                hangSXs = getListHangSX()
                .Where(x => x.Active == active)
                .Skip(page * 15)
                .Take(15)
                .ToList();
                var r = await RenderDvts(hangSXs);
                return r;
            }
            else
            {
                hangSXs = getListHangSX()
                .Skip(page * 15)
                .Take(15)
                .ToList();
                var r = await RenderDvts(hangSXs);
                return r;
            }
        }
        [HttpPost("searchTableHangSX")]
        public async Task<string> searchHangSX(string key, bool active)
        {
            List<HangSanXuat> hsxs;
            if (active)
            {
                hsxs = getListHangSX()
                .Where(x => x.Active == active && (x.MaHsx + " " + x.TenHsx).ToLower().Contains(key.ToLower()))
                .ToList();
                var r = await RenderDvts(hsxs);

                return r;
            }
            else
            {
                hsxs = getListHangSX()
                .Where(x => (x.MaHsx + " " + x.TenHsx).ToLower().Contains(key.ToLower()))
                .ToList();
                var r = await RenderDvts(hsxs);

                return r;
            }

        }
        [HttpGet("show-modal/{id}")]
        public IActionResult showEdit(int id)
        {
            var hsx = getListHangSX().Find(x => x.Id == id);

            PartialViewResult partialViewResult = PartialView("FormHangSX", hsx == null ? new HangSanXuat() : hsx);
            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);
            return Ok(new
            {
                view = viewContent,
                title = hsx == null ? "Thêm hãng sản xuất" : "Chỉnh sửa hãng sản xuất"
            });
        }
        [HttpPost("update-hangsx")]
        public async Task<IActionResult> updateHSX([FromBody] HangSanXuat hsx)
        {
            var tran = _dACNPMContext.Database.BeginTransaction();
            int _userId = int.Parse(User.Identity.Name);
            try
            {
                if (hsx.Id == 0)
                {
                    hsx.Active = true;
                    hsx.Nvtao = int.Parse(User.Identity.Name);
                    hsx.NgayTao = DateTime.Now;
                    hsx.Idcn = int.Parse(User.FindFirstValue("IdCn"));
                    await _dACNPMContext.HangSanXuats.AddAsync(hsx);
                }
                else
                {
                    var dvtdb = getListHangSX().Find(x => x.Id == hsx.Id);

                    dvtdb.MaHsx = hsx.MaHsx;
                    dvtdb.TenHsx = hsx.TenHsx;

                    dvtdb.Nvsua = int.Parse(User.Identity.Name);
                    dvtdb.NgaySua = DateTime.Now;

                    _dACNPMContext.HangSanXuats.Update(dvtdb);
                }
                await _dACNPMContext.SaveChangesAsync();
                await tran.CommitAsync();
                _memoryCache.Remove("HangSx_" + _userId);
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

        [HttpPost("remove-hsx")]
        public async Task<IActionResult> removeNSX(int Id, bool active, int page, string key)
        {
            int _userId = int.Parse(User.Identity.Name);
            var hsxdb = getListHangSX().Find(x => x.Id == Id);
            hsxdb.Active = !hsxdb.Active;
            hsxdb.Nvsua = int.Parse(User.Identity.Name);
            hsxdb.NgaySua = DateTime.Now;
            _dACNPMContext.HangSanXuats.Update(hsxdb);

            await _dACNPMContext.SaveChangesAsync();

            _memoryCache.Remove("HangSx_" + _userId);

            List<HangSanXuat> hsxs;
            string r;
            if (key == null)
            {
                if (active)
                {
                    hsxs = getListHangSX()
                    .Where(x => x.Active == active)
                    .Skip(page * 15)
                    .Take(15)
                    .ToList();
                    r = await RenderDvts(hsxs);
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
                    hsxs = getListHangSX()
                    .Skip(page * 15)
                    .Take(15)
                    .ToList();
                    r = await RenderDvts(hsxs);
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
                    hsxs = getListHangSX()
                    .Where(x => x.Active == active && (x.MaHsx + " " + x.TenHsx).ToLower().Contains(key.ToLower()))
                    .ToList();
                    r = await RenderDvts(hsxs);
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
                    hsxs = getListHangSX()
                    .Where(x => (x.MaHsx + " " + x.TenHsx).ToLower().Contains(key.ToLower()))
                    .ToList();
                    r = await RenderDvts(hsxs);
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
        async Task<string> RenderDvts(List<HangSanXuat> hangsxs)
        {
            ConcurrentBag<string> str = new ConcurrentBag<string>();
            _nvs = await getListNhanVien();
            var phanQuyenNSX = GetPhanQuyenHangSX().Result;

            string can = "lni lni-trash-can";
            string re = "lni lni-spinner-arrow";

            await Task.Run(() =>
                Parallel.ForEach(hangsxs, hsx =>
                {
                    string t = hsx.Active.Value ? can : re;
                    string btnSua = phanQuyenNSX.Sua.Value
                                    ? $"<button onclick='showModalEdit({hsx.Id})' class='text-primary'  data-bs-toggle='modal' data-bs-target='#staticBackdrop'><i class='lni lni-pencil'></i></button>"
                                    : "";
                    string btnXoa = phanQuyenNSX.Xoa.Value
                                    ? $"<button onclick='deleteNSX({hsx.Id})' class='text-danger'><i class='{t}'></i></button>"
                                    : "";
                    str.Add($"<tr>" +
                                    $"<td class=\"text-center\">{hsx.MaHsx}</td>" +
                                    $"<td>{hsx.TenHsx}</td>" +
                                    $"<td>{getNhanVien(hsx.Nvtao).TenNv}</td>" +
                                    $"<td class=\"text-center\">{formatDay(hsx.NgayTao)}</td>" +
                                    $"<td>{getNhanVien(hsx.Nvsua).TenNv}</td>" +
                                    $"<td class=\"text-center\">{formatDay(hsx.NgaySua)}</td>" +
                                    $"<td class=\"last-td-column\">" +
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
        async Task<PhanQuyenChucNang> GetPhanQuyenHangSX()
        {
            int idPq = int.Parse(User.FindFirstValue("IdPq"));

            return await _dACNPMContext.PhanQuyenChucNangs
                .Include(x => x.IdchucNangNavigation)
                .FirstOrDefaultAsync(x => x.Idpq == idPq
                && x.IdchucNangNavigation.MaChucNang.Equals("DM_HangSX"));
        }
        List<HangSanXuat> getListHangSX()
        {
            int _userId = int.Parse(User.Identity.Name);
            return _memoryCache.GetOrCreate("HangSx_" + _userId, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return _dACNPMContext.HangSanXuats.ToList();
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
