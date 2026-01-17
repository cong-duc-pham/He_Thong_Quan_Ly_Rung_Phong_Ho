using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyRungPhongHo.Data;
using QuanLyRungPhongHo.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyRungPhongHo.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            // Nếu đã đăng nhập rồi thì đá về trang chủ
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // 1. Mã hóa mật khẩu người dùng nhập vào để so sánh
                string inputHash = HashPassword(model.MatKhau);

                // 2. Tìm tài khoản trong DB
                var taiKhoan = await _context.TaiKhoans
                    .Include(t => t.NhanSu) // Join bảng nhân sự để lấy tên hiển thị
                    .FirstOrDefaultAsync(t => t.TenDangNhap == model.TenDangNhap);

                if (taiKhoan != null)
                {
                    // KIỂM TRA MẬT KHẨU
                    // Lưu ý: So sánh mã hash. 
                    // Nếu dữ liệu cũ bạn nhập tay là "123" (chưa hash) thì mở comment dòng '||' để test tạm
                    bool isPasswordCorrect = (taiKhoan.MatKhau == inputHash);
                    // || taiKhoan.MatKhau == model.MatKhau; // (Bật dòng này nếu muốn chấp nhận cả pass chưa mã hóa lúc test)

                    if (isPasswordCorrect)
                    {
                        // 3. Tạo Claims (Thông tin định danh)
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, taiKhoan.TenDangNhap),
                            new Claim(ClaimTypes.Role, taiKhoan.Quyen ?? "NhanVien"), // Mặc định nếu null
                            new Claim("MaTK", taiKhoan.MaTK.ToString()),
                            new Claim("HoTen", taiKhoan.NhanSu?.HoTen ?? "Người dùng"),
                            new Claim("Avatar", "default-avatar.png") // Ví dụ thêm ảnh
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = model.RememberMe,
                            ExpiresUtc = model.RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddHours(1)
                        };

                        // 4. Đăng nhập (Ghi Cookie)
                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        // 5. Lưu Session (Nếu dự án bắt buộc dùng Session)
                        HttpContext.Session.SetString("UserName", taiKhoan.TenDangNhap);
                        HttpContext.Session.SetString("FullName", taiKhoan.NhanSu?.HoTen ?? "");

                        // 6. Điều hướng
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        return RedirectToAction("Index", "Home");
                    }
                }

                // Nếu sai
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        public IActionResult AccessDenied()
        {
            return View(); // Tạo View AccessDenied.cshtml thông báo không có quyền
        }

        // Hàm mã hóa SHA256
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}