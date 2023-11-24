using DA_CNPM_VatTu.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DA_CNPM_VatTu.Controllers
{
    [Authorize(Roles = "QD_CachXuat")]
    [Route("QuyDinh/[controller]")]
    public class CachXuatController : Controller
    {
        [HttpGet]
        public IActionResult CachXuat()
        {
            var context = new DACNPMContext();
            CachXuat c = context.CachXuats.Find(1);
            var pqcn = GetPhanQuyenCachXuat(context).Result;
            ViewBag.PhanQuyenCachXuat = pqcn;
            ViewData["title"] = pqcn.IdchucNangNavigation.TenChucNang;
            ViewBag.TinhGiaXuat = context.TinhGiaXuats.ToList();
            ViewBag.TiLeCanhBaoSi = context.TiLeCanhBaos.FirstOrDefault(x => x.TenTiLe == "Si");
            ViewBag.TiLeCanhBaoLe = context.TiLeCanhBaos.FirstOrDefault(x => x.TenTiLe == "Le");
            return View(c);
        }
        [HttpPost("change")]
        public IActionResult updateCachXuat(int CachXuat, int giaXuat, double Si, double Le)
        {
            var context = new DACNPMContext();
            context.TiLeCanhBaos.FirstOrDefault(x => x.TenTiLe == "Si").TiLe = Si;
            context.TiLeCanhBaos.FirstOrDefault(x => x.TenTiLe == "Le").TiLe = Le;

			CachXuat c = context.CachXuats.Find(1);
            if (CachXuat == 1)
            {
                c.TheoTgnhap = true;
                c.TheoHsd = false;
                context.CachXuats.Update(c);
                context.SaveChanges();
            }
            if (CachXuat == 2)
            {
                c.TheoTgnhap = false;
                c.TheoHsd = true;
                context.CachXuats.Update(c);
                context.SaveChanges();

            }
            var tinhGiaXuats = context.TinhGiaXuats.ToList();
            foreach (TinhGiaXuat t in tinhGiaXuats)
            {
                if (t.Id == giaXuat)
                {
                    t.GiaTri = true;
                    context.TinhGiaXuats.Update(t);
                }
                else
                {
                    t.GiaTri = false;
                    context.TinhGiaXuats.Update(t);
                }
            }
            context.SaveChanges();

            return Ok(new
            {
                statusCode = 200,
                message = "Thành công!",
                color = "bg-success"
            });
        }
        async Task<PhanQuyenChucNang> GetPhanQuyenCachXuat(DACNPMContext context)
        {
            int idPq = int.Parse(User.FindFirstValue("IdPq"));

            return await context.PhanQuyenChucNangs
                .Include(x => x.IdchucNangNavigation)
                .FirstOrDefaultAsync(x => x.Idpq == idPq
                && x.IdchucNangNavigation.MaChucNang.Equals("QD_CachXuat"));
        }
    }
}
