using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyRungPhongHo.Attributes;
using QuanLyRungPhongHo.Data;
using QuanLyRungPhongHo.Models;
using System.Security.Claims;

namespace QuanLyRungPhongHo.Controllers
{
    [Authorize]
    public class NhatKyBaoVeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 15;
        private static readonly List<string> LoaiSuViecOptions = new()
        {
            "Tuần tra",
            "Cháy rừng",
            "Chặt phá",
            "Săn bắt",
            "Lấn chiếm",
            "Vận chuyển",
            "Khác"
        };

        public NhatKyBaoVeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: NhatKyBaoVe
        [CheckPermission("NhatKyBaoVe.View")]
        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, int? maLo, int? maNV, string? loaiSuViec, string? keyword, int? pageNumber)
        {
            try
            {
                // Giữ lại filter để bind vào view
                ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
                ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");
                ViewData["MaLo"] = maLo;
                ViewData["MaNV"] = maNV;
                ViewData["LoaiSuViec"] = loaiSuViec;
                ViewData["Keyword"] = keyword;

                await LoadDropdownsAsync();

                var query = _context.NhatKyBaoVes
                    .Include(nk => nk.LoRung)!.ThenInclude(l => l.DanhMucThon)!.ThenInclude(t => t.DanhMucXa)
                    .Include(nk => nk.NhanSu)
                    .AsQueryable();

                if (fromDate.HasValue)
                {
                    var from = fromDate.Value.Date;
                    query = query.Where(nk => nk.NgayGhi >= from);
                }

                if (toDate.HasValue)
                {
                    var to = toDate.Value.Date.AddDays(1); // < next day
                    query = query.Where(nk => nk.NgayGhi < to);
                }

                if (maLo.HasValue)
                {
                    query = query.Where(nk => nk.MaLo == maLo.Value);
                }

                if (maNV.HasValue)
                {
                    query = query.Where(nk => nk.MaNV_GhiNhan == maNV.Value);
                }

                if (!string.IsNullOrWhiteSpace(loaiSuViec))
                {
                    query = query.Where(nk => nk.LoaiSuViec == loaiSuViec);
                }

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(nk =>
                        (nk.NoiDung != null && nk.NoiDung.Contains(keyword)) ||
                        (nk.LoaiSuViec != null && nk.LoaiSuViec.Contains(keyword)));
                }

                int totalRecords = await query.CountAsync();
                int currentPage = pageNumber ?? 1;
                var pagedData = await query
                    .OrderByDescending(nk => nk.NgayGhi)
                    .ThenByDescending(nk => nk.MaNK)
                    .Skip((currentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                ViewData["PageNumber"] = currentPage;
                ViewData["TotalPages"] = (int)Math.Ceiling(totalRecords / (double)PageSize);
                ViewData["TotalRecords"] = totalRecords;
                ViewBag.LoaiSuViec = LoaiSuViecOptions;

                return View(pagedData);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi tải dữ liệu: {ex.Message}";
                return View(new List<NhatKyBaoVe>());
            }
        }

        // GET: NhatKyBaoVe/Create
        [CheckPermission("NhatKyBaoVe.Create")]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();
            ViewBag.LoaiSuViec = LoaiSuViecOptions;

            var current = await GetCurrentNhanSuAsync();
            ViewBag.CurrentNhanSuName = current.HoTen ?? User.Identity?.Name;
            ViewBag.CurrentMaNV = current.MaNV;

            var model = new NhatKyBaoVe
            {
                NgayGhi = DateTime.Now
            };

            return View(model);
        }

        // POST: NhatKyBaoVe/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPermission("NhatKyBaoVe.Create")]
        public async Task<IActionResult> Create([Bind("NgayGhi,LoaiSuViec,NoiDung,MaLo,ToaDoGPS")] NhatKyBaoVe nhatKy)
        {
            var current = await GetCurrentNhanSuAsync();
            if (!current.MaNV.HasValue)
            {
                ModelState.AddModelError("", "Không xác định được nhân sự ghi nhận. Vui lòng đăng nhập lại hoặc liên hệ quản trị.");
            }

            if (string.IsNullOrWhiteSpace(nhatKy.LoaiSuViec))
            {
                ModelState.AddModelError("LoaiSuViec", "Vui lòng chọn loại sự việc.");
            }

            if (!nhatKy.MaLo.HasValue)
            {
                ModelState.AddModelError("MaLo", "Vui lòng chọn lô rừng.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    nhatKy.MaNV_GhiNhan = current.MaNV;
                    _context.Add(nhatKy);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thêm nhật ký thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi lưu nhật ký: {ex.Message}");
                }
            }

            await LoadDropdownsAsync();
            ViewBag.LoaiSuViec = LoaiSuViecOptions;
            ViewBag.CurrentNhanSuName = current.HoTen ?? User.Identity?.Name;
            ViewBag.CurrentMaNV = current.MaNV;
            return View(nhatKy);
        }

        // GET: NhatKyBaoVe/Edit/5
        [CheckPermission("NhatKyBaoVe.Edit")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhatKy = await _context.NhatKyBaoVes
                .Include(nk => nk.LoRung)!.ThenInclude(l => l.DanhMucThon)!.ThenInclude(t => t.DanhMucXa)
                .Include(nk => nk.NhanSu)
                .FirstOrDefaultAsync(nk => nk.MaNK == id);

            if (nhatKy == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy nhật ký.";
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdownsAsync();
            ViewBag.LoaiSuViec = LoaiSuViecOptions;
            ViewBag.NguoiGhiNhan = nhatKy.NhanSu?.HoTen;
            return View(nhatKy);
        }

        // POST: NhatKyBaoVe/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPermission("NhatKyBaoVe.Edit")]
        public async Task<IActionResult> Edit(int id, [Bind("MaNK,NgayGhi,LoaiSuViec,NoiDung,MaLo,ToaDoGPS")] NhatKyBaoVe nhatKy)
        {
            if (id != nhatKy.MaNK)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(nhatKy.LoaiSuViec))
            {
                ModelState.AddModelError("LoaiSuViec", "Vui lòng chọn loại sự việc.");
            }

            if (!nhatKy.MaLo.HasValue)
            {
                ModelState.AddModelError("MaLo", "Vui lòng chọn lô rừng.");
            }

            var entity = await _context.NhatKyBaoVes.FirstOrDefaultAsync(nk => nk.MaNK == id);
            if (entity == null)
            {
                TempData["ErrorMessage"] = "Nhật ký không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    entity.NgayGhi = nhatKy.NgayGhi;
                    entity.LoaiSuViec = nhatKy.LoaiSuViec;
                    entity.NoiDung = nhatKy.NoiDung;
                    entity.MaLo = nhatKy.MaLo;
                    entity.ToaDoGPS = nhatKy.ToaDoGPS;

                    _context.Update(entity);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật nhật ký thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NhatKyBaoVeExists(nhatKy.MaNK))
                    {
                        TempData["ErrorMessage"] = "Nhật ký không tồn tại.";
                        return RedirectToAction(nameof(Index));
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi cập nhật: {ex.Message}");
                }
            }

            await LoadDropdownsAsync();
            ViewBag.LoaiSuViec = LoaiSuViecOptions;
            return View(nhatKy);
        }

        // GET: NhatKyBaoVe/Delete/5
        [CheckPermission("NhatKyBaoVe.Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhatKy = await _context.NhatKyBaoVes
                .Include(nk => nk.LoRung)!.ThenInclude(l => l.DanhMucThon)!.ThenInclude(t => t.DanhMucXa)
                .Include(nk => nk.NhanSu)
                .FirstOrDefaultAsync(m => m.MaNK == id);

            if (nhatKy == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy nhật ký.";
                return RedirectToAction(nameof(Index));
            }

            return View(nhatKy);
        }

        // POST: NhatKyBaoVe/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [CheckPermission("NhatKyBaoVe.Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var nhatKy = await _context.NhatKyBaoVes.FindAsync(id);
                if (nhatKy == null)
                {
                    TempData["ErrorMessage"] = "Nhật ký không tồn tại.";
                    return RedirectToAction(nameof(Index));
                }

                _context.NhatKyBaoVes.Remove(nhatKy);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa nhật ký.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Helpers
        private async Task LoadDropdownsAsync()
        {
            var loRungs = await _context.LoRungs
                .Include(l => l.DanhMucThon)!.ThenInclude(t => t.DanhMucXa)
                .OrderBy(l => l.MaLo)
                .ToListAsync();
            ViewBag.LoRungs = loRungs.Select(l => new SelectListItem
            {
                Value = l.MaLo.ToString(),
                Text = $"Lô {l.MaLo} - TK {l.SoTieuKhu} / Khoảnh {l.SoKhoanh} / Lô {l.SoLo} - {(l.DanhMucThon != null ? l.DanhMucThon.TenThon : "")} - {(l.DanhMucThon?.DanhMucXa?.TenXa ?? "")}".Trim()
            }).ToList();

            var nhanSus = await _context.NhanSus.OrderBy(ns => ns.HoTen).ToListAsync();
            ViewBag.NhanSus = nhanSus;
        }

        private async Task<(int? MaNV, string? HoTen)> GetCurrentNhanSuAsync()
        {
            var maTkClaim = User.FindFirst("MaTK")?.Value;
            if (int.TryParse(maTkClaim, out int maTK))
            {
                var taiKhoan = await _context.TaiKhoans
                    .Include(t => t.NhanSu)
                    .FirstOrDefaultAsync(t => t.MaTK == maTK);
                return (taiKhoan?.MaNV, taiKhoan?.NhanSu?.HoTen ?? taiKhoan?.TenDangNhap);
            }
            return (null, null);
        }

        private async Task<bool> NhatKyBaoVeExists(int id)
        {
            return await _context.NhatKyBaoVes.AnyAsync(e => e.MaNK == id);
        }
    }
}
