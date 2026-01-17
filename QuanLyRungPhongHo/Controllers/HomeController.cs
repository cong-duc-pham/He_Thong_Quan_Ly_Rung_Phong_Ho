using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace QuanLyRungPhongHo.Controllers
{
    [Authorize] // Yêu cầu đăng nhập cho toàn bộ controller
    public class HomeController : Controller
    {
        // GET: Home/Index - Tất cả user đã đăng nhập đều truy cập được
        public IActionResult Index()
        {
            // Lấy thông tin từ Claims
            ViewBag.TenDangNhap = User.Identity?.Name;
            ViewBag.HoTen = User.FindFirst("HoTen")?.Value;
            ViewBag.ChucVu = User.FindFirst("ChucVu")?.Value;
            ViewBag.Quyen = User.FindFirst(ClaimTypes.Role)?.Value;

            // Hoặc lấy từ Session
            ViewBag.MaTK = HttpContext.Session.GetInt32("MaTK");

            return View();
        }

        // GET: Home/AdminOnly - Chỉ Admin mới truy cập được
        [Authorize(Roles = "Admin")]
        public IActionResult AdminOnly()
        {
            return View();
        }

        // GET: Home/NhanVienThon - Nhân viên thôn truy cập được
        [Authorize(Roles = "NhanVien_Thon,Admin")]
        public IActionResult NhanVienThonPage()
        {
            return View();
        }

        // GET: Home/About
        public IActionResult About()
        {
            ViewBag.Message = "Trang giới thiệu về hệ thống quản lý rừng phòng hộ.";
            return View();
        }

        // GET: Home/Contact
        public IActionResult Contact()
        {
            ViewBag.Message = "Thông tin liên hệ.";
            return View();
        }

        // GET: Home/Error
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}