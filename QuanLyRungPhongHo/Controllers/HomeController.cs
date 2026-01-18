using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using QuanLyRungPhongHo.Data;
using QuanLyRungPhongHo.Models;
using Microsoft.EntityFrameworkCore;

namespace QuanLyRungPhongHo.Controllers
{
    [Authorize] // Yêu cầu đăng nhập cho toàn bộ controller
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Home/Index - Tất cả user đã đăng nhập đều truy cập được
        public async Task<IActionResult> Index()
        {
            // Lấy MaNV từ Claims
            var maTKClaim = User.FindFirst("MaTK")?.Value;
            int? maNV = null;
            if (int.TryParse(maTKClaim, out int maTK))
            {
                var taiKhoan = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaTK == maTK);
                maNV = taiKhoan?.MaNV;
            }

            // Tính toán KPI
            var viewModel = new HomeViewModel
            {
                // Lấy thông tin người dùng từ Claims
                TenDangNhap = User.Identity?.Name,
                HoTen = User.FindFirst("HoTen")?.Value,
                ChucVu = User.FindFirst("ChucVu")?.Value,
                Quyen = User.FindFirst(ClaimTypes.Role)?.Value,
                MaNV = maNV,

                // KPI 1: Tổng diện tích rừng
                TongDienTich = await _context.LoRungs
                    .Where(l => l.DienTich.HasValue)
                    .SumAsync(l => l.DienTich.Value),

                // KPI 2: Tổng số lô rừng
                TongSoLoRung = await _context.LoRungs.CountAsync(),

                // KPI 3: Tổng số nhân sự
                TongSoNhanSu = await _context.NhanSus.CountAsync(),

                // KPI 4: Số cảnh báo trong 7 ngày (không bao gồm "Tuần tra")
                SoCanhBao7Ngay = await _context.NhatKyBaoVes
                    .Where(nk => nk.NgayGhi >= DateTime.Now.AddDays(-7)
                              && nk.LoaiSuViec != null
                              && nk.LoaiSuViec != "Tuần tra")
                    .CountAsync()
            };

            // CẢNH BÁO KHẨN CẤP - Top 10 sự kiện gần nhất (không phải tuần tra)
            try
            {
                var canhBaoQuery = await _context.NhatKyBaoVes
                    .Include(nk => nk.LoRung)
                        .ThenInclude(lr => lr!.DanhMucThon)
                        .ThenInclude(dt => dt!.DanhMucXa)
                    .Include(nk => nk.NhanSu)
                    .Where(nk => nk.LoaiSuViec != null && nk.LoaiSuViec != "Tuần tra")
                    .OrderByDescending(nk => nk.NgayGhi)
                    .Take(10)
                    .ToListAsync();

                viewModel.DanhSachCanhBao = canhBaoQuery.Select(nk => new CanhBaoKhanCap
                {
                    MaNK = nk.MaNK,
                    NgayGhi = nk.NgayGhi,
                    LoaiSuViec = nk.LoaiSuViec ?? "",
                    NoiDung = nk.NoiDung ?? "",
                    ViTri = nk.LoRung != null
                        ? $"TK {nk.LoRung.SoTieuKhu} - Khoảnh {nk.LoRung.SoKhoanh} - Lô {nk.LoRung.SoLo}"
                        : "Chưa xác định",
                    TenXa = nk.LoRung?.DanhMucThon?.DanhMucXa?.TenXa ?? "",
                    NguoiGhiNhan = nk.NhanSu?.HoTen ?? "",
                    MucDoClass = nk.LoaiSuViec == "Cháy rừng" ? "danger" :
                                 nk.LoaiSuViec == "Chặt phá" ? "danger" :
                                 nk.LoaiSuViec == "Săn bắt" ? "warning" : "info"
                }).ToList();
            }
            catch (Exception)
            {
                viewModel.DanhSachCanhBao = new List<CanhBaoKhanCap>();
            }

            // CÔNG VIỆC CỦA TÔI - Nếu có MaNV
            if (maNV.HasValue)
            {
                try
                {
                    var nhanSu = await _context.NhanSus
                        .Include(ns => ns.DanhMucXa)
                        .FirstOrDefaultAsync(ns => ns.MaNV == maNV.Value);

                    if (nhanSu != null && nhanSu.MaXa != null)
                    {
                        // Số lô rừng và diện tích phụ trách (theo xã)
                        var loRungPhuTrach = await _context.LoRungs
                            .Include(l => l.DanhMucThon)
                            .Where(l => l.DanhMucThon != null && l.DanhMucThon.MaXa == nhanSu.MaXa)
                            .ToListAsync();

                        viewModel.SoLoRungPhuTrach = loRungPhuTrach.Count;
                        viewModel.DienTichPhuTrach = loRungPhuTrach
                            .Where(l => l.DienTich.HasValue)
                            .Sum(l => l.DienTich!.Value);
                    }

                    // Số nhật ký đã ghi
                    viewModel.SoNhatKyDaGhi = await _context.NhatKyBaoVes
                        .Where(nk => nk.MaNV_GhiNhan == maNV.Value)
                        .CountAsync();

                    // Nhật ký gần đây của tôi (5 bản ghi)
                    var nhatKyQuery = await _context.NhatKyBaoVes
                        .Include(nk => nk.LoRung)
                        .Where(nk => nk.MaNV_GhiNhan == maNV.Value)
                        .OrderByDescending(nk => nk.NgayGhi)
                        .Take(5)
                        .ToListAsync();

                    viewModel.NhatKyGanDay = nhatKyQuery.Select(nk => new NhatKyGanDay
                    {
                        NgayGhi = nk.NgayGhi,
                        LoaiSuViec = nk.LoaiSuViec ?? "",
                        NoiDung = nk.NoiDung ?? "",
                        ViTri = nk.LoRung != null
                            ? $"TK {nk.LoRung.SoTieuKhu} - Khoảnh {nk.LoRung.SoKhoanh}"
                            : ""
                    }).ToList();
                }
                catch (Exception)
                {
                    // Nếu có lỗi, để giá trị mặc định
                }
            }

            // Lưu MaTK vào Session (nếu cần)
            ViewBag.MaTK = HttpContext.Session.GetInt32("MaTK");

            return View(viewModel);
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