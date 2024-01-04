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
    [Authorize(Roles = "DM_BaoHanh")]
    [Route("DanhMuc/[controller]")]
    public class BaoHanhController : Controller
    {
        private DACNPMContext _dACNPMContext;
        private ICompositeViewEngine _viewEngine;
        private readonly ILogger<BaoHanhController> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private List<NhanVien> _nvs;
        private readonly IMemoryCache _memoryCache;
        public BaoHanhController(ILogger<BaoHanhController> logger,
            ICompositeViewEngine viewEngine, IMemoryCache memoryCache, IWebHostEnvironment hostingEnvironment)
        {
            _dACNPMContext = new DACNPMContext();
            _logger = logger;
            _viewEngine = viewEngine;
            _memoryCache = memoryCache;
            _hostingEnvironment = hostingEnvironment;
        }
        [HttpGet]
        public async Task<IActionResult> BaoHanh()
        {
            var Bhs = await getListBaoHanh();
            ViewBag.Bhs = Bhs.Where(x => x.Active == true)
            .Take(15)
            .ToList();

            var pqcn = await GetPhanQuyenBaoHanh();
            ViewBag.phanQuyenBH = pqcn;
            ViewData["title"] = pqcn.IdchucNangNavigation.TenChucNang;
            ViewBag.NhanViens = await getListNhanVien();
            return View();
        }
        [HttpPost("change-page")]
        public async Task<string> changePage(int page, bool active)
        {
            List<BaoHanh> BaoHanhs;
            if (active)
            {
                BaoHanhs = getListBaoHanh().Result.AsParallel()
                .Where(x => x.Active == active)
                .Skip(page * 15)
                .Take(15)
                .ToList();
                var r = await RenderBhs(BaoHanhs);
                return r;
            }
            else
            {
                BaoHanhs = getListBaoHanh().Result.AsParallel()
                .Skip(page * 15)
                .Take(15)
                .ToList();
                var r = await RenderBhs(BaoHanhs);
                return r;
            }
        }
        async Task<List<BaoHanh>> getListBaoHanh()
        {
            int _userId = int.Parse(User.Identity.Name);
            return await _memoryCache.GetOrCreateAsync("baohanhs_" + _userId, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return await _dACNPMContext.BaoHanhs.ToListAsync();
            });
        }
        [HttpPost("searchTableBaoHanh")]
        public async Task<string> searchNuocSX(string key, bool active)
        {
            List<BaoHanh> BHS;
            if (active)
            {
                BHS = getListBaoHanh().Result
                .Where(x => x.Active == active && (x.MaBh + " " + x.TenBh).ToLower().Contains(key.ToLower()))
                .ToList();
                var r = await RenderBhs(BHS);

                return r;
            }
            else
            {
                BHS = getListBaoHanh().Result
                .Where(x => (x.MaBh + " " + x.TenBh).ToLower().Contains(key.ToLower()))
                .ToList();
                var r = await RenderBhs(BHS);

                return r;
            }

        }
        [HttpGet("show-modal/{id}")]
        public IActionResult showEdit(int id)
        {
            var bh = getListBaoHanh().Result.Find(x => x.Id == id);

            PartialViewResult partialViewResult = PartialView("FormBaoHanh", bh == null ? new BaoHanh() : bh);
            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);
            return Ok(new
            {
                view = viewContent,
                title = bh == null ? "Thêm bảo hành" : "Chỉnh sửa bảo hành"
            });
        }
        [HttpPost("update-bh")]
        public async Task<IActionResult> updateHSX([FromBody] BaoHanh hh)
        {
            var tran = _dACNPMContext.Database.BeginTransaction();
            int _userId = int.Parse(User.Identity.Name);
            try
            {
                if (hh.Id == 0)
                {
                    hh.Active = true;
                    hh.Nvtao = int.Parse(User.Identity.Name);
                    hh.NgayTao = DateTime.Now;
                    await _dACNPMContext.BaoHanhs.AddAsync(hh);
                }
                else
                {
                    var bhDb = getListBaoHanh().Result.Find(x => x.Id == hh.Id);

                    bhDb.MaBh = hh.MaBh;
                    bhDb.TenBh = hh.TenBh;
                    bhDb.SoNgay = hh.SoNgay;

                    bhDb.Nvsua = int.Parse(User.Identity.Name);
                    bhDb.NgaySua = DateTime.Now;

                    _dACNPMContext.BaoHanhs.Update(bhDb);
                }
                await _dACNPMContext.SaveChangesAsync();
                await tran.CommitAsync();
                _memoryCache.Remove("baohanhs_" + _userId);
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
        [HttpPost("remove-bh")]
        public async Task<IActionResult> removeBh(int Id, bool active, int page, string key)
        {
            int _userId = int.Parse(User.Identity.Name);
            var bh = getListBaoHanh().Result.Find(x => x.Id == Id);
            bh.Active = !bh.Active;
            bh.Nvsua = int.Parse(User.Identity.Name);
            bh.NgaySua = DateTime.Now;
            _dACNPMContext.BaoHanhs.Update(bh);

            await _dACNPMContext.SaveChangesAsync();

            _memoryCache.Remove("baohanhs_" + _userId);

            List<BaoHanh> bhs;
            string r;
            if (key == null)
            {
                if (active)
                {
                    bhs = getListBaoHanh().Result
                    .Where(x => x.Active == active)
                    .Skip(page * 15)
                    .Take(15)
                    .ToList();
                    r = await RenderBhs(bhs);
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
                    bhs = getListBaoHanh().Result
                    .Skip(page * 15)
                    .Take(15)
                    .ToList();
                    r = await RenderBhs(bhs);
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
                    bhs = getListBaoHanh().Result
                    .Where(x => x.Active == active && (x.MaBh + " " + x.TenBh).ToLower().Contains(key.ToLower()))
                    .ToList();
                    r = await RenderBhs(bhs);
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
                    bhs = getListBaoHanh().Result
                    .Where(x => (x.MaBh + " " + x.TenBh).ToLower().Contains(key.ToLower()))
                    .ToList();
                    r = await RenderBhs(bhs);
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
        async Task<PhanQuyenChucNang> GetPhanQuyenBaoHanh()
        {
            int idPq = int.Parse(User.FindFirstValue("IdPq"));

            return await _dACNPMContext.PhanQuyenChucNangs
                .Include(x => x.IdchucNangNavigation)
                .FirstOrDefaultAsync(x => x.Idpq == idPq
                && x.IdchucNangNavigation.MaChucNang.Equals("DM_BaoHanh"));
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
        async Task<string> RenderBhs(List<BaoHanh> hangsxs)
        {
            ConcurrentBag<string> str = new ConcurrentBag<string>();
            _nvs = await getListNhanVien();
            var phanQuyenNSX = GetPhanQuyenBaoHanh().Result;

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
                                    ? $"<button onclick='deleteBh({hsx.Id})' class='text-danger'><i class='{t}'></i></button>"
                                    : "";
                    str.Add($"<tr>" +
                                    $"<td class=\"text-center\">{hsx.MaBh}</td>" +
                                    $"<td>{hsx.TenBh}</td>" +
                                    $"<td>{hsx.SoNgay}</td>" +
                                    $"<td>{getNhanVien(hsx.Nvtao).TenNv}</td>" +
                                    $"<td class=\"text-center\">{formatDay(hsx.NgayTao)}</td>" +
                                    $"<td>{getNhanVien(hsx.Nvsua).TenNv}</td>" +
                                    $"<td class=\"text-center\">{formatDay(hsx.NgaySua)}</td>" +
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
