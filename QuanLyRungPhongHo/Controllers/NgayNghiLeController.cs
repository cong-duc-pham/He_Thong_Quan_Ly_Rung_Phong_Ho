using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyRungPhongHo.Data;
using QuanLyRungPhongHo.Models;

namespace QuanLyRungPhongHo.Controllers
{
    [Authorize]
    public class NgayNghiLeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NgayNghiLeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: NgayNghiLe/Index
        public async Task<IActionResult> Index()
        {
            var ngayNghiLes = await _context.NgayNghiLes
                .OrderByDescending(n => n.NgayBatDau)
                .ToListAsync();
            return View(ngayNghiLes);
        }

        // GET: NgayNghiLe/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: NgayNghiLe/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NgayNghiLe ngayNghiLe)
        {
            try
            {
                // Kiểm tra các trường bắt buộc
                if (string.IsNullOrWhiteSpace(ngayNghiLe.TenNgayNghi))
                {
                    TempData["ErrorMessage"] = "Vui lòng nhập tên ngày nghỉ!";
                    return View(ngayNghiLe);
                }

                if (string.IsNullOrWhiteSpace(ngayNghiLe.LoaiNgayNghi))
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn loại ngày nghỉ!";
                    return View(ngayNghiLe);
                }

                // Allow any date for scheduling purposes
                
                if (ngayNghiLe.NgayKetThuc < ngayNghiLe.NgayBatDau)
                {
                    TempData["ErrorMessage"] = "Ngày kết thúc phải sau hoặc bằng ngày bắt đầu!";
                    return View(ngayNghiLe);
                }

                _context.NgayNghiLes.Add(ngayNghiLe);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "✓ Thêm ngày nghỉ lễ thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return View(ngayNghiLe);
            }
        }

        // GET: NgayNghiLe/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ngayNghiLe = await _context.NgayNghiLes.FindAsync(id);
            if (ngayNghiLe == null)
            {
                return NotFound();
            }

            return View(ngayNghiLe);
        }

        // POST: NgayNghiLe/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NgayNghiLe ngayNghiLe)
        {
            if (id != ngayNghiLe.MaNgayNghi)
            {
                return NotFound();
            }

            try
            {
                // Kiểm tra các trường bắt buộc
                if (string.IsNullOrWhiteSpace(ngayNghiLe.TenNgayNghi))
                {
                    TempData["ErrorMessage"] = "Vui lòng nhập tên ngày nghỉ!";
                    return View(ngayNghiLe);
                }

                if (string.IsNullOrWhiteSpace(ngayNghiLe.LoaiNgayNghi))
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn loại ngày nghỉ!";
                    return View(ngayNghiLe);
                }

                // Allow any date for scheduling purposes

                if (ngayNghiLe.NgayKetThuc < ngayNghiLe.NgayBatDau)
                {
                    TempData["ErrorMessage"] = "Ngày kết thúc phải sau hoặc bằng ngày bắt đầu!";
                    return View(ngayNghiLe);
                }

                _context.Update(ngayNghiLe);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "✓ Cập nhật ngày nghỉ lễ thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NgayNghiLeExists(ngayNghiLe.MaNgayNghi))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return View(ngayNghiLe);
            }
        }

        // POST: NgayNghiLe/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var ngayNghiLe = await _context.NgayNghiLes.FindAsync(id);
                if (ngayNghiLe == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy ngày nghỉ lễ!";
                    return RedirectToAction(nameof(Index));
                }

                _context.NgayNghiLes.Remove(ngayNghiLe);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "✓ Xóa ngày nghỉ lễ thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool NgayNghiLeExists(int id)
        {
            return _context.NgayNghiLes.Any(e => e.MaNgayNghi == id);
        }
    }
}
