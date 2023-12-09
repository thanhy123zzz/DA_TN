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
    [Authorize(Roles = "DM_HangHoa")]
    [Route("DanhMuc/[controller]")]
    public class HangHoaController : Controller
    {
        private DACNPMContext _dACNPMContext;
        private ICompositeViewEngine _viewEngine;
        private readonly ILogger<DonViTinhController> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private List<NhanVien> _nvs;
        private readonly IMemoryCache _memoryCache;
        public HangHoaController(ILogger<DonViTinhController> logger,
            ICompositeViewEngine viewEngine, IMemoryCache memoryCache, IWebHostEnvironment hostingEnvironment)
        {
            _dACNPMContext = new DACNPMContext();
            _logger = logger;
            _viewEngine = viewEngine;
            _memoryCache = memoryCache;
            _hostingEnvironment = hostingEnvironment;
        }
        [HttpGet]
        public async Task<IActionResult> HangHoa()
        {
            var HangHoas = await getListHH();
            ViewBag.HangHoas = HangHoas.Where(x => x.Active == true)
                .OrderBy(x=>x.TenHh.Trim())
            .Take(15)
            .ToList();

            var pqcn = await GetPhanQuyenHH();
            ViewBag.phanQuyenHH = pqcn;
            ViewData["title"] = pqcn.IdchucNangNavigation.TenChucNang;
            ViewBag.NhanViens = await getListNhanVien();
            return View();
        }
        [HttpGet("show-modal/{id}")]
        public async Task<IActionResult> showEdit(int id)
        {
            var Hh = getListHH().Result.FirstOrDefault(x => x.Id == id);
            if (Hh == null)
            {
                Hh = new HangHoa();
                Hh.MaHh = await getSoPhieu();
            }
            async Task<string> getSoPhieu()
            {
                var context = new DACNPMContext();
                //ID chi nhánh
                int cn = int.Parse(User.FindFirstValue("IdCn"));

                DateTime d = DateTime.Now;
                string ngayThangNam = d.ToString("yyMMdd");

                QuyDinhMa qd = await context.QuyDinhMas.FirstOrDefaultAsync(x => x.TiepDauNgu == "H");
                string SoPhieu = cn + "_" + qd.TiepDauNgu + ngayThangNam;
                var list = await context.SoThuTus.FirstOrDefaultAsync(x => x.Ngay.Date == DateTime.Now.Date && x.Loai.Equals("HangHoa"));
                int stt;
                if (list == null)
                {
                    SoThuTu sttt = new SoThuTu();
                    sttt.Ngay = DateTime.ParseExact(DateTime.Now.ToString("dd-MM-yyyy"), "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    sttt.Stt = 1;
                    sttt.Loai = "HangHoa";
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
            PartialViewResult partialViewResult = PartialView("FormHH", Hh);
            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);
            return Ok(new
            {
                view = viewContent,
                title = Hh == null ? "Thêm hàng hoá" : "Chỉnh sửa hàng hoá"
            });
        }
        [HttpPost("searchTableHH")]
        public async Task<string> searchHH(string key, bool active)
        {
            List<HangHoa> hhs;
            if (active)
            {
                hhs = getListHH().Result
                .Where(x => x.Active == active && (
                x.MaHh.ToLower().Contains(key.ToLower()) ||
                x.TenHh?.ToLower().Contains(key.ToLower()) == true ||
                x.IddvtchinhNavigation?.TenDvt.ToLower().Contains(key.ToLower()) == true ||
                x.IdnhhNavigation?.TenNhh.ToLower().Contains(key.ToLower()) == true ||
                x.IdnsxNavigation?.TenNsx.ToLower().Contains(key.ToLower()) == true ||
                x.IdhsxNavigation?.TenHsx?.ToLower().Contains(key.ToLower()) == true))
                .OrderBy(x => x.TenHh.Trim())
                .ToList();
                var r = await RenderHhs(hhs);

                return r;
            }
            else
            {
                hhs = getListHH().Result
                .Where(x => x.MaHh.ToLower().Contains(key.ToLower()) ||
                x.TenHh?.ToLower().Contains(key.ToLower()) == true ||
                x.IddvtchinhNavigation?.TenDvt.ToLower().Contains(key.ToLower()) == true ||
                x.IdnhhNavigation?.TenNhh.ToLower().Contains(key.ToLower()) == true ||
                x.IdnsxNavigation?.TenNsx.ToLower().Contains(key.ToLower()) == true ||
                x.IdhsxNavigation?.TenHsx?.ToLower().Contains(key.ToLower()) == true)
                .OrderBy(x => x.TenHh.Trim())
                .ToList();
                var r = await RenderHhs(hhs);

                return r;
            }

        }
        [HttpPost("change-page")]
        public async Task<string> changePage(int page, bool active)
        {
            List<HangHoa> hhs;
            if (active)
            {
                hhs = getListHH().Result
                .Where(x => x.Active == active)
                .OrderBy(x => x.TenHh.Trim())
                .Skip(page * 15)
                .Take(15)
                .ToList();
                var r = await RenderHhs(hhs);
                return r;
            }
            else
            {
                hhs = getListHH().Result
                    .OrderBy(x => x.TenHh.Trim())
                .Skip(page * 15)
                .Take(15)
                .ToList();
                var r = await RenderHhs(hhs);
                return r;
            }
        }
        [HttpPost("api/dvts")]
        public async Task<IActionResult> optionDVTS()
        {
            return Ok(await _dACNPMContext.DonViTinhs
                .AsNoTracking()
                .Where(x => x.Active == true)
                .Select(x => new
            {
                id = x.Id,
                ma = x.MaDvt,
                ten = x.TenDvt,
            }).ToListAsync());
        }
        [HttpPost("api/nhhs")]
        public async Task<IActionResult> optionNhh()
        {
            return Ok(await _dACNPMContext.NhomHangHoas
            .AsNoTracking()
                .Where(x => x.Active == true)
            .Select(x => new
            {
                id = x.Id,
                ma = x.MaNhh,
                ten = x.TenNhh,
            }).ToListAsync());
        }
        [HttpPost("api/nsx")]
        public async Task<IActionResult> optionNsx()
        {
            return Ok(await _dACNPMContext.NuocSanXuats.AsNoTracking()
                .Where(x => x.Active == true).Select(x => new
            {
                id = x.Id,
                ma = x.MaNsx,
                ten = x.TenNsx,
            }).ToListAsync());
        }
        [HttpPost("api/hsx")]
        public async Task<IActionResult> optionHangSx()
        {
            return Ok(await _dACNPMContext.HangSanXuats
                .AsNoTracking()
                .Where(x => x.Active == true).Select(x => new
            {
                id = x.Id,
                ma = x.MaHsx,
                ten = x.TenHsx,
            }).ToListAsync());
        }

        [HttpPost("api/bh")]
        public async Task<IActionResult> optionBH()
        {
            return Ok(await _dACNPMContext.BaoHanhs
                .AsNoTracking()
                .Where(x => x.Active == true)
                .Select(x => new
            {
                id = x.Id,
                ma = x.MaBh,
                ten = x.TenBh,
            }).ToListAsync());
        }


        [HttpPost("update-hh")]
        public async Task<IActionResult> updateHH(MapHangHoa mapHH)
        {
            var tran = _dACNPMContext.Database.BeginTransaction();
            try
            {
                HangHoa hh = mapHH.HangHoa;
                int _userId = int.Parse(User.Identity.Name);
                if (hh != null)
                {
                    string webRootPath = _hostingEnvironment.WebRootPath;
                    string imagesPath = Path.Combine(webRootPath, "assets", "images", "HangHoa");

                    if (mapHH.FormFile != null)
                    {
                        string fileName = hh.MaHh + Path.GetExtension(mapHH.FormFile.FileName);
                        string filePath = Path.Combine(imagesPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await mapHH.FormFile.CopyToAsync(stream);
                        }
                        hh.Avatar = "/assets/images/HangHoa/" + fileName;
                    }

                    if (hh.Id == 0)
                    {
                        hh.Nvtao = _userId;
                        hh.NgayTao = DateTime.Now;
                        hh.Active = true;
                        hh.Idcn = int.Parse(User.FindFirstValue("IdCn"));
                        await _dACNPMContext.HangHoas.AddAsync(hh);
                    }
                    else
                    {
                        var hhDB = await _dACNPMContext.HangHoas.FindAsync(hh.Id);

                        hhDB.MaHh = hh.MaHh;
                        hhDB.TenHh = hh.TenHh;
                        hhDB.Idnhh = hh.Idnhh;
                        hhDB.Idnsx = hh.Idnsx;
                        hhDB.Idhsx = hh.Idhsx;
                        hhDB.IdbaoHanh = hh.IdbaoHanh;
                        hhDB.Iddvtchinh = hh.Iddvtchinh;
                        hhDB.Avatar = hh.Avatar == null ? hhDB.Avatar : hh.Avatar;
                        hhDB.Nvsua = _userId;
                        hhDB.NgaySua = DateTime.Now;

                        _dACNPMContext.HangHoas.Update(hhDB);
                    }
                }
                _dACNPMContext.SaveChanges();
                tran.Commit();

                _memoryCache.Remove("HangHoas_" + _userId);
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
            var itemDB = await _dACNPMContext.HangHoas.FindAsync(Id);
            itemDB.Active = !itemDB.Active;
            itemDB.Nvsua = int.Parse(User.Identity.Name);
            itemDB.NgaySua = DateTime.Now;
            _dACNPMContext.HangHoas.Update(itemDB);

            await _dACNPMContext.SaveChangesAsync();

            _memoryCache.Remove("HangHoas_" + _userId);

            List<HangHoa> hhs;
            string r;
            if (key == null)
            {
                if (active)
                {
                    hhs = getListHH().Result
                    .Where(x => x.Active == active)
                    .OrderBy(x => x.TenHh.Trim())
                    .Skip(page * 15)
                    .Take(15)
                    .ToList();
                    r = await RenderHhs(hhs);
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
                    hhs = getListHH().Result
                        .OrderBy(x => x.TenHh.Trim())
                    .Skip(page * 15)
                    .Take(15)
                    .ToList();
                    r = await RenderHhs(hhs);
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
                    hhs = getListHH().Result
                    .Where(x => x.Active == active && (
                    x.MaHh.ToLower().Contains(key.ToLower()) ||
                    x.TenHh.ToLower().Contains(key.ToLower()) ||
                    x.IddvtchinhNavigation.TenDvt.ToLower().Contains(key.ToLower()) ||
                    x.IdnhhNavigation.TenNhh.ToLower().Contains(key.ToLower()) ||
                    x.IdnsxNavigation.TenNsx.ToLower().Contains(key.ToLower()) ||
                    x.IdhsxNavigation.TenHsx.ToLower().Contains(key.ToLower())))
                    .OrderBy(x => x.TenHh.Trim())
                    .ToList();
                    r = await RenderHhs(hhs);
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
                    hhs = getListHH().Result
                    .Where(x =>
                    x.MaHh.ToLower().Contains(key.ToLower()) ||
                    x.TenHh.ToLower().Contains(key.ToLower()) ||
                    x.IddvtchinhNavigation.TenDvt.ToLower().Contains(key.ToLower()) ||
                    x.IdnhhNavigation.TenNhh.ToLower().Contains(key.ToLower()) ||
                    x.IdnsxNavigation.TenNsx.ToLower().Contains(key.ToLower()) ||
                    x.IdhsxNavigation.TenHsx.ToLower().Contains(key.ToLower()))
                    .OrderBy(x => x.TenHh.Trim())
                    .ToList();
                    r = await RenderHhs(hhs);
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
        async Task<string> RenderHhs(List<HangHoa> Hhs)
        {
            ConcurrentBag<string> str = new ConcurrentBag<string>();
            _nvs = await getListNhanVien();
            var phanQuyenNhomHH = GetPhanQuyenHH().Result;

            string can = "lni lni-trash-can";
            string re = "lni lni-spinner-arrow";
            Hhs.ForEach(hh =>
            {
                string t = hh.Active.Value ? can : re;
                string btnSua = phanQuyenNhomHH.Sua.Value
                                ? $"<button onclick='showModalEdit({hh.Id})' class='text-primary'  data-bs-toggle='modal' data-bs-target='#staticBackdrop'><i class='lni lni-pencil'></i></button>"
                                : "";
                string btnXoa = phanQuyenNhomHH.Xoa.Value
                                ? $"<button onclick='deleteHH({hh.Id})' class='text-danger'><i class='{t}'></i></button>"
                                : "";
                str.Add($"<tr>" +
                                $"<td><div class='product'><div class='image'><img class='image-modal' src='{hh.Avatar}' alt='{hh.TenHh}'></div></div></td>" +
                                $"<td>{hh.MaHh}</td>" +
                                $"<td>{hh.TenHh}</td>" +
                                $"<td>{(hh.IdnhhNavigation == null ? "" : hh.IdnhhNavigation.TenNhh)}</td>" +
                                $"<td>{(hh.IdnsxNavigation == null ? "" : hh.IdnsxNavigation.TenNsx)}</td>" +
                                $"<td>{(hh.IdhsxNavigation == null ? "" : hh.IdhsxNavigation.TenHsx)}</td>" +
                                $"<td>{(hh.IddvtchinhNavigation == null ? "" : hh.IddvtchinhNavigation.TenDvt)}</td>" +
                                $"<td>{(hh.IdbaoHanhNavigation == null ? "" : hh.IdbaoHanhNavigation.TenBh)}</td>" +
                                $"<td>{getNhanVien(hh.Nvtao).TenNv}</td>" +
                                $"<td>{formatDay(hh.NgayTao)}</td>" +
                                $"<td>{getNhanVien(hh.Nvsua).TenNv}</td>" +
                                $"<td>{formatDay(hh.NgaySua)}</td>" +
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
        async Task<PhanQuyenChucNang> GetPhanQuyenHH()
        {
            int idPq = int.Parse(User.FindFirstValue("IdPq"));

            return await _dACNPMContext.PhanQuyenChucNangs
                .Include(x => x.IdchucNangNavigation)
                .FirstOrDefaultAsync(x => x.Idpq == idPq
                && x.IdchucNangNavigation.MaChucNang.Equals("DM_HangHoa"));
        }
        async Task<List<HangHoa>> getListHH()
        {
            int _userId = int.Parse(User.Identity.Name);
            return await _memoryCache.GetOrCreateAsync("HangHoas_" + _userId, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return await _dACNPMContext.HangHoas
                                .Include(x => x.IddvtchinhNavigation)
                                .Include(x => x.IdhsxNavigation)
                                .Include(x => x.IdnsxNavigation)
                                .Include(x => x.IdnhhNavigation)
                                .Include(x => x.IdbaoHanhNavigation)
                                .ToListAsync();
            });
        }
        async Task<List<HangSanXuat>> getListHangSX()
        {
            int _userId = int.Parse(User.Identity.Name);
            return await _memoryCache.GetOrCreateAsync("HangSx_" + _userId, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return await _dACNPMContext.HangSanXuats.ToListAsync();
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
        async Task<List<NhomHangHoa>> getListNhomHH()
        {
            int _userId = int.Parse(User.Identity.Name);
            return await _memoryCache.GetOrCreateAsync("NhomHH_" + _userId, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return await _dACNPMContext.NhomHangHoas.ToListAsync();
            });
        }
        async Task<List<NuocSanXuat>> getListNuocSX()
        {
            int _userId = int.Parse(User.Identity.Name);
            return await _memoryCache.GetOrCreateAsync("NuocSx_" + _userId, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return await _dACNPMContext.NuocSanXuats.ToListAsync();
            });
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
        async Task<List<DonViTinh>> getListDVT()
        {
            int idcn = int.Parse(User.FindFirstValue("IdCn"));
            return await _memoryCache.GetOrCreateAsync("Dvts_" + idcn, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return await _dACNPMContext.DonViTinhs.ToListAsync();
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
