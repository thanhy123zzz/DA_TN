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
            ViewBag.NhaCCs = await _dACNPMContext.NhaCungCaps.Where(x => x.Active == true)
                .OrderBy(x=>x.TenNcc)
            .Take(15)
            .ToListAsync();

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
                nccs = await _dACNPMContext.NhaCungCaps
                .Where(x => x.Active == active)
                .OrderBy(x => x.TenNcc)
                .Skip(page * 15)
                .Take(15)
                .ToListAsync();
                var r = await RenderNccs(nccs);
                return r;
            }
            else
            {
                nccs = await _dACNPMContext.NhaCungCaps
                    .OrderBy(x => x.TenNcc)
                .Skip(page * 15)
                .Take(15)
                .ToListAsync();
                var r = await RenderNccs(nccs);
                return r;
            }
        }
        [HttpPost("searchTableNhaCC")]
        public async Task<string> searchNhaCC(string key, bool active)
        {
            List<NhaCungCap> nsxs;
            key = key;
            if (active)
            {
                nsxs = await _dACNPMContext.NhaCungCaps
                .Where(x => ((x.MaNcc != null && x.MaNcc.ToLower().Contains(key)) ||
                                               (x.DiaChi != null && x.DiaChi.ToLower().Contains(key)) ||
                                               (x.Sdt != null && x.Sdt.ToLower().Contains(key)) ||
                                               (x.Email != null && x.Email.ToLower().Contains(key)) ||
                                               (x.GhiChu != null && x.GhiChu.ToLower().Contains(key)) ||
                                               (x.TenNcc != null && x.TenNcc.ToLower().Contains(key))) &&
                                                x.Active == true)
                .OrderBy(x => x.TenNcc)
                .ToListAsync();
                var r = await RenderNccs(nsxs);

                return r;
            }
            else
            {
                nsxs = await _dACNPMContext.NhaCungCaps
                .Where(x => ((x.MaNcc != null && x.MaNcc.ToLower().Contains(key)) ||
                                               (x.DiaChi != null && x.DiaChi.ToLower().Contains(key)) ||
                                               (x.Sdt != null && x.Sdt.ToLower().Contains(key)) ||
                                               (x.Email != null && x.Email.ToLower().Contains(key)) ||
                                               (x.GhiChu != null && x.GhiChu.ToLower().Contains(key)) ||
                                               (x.TenNcc != null && x.TenNcc.ToLower().Contains(key))) &&
                                                x.Active == true)
                .OrderBy(x => x.TenNcc)
                .ToListAsync();
                var r = await RenderNccs(nsxs);

                return r;
            }

        }
        [HttpGet("show-modal/{id}")]
        public async Task<IActionResult> showEdit(int id)
        {
            var nhaCC = await _dACNPMContext.NhaCungCaps.FindAsync(id);
            if (nhaCC == null)
            {
                nhaCC = new NhaCungCap();
                nhaCC.MaNcc = await getSoPhieu();
            }
            async Task<string> getSoPhieu()
            {
                var context = new DACNPMContext();
                //ID chi nhánh
                int cn = int.Parse(User.FindFirstValue("IdCn"));

                DateTime d = DateTime.Now;
                string ngayThangNam = d.ToString("yyMMdd");

                QuyDinhMa qd = await context.QuyDinhMas.FirstOrDefaultAsync(x => x.TiepDauNgu == "C");
                string SoPhieu = cn + "_" + qd.TiepDauNgu + ngayThangNam;
                var list = await context.SoThuTus.FirstOrDefaultAsync(x => x.Ngay.Date == DateTime.Now.Date && x.Loai.Equals("NhaCungCap"));
                int stt;
                if (list == null)
                {
                    SoThuTu sttt = new SoThuTu();
                    sttt.Ngay = DateTime.ParseExact(DateTime.Now.ToString("dd-MM-yyyy"), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    sttt.Stt = 1;
                    sttt.Loai = "NhaCungCap";
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
            PartialViewResult partialViewResult = PartialView("FormNhaCC", nhaCC);
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
                    var nhacc = await _dACNPMContext.NhaCungCaps.FindAsync(ncc.Id);

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
            var itemDB = await _dACNPMContext.NhaCungCaps.FindAsync(Id);
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
                key = key.ToLower();
                if (active)
                {
                    nsxs = await _dACNPMContext.NhaCungCaps
                    .Where(x => x.Active == active)
                    .OrderBy(x => x.TenNcc)
                    .Skip(page * 15)
                    .Take(15)
                    .ToListAsync();
                    r = await RenderNccs(nsxs);
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
                    nsxs = await _dACNPMContext.NhaCungCaps
                        .OrderBy(x => x.TenNcc)
                    .Skip(page * 15)
                    .Take(15)
                    .ToListAsync();
                    r = await RenderNccs(nsxs);
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
                    nsxs = await _dACNPMContext.NhaCungCaps
                    .Where(x => ((x.MaNcc != null && x.MaNcc.ToLower().Contains(key)) ||
                                               (x.DiaChi != null && x.DiaChi.ToLower().Contains(key)) ||
                                               (x.Sdt != null && x.Sdt.ToLower().Contains(key)) ||
                                               (x.Email != null && x.Email.ToLower().Contains(key)) ||
                                               (x.GhiChu != null && x.GhiChu.ToLower().Contains(key)) ||
                                               (x.TenNcc != null && x.TenNcc.ToLower().Contains(key))) &&
                                                x.Active == true)
                    .OrderBy(x => x.TenNcc)
                    .ToListAsync();
                    r = await RenderNccs(nsxs);
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
                    nsxs = await _dACNPMContext.NhaCungCaps
                    .Where(x => ((x.MaNcc != null && x.MaNcc.ToLower().Contains(key)) ||
                                               (x.DiaChi != null && x.DiaChi.ToLower().Contains(key)) ||
                                               (x.Sdt != null && x.Sdt.ToLower().Contains(key)) ||
                                               (x.Email != null && x.Email.ToLower().Contains(key)) ||
                                               (x.GhiChu != null && x.GhiChu.ToLower().Contains(key)) ||
                                               (x.TenNcc != null && x.TenNcc.ToLower().Contains(key))) &&
                                                x.Active == true)
                    .OrderBy(x => x.TenNcc)
                    .ToListAsync();
                    r = await RenderNccs(nsxs);
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
        async Task<string> RenderNccs(List<NhaCungCap> nhaCCs)
        {
            ConcurrentBag<string> str = new ConcurrentBag<string>();
            _nvs = await getListNhanVien();
            var phanQuyenNhaCC = GetPhanQuyenNhaCC().Result;

            string can = "lni lni-trash-can";
            string re = "lni lni-spinner-arrow";

            nhaCCs.ForEach(ncc =>
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
                                $"<td class='last-td-column'>" +
                                    $"<div class='action justify-content-end'>" +
                                        $"{btnSua}" +
                                        $"{btnXoa}" +
                                    $"</div>" +
                                $"</td>" +
                          $"</tr>");
            });
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
