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
    [Authorize(Roles = "DM_NhomHH")]
    [Route("DanhMuc/[controller]")]
    public class NhomHHController : Controller
    {
        private DACNPMContext _dACNPMContext;
        private ICompositeViewEngine _viewEngine;
        private readonly ILogger<DonViTinhController> _logger;
        private List<NhanVien> _nvs;
        private readonly IMemoryCache _memoryCache;
        public NhomHHController(ILogger<DonViTinhController> logger, ICompositeViewEngine viewEngine, IMemoryCache memoryCache)
        {
            _dACNPMContext = new DACNPMContext();
            _logger = logger;
            _viewEngine = viewEngine;
            _memoryCache = memoryCache;
        }
        [HttpGet]
        public async Task<IActionResult> NhomHangHoa()
        {
            ViewBag.NhomHHs = getListNhomHH().Where(x => x.Active == true)
            .Take(10)
            .ToList();

            var pqcn = await GetPhanQuyenNhomHH();
            ViewBag.phanQuyenNhomHH = pqcn;
            ViewData["title"] = pqcn.IdchucNangNavigation.TenChucNang;
            ViewBag.NhanViens = await getListNhanVien();
            return View();
        }
        [HttpPost("change-page")]
        public async Task<string> changePage(int page, bool active)
        {
            List<NhomHangHoa> donViTinhs;
            if (active)
            {
                donViTinhs = getListNhomHH()
                .Where(x => x.Active == active)
                .Skip(page * 10)
                .Take(10)
                .ToList();
                var r = await RenderDvts(donViTinhs);
                return r;
            }
            else
            {
                donViTinhs = getListNhomHH()
                .Skip(page * 10)
                .Take(10)
                .ToList();
                var r = await RenderDvts(donViTinhs);
                return r;
            }
        }
        [HttpPost("searchTableNhomHH")]
        public async Task<string> searchNhomHH(string key, bool active)
        {
            List<NhomHangHoa> nsxs;
            if (active)
            {
                nsxs = getListNhomHH()
                .Where(x => x.Active == active && (x.MaNhh + " " + x.TenNhh).ToLower().Contains(key.ToLower()))
                .ToList();
                var r = await RenderDvts(nsxs);

                return r;
            }
            else
            {
                nsxs = getListNhomHH()
                .Where(x => (x.MaNhh + " " + x.TenNhh).ToLower().Contains(key.ToLower()))
                .ToList();
                var r = await RenderDvts(nsxs);

                return r;
            }

        }
        [HttpGet("show-modal/{id}")]
        public IActionResult showEdit(int id)
        {
            var nhomHH = getListNhomHH().Find(x => x.Id == id);

            PartialViewResult partialViewResult = PartialView("FormNhomHH", nhomHH == null ? new NhomHangHoa() : nhomHH);
            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);
            return Ok(new
            {
                view = viewContent,
                title = nhomHH == null ? "Thêm nhóm hàng hoá" : "Chỉnh sửa nhóm hàng hoá"
            });
        }
        [HttpPost("update-nhomhh")]
        public async Task<IActionResult> updateNhomHH([FromBody] NhomHangHoa nhh)
        {
            var tran = _dACNPMContext.Database.BeginTransaction();
            int _userId = int.Parse(User.Identity.Name);
            try
            {
                if (nhh.Id == 0)
                {
                    nhh.Active = true;
                    nhh.Nvtao = int.Parse(User.Identity.Name);
                    nhh.NgayTao = DateTime.Now;
                    nhh.Idcn = int.Parse(User.FindFirstValue("IdCn"));
                    await _dACNPMContext.NhomHangHoas.AddAsync(nhh);
                }
                else
                {
                    var dvtdb = getListNhomHH().Find(x => x.Id == nhh.Id);

                    dvtdb.MaNhh = nhh.MaNhh;
                    dvtdb.TenNhh = nhh.TenNhh;

                    dvtdb.Nvsua = int.Parse(User.Identity.Name);
                    dvtdb.NgaySua = DateTime.Now;

                    _dACNPMContext.NhomHangHoas.Update(dvtdb);
                }
                await _dACNPMContext.SaveChangesAsync();
                await tran.CommitAsync();
                _memoryCache.Remove("NhomHH_" + _userId);
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
            var itemDB = getListNhomHH().Find(x => x.Id == Id);
            itemDB.Active = !itemDB.Active;
            itemDB.Nvsua = int.Parse(User.Identity.Name);
            itemDB.NgaySua = DateTime.Now;
            _dACNPMContext.NhomHangHoas.Update(itemDB);

            await _dACNPMContext.SaveChangesAsync();

            _memoryCache.Remove("NhomHH_" + _userId);

            List<NhomHangHoa> nsxs;
            string r;
            if (key == null)
            {
                if (active)
                {
                    nsxs = getListNhomHH()
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
                    nsxs = getListNhomHH()
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
                    nsxs = getListNhomHH()
                    .Where(x => x.Active == active && (x.MaNhh + " " + x.TenNhh).ToLower().Contains(key.ToLower()))
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
                    nsxs = getListNhomHH()
                    .Where(x => (x.MaNhh + " " + x.TenNhh).ToLower().Contains(key.ToLower()))
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
        async Task<string> RenderDvts(List<NhomHangHoa> nhomHhs)
        {
            ConcurrentBag<string> str = new ConcurrentBag<string>();
            _nvs = await getListNhanVien();
            var phanQuyenNhomHH = GetPhanQuyenNhomHH().Result;

            string can = "lni lni-trash-can";
            string re = "lni lni-spinner-arrow";

            await Task.Run(() =>
                Parallel.ForEach(nhomHhs, nsx =>
                {
                    string t = nsx.Active.Value ? can : re;
                    string btnSua = phanQuyenNhomHH.Sua.Value
                                    ? $"<button onclick='showModalEdit({nsx.Id})' class='text-primary'  data-bs-toggle='modal' data-bs-target='#staticBackdrop'><i class='lni lni-pencil'></i></button>"
                                    : "";
                    string btnXoa = phanQuyenNhomHH.Xoa.Value
                                    ? $"<button onclick='deleteNhomHH({nsx.Id})' class='text-danger'><i class='{t}'></i></button>"
                                    : "";
                    str.Add($"<tr>" +
                                    $"<td>{nsx.MaNhh}</td>" +
                                    $"<td>{nsx.TenNhh}</td>" +
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
        async Task<PhanQuyenChucNang> GetPhanQuyenNhomHH()
        {
            int idPq = int.Parse(User.FindFirstValue("IdPq"));

            return await _dACNPMContext.PhanQuyenChucNangs
                .Include(x => x.IdchucNangNavigation)
                .FirstOrDefaultAsync(x => x.Idpq == idPq
                && x.IdchucNangNavigation.MaChucNang.Equals("DM_NhomHH"));
        }
        List<NhomHangHoa> getListNhomHH()
        {
            int _userId = int.Parse(User.Identity.Name);
            return _memoryCache.GetOrCreate("NhomHH_" + _userId, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return _dACNPMContext.NhomHangHoas.ToList();
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
