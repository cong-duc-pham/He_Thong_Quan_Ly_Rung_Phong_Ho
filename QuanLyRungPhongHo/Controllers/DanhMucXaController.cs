using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyRungPhongHo.Data;
using QuanLyRungPhongHo.Models;
using QuanLyRungPhongHo.Attributes;

namespace QuanLyRungPhongHo.Controllers
{
    [Authorize] // Yêu cầu đăng nhập
    public class DanhMucXaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10; // Số bản ghi trên 1 trang

        public DanhMucXaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DanhMucXa
        // Hiển thị danh sách Xã với tìm kiếm và phân trang
        [CheckPermission("DanhMucXa.View")]
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            try
            {
                // Lưu từ khóa tìm kiếm để hiển thị lại trong view
                ViewData["CurrentFilter"] = searchString;

                // Query cơ bản
                var query = _context.DanhMucXas.AsQueryable();

                // Tìm kiếm theo tên xã
                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(x => x.TenXa.Contains(searchString) || x.MaXa.Contains(searchString));
                }

                // Tính tổng số bản ghi
                int totalRecords = await query.CountAsync();
                ViewData["TotalRecords"] = totalRecords;

                // Phân trang
                int currentPage = pageNumber ?? 1;
                var pagedData = await query
                    .OrderBy(x => x.MaXa)
                    .Skip((currentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                // Tính toán thông tin phân trang
                ViewData["PageNumber"] = currentPage;
                ViewData["TotalPages"] = (int)Math.Ceiling(totalRecords / (double)PageSize);

                return View(pagedData);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi tải dữ liệu: {ex.Message}";
                return View(new List<DanhMucXa>());
            }
        }

        // GET: DanhMucXa/Create
        [CheckPermission("DanhMucXa.Create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: DanhMucXa/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPermission("DanhMucXa.Create")]
        public async Task<IActionResult> Create([Bind("MaXa,TenXa")] DanhMucXa danhMucXa)
        {
            try
            {
                // Kiểm tra trùng Mã Xã
                if (await _context.DanhMucXas.AnyAsync(x => x.MaXa == danhMucXa.MaXa))
                {
                    ModelState.AddModelError("MaXa", "Mã xã đã tồn tại trong hệ thống");
                    return View(danhMucXa);
                }

                if (ModelState.IsValid)
                {
                    _context.Add(danhMucXa);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Thêm xã '{danhMucXa.TenXa}' thành công!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi khi thêm xã: {ex.Message}");
            }

            return View(danhMucXa);
        }

        // GET: DanhMucXa/Edit/5
        [CheckPermission("DanhMucXa.Edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var danhMucXa = await _context.DanhMucXas.FindAsync(id);
                if (danhMucXa == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy xã";
                    return RedirectToAction(nameof(Index));
                }
                return View(danhMucXa);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: DanhMucXa/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPermission("DanhMucXa.Edit")]
        public async Task<IActionResult> Edit(string id, [Bind("MaXa,TenXa")] DanhMucXa danhMucXa)
        {
            if (id != danhMucXa.MaXa)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(danhMucXa);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Cập nhật xã '{danhMucXa.TenXa}' thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await DanhMucXaExists(danhMucXa.MaXa))
                    {
                        TempData["ErrorMessage"] = "Xã không tồn tại";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi cập nhật: {ex.Message}");
                }
            }
            return View(danhMucXa);
        }

        // GET: DanhMucXa/Delete/5
        [CheckPermission("DanhMucXa.Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var danhMucXa = await _context.DanhMucXas
                    .Include(x => x.DanhMucThons)
                    .FirstOrDefaultAsync(m => m.MaXa == id);

                if (danhMucXa == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy xã";
                    return RedirectToAction(nameof(Index));
                }

                // Kiểm tra ràng buộc: không cho xóa nếu đã có thôn
                if (danhMucXa.DanhMucThons.Any())
                {
                    TempData["ErrorMessage"] = $"Không thể xóa xã '{danhMucXa.TenXa}' vì đã có {danhMucXa.DanhMucThons.Count} thôn/bản trực thuộc!";
                    return RedirectToAction(nameof(Index));
                }

                return View(danhMucXa);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: DanhMucXa/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [CheckPermission("DanhMucXa.Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var danhMucXa = await _context.DanhMucXas
                    .Include(x => x.DanhMucThons)
                    .FirstOrDefaultAsync(x => x.MaXa == id);

                if (danhMucXa == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy xã";
                    return RedirectToAction(nameof(Index));
                }

                // Kiểm tra lại ràng buộc
                if (danhMucXa.DanhMucThons.Any())
                {
                    TempData["ErrorMessage"] = $"Không thể xóa xã '{danhMucXa.TenXa}' vì đã có thôn/bản trực thuộc!";
                    return RedirectToAction(nameof(Index));
                }

                _context.DanhMucXas.Remove(danhMucXa);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Xóa xã '{danhMucXa.TenXa}' thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // API: Kiểm tra xã có tồn tại không
        private async Task<bool> DanhMucXaExists(string id)
        {
            return await _context.DanhMucXas.AnyAsync(e => e.MaXa == id);
        }

        // API: Lấy danh sách phân trang phục vụ AJAX
        [HttpGet]
        public async Task<JsonResult> GetPaged(string? searchString, int pageNumber = 1, int pageSize = PageSize)
        {
            try
            {
                pageNumber = pageNumber <= 0 ? 1 : pageNumber;
                pageSize = pageSize <= 0 ? PageSize : pageSize;

                var query = _context.DanhMucXas.AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    query = query.Where(x => x.TenXa.Contains(searchString) || x.MaXa.Contains(searchString));
                }

                var totalRecords = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

                var items = await query
                    .OrderBy(x => x.MaXa)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new
                    {
                        x.MaXa,
                        x.TenXa,
                        SoThon = x.DanhMucThons.Count()
                    })
                    .ToListAsync();

                return Json(new
                {
                    items,
                    pagination = new
                    {
                        pageNumber,
                        pageSize,
                        totalPages,
                        totalRecords
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // API: Lấy danh sách xã (dùng cho AJAX)
        [HttpGet]
        public async Task<JsonResult> GetAll()
        {
            try
            {
                var danhMucXa = await _context.DanhMucXas
                    .Select(x => new
                    {
                        x.MaXa,
                        x.TenXa,
                        SoThon = x.DanhMucThons.Count()
                    })
                    .ToListAsync();
                return Json(danhMucXa);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}
