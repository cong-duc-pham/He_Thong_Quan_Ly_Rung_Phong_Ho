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
                DonViCongTac = nhanSu?.DanhMucXa?.TenXa,
                MaTaiKhoan = taiKhoan?.MaTK,
                MaNhanVien = nhanSu?.MaNV
            };

            return View(viewModel);
        }
    }
}
