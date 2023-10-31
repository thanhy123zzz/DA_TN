using DA_CNPM_VatTu.Models.Entities;
using DA_CNPM_VatTu.Models.MapData;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace DA_CNPM_VatTu.Controllers
{
    [AllowAnonymous]
    [Route("[controller]")]
    public class LoginController : Controller
    {
        private DACNPMContext _dACNPMContext;
        private IMemoryCache _memoryCache;
        public LoginController(IMemoryCache memoryCache)
        {
            _dACNPMContext = new DACNPMContext();
            _memoryCache = memoryCache;
        }

        [AllowAnonymous]
        [HttpGet("/login")]
        public async Task<IActionResult> login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }
            return View();
        }
        [HttpPost("/api/xac-thuc")]
        public async Task<IActionResult> verifyer([FromBody] UserLogin userLG)
        {
            var user = await _dACNPMContext.NhanViens
                .Include(x => x.IdtkNavigation)
                .Where(x => x.IdtkNavigation.TaiKhoan == userLG.UserName
                        && x.IdtkNavigation.MatKhau == userLG.PassWord
                        && x.Active == true
                ).FirstOrDefaultAsync();
            if (user == null)
            {
                return Ok(new
                {
                    statusCode = 0,
                    message = "Đăng nhập thất bại"
                });
            }
            else
            {
                var phanQuyenNhanViens = await _dACNPMContext.PhanQuyenNhanViens
                    .Include(x => x.IdpqNavigation)
                    .Include(x => x.IdpqNavigation.IdcnNavigation)
                    .Include(x => x.IdpqNavigation.IdvtNavigation)
                    .Where(x => x.Idnv == user.Id && x.Active == true)
                    .ToListAsync();
                _memoryCache.Set("phanQuyenNhanViens", phanQuyenNhanViens, TimeSpan.FromMinutes(5));

                return Ok(new
                {
                    statusCode = 200,
                    message = "Đăng nhập thành công, hãy chọn chi nhánh và vai trò cần đăng nhập!",
                    chiNhanh = phanQuyenNhanViens.Select(x => new
                    {
                        id = x.IdpqNavigation.Idcn,
                        tenCn = x.IdpqNavigation.IdcnNavigation.TenCn
                    }).Distinct().ToList()
                });
            }
        }
        [HttpPost("/api/change-chinhanh")]
        public async Task<IActionResult> change_ChiNhanh(int Cn)
        {
            var phanQuyens = (List<PhanQuyenNhanVien>)_memoryCache.Get("phanQuyenNhanViens");

            return Ok(new
            {
                vaiTro = phanQuyens.Where(x => x.IdpqNavigation.Idcn == Cn).Select(x => new
                {
                    id = x.IdpqNavigation.Idvt,
                    tenVt = x.IdpqNavigation.IdvtNavigation.TenVaiTro
                }).ToList(),
            });
        }
        [HttpPost("/api/login")]
        public async Task<IActionResult> loginWittPhanCong([FromBody] UserLogin userLG)
        {
            if (!User.Identity.IsAuthenticated)
            {
                var user = await _dACNPMContext.NhanViens
                    .Include(x => x.IdtkNavigation)
                    .FirstOrDefaultAsync(x => x.IdtkNavigation.TaiKhoan == userLG.UserName);

                var phanQuyen = await _dACNPMContext.PhanQuyens
                    .Include(x => x.IdcnNavigation)
                    .Include(x => x.IdvtNavigation)
                    .FirstOrDefaultAsync(x => x.Idvt == userLG.IdVt
                    && x.Idcn == userLG.IdChiNhanh);

                var chucNangs = await _dACNPMContext.PhanQuyenChucNangs
                    .Include(x => x.IdchucNangNavigation)
                    .Where(x => x.Idpq == phanQuyen.Id).ToListAsync();

                var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, user.Id.ToString(),user.TenNv,user.Avatar),
                                new Claim("IdVt",phanQuyen.Idvt.ToString(),phanQuyen.IdvtNavigation.TenVaiTro),
                                new Claim("IdCn",phanQuyen.Idcn.ToString(),phanQuyen.IdcnNavigation.TenCn),
                                new Claim("IdPq",phanQuyen.Id.ToString()),
                            };
                foreach (var p in chucNangs)
                {
                    claims.Add(new(ClaimTypes.Role, p.IdchucNangNavigation.MaChucNang
                        , p.IdchucNangNavigation.TenChucNang, p.IdchucNangNavigation.Url));
                }
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // Sign-in user
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(240)
                    });
                return Ok(new
                {
                    statusCode = 200,
                    message = "Đăng nhập thành công!"
                });
            }
            else
            {
                return BadRequest();
            }
        }

        [Route("/logout")]
        public async Task<IActionResult> Logout()
        {
            /*int _userId = int.Parse(User.Identity.Name);*/
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            /*_memoryCache.Remove("chucNang_" + _userId);
            _memoryCache.Remove("vaiTros_" + _userId);
            _memoryCache.Remove("phanQuyenNhanVienCNs_" + _userId);*/

            return Redirect("/login");
        }
    }

}
