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
using System.Globalization;
using System.Security.Claims;

namespace DA_CNPM_VatTu.Controllers
{
    [Authorize(Roles = "DM_NhanVien")]
    [Route("DanhMuc/[controller]")]
    public class NhanVienController : Controller
    {
        private DACNPMContext _dACNPMContext;
        private ICompositeViewEngine _viewEngine;
        private readonly ILogger<DonViTinhController> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private List<NhanVien> _nvs;
        private readonly IMemoryCache _memoryCache;
        public NhanVienController(ILogger<DonViTinhController> logger,
            ICompositeViewEngine viewEngine, IMemoryCache memoryCache, IWebHostEnvironment hostingEnvironment)
        {
            _dACNPMContext = new DACNPMContext();
            _logger = logger;
            _viewEngine = viewEngine;
            _memoryCache = memoryCache;
            _hostingEnvironment = hostingEnvironment;
        }
        [HttpGet]
        public async Task<IActionResult> NhanVien()
        {
            var nvs = getListNhanVien().Result.AsParallel().Where(x => x.Active == true)
            .Take(10)
            .ToList();
            ViewBag.NhanVienTables = nvs;

            var pqcn = await GetPhanQuyenNhanVien();
            ViewBag.phanQuyenNhanVien = pqcn;
            ViewData["title"] = pqcn.IdchucNangNavigation.TenChucNang;
            ViewBag.NhanViens = await getListNhanVien();
            return View();
        }
        [HttpPost("searchTableNV")]
        public async Task<string> searchNV(string key, bool active)
        {
            List<NhanVien> nvs;
            if (active)
            {
                nvs = getListNhanVien().Result.AsParallel()
                .Where(x => x.Active == active && (
                x.MaNv.ToLower().Contains(key.ToLower()) ||
                x.IdnnvNavigation?.TenNnv.ToLower().Contains(key.ToLower()) == true ||
                x.TenNv.ToLower().Contains(key.ToLower()) ||
                x.Sdt.ToLower().Contains(key.ToLower()) ||
                x.Cccd.ToLower().Contains(key.ToLower())))
                .ToList();
                var r = await RenderNvs(nvs);

                return r;
            }
            else
            {
                nvs = getListNhanVien().Result.AsParallel()
                .Where(x =>
                x.MaNv.ToLower().Contains(key.ToLower()) ||
                x.IdnnvNavigation?.TenNnv.ToLower().Contains(key.ToLower()) == true ||
                x.TenNv.ToLower().Contains(key.ToLower()) ||
                x.Sdt.ToLower().Contains(key.ToLower()) ||
                x.Cccd.ToLower().Contains(key.ToLower()))
                .ToList();
                var r = await RenderNvs(nvs);

                return r;
            }

        }
        [HttpPost("change-page")]
        public async Task<string> changePage(int page, bool active)
        {
            List<NhanVien> nvs;
            if (active)
            {
                nvs = getListNhanVien().Result.AsParallel()
                .Where(x => x.Active == active)
                .Skip(page * 10)
                .Take(10)
                .ToList();
                var r = await RenderNvs(nvs);
                return r;
            }
            else
            {
                nvs = getListNhanVien().Result.AsParallel()
                .Skip(page * 10)
                .Take(10)
                .ToList();
                var r = await RenderNvs(nvs);
                return r;
            }
        }
        [HttpGet("show-modal/{id}")]
        public async Task<IActionResult> showEdit(int id)
        {
            var a = getListNhomNV();
            var NV = getListNhanVien().Result.FirstOrDefault(x => x.Id == id);

            PartialViewResult partialViewResult = PartialView("FormNV", NV == null ? new NhanVien() : NV);
            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);
            Task.WaitAll(a);
            return Ok(new
            {
                view = viewContent,
                title = NV == null ? "Thêm nhân viên" : "Chỉnh sửa nhân viên"
            });
        }
        [HttpPost("api/nnvs")]
        public async Task<IActionResult> optionNvs()
        {
            var nhhs = _dACNPMContext.NhomNhanViens
                .Where(x => x.Active == true);
            return Ok(await nhhs.Select(x => new
            {
                id = x.Id,
                ma = x.MaNnv,
                ten = x.TenNnv,
            }).ToListAsync());
        }
        [HttpPost("update-nv")]
        public async Task<IActionResult> updateNV(MapNhanVien mapNV)
        {
            var tran = _dACNPMContext.Database.BeginTransaction();
            try
            {
                NhanVien nv = mapNV.NhanVien;
                int _userId = int.Parse(User.Identity.Name);
                if (nv != null)
                {
                    string webRootPath = _hostingEnvironment.WebRootPath;
                    string imagesPath = Path.Combine(webRootPath, "assets", "images", "NhanVien");

                    if (mapNV.FormFile != null)
                    {
                        string fileName = nv.MaNv + Path.GetExtension(mapNV.FormFile.FileName);
                        string filePath = Path.Combine(imagesPath, fileName);

                        if (System.IO.File.Exists(filePath))
                        {
                            // Tệp tin đã tồn tại, thực hiện replace
                            string tempFilePath = Path.Combine(imagesPath, "temp_" + fileName);

                            // Copy nội dung của tệp tin mới vào tệp tin tạm thời
                            using (var stream = new FileStream(tempFilePath, FileMode.Create))
                            {
                                await mapNV.FormFile.CopyToAsync(stream);
                            }

                            // Thực hiện replace tệp tin cũ bằng tệp tin mới
                            System.IO.File.Replace(tempFilePath, filePath, null);
                        }
                        else
                        {
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await mapNV.FormFile.CopyToAsync(stream);
                            }
                        }

                        nv.Avatar = "/assets/images/NhanVien/" + fileName;
                    }

                    if (nv.Id == 0)
                    {
                        Account ac = mapNV.Account;
                        if (ac.TaiKhoan != null && ac.MatKhau != null)
                        {
                            _dACNPMContext.Accounts.Add(ac);
                            _dACNPMContext.SaveChanges();
                            nv.Idtk = ac.Id;
                        }

                        nv.Nvtao = _userId;
                        nv.NgayTao = DateTime.Now;
                        nv.Active = true;
                        try
                        {
                            nv.NgaySinh = DateTime.ParseExact(mapNV.NgaySinh, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                        }
                        catch (Exception e)
                        {

                        }
                        await _dACNPMContext.NhanViens.AddAsync(nv);
                    }
                    else
                    {
                        var nvDB = await _dACNPMContext.NhanViens
                            .Include(x => x.IdtkNavigation)
                            .FirstOrDefaultAsync(x => x.Id == nv.Id);
                        if (nvDB.IdtkNavigation == null)
                        {
                            if (mapNV.Account.TaiKhoan != null && mapNV.Account.MatKhau != null)
                            {
                                Account ac = new Account();
                                ac.TaiKhoan = mapNV.Account.TaiKhoan;
                                ac.MatKhau = mapNV.Account.MatKhau;
                                await _dACNPMContext.Accounts.AddAsync(ac);
                                nvDB.Idtk = ac.Id;
                            }
                        }
                        else
                        {
                            if (mapNV.Account.TaiKhoan != nvDB.IdtkNavigation.TaiKhoan || mapNV.Account.MatKhau != nvDB.IdtkNavigation.MatKhau)
                            {
                                Account ac = nvDB.IdtkNavigation;
                                ac.TaiKhoan = mapNV.Account.TaiKhoan;
                                ac.MatKhau = mapNV.Account.MatKhau;
                                _dACNPMContext.Accounts.Update(ac);

                            }
                        }

                        nvDB.MaNv = nv.MaNv;
                        nvDB.TenNv = nv.TenNv;
                        nvDB.Email = nv.Email;
                        nvDB.DiaChi = nv.DiaChi;
                        nvDB.QueQuan = nv.QueQuan;
                        nvDB.Sdt = nv.Sdt;
                        nvDB.Cccd = nv.Cccd;
                        nvDB.GioiTinh = nv.GioiTinh;
                        nvDB.NgaySinh = DateTime.ParseExact(mapNV.NgaySinh, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                        nvDB.Idnnv = nv.Idnnv;
                        nvDB.Avatar = nv.Avatar == null ? nvDB.Avatar : nv.Avatar;

                        nvDB.Nvsua = _userId;
                        nvDB.NgaySua = DateTime.Now;

                        _dACNPMContext.NhanViens.Update(nvDB);
                    }
                }
                _dACNPMContext.SaveChanges();
                tran.Commit();

                _memoryCache.Remove("NhanViens_" + _userId);
                return Ok(new
                {
                    statusCode = 200,
                    message = "Thành công!",
                    color = "bg-success"
                });

            }
            catch (Exception e)
            {
                tran.Rollback();
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
            var itemDB = await _dACNPMContext.NhanViens.FindAsync(Id);
            itemDB.Active = !itemDB.Active;
            itemDB.Nvsua = int.Parse(User.Identity.Name);
            itemDB.NgaySua = DateTime.Now;
            _dACNPMContext.NhanViens.Update(itemDB);

            await _dACNPMContext.SaveChangesAsync();

            _memoryCache.Remove("NhanViens_" + _userId);

            List<NhanVien> nvs;
            string r;
            if (key == null)
            {
                if (active)
                {
                    nvs = getListNhanVien().Result.AsParallel()
                    .Where(x => x.Active == active)
                    .Skip(page * 10)
                    .Take(10)
                    .ToList();
                    r = await RenderNvs(nvs);
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
                    nvs = getListNhanVien().Result.AsParallel()
                    .Skip(page * 10)
                    .Take(10)
                    .ToList();
                    r = await RenderNvs(nvs);
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
                    nvs = getListNhanVien().Result.AsParallel()
                    .Where(x => x.Active == active && (
                    x.MaNv.ToLower().Contains(key.ToLower()) ||
                    x.IdnnvNavigation?.TenNnv.ToLower().Contains(key.ToLower()) == true ||
                    x.IdtkNavigation?.TaiKhoan.ToLower().Contains(key.ToLower()) == true ||
                    x.TenNv.ToLower().Contains(key.ToLower()) ||
                    x.DiaChi.ToLower().Contains(key.ToLower()) ||
                    x.Sdt.ToLower().Contains(key.ToLower()) ||
                    x.Email.ToLower().Contains(key.ToLower()) ||
                    x.Cccd.ToLower().Contains(key.ToLower()) ||
                    x.QueQuan.ToLower().Contains(key.ToLower())))
                    .ToList();
                    r = await RenderNvs(nvs);
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
                    nvs = getListNhanVien().Result.AsParallel()
                    .Where(x =>
                    x.MaNv.ToLower().Contains(key.ToLower()) ||
                    x.IdnnvNavigation?.TenNnv.ToLower().Contains(key.ToLower()) == true ||
                    x.IdtkNavigation?.TaiKhoan.ToLower().Contains(key.ToLower()) == true ||
                    x.TenNv.ToLower().Contains(key.ToLower()) ||
                    x.DiaChi.ToLower().Contains(key.ToLower()) ||
                    x.Sdt.ToLower().Contains(key.ToLower()) ||
                    x.Email.ToLower().Contains(key.ToLower()) ||
                    x.Cccd.ToLower().Contains(key.ToLower()) ||
                    x.QueQuan.ToLower().Contains(key.ToLower()))
                    .ToList();
                    r = await RenderNvs(nvs);
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
        async Task<List<NhanVien>> getListNhanVien()
        {
            int _userId = int.Parse(User.Identity.Name);
            return await _memoryCache.GetOrCreate("NhanViens_" + _userId, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return await _dACNPMContext.NhanViens
                .Include(x => x.IdnnvNavigation)
                .Include(x => x.IdtkNavigation)
                .ToListAsync();
            });
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
        async Task<PhanQuyenChucNang> GetPhanQuyenNhanVien()
        {
            int idPq = int.Parse(User.FindFirstValue("IdPq"));

            return await _dACNPMContext.PhanQuyenChucNangs
                .Include(x => x.IdchucNangNavigation)
                .FirstOrDefaultAsync(x => x.Idpq == idPq
                && x.IdchucNangNavigation.MaChucNang.Equals("DM_NhanVien"));
        }
        async Task<string> RenderNvs(List<NhanVien> nvs)
        {
            ConcurrentBag<string> str = new ConcurrentBag<string>();
            _nvs = await getListNhanVien();
            var phanQuyenNV = GetPhanQuyenNhanVien().Result;

            string can = "lni lni-trash-can";
            string re = "lni lni-spinner-arrow";

            await Task.Run(() =>
                Parallel.ForEach(nvs, nv =>
                {
                    string t = nv.Active.Value ? can : re;
                    string btnSua = phanQuyenNV.Sua.Value
                                    ? $"<button onclick='showModalEdit({nv.Id})' class='text-primary'  data-bs-toggle='modal' data-bs-target='#staticBackdrop'><i class='lni lni-pencil'></i></button>"
                                    : "";
                    string btnXoa = phanQuyenNV.Xoa.Value
                                    ? $"<button onclick='deleteNV({nv.Id})' class='text-danger'><i class='{t}'></i></button>"
                                    : "";
                    str.Add($"<tr>" +
                                    $"<td><div class='product'><div class='image'><img class='image-modal' src='{nv.Avatar}' alt='{nv.TenNv}'></div></div></td>" +
                                    $"<td class ='text-center'>{nv.MaNv}</td>" +
                                    $"<td>{nv.TenNv}</td>" +
                                    $"<td>{nv.DiaChi}</td>" +
                                    $"<td>{nv.QueQuan}</td>" +
                                    $"<td>{nv.Sdt}</td>" +
                                    $"<td>{nv.Email}</td>" +
                                    $"<td>{nv.Cccd}</td>" +
                                    $"<td>{(nv.GioiTinh.Value == true ? "Nam" : "Nữ")}</td>" +
                                    $"<td>{nv.NgaySinh.Value.Date.ToString("dd-MM-yyyy")}</td>" +
                                    $"<td>{(nv.IdnnvNavigation == null ? "" : nv.IdnnvNavigation.TenNnv)}</td>" +
                                    $"<td>{(nv.IdtkNavigation == null ? "" : nv.IdtkNavigation.TaiKhoan)}</td>" +
                                    $"<td>{(nv.IdtkNavigation == null ? "" : nv.IdtkNavigation.MatKhau)}</td>" +
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
