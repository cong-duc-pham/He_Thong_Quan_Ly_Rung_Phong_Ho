using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyRungPhongHo.Data;
using QuanLyRungPhongHo.Models;
using QuanLyRungPhongHo.Services;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyRungPhongHo.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IOtpService _otpService;

        public AccountController(ApplicationDbContext context, IOtpService otpService)
        {
            _context = context;
            _otpService = otpService;
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

        // ==================== PASSWORD RESET ====================

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return await Task.FromResult(View());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kiểm tra email tồn tại trong hệ thống
            var nhanSu = await _context.NhanSus
                .FirstOrDefaultAsync(n => n.Email == model.Email);

            if (nhanSu == null)
            {
                ModelState.AddModelError("", "Email không tồn tại trong hệ thống.");
                return View(model);
            }

            // Kiểm tra tài khoản liên kết
            var taiKhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(t => t.MaNV == nhanSu.MaNV);

            if (taiKhoan == null)
            {
                ModelState.AddModelError("", "Tài khoản chưa được kích hoạt.");
                return View(model);
            }

            // Gửi OTP qua email
            await _otpService.GenerateOtpAsync(model.Email, isEmail: true);

            // Lưu vào session để xác nhận
            HttpContext.Session.SetString("ResetEmail", model.Email);
            HttpContext.Session.SetInt32("ResetAttempt", 0);

            TempData["SuccessMessage"] = $"Mã OTP đã được gửi tới email {model.Email}";

            return RedirectToAction("VerifyOtp", new { email = model.Email });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }

            var model = new OtpVerificationViewModel { Email = email };
            ViewBag.RemainingTime = _otpService.GetOtpRemainingTime(email);
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(OtpVerificationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.RemainingTime = _otpService.GetOtpRemainingTime(model.Email);
                return View(model);
            }

            // Xác nhận OTP
            if (!_otpService.VerifyOtp(model.Email, model.OtpCode))
            {
                ModelState.AddModelError("OtpCode", "Mã OTP không đúng hoặc đã hết hạn.");
                ViewBag.RemainingTime = _otpService.GetOtpRemainingTime(model.Email);
                return View(model);
            }

            // Lưu vào session để bước tiếp theo
            HttpContext.Session.SetString("VerifiedEmail", model.Email);

            return RedirectToAction("ResetPassword", new { email = model.Email });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email)
        {
            var verifiedEmail = HttpContext.Session.GetString("VerifiedEmail");
            if (string.IsNullOrEmpty(verifiedEmail) || verifiedEmail != email)
            {
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPasswordViewModel { Email = email };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var verifiedEmail = HttpContext.Session.GetString("VerifiedEmail");
            if (string.IsNullOrEmpty(verifiedEmail) || verifiedEmail != model.Email)
            {
                return RedirectToAction("ForgotPassword");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Tìm nhân sự từ email
                var nhanSu = await _context.NhanSus
                    .FirstOrDefaultAsync(n => n.Email == model.Email);

                if (nhanSu == null)
                {
                    return RedirectToAction("ForgotPassword");
                }

                // Tìm tài khoản
                var taiKhoan = await _context.TaiKhoans
                    .FirstOrDefaultAsync(t => t.MaNV == nhanSu.MaNV);

                if (taiKhoan == null)
                {
                    return RedirectToAction("ForgotPassword");
                }

                // Hash mật khẩu mới
                string hashedPassword = HashPassword(model.MatKhauMoi);

                // Cập nhật mật khẩu
                taiKhoan.MatKhau = hashedPassword;
                _context.TaiKhoans.Update(taiKhoan);
                await _context.SaveChangesAsync();

                // Xóa session xác nhận
                HttpContext.Session.Remove("VerifiedEmail");
                HttpContext.Session.Remove("ResetEmail");

                TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                return View(model);
            }
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