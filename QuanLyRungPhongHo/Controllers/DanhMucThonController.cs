using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyRungPhongHo.Attributes;
using QuanLyRungPhongHo.Data;
using QuanLyRungPhongHo.Models;

namespace QuanLyRungPhongHo.Controllers
{
    [Authorize]
    public class DanhMucThonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10;

        public DanhMucThonController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Tự động sinh mã thôn (T_<MaXa>_001, T_<MaXa>_002, ...)
        private async Task<string> GenerateMaThon(string maXa)
        {
            var lastMaThon = await _context.DanhMucThons
                .Where(t => t.MaXa == maXa && t.MaThon.StartsWith($"T_{maXa}_"))
                .OrderByDescending(t => t.MaThon)
                .Select(t => t.MaThon)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(lastMaThon))
            {
                return $"T_{maXa}_001";
            }

            // Lấy phần số từ mã cuối cùng
            var parts = lastMaThon.Split('_');
            if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
            {
                return $"T_{maXa}_{(lastNumber + 1):D3}";
            }

            return $"T_{maXa}_001";
        }

        // GET: DanhMucThon
        // Hiển thị danh sách Thôn với tìm kiếm theo Xã/Tên và phân trang
        [CheckPermission("DanhMucThon.View")]
        public async Task<IActionResult> Index(string searchXa, string searchString, int? pageNumber)
        {
            try
            {
                // Lưu filter để hiển thị lại
                ViewData["CurrentFilterXa"] = searchXa;
                ViewData["CurrentFilter"] = searchString;

                // Load danh sách xã cho dropdown filter
                ViewBag.DanhMucXa = await _context.DanhMucXas
                    .OrderBy(x => x.TenXa)
                    .ToListAsync();

                // Query cơ bản với Include Xã
                var query = _context.DanhMucThons
                    .Include(t => t.DanhMucXa)
                    .AsQueryable();

                // Lọc theo Xã
                if (!string.IsNullOrEmpty(searchXa))
                {
                    query = query.Where(t => t.MaXa == searchXa);
                }

                // Tìm kiếm theo tên thôn
                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(t => t.TenThon.Contains(searchString) || t.MaThon.Contains(searchString));
                }

                // Tính tổng số bản ghi
                int totalRecords = await query.CountAsync();
                ViewData["TotalRecords"] = totalRecords;

                // Phân trang
                int currentPage = pageNumber ?? 1;
                var pagedData = await query
                    .OrderBy(t => t.DanhMucXa.TenXa)
                    .ThenBy(t => t.TenThon)
                    .Skip((currentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                ViewData["PageNumber"] = currentPage;
                ViewData["TotalPages"] = (int)Math.Ceiling(totalRecords / (double)PageSize);

                return View(pagedData);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi tải dữ liệu: {ex.Message}";
                return View(new List<DanhMucThon>());
            }
        }

        // GET: DanhMucThon/Create
        [CheckPermission("DanhMucThon.Create")]
        public async Task<IActionResult> Create()
        {
            // Load danh sách xã cho dropdown
            ViewData["MaXa"] = new SelectList(
                await _context.DanhMucXas.OrderBy(x => x.TenXa).ToListAsync(),
                "MaXa",
                "TenXa"
            );
            return View();
        }

        // POST: DanhMucThon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPermission("DanhMucThon.Create")]
        public async Task<IActionResult> Create([Bind("TenThon,MaXa")] DanhMucThon danhMucThon)
        {
            try
            {
                // Tự động sinh mã thôn
                danhMucThon.MaThon = await GenerateMaThon(danhMucThon.MaXa);

                // Xóa lỗi validation của MaThon vì đã tự động sinh
                ModelState.Remove("MaThon");

                if (ModelState.IsValid)
                {
                    _context.Add(danhMucThon);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Thêm thôn '{danhMucThon.TenThon}' (Mã: {danhMucThon.MaThon}) thành công!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi khi thêm thôn: {ex.Message}");
            }

            // Reload dropdown nếu có lỗi
            ViewData["MaXa"] = new SelectList(
                await _context.DanhMucXas.OrderBy(x => x.TenXa).ToListAsync(),
                "MaXa",
                "TenXa",
                danhMucThon.MaXa
            );
            return View(danhMucThon);
        }

        // GET: DanhMucThon/Edit/5
        [CheckPermission("DanhMucThon.Edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var danhMucThon = await _context.DanhMucThons.FindAsync(id);
                if (danhMucThon == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thôn";
                    return RedirectToAction(nameof(Index));
                }

                ViewData["MaXa"] = new SelectList(
                    await _context.DanhMucXas.OrderBy(x => x.TenXa).ToListAsync(),
                    "MaXa",
                    "TenXa",
                    danhMucThon.MaXa
                );
                return View(danhMucThon);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: DanhMucThon/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPermission("DanhMucThon.Edit")]
        public async Task<IActionResult> Edit(string id, [Bind("MaThon,TenThon,MaXa")] DanhMucThon danhMucThon)
        {
            if (id != danhMucThon.MaThon)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(danhMucThon);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Cập nhật thôn '{danhMucThon.TenThon}' thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await DanhMucThonExists(danhMucThon.MaThon))
                    {
                        TempData["ErrorMessage"] = "Thôn không tồn tại";
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

            ViewData["MaXa"] = new SelectList(
                await _context.DanhMucXas.OrderBy(x => x.TenXa).ToListAsync(),
                "MaXa",
                "TenXa",
                danhMucThon.MaXa
            );
            return View(danhMucThon);
        }

        // GET: DanhMucThon/Delete/5
        [CheckPermission("DanhMucThon.Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var danhMucThon = await _context.DanhMucThons
                    .Include(t => t.DanhMucXa)
                    .Include(t => t.LoRungs)
                    .FirstOrDefaultAsync(m => m.MaThon == id);

                if (danhMucThon == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thôn";
                    return RedirectToAction(nameof(Index));
                }

                // Kiểm tra ràng buộc: không cho xóa nếu đã có lô rừng
                if (danhMucThon.LoRungs.Any())
                {
                    TempData["ErrorMessage"] = $"Không thể xóa thôn '{danhMucThon.TenThon}' vì đã có {danhMucThon.LoRungs.Count} lô rừng!";
                    return RedirectToAction(nameof(Index));
                }

                return View(danhMucThon);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: DanhMucThon/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [CheckPermission("DanhMucThon.Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var danhMucThon = await _context.DanhMucThons
                    .Include(t => t.LoRungs)
                    .FirstOrDefaultAsync(t => t.MaThon == id);

                if (danhMucThon == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thôn";
                    return RedirectToAction(nameof(Index));
                }

                // Kiểm tra lại ràng buộc
                if (danhMucThon.LoRungs.Any())
                {
                    TempData["ErrorMessage"] = $"Không thể xóa thôn '{danhMucThon.TenThon}' vì đã có lô rừng!";
                    return RedirectToAction(nameof(Index));
                }

                _context.DanhMucThons.Remove(danhMucThon);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Xóa thôn '{danhMucThon.TenThon}' thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // API: Kiểm tra thôn có tồn tại không
        private async Task<bool> DanhMucThonExists(string id)
        {
            return await _context.DanhMucThons.AnyAsync(e => e.MaThon == id);
        }

        // API: Lấy danh sách phân trang phục vụ AJAX
        [HttpGet]
        [CheckPermission("DanhMucThon.View")]
        public async Task<JsonResult> GetPaged(string? searchXa, string? searchString, int pageNumber = 1, int pageSize = PageSize)
        {
            try
            {
                pageNumber = pageNumber <= 0 ? 1 : pageNumber;
                pageSize = pageSize <= 0 ? PageSize : pageSize;

                var query = _context.DanhMucThons
                    .Include(t => t.DanhMucXa)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchXa))
                {
                    query = query.Where(t => t.MaXa == searchXa);
                }

                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    query = query.Where(t => t.TenThon.Contains(searchString) || t.MaThon.Contains(searchString));
                }

                var totalRecords = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

                var items = await query
                    .OrderBy(t => t.DanhMucXa.TenXa)
                    .ThenBy(t => t.TenThon)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new
                    {
                        t.MaThon,
                        t.TenThon,
                        t.MaXa,
                        TenXa = t.DanhMucXa.TenXa,
                        SoLoRung = t.LoRungs.Count()
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

        // API: Lấy danh sách thôn (dùng cho AJAX)
        [HttpGet]
        [CheckPermission("DanhMucThon.View")]
        public async Task<JsonResult> GetAll(string? maXa)
        {
            try
            {
                var query = _context.DanhMucThons.Include(t => t.DanhMucXa).AsQueryable();

                // Lọc theo Xã nếu có
                if (!string.IsNullOrEmpty(maXa))
                {
                    query = query.Where(t => t.MaXa == maXa);
                }

                var danhMucThon = await query
                    .Select(t => new
                    {
                        t.MaThon,
                        t.TenThon,
                        t.MaXa,
                        TenXa = t.DanhMucXa.TenXa,
                        SoLoRung = t.LoRungs.Count()
                    })
                    .ToListAsync();

                return Json(danhMucThon);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // API: Lấy danh sách thôn theo Xã (cho dropdown cascade)
        [HttpGet]
        [CheckPermission("DanhMucThon.View")]
        public async Task<JsonResult> GetByXa(string maXa)
        {
            try
            {
                var thons = await _context.DanhMucThons
                    .Where(t => t.MaXa == maXa)
                    .OrderBy(t => t.TenThon)
                    .Select(t => new
                    {
                        t.MaThon,
                        t.TenThon
                    })
                    .ToListAsync();

                return Json(thons);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}
