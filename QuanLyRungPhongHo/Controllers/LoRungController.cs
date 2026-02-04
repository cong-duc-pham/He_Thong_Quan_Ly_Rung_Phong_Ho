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
    public class LoRungController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 15;

        public LoRungController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: LoRung
        // Hiển thị danh sách Lô rừng với multi-filter và phân trang
        [CheckPermission("LoRung.View")]
        public async Task<IActionResult> Index(string searchXa, string searchThon, string searchLoai, string searchTrangThai, int? pageNumber)
        {
            try
            {
                // Lưu filter
                ViewData["CurrentFilterXa"] = searchXa;
                ViewData["CurrentFilterThon"] = searchThon;
                ViewData["CurrentFilterLoai"] = searchLoai;
                ViewData["CurrentFilterTrangThai"] = searchTrangThai;

                // Load danh sách cho dropdown filter
                ViewBag.DanhMucXa = await _context.DanhMucXas.OrderBy(x => x.TenXa).ToListAsync();
                ViewBag.DanhMucThon = await _context.DanhMucThons.OrderBy(t => t.TenThon).ToListAsync();

                // Danh sách loại rừng và trạng thái (cố định hoặc lấy từ DB)
                ViewBag.LoaiRung = new List<string>
                {
                    "Rừng phòng hộ đầu nguồn",
                    "Rừng phòng hộ ven biển",
                    "Rừng đặc dụng",
                    "Rừng sản xuất"
                };

                ViewBag.TrangThai = new List<string>
                {
                    "Rừng giàu",
                    "Rừng trung bình",
                    "Rừng nghèo",
                    "Đất trống"
                };

                // Query với Include
                var query = _context.LoRungs
                    .Include(l => l.DanhMucThon)
                    .ThenInclude(t => t.DanhMucXa)
                    .AsQueryable();

                // Lọc theo Thôn (ưu tiên)
                if (!string.IsNullOrEmpty(searchThon))
                {
                    query = query.Where(l => l.MaThon == searchThon);
                }
                // Lọc theo Xã (nếu không có Thôn)
                else if (!string.IsNullOrEmpty(searchXa))
                {
                    query = query.Where(l => l.DanhMucThon != null && l.DanhMucThon.MaXa == searchXa);
                }

                // Lọc theo Loại rừng
                if (!string.IsNullOrEmpty(searchLoai))
                {
                    query = query.Where(l => l.LoaiRung == searchLoai);
                }

                // Lọc theo Trạng thái
                if (!string.IsNullOrEmpty(searchTrangThai))
                {
                    query = query.Where(l => l.TrangThai == searchTrangThai);
                }

                // Tính tổng số bản ghi
                int totalRecords = await query.CountAsync();
                ViewData["TotalRecords"] = totalRecords;

                // Phân trang
                int currentPage = pageNumber ?? 1;
                var pagedData = await query
                    .OrderBy(l => l.MaLo)
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
                return View(new List<LoRung>());
            }
        }

        // GET: LoRung/Create
        [CheckPermission("LoRung.Create")]
        public async Task<IActionResult> Create()
        {
            // Load dropdown Xã
            ViewBag.DanhMucXa = await _context.DanhMucXas.OrderBy(x => x.TenXa).ToListAsync();

            // Dropdown Loại rừng
            ViewBag.LoaiRung = new SelectList(new List<string>
            {
                "Rừng phòng hộ đầu nguồn",
                "Rừng phòng hộ ven biển",
                "Rừng đặc dụng",
                "Rừng sản xuất"
            });

            // Dropdown Trạng thái
            ViewBag.TrangThai = new SelectList(new List<string>
            {
                "Rừng giàu",
                "Rừng trung bình",
                "Rừng nghèo",
                "Đất trống"
            });

            return View();
        }

        // POST: LoRung/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPermission("LoRung.Create")]
        public async Task<IActionResult> Create([Bind("SoTieuKhu,SoKhoanh,SoLo,MaThon,DienTich,LoaiRung,TrangThai")] LoRung loRung)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(loRung);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Thêm lô rừng {loRung.SoTieuKhu}-{loRung.SoKhoanh}-{loRung.SoLo} thành công!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi khi thêm lô rừng: {ex.Message}");
            }

            // Reload dropdown nếu có lỗi
            ViewBag.DanhMucXa = await _context.DanhMucXas.OrderBy(x => x.TenXa).ToListAsync();
            ViewBag.LoaiRung = new SelectList(new List<string>
            {
                "Rừng phòng hộ đầu nguồn",
                "Rừng phòng hộ ven biển",
                "Rừng đặc dụng",
                "Rừng sản xuất"
            });
            ViewBag.TrangThai = new SelectList(new List<string>
            {
                "Rừng giàu",
                "Rừng trung bình",
                "Rừng nghèo",
                "Đất trống"
            });

            return View(loRung);
        }

        // GET: LoRung/Edit/5
        [CheckPermission("LoRung.Edit")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var loRung = await _context.LoRungs
                    .Include(l => l.DanhMucThon)
                    .FirstOrDefaultAsync(l => l.MaLo == id);

                if (loRung == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy lô rừng";
                    return RedirectToAction(nameof(Index));
                }

                // Load dropdown
                ViewBag.DanhMucXa = await _context.DanhMucXas.OrderBy(x => x.TenXa).ToListAsync();
                ViewBag.DanhMucThon = await _context.DanhMucThons
                    .Where(t => t.MaXa == loRung.DanhMucThon.MaXa)
                    .OrderBy(t => t.TenThon)
                    .ToListAsync();

                ViewBag.LoaiRung = new SelectList(new List<string>
                {
                    "Rừng phòng hộ đầu nguồn",
                    "Rừng phòng hộ ven biển",
                    "Rừng đặc dụng",
                    "Rừng sản xuất"
                }, loRung.LoaiRung);

                ViewBag.TrangThai = new SelectList(new List<string>
                {
                    "Rừng giàu",
                    "Rừng trung bình",
                    "Rừng nghèo",
                    "Đất trống"
                }, loRung.TrangThai);

                return View(loRung);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: LoRung/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPermission("LoRung.Edit")]
        public async Task<IActionResult> Edit(int id, [Bind("MaLo,SoTieuKhu,SoKhoanh,SoLo,MaThon,DienTich,LoaiRung,TrangThai")] LoRung loRung)
        {
            if (id != loRung.MaLo)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(loRung);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Cập nhật lô rừng thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await LoRungExists(loRung.MaLo))
                    {
                        TempData["ErrorMessage"] = "Lô rừng không tồn tại";
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

            // Reload dropdown nếu có lỗi
            ViewBag.DanhMucXa = await _context.DanhMucXas.OrderBy(x => x.TenXa).ToListAsync();
            ViewBag.LoaiRung = new SelectList(new List<string>
            {
                "Rừng phòng hộ đầu nguồn",
                "Rừng phòng hộ ven biển",
                "Rừng đặc dụng",
                "Rừng sản xuất"
            });
            ViewBag.TrangThai = new SelectList(new List<string>
            {
                "Rừng giàu",
                "Rừng trung bình",
                "Rừng nghèo",
                "Đất trống"
            });

            return View(loRung);
        }

        // GET: LoRung/Delete/5
        [CheckPermission("LoRung.Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var loRung = await _context.LoRungs
                    .Include(l => l.DanhMucThon)
                    .ThenInclude(t => t.DanhMucXa)
                    .FirstOrDefaultAsync(m => m.MaLo == id);

                if (loRung == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy lô rừng";
                    return RedirectToAction(nameof(Index));
                }

                return View(loRung);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: LoRung/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [CheckPermission("LoRung.Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var loRung = await _context.LoRungs.FindAsync(id);
                if (loRung == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy lô rừng";
                    return RedirectToAction(nameof(Index));
                }

                _context.LoRungs.Remove(loRung);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa lô rừng thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // API: Kiểm tra lô rừng có tồn tại không
        private async Task<bool> LoRungExists(int id)
        {
            return await _context.LoRungs.AnyAsync(e => e.MaLo == id);
        }

        // API: Lấy danh sách lô rừng phân trang (AJAX)
        [HttpGet]
        [CheckPermission("LoRung.View")]
        public async Task<JsonResult> GetPaged(string? searchXa, string? searchThon, string? searchLoai, string? searchTrangThai, int pageNumber = 1, int pageSize = PageSize)
        {
            try
            {
                pageNumber = pageNumber <= 0 ? 1 : pageNumber;
                pageSize = pageSize <= 0 ? PageSize : pageSize;

                var query = _context.LoRungs
                    .Include(l => l.DanhMucThon)
                    .ThenInclude(t => t.DanhMucXa)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchThon))
                {
                    query = query.Where(l => l.MaThon == searchThon);
                }
                else if (!string.IsNullOrEmpty(searchXa))
                {
                    query = query.Where(l => l.DanhMucThon != null && l.DanhMucThon.MaXa == searchXa);
                }

                if (!string.IsNullOrEmpty(searchLoai))
                {
                    query = query.Where(l => l.LoaiRung == searchLoai);
                }

                if (!string.IsNullOrEmpty(searchTrangThai))
                {
                    query = query.Where(l => l.TrangThai == searchTrangThai);
                }

                var totalRecords = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

                var items = await query
                    .OrderBy(l => l.MaLo)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(l => new
                    {
                        l.MaLo,
                        l.SoTieuKhu,
                        l.SoKhoanh,
                        l.SoLo,
                        l.MaThon,
                        TenThon = l.DanhMucThon != null ? l.DanhMucThon.TenThon : string.Empty,
                        TenXa = l.DanhMucThon != null && l.DanhMucThon.DanhMucXa != null ? l.DanhMucThon.DanhMucXa.TenXa : string.Empty,
                        l.DienTich,
                        l.LoaiRung,
                        l.TrangThai
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

        // API: Lấy danh sách lô rừng (dùng cho AJAX)
        [HttpGet]
        [CheckPermission("LoRung.View")]
        public async Task<JsonResult> GetAll(string? maXa, string? maThon, string? loaiRung, string? trangThai)
        {
            try
            {
                var query = _context.LoRungs
                    .Include(l => l.DanhMucThon)
                    .ThenInclude(t => t.DanhMucXa)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(maThon))
                {
                    query = query.Where(l => l.MaThon == maThon);
                }
                else if (!string.IsNullOrEmpty(maXa))
                {
                    query = query.Where(l => l.DanhMucThon != null && l.DanhMucThon.MaXa == maXa);
                }

                if (!string.IsNullOrEmpty(loaiRung))
                {
                    query = query.Where(l => l.LoaiRung == loaiRung);
                }

                if (!string.IsNullOrEmpty(trangThai))
                {
                    query = query.Where(l => l.TrangThai == trangThai);
                }

                var loRung = await query
                    .Select(l => new
                    {
                        l.MaLo,
                        l.SoTieuKhu,
                        l.SoKhoanh,
                        l.SoLo,
                        l.MaThon,
                        TenThon = l.DanhMucThon != null ? l.DanhMucThon.TenThon : "",
                        TenXa = l.DanhMucThon != null && l.DanhMucThon.DanhMucXa != null ? l.DanhMucThon.DanhMucXa.TenXa : "",
                        l.DienTich,
                        l.LoaiRung,
                        l.TrangThai
                    })
                    .ToListAsync();

                return Json(loRung);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // API: Lấy chi tiết lô rừng
        [HttpGet]
        [CheckPermission("LoRung.View")]
        public async Task<JsonResult> GetDetails(int id)
        {
            try
            {
                var loRung = await _context.LoRungs
                    .Include(l => l.DanhMucThon)
                    .ThenInclude(t => t.DanhMucXa)
                    .Where(l => l.MaLo == id)
                    .Select(l => new
                    {
                        l.MaLo,
                        l.SoTieuKhu,
                        l.SoKhoanh,
                        l.SoLo,
                        l.MaThon,
                        TenThon = l.DanhMucThon != null ? l.DanhMucThon.TenThon : "",
                        MaXa = l.DanhMucThon != null ? l.DanhMucThon.MaXa : "",
                        TenXa = l.DanhMucThon != null && l.DanhMucThon.DanhMucXa != null ? l.DanhMucThon.DanhMucXa.TenXa : "",
                        l.DienTich,
                        l.LoaiRung,
                        l.TrangThai
                    })
                    .FirstOrDefaultAsync();

                return Json(loRung);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}
