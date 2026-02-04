using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyRungPhongHo.Data;
using QuanLyRungPhongHo.Models;

namespace QuanLyRungPhongHo.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var maTkClaim = User.FindFirst("MaTK")?.Value;

            TaiKhoan? taiKhoan = null;
            NhanSu? nhanSu = null;

            if (int.TryParse(maTkClaim, out int maTk))
            {
                taiKhoan = await _context.TaiKhoans
                    .Include(t => t.NhanSu)
                        .ThenInclude(ns => ns!.DanhMucXa)
                    .FirstOrDefaultAsync(t => t.MaTK == maTk);

                nhanSu = taiKhoan?.NhanSu;
            }

            var viewModel = new SettingsViewModel
            {
                HoTen = User.FindFirst("HoTen")?.Value ?? nhanSu?.HoTen,
                TenDangNhap = User.Identity?.Name ?? taiKhoan?.TenDangNhap,
                ChucVu = User.FindFirst("ChucVu")?.Value ?? nhanSu?.ChucVu,
                Quyen = User.FindFirst(ClaimTypes.Role)?.Value ?? taiKhoan?.Quyen,
                SoDienThoai = nhanSu?.SDT,
                Email = nhanSu?.Email,
                GhiChuNoiBo = nhanSu?.GhiChuNoiBo,
                DonViCongTac = nhanSu?.DanhMucXa?.TenXa,
                MaTaiKhoan = taiKhoan?.MaTK,
                MaNhanVien = nhanSu?.MaNV
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SaveContactSettings([FromBody] SettingsViewModel model)
        {
            try
            {
                var maTkClaim = User.FindFirst("MaTK")?.Value;
                if (!int.TryParse(maTkClaim, out int maTk))
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin tài khoản" });
                }

                var taiKhoan = await _context.TaiKhoans
                    .Include(t => t.NhanSu)
                    .FirstOrDefaultAsync(t => t.MaTK == maTk);

                if (taiKhoan?.NhanSu == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin nhân sự" });
                }

                // Cập nhật thông tin
                taiKhoan.NhanSu.Email = model.Email?.Trim();
                taiKhoan.NhanSu.SDT = model.SoDienThoai?.Trim();
                taiKhoan.NhanSu.GhiChuNoiBo = model.GhiChuNoiBo?.Trim();

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã lưu cài đặt cá nhân thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}
