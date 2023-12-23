using DA_CNPM_VatTu.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Claims;

namespace DA_CNPM_VatTu.Controllers
{
    [Authorize(Roles = "DM_KhachHang")]
    [Route("DanhMuc/[controller]")]
    public class KhachHangController : Controller
    {
        private DACNPMContext _dACNPMContext;
        private ICompositeViewEngine _viewEngine;
        private readonly ILogger<DonViTinhController> _logger;
        private List<NhanVien> _nvs;
        private readonly IMemoryCache _memoryCache;
        public KhachHangController(ILogger<DonViTinhController> logger, ICompositeViewEngine viewEngine, IMemoryCache memoryCache)
        {
            _dACNPMContext = new DACNPMContext();
            _logger = logger;
            _viewEngine = viewEngine;
            _memoryCache = memoryCache;
        }
        [HttpGet]
        public async Task<IActionResult> KhachHangAsync()
        {
            ViewBag.KHs = getListKH().Result.Where(x => x.Active == true)
            .Take(15)
            .ToList();

            var pqcn = await GetPhanQuyenKH();
            ViewBag.phanQuyenKH = pqcn;
            ViewData["title"] = pqcn.IdchucNangNavigation.TenChucNang;
            ViewBag.NhanViens = await getListNhanVien();
            return View();
        }
        [HttpPost("change-page")]
        public async Task<string> changePage(int page, bool active)
        {
            List<KhachHang> KHs;
            if (active)
            {
                KHs = getListKH().Result.AsParallel()
                .Where(x => x.Active == active)
                .Skip(page * 15)
                .Take(15)
                .ToList();
                var r = await RenderDvts(KHs);
                return r;
            }
            else
            {
                KHs = getListKH().Result.AsParallel()
                .Skip(page * 15)
                .Take(15)
                .ToList();
                var r = await RenderDvts(KHs);
                return r;
            }
        }
        [HttpPost("searchTableKH")]
        public async Task<string> searchTableKH(string key, bool active)
        {
            List<KhachHang> khs;
            if (active)
            {
                khs = getListKH().Result.AsParallel()
                .Where(x => x.Active == active && (
                x.MaKh.ToLower().Contains(key.ToLower()) ||
                x.TenKh.ToLower().Contains(key.ToLower()) ||
                x.DiaChi.ToLower().Contains(key.ToLower()) ||
                x.Sdt.ToLower().Contains(key.ToLower()) ||
                x.Email.ToLower().Contains(key.ToLower()) ||
                x.MaSoThue.ToLower().Contains(key.ToLower()) ||
                x.GhiChu.ToLower().Contains(key.ToLower())))
                .ToList();
                var r = await RenderDvts(khs);

                return r;
            }
            else
            {
                khs = getListKH().Result.AsParallel()
                .Where(x =>
                x.MaKh.ToLower().Contains(key.ToLower()) ||
                x.TenKh.ToLower().Contains(key.ToLower()) ||
                x.DiaChi.ToLower().Contains(key.ToLower()) ||
                x.Sdt.ToLower().Contains(key.ToLower()) ||
                x.Email.ToLower().Contains(key.ToLower()) ||
                x.MaSoThue.ToLower().Contains(key.ToLower()) ||
                x.GhiChu.ToLower().Contains(key.ToLower())
                )
                .ToList();
                var r = await RenderDvts(khs);

                return r;
            }

        }
        [HttpGet("show-modal/{id}")]
        public async Task<IActionResult> showEdit(int id)
        {
            var kh = getListKH().Result.Find(x => x.Id == id);
            if (kh == null)
            {
                kh = new KhachHang();
                kh.MaKh = await getSoPhieu();
            }
            async Task<string> getSoPhieu()
            {
                var context = new DACNPMContext();
                //ID chi nhánh
                int cn = int.Parse(User.FindFirstValue("IdCn"));

                DateTime d = DateTime.Now;
                string ngayThangNam = d.ToString("yyMMdd");

                QuyDinhMa qd = await context.QuyDinhMas.FirstOrDefaultAsync(x=>x.TiepDauNgu == "K");
                string SoPhieu = cn + "_" + qd.TiepDauNgu + ngayThangNam;
                var list = await context.SoThuTus.FirstOrDefaultAsync(x => x.Ngay.Date == DateTime.Now.Date && x.Loai.Equals("KhachHang"));
                int stt;
                if (list == null)
                {
                    SoThuTu sttt = new SoThuTu();
                    sttt.Ngay = DateTime.ParseExact(DateTime.Now.ToString("dd-MM-yyyy"), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    sttt.Stt = 1;
                    sttt.Loai = "KhachHang";
                    await context.SoThuTus.AddAsync(sttt);
                    context.SaveChanges();
                    stt = 1;
                }
                else
                {
                    stt = list.Stt + 1;
                    list.Stt++;
                    context.SaveChanges();
                }
                SoPhieu += stt;
                while (true)
                {
                    if (qd.DoDai == SoPhieu.Length) break;
                    SoPhieu = SoPhieu.Insert(SoPhieu.Length - stt.ToString().Length, "0");
                }
                
                return SoPhieu;
            }
            PartialViewResult partialViewResult = PartialView("FormKH", kh);
            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);
            return Ok(new
            {
                view = viewContent,
                title = kh == null ? "Thêm khách hàng" : "Chỉnh sửa khách hàng"
            });
        }
        [HttpPost("update-kh")]
        public async Task<IActionResult> updateKH([FromBody] KhachHang kh)
        {
            var tran = _dACNPMContext.Database.BeginTransaction();
            int _userId = int.Parse(User.Identity.Name);
            try
            {
                if (kh.Id == 0)
                {
                    kh.Active = true;
                    kh.Idnvsale = int.Parse(User.Identity.Name);
                    kh.NgayTao = DateTime.Now;
                    await _dACNPMContext.KhachHangs.AddAsync(kh);
                }
                else
                {
                    var nhacc = getListKH().Result.Find(x => x.Id == kh.Id);

                    nhacc.MaKh = kh.MaKh;
                    nhacc.TenKh = kh.TenKh;
                    nhacc.DiaChi = kh.DiaChi;
                    nhacc.Email = kh.Email;
                    nhacc.GhiChu = kh.GhiChu;
                    nhacc.Sdt = kh.Sdt;
                    nhacc.LoaiKh = kh.LoaiKh;
                    nhacc.MaSoThue = kh.MaSoThue;

                    nhacc.NgaySua = DateTime.Now;

                    _dACNPMContext.KhachHangs.Update(nhacc);
                }
                await _dACNPMContext.SaveChangesAsync();
                await tran.CommitAsync();
                _memoryCache.Remove("KhachHangs_" + _userId);
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
            var itemDB = getListKH().Result.Find(x => x.Id == Id);
            itemDB.Active = !itemDB.Active;
            itemDB.NgaySua = DateTime.Now;
            _dACNPMContext.KhachHangs.Update(itemDB);

            await _dACNPMContext.SaveChangesAsync();

            _memoryCache.Remove("KhachHangs_" + _userId);

            List<KhachHang> khs;
            string r;
            if (key == null)
            {
                if (active)
                {
                    khs = getListKH().Result.AsParallel()
                    .Where(x => x.Active == active)
                    .Skip(page * 15)
                    .Take(15)
                    .ToList();
                    r = await RenderDvts(khs);
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
                    khs = getListKH().Result.AsParallel()
                    .Skip(page * 15)
                    .Take(15)
                    .ToList();
                    r = await RenderDvts(khs);
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
                    khs = getListKH().Result.AsParallel()
                    .Where(x => x.Active == active && (
                    x.MaKh.ToLower().Contains(key.ToLower()) ||
                    x.TenKh.ToLower().Contains(key.ToLower()) ||
                    x.DiaChi.ToLower().Contains(key.ToLower()) ||
                    x.Sdt.ToLower().Contains(key.ToLower()) ||
                    x.Email.ToLower().Contains(key.ToLower()) ||
                    x.MaSoThue.ToLower().Contains(key.ToLower()) ||
                    x.GhiChu.ToLower().Contains(key.ToLower())))
                    .ToList();
                    r = await RenderDvts(khs);
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
                    khs = getListKH().Result.AsParallel()
                    .Where(x =>
                    x.MaKh.ToLower().Contains(key.ToLower()) ||
                    x.TenKh.ToLower().Contains(key.ToLower()) ||
                    x.DiaChi.ToLower().Contains(key.ToLower()) ||
                    x.Sdt.ToLower().Contains(key.ToLower()) ||
                    x.Email.ToLower().Contains(key.ToLower()) ||
                    x.MaSoThue.ToLower().Contains(key.ToLower()) ||
                    x.GhiChu.ToLower().Contains(key.ToLower())
                    )
                    .ToList();
                    r = await RenderDvts(khs);
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
        async Task<string> RenderDvts(List<KhachHang> Khs)
        {
            ConcurrentBag<string> str = new ConcurrentBag<string>();
            _nvs = await getListNhanVien();
            var phanQuyenKhachHang = GetPhanQuyenKH().Result;

            string can = "lni lni-trash-can";
            string re = "lni lni-spinner-arrow";

            await Task.Run(() =>
                Parallel.ForEach(Khs, kh =>
                {
                    string t = kh.Active.Value ? can : re;
                    string btnSua = phanQuyenKhachHang.Sua.Value
                                    ? $"<button onclick='showModalEdit({kh.Id})' class='text-primary'  data-bs-toggle='modal' data-bs-target='#staticBackdrop'><i class='lni lni-pencil'></i></button>"
                                    : "";
                    string btnXoa = phanQuyenKhachHang.Xoa.Value
                                    ? $"<button onclick='deleteKH({kh.Id})' class='text-danger'><i class='{t}'></i></button>"
                                    : "";
                    str.Add($"<tr>" +
                                    $"<td class ='text-center'>{kh.MaKh}</td>" +
                                    $"<td>{kh.TenKh}</td>" +
                                    $"<td>{kh.DiaChi}</td>" +
                                    $"<td>{kh.Sdt}</td>" +
                                    $"<td>{kh.Email}</td>" +
                                    $"<td>{kh.MaSoThue}</td>" +
                                    $"<td>{(kh.LoaiKh.Value ? "Sỉ" : "Lẻ")}</td>" +
                                    $"<td>{kh.GhiChu}</td>" +
                                    $"<td class='last-td-column'>" +
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
        async Task<PhanQuyenChucNang> GetPhanQuyenKH()
        {
            int idPq = int.Parse(User.FindFirstValue("IdPq"));

            return await _dACNPMContext.PhanQuyenChucNangs
                .Include(x => x.IdchucNangNavigation)
                .FirstOrDefaultAsync(x => x.Idpq == idPq
                && x.IdchucNangNavigation.MaChucNang.Equals("DM_KhachHang"));
        }
        async Task<List<KhachHang>> getListKH()
        {
            int _userId = int.Parse(User.Identity.Name);
            return await _memoryCache.GetOrCreate("KhachHangs_" + _userId, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return await _dACNPMContext.KhachHangs.ToListAsync();
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
