using ABC.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ABC.Controllers
{
    public class AccountController : Controller
    {
        private readonly QlpcthucTapContext _context;

        public AccountController(QlpcthucTapContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            // Kiểm tra null trước khi truy cập Identity
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Xác thực người dùng từ bảng Taikhoan
                var user = await _context.Taikhoans
                    .Include(t => t.MaQuyenNavigation)
                    .FirstOrDefaultAsync(u => u.TaiKhoan == model.Username && u.MatKhau == model.Password);
                
                if (user != null)
                {
                    // Lấy tên role từ bảng Quyen
                    var roleName = user.MaQuyenNavigation?.TenQuyen ?? "User";
                    
                    // Tạo danh sách claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.TaiKhoan),
                        new Claim(ClaimTypes.NameIdentifier, user.MaTk.ToString()),
                        new Claim(ClaimTypes.Role, roleName)
                    };

                    // Tạo principal cho đăng nhập
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(claimsIdentity);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                    };

                    // Đăng nhập người dùng
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal,
                        authProperties);

                    // Lưu thông tin vào session
                    HttpContext.Session.SetInt32("UserId", user.MaTk);
                    HttpContext.Session.SetString("UserRole", roleName);
                    HttpContext.Session.SetString("Username", user.TaiKhoan);

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}