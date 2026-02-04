using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyRungPhongHo.Attributes;
using QuanLyRungPhongHo.Data;

namespace QuanLyRungPhongHo.Controllers
{
    [Authorize]
    public class SinhVatController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SinhVatController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SinhVat
        [CheckPermission("SinhVat.View")]
        public async Task<IActionResult> Index()
        {
            var sinhVats = await _context.SinhVats
                .Include(s => s.LoRung)
                .ToListAsync();
            return View(sinhVats);
        }

        // GET: SinhVat/Create
        [CheckPermission("SinhVat.Create")]
        public async Task<IActionResult> Create()
        {
            await LoadLoRungSelectList();
            return View();
        }

        // POST: SinhVat/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPermission("SinhVat.Create")]
        public async Task<IActionResult> Create(SinhVat sinhVat)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(sinhVat);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Thêm sinh vật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }

            await LoadLoRungSelectList(sinhVat.MaLo);
            return View(sinhVat);
        }

        // GET: SinhVat/Edit/5
        [CheckPermission("SinhVat.Edit")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sinhVat = await _context.SinhVats.FindAsync(id);
            if (sinhVat == null)
            {
                return NotFound();
            }

            await LoadLoRungSelectList(sinhVat.MaLo);
            return View(sinhVat);
        }

        // POST: SinhVat/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckPermission("SinhVat.Edit")]
        public async Task<IActionResult> Edit(int id, SinhVat sinhVat)
        {
            if (id != sinhVat.MaSV)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sinhVat);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật sinh vật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SinhVatExists(sinhVat.MaSV))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }

            await LoadLoRungSelectList(sinhVat.MaLo);
            return View(sinhVat);
        }

        // GET: SinhVat/Delete/5
        [CheckPermission("SinhVat.Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sinhVat = await _context.SinhVats
                .Include(s => s.LoRung)
                .FirstOrDefaultAsync(m => m.MaSV == id);

            if (sinhVat == null)
            {
                return NotFound();
            }

            return View(sinhVat);
        }

        // POST: SinhVat/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [CheckPermission("SinhVat.Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var sinhVat = await _context.SinhVats.FindAsync(id);
                if (sinhVat != null)
                {
                    _context.SinhVats.Remove(sinhVat);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Xóa sinh vật thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Không thể xóa sinh vật: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SinhVatExists(int id)
        {
            return _context.SinhVats.Any(e => e.MaSV == id);
        }

        // Helper method để load danh sách lô rừng
        private async Task LoadLoRungSelectList(int? selectedValue = null)
        {
            var loRungs = await _context.LoRungs
                .Select(l => new
                {
                    l.MaLo,
                    DisplayText = "TK " + l.SoTieuKhu + " - K " + l.SoKhoanh + " - L " + l.SoLo
                })
                .ToListAsync();

            ViewBag.MaLo = new SelectList(loRungs, "MaLo", "DisplayText", selectedValue);
        }
    }
}