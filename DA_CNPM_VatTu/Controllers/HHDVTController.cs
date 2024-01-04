using DA_CNPM_VatTu.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace DA_CNPM_VatTu.Controllers
{
    [Authorize(Roles = "QD_HHDVT")]
    [Route("/QuyDinh/[controller]")]
    public class HHDVTController : Controller
    {
        private DACNPMContext _dACNPMContext;
        private readonly IMemoryCache _memoryCache;
        private ICompositeViewEngine _viewEngine;
        public HHDVTController(IMemoryCache memoryCache, ICompositeViewEngine viewEngine)
        {
            _dACNPMContext = new DACNPMContext();
            _memoryCache = memoryCache;
            _viewEngine = viewEngine;
        }
        [HttpGet]
        public async Task<IActionResult> HHDVT()
        {
            var pqcn = await GetPhanQuyenHHDVT();
            ViewBag.PhanQuyenPQ = pqcn;
            ViewBag.HHs = getListHH().Result.Where(x=>x.Active == true).OrderBy(x=>x.TenHh.Trim()).ToList();
            ViewData["title"] = pqcn.IdchucNangNavigation.TenChucNang;
            return View();
        }
        [HttpPost("search-hh")]
        public async Task<IActionResult> searchHH(string key, bool tt)
        {
			var Si = _dACNPMContext.TiLeCanhBaos.FirstOrDefault(x => x.TenTiLe == "Si").TiLe;
			var Le = _dACNPMContext.TiLeCanhBaos.FirstOrDefault(x => x.TenTiLe == "Le").TiLe;
			if (tt)
            {
                var hangTons = _dACNPMContext.HangTonKhos
                    .Include(x => x.IdhhNavigation)
                    .Include(x => x.IdhhNavigation.IddvtchinhNavigation)
                    .Include(x => x.IdhhNavigation.Hhdvts)
                    .AsEnumerable()
                    .GroupBy(x => x.IdhhNavigation)
                    .Where(x => ((x.Key.GiaBanLe <= (x.Max(y => y.GiaNhap) * Le))
                    || (x.Key.GiaBanSi <= (x.Max(y => y.GiaNhap) * Si))
                    || x.Key.Hhdvts.Where(x => x.Active == true).Any(y => y.GiaBanLe <= (x.Max(z => z.GiaNhap) * Le * y.SlquyDoi))
                    || x.Key.Hhdvts.Where(x => x.Active == true).Any(y => y.GiaBanSi <= (x.Max(z => z.GiaNhap) * Si * y.SlquyDoi))
                    || (x.Key.GiaBanLe == null && x.Key.GiaBanSi == null && x.Key.TiLeLe == null && x.Key.TiLeSi == null)
                    ) && (key == null ? true : (x.Key.TenHh + " " + x.Key.MaHh).ToLower().Contains(key.ToLower()))
                    )
                    .Select(x => new
                    {
                        Id = x.Key.Id,
                        TenHh = x.Key.TenHh,
                        MaHh = x.Key.MaHh,
                    })
                    .ToList();
                return Ok(new
                {
                    data = hangTons
                });
            }
            else
            {
                var listPQnv = await getListHH();
                if (key == null)
                {
                    return Ok(new
                    {
                        data = listPQnv.AsParallel()
                        .Where(x => x.Active == true)
                        .Select(x => new
                        {
                            Id = x.Id,
                            TenHh = x.TenHh,
                            MaHh = x.MaHh
                        }).OrderBy(x=>x.TenHh)
                        .ToList(),
                    });
                }
                var vts = listPQnv.AsParallel()
                    .Where(x => (x.TenHh + " " + x.MaHh).ToLower().Contains(key.ToLower()) && x.Active == true)
                    .ToList();
                return Ok(new
                {
                    data = vts.Select(x => new
                    {
                        Id = x.Id,
                        TenHh = x.TenHh,
                        MaHh = x.MaHh
                    })
                });
            }

        }
        [HttpPost("load-hhdvt")]
        public async Task<IActionResult> loadHHDVT(int idHh, bool tt)
        {
            /*ViewBag.Hhdvts = getListHHdvt().Result.AsParallel()
                .Where(x => x.Idhh == idHh && x.Active == true).ToList();

            ViewBag.Hhdvt = getListHH().Result.AsParallel().FirstOrDefault(x => x.Id == idHh);

            ViewBag.PhanQuyenPQ = await GetPhanQuyenHHDVT();
            ViewBag.Si = _dACNPMContext.TiLeCanhBaos.FirstOrDefault(x => x.TenTiLe == "Si").TiLe;
            ViewBag.Le = _dACNPMContext.TiLeCanhBaos.FirstOrDefault(x => x.TenTiLe == "Le").TiLe;
            ViewBag.giaNhaps = _dACNPMContext.HangTonKhos
                .Include(x => x.IdhhNavigation.IddvtchinhNavigation)
                .OrderByDescending(x => x.GiaNhap)
                .Where(x => x.Idhh == idHh).ToList();
            return PartialView("loadTableDVTHH");*/
            var ht =  _dACNPMContext.HangTonKhos
                .Include(x=>x.IdhhNavigation.IddvtchinhNavigation)
                .Where(x => x.Idhh == idHh).OrderByDescending(x => x.NgayNhap);
            var maxTon = ht.Max(x => x.GiaNhap * (1 - x.Cktm / 100) * (1 + x.Thue / 100)) ?? 0;
            return Ok(new
            {
                hh = await _dACNPMContext.HangHoas.Include(x=>x.IddvtchinhNavigation).FirstOrDefaultAsync(x=>x.Id == idHh),
                hhDvts = await _dACNPMContext.Hhdvts
                .Include(x=>x.IddvtNavigation)
                .Where(x => x.Idhh == idHh && x.Active == true).ToListAsync(),
                hangTonKhos = await ht.ToListAsync(),
                maxTon,
                si = _dACNPMContext.TiLeCanhBaos.FirstOrDefault(x => x.TenTiLe == "Si").TiLe ?? 0,
                le = _dACNPMContext.TiLeCanhBaos.FirstOrDefault(x => x.TenTiLe == "Le").TiLe ?? 0,
            });
        }
        [HttpGet("change-tt")]
        public async Task<IActionResult> ThayDoiTrangThai(bool tt)
        {
			var Si = _dACNPMContext.TiLeCanhBaos.FirstOrDefault(x => x.TenTiLe == "Si").TiLe;
			var Le = _dACNPMContext.TiLeCanhBaos.FirstOrDefault(x => x.TenTiLe == "Le").TiLe;
			if (tt)
            {
                var hangTons = _dACNPMContext.HangTonKhos
                    .Include(x => x.IdhhNavigation)
                    .Include(x => x.IdhhNavigation.IddvtchinhNavigation)
                    .Include(x => x.IdhhNavigation.Hhdvts)
                    .AsEnumerable()
                    .GroupBy(x => x.IdhhNavigation)
                    .Where(x => (x.Key.GiaBanLe <= (x.Max(y => y.GiaNhap * (1 - y.Cktm / 100) * (1 + y.Thue / 100)) * Le))
                    || (x.Key.GiaBanSi <= (x.Max(y => y.GiaNhap * (1 - y.Cktm / 100) * (1 + y.Thue / 100)) * Si))
                    || x.Key.Hhdvts.Where(x => x.Active == true).Any(y => y.GiaBanLe <= (x.Max(z => z.GiaNhap * (1 - z.Cktm / 100) * (1 + z.Thue / 100)) * Le * y.SlquyDoi))
                    || x.Key.Hhdvts.Where(x => x.Active == true).Any(y => y.GiaBanSi <= (x.Max(z => z.GiaNhap * (1 - z.Cktm / 100) * (1 + z.Thue / 100)) * Si * y.SlquyDoi))
                    || (x.Key.GiaBanLe == null && x.Key.GiaBanSi == null && x.Key.TiLeLe == null && x.Key.TiLeSi == null))
                    .Select(x => new
                    {
                        x.Key.Id,
                        x.Key.TenHh,
                        x.Key.MaHh,
                    }).OrderBy(x=>x.TenHh)
                    .ToList();
                return Ok(new
                {
                    data = hangTons
                });
            }
            else
            {
                return Ok(new
                {
                    data = getListHH().Result.Select(x => new
                    {
                        x.Id,
                        TenHh = x.TenHh.Trim(),
                        x.MaHh
                    }).OrderBy(x=>x.TenHh).ToList()
                });
            }

        }
        [HttpPost("show-modal-hhdvt")]
        public async Task<IActionResult> show_Modal_hhdvt(int idHhdvt, int idHh)
        {
            ViewBag.IdHH = idHh;
            var hhdvt = await _dACNPMContext.Hhdvts.Include(x=>x.IddvtNavigation).FirstOrDefaultAsync(x => x.Id == idHhdvt);
            PartialViewResult partialViewResult = PartialView("formHHDVT", hhdvt == null ? new Hhdvt() : hhdvt);
            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);
            return Ok(new
            {
                view = viewContent,
                title = hhdvt == null ? "Thêm đơn vị tính hàng hoá" : "Chỉnh sửa đơn vị tính hàng hoá"
            });
        }
        [HttpPost("show-modal-hhdvtc")]
        public async Task<IActionResult> show_Modal_hhdvtc(int idHh)
        {
            ViewBag.IdHH = idHh;
            var hh = await _dACNPMContext.HangHoas.Include(x=>x.IddvtchinhNavigation).FirstOrDefaultAsync(x => x.Id == idHh);
            PartialViewResult partialViewResult = PartialView("formHHDVTC", hh);
            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);
            return Ok(new
            {
                view = viewContent,
                title = "Chỉnh sửa đơn vị tính chính"
            });
        }
        [HttpPost("api/dvts")]
        public async Task<IActionResult> getDvtExcept(int idHh, int idDvt)
        {
            var dvtChinh = getListHH().Result.FirstOrDefault(x => x.Id == idHh).Iddvtchinh;
            var listDvt = getListHHdvt().Result
                .Where(x => x.Idhh == idHh && x.Active == true)
                .Select(x => x.Iddvt).ToList();
            listDvt.Add(dvtChinh.Value);
            if (idDvt != null)
            {
                listDvt.Remove(idDvt);
            }
            var dvts = getListDVT().Result
                .Where(x => !listDvt.Any(y => y == x.Id)).ToList();
            return Ok(dvts.Select(x => new
            {
                id = x.Id,
                ma = x.MaDvt,
                ten = x.TenDvt,
            }).ToList());
        }
        [HttpPost("update-hhdvt")]
        public async Task<IActionResult> updateDVTPhu([FromBody] Hhdvt hhdvt)
        {
            var tran = _dACNPMContext.Database.BeginTransaction();
            int _userId = int.Parse(User.Identity.Name);
            bool tt = hhdvt.Active.Value;
            try
            {
                var Si = _dACNPMContext.TiLeCanhBaos.FirstOrDefault(x => x.TenTiLe == "Si").TiLe;
                var Le = _dACNPMContext.TiLeCanhBaos.FirstOrDefault(x => x.TenTiLe == "Le").TiLe;
                ViewBag.Si = Si;
                ViewBag.Le = Le;
                if (hhdvt.Id == 0)
                {
                    hhdvt.Active = true;
                    hhdvt.Nvtao = int.Parse(User.Identity.Name);
                    hhdvt.NgayTao = DateTime.Now;
                    hhdvt.Idcn = int.Parse(User.FindFirstValue("IdCn"));
                    hhdvt.TiLeLe = hhdvt.TiLeLe == 0 ? null : hhdvt.TiLeLe;
                    hhdvt.TiLeSi = hhdvt.TiLeSi == 0 ? null : hhdvt.TiLeSi;
                    hhdvt.GiaBanLe = hhdvt.GiaBanLe == 0 ? null : hhdvt.GiaBanLe;
                    hhdvt.GiaBanSi = hhdvt.GiaBanSi == 0 ? null : hhdvt.GiaBanSi;
                    await _dACNPMContext.Hhdvts.AddAsync(hhdvt);
                }
                else
                {
                    var hhdvtDB = await _dACNPMContext.Hhdvts.FindAsync(hhdvt.Id);

                    hhdvtDB.Iddvt = hhdvt.Iddvt;
                    hhdvtDB.TiLeLe = hhdvt.TiLeLe == 0 ? null : hhdvt.TiLeLe;
                    hhdvtDB.TiLeSi = hhdvt.TiLeSi == 0 ? null : hhdvt.TiLeSi;
                    hhdvtDB.GiaBanLe = hhdvt.GiaBanLe == 0 ? null : hhdvt.GiaBanLe;
                    hhdvtDB.GiaBanSi = hhdvt.GiaBanSi == 0 ? null : hhdvt.GiaBanSi;
                    hhdvtDB.SlquyDoi = hhdvt.SlquyDoi;

                    hhdvtDB.Nvsua = int.Parse(User.Identity.Name);
                    hhdvtDB.NgaySua = DateTime.Now;

                    _dACNPMContext.Hhdvts.Update(hhdvtDB);
                }

                await _dACNPMContext.SaveChangesAsync();
                await tran.CommitAsync();
                _memoryCache.Remove("Hhdvts_" + _userId);


                ViewBag.giaNhaps = _dACNPMContext.HangTonKhos
                .Include(x => x.IdhhNavigation.IddvtchinhNavigation)
                .OrderByDescending(x => x.GiaNhap)
                .Where(x => x.Idhh == hhdvt.Idhh).ToList();
                ViewBag.Hhdvts = getListHHdvt().Result.AsParallel()
                .Where(x => x.Idhh == hhdvt.Idhh && x.Active == true).ToList();
                ViewBag.Hhdvt = getListHH().Result.AsParallel().FirstOrDefault(x => x.Id == hhdvt.Idhh);

                ViewBag.PhanQuyenPQ = await GetPhanQuyenHHDVT();
                PartialViewResult partialViewResult = PartialView("loadTableDVTHH");
                string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);
				
				if (tt)
                {
                    var hangTons = _dACNPMContext.HangTonKhos
                    .Include(x => x.IdhhNavigation)
                    .Include(x => x.IdhhNavigation.IddvtchinhNavigation)
                    .Include(x => x.IdhhNavigation.Hhdvts)
                    .AsEnumerable()
                    .GroupBy(x => x.IdhhNavigation)
                    .Where(x => (x.Key.GiaBanLe <= (x.Max(y => y.GiaNhap * (1 - y.Cktm / 100) * (1 + y.Thue / 100)) * Le))
                    || (x.Key.GiaBanSi <= (x.Max(y => y.GiaNhap * (1 - y.Cktm / 100) * (1 + y.Thue / 100)) * Si))
                    || x.Key.Hhdvts.Where(x => x.Active == true).Any(y => y.GiaBanLe <= (x.Max(z => z.GiaNhap * (1 - z.Cktm / 100) * (1 + z.Thue / 100)) * Le * y.SlquyDoi))
                    || x.Key.Hhdvts.Where(x => x.Active == true).Any(y => y.GiaBanSi <= (x.Max(z => z.GiaNhap * (1 - z.Cktm / 100) * (1 + z.Thue / 100)) * Si * y.SlquyDoi))
                    || (x.Key.GiaBanLe == null && x.Key.GiaBanSi == null && x.Key.TiLeLe == null && x.Key.TiLeSi == null))
                    .Select(x => new
                    {
                        Id = x.Key.Id,
                        TenHh = x.Key.TenHh,
                        MaHh = x.Key.MaHh,
                    })
                    .ToList();
                    return Ok(new
                    {
                        statusCode = 200,
                        message = "Thành công!",
                        color = "bg-success",
                        viewData = viewContent,
                        data = hangTons
                    });
                }
                else
                {
                    return Ok(new
                    {
                        statusCode = 200,
                        message = "Thành công!",
                        color = "bg-success",
                        viewData = viewContent
                    });
                }
            }
            catch (Exception e)
            {
                await tran.RollbackAsync();
                return Ok(new
                {
                    statusCode = 500,
                    message = "Thất bại!",
                    color = "bg-danger",
                });
            }
        }
        [HttpPost("update-hhdvtc")]
        public async Task<IActionResult> updateDVTChinh([FromBody] HangHoa hh)
        {
            var tran = _dACNPMContext.Database.BeginTransaction();
            int _userId = int.Parse(User.Identity.Name);
            bool tt = hh.Active.Value;
            try
            {
                var Si = _dACNPMContext.TiLeCanhBaos.FirstOrDefault(x => x.TenTiLe == "Si").TiLe;
                var Le = _dACNPMContext.TiLeCanhBaos.FirstOrDefault(x => x.TenTiLe == "Le").TiLe;
                ViewBag.Si = Si;
                ViewBag.Le = Le;
                var hhdvtDB = await _dACNPMContext.HangHoas.FindAsync(hh.Id);

                hhdvtDB.Iddvtchinh = hh.Iddvtchinh;
                hhdvtDB.TiLeLe = hh.TiLeLe == 0 ? null : hh.TiLeLe;
                hhdvtDB.TiLeSi = hh.TiLeSi == 0 ? null : hh.TiLeSi;
                hhdvtDB.GiaBanLe = hh.GiaBanLe == 0 ? null : hh.GiaBanLe;
                hhdvtDB.GiaBanSi = hh.GiaBanSi == 0 ? null : hh.GiaBanSi;

                hhdvtDB.Nvsua = int.Parse(User.Identity.Name);
                hhdvtDB.NgaySua = DateTime.Now;

                _dACNPMContext.HangHoas.Update(hhdvtDB);

                await _dACNPMContext.SaveChangesAsync();
                await tran.CommitAsync();
                _memoryCache.Remove("HangHoas_" + _userId);
                ViewBag.giaNhaps = _dACNPMContext.HangTonKhos
                .Include(x => x.IdhhNavigation.IddvtchinhNavigation)
                .OrderByDescending(x => x.GiaNhap)
                .Where(x => x.Idhh == hh.Id).ToList();
                ViewBag.Hhdvts = getListHHdvt().Result.AsParallel()
                .Where(x => x.Idhh == hh.Id && x.Active == true).ToList();

                ViewBag.Hhdvt = getListHH().Result.AsParallel().FirstOrDefault(x => x.Id == hh.Id);

                ViewBag.PhanQuyenPQ = await GetPhanQuyenHHDVT();
                PartialViewResult partialViewResult = PartialView("loadTableDVTHH");
                string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);
				if (tt)
                {
                    var hangTons = _dACNPMContext.HangTonKhos
                    .Include(x => x.IdhhNavigation)
                    .Include(x => x.IdhhNavigation.IddvtchinhNavigation)
                    .Include(x => x.IdhhNavigation.Hhdvts)
                    .AsEnumerable()
                    .GroupBy(x => x.IdhhNavigation)
                    .Where(x => (x.Key.GiaBanLe <= (x.Max(y => y.GiaNhap * (1 - y.Cktm / 100) * (1 + y.Thue / 100)) * Le))
                    || (x.Key.GiaBanSi <= (x.Max(y => y.GiaNhap * (1 - y.Cktm / 100) * (1 + y.Thue / 100)) * Si))
                    || x.Key.Hhdvts.Where(x => x.Active == true).Any(y => y.GiaBanLe <= (x.Max(z => z.GiaNhap * (1 - z.Cktm / 100) * (1 + z.Thue / 100)) * Le * y.SlquyDoi))
                    || x.Key.Hhdvts.Where(x => x.Active == true).Any(y => y.GiaBanSi <= (x.Max(z => z.GiaNhap * (1 - z.Cktm / 100) * (1 + z.Thue / 100)) * Si * y.SlquyDoi))
                    || (x.Key.GiaBanLe == null && x.Key.GiaBanSi == null && x.Key.TiLeLe == null && x.Key.TiLeSi == null))
                    .Select(x => new
                    {
                        Id = x.Key.Id,
                        TenHh = x.Key.TenHh,
                        MaHh = x.Key.MaHh,
                    })
                    .ToList();

                    return Ok(new
                    {
                        statusCode = 200,
                        message = "Thành công!",
                        color = "bg-success",
                        viewData = viewContent,
                        data = hangTons
                    });
                }
                else
                {
                    return Ok(new
                    {
                        statusCode = 200,
                        message = "Thành công!",
                        color = "bg-success",
                        viewData = viewContent
                    });
                }
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
        public async Task<IActionResult> remove(int idHhdvt, int idHh)
        {
            var Si = _dACNPMContext.TiLeCanhBaos.FirstOrDefault(x => x.TenTiLe == "Si").TiLe;
            var Le = _dACNPMContext.TiLeCanhBaos.FirstOrDefault(x => x.TenTiLe == "Le").TiLe;
            ViewBag.Si = Si;
            ViewBag.Le = Le;
            int _userId = int.Parse(User.Identity.Name);
            var itemDB = await _dACNPMContext.Hhdvts.FindAsync(idHhdvt);
            itemDB.Active = !itemDB.Active;
            itemDB.Nvsua = int.Parse(User.Identity.Name);
            itemDB.NgaySua = DateTime.Now;
            _dACNPMContext.Hhdvts.Update(itemDB);

            await _dACNPMContext.SaveChangesAsync();

            _memoryCache.Remove("Hhdvts_" + _userId);

            ViewBag.Hhdvts = getListHHdvt().Result.AsParallel()
                .Where(x => x.Idhh == idHh && x.Active == true).ToList();
            ViewBag.Hhdvt = getListHH().Result.AsParallel().FirstOrDefault(x => x.Id == idHh);
            ViewBag.PhanQuyenPQ = await GetPhanQuyenHHDVT();
            PartialViewResult partialViewResult = PartialView("loadTableDVTHH");
            string viewContent = ConvertViewToString(ControllerContext, partialViewResult, _viewEngine);

            return Ok(new
            {
                statusCode = 200,
                message = "Thành công!",
                color = "bg-success",
                viewData = viewContent
            });
        }
        async Task<PhanQuyenChucNang> GetPhanQuyenHHDVT()
        {
            int idPq = int.Parse(User.FindFirstValue("IdPq"));

            return await _dACNPMContext.PhanQuyenChucNangs
                .Include(x => x.IdchucNangNavigation)
                .FirstOrDefaultAsync(x => x.Idpq == idPq
                && x.IdchucNangNavigation.MaChucNang.Equals("QD_HHDVT"));
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
                                .ToListAsync();
            });
        }
        async Task<List<Hhdvt>> getListHHdvt()
        {
            int idCn = int.Parse(User.FindFirstValue("IdCn"));
            int _userId = int.Parse(User.Identity.Name);
            return await _memoryCache.GetOrCreateAsync("Hhdvts_" + _userId, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return await _dACNPMContext.Hhdvts
                                .Include(x => x.IddvtNavigation)
                                .Where(x => x.Idcn == idCn)
                                .ToListAsync();
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
