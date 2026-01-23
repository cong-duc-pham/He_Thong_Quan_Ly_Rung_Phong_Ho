using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyRungPhongHo.Data;
using QuanLyRungPhongHo.Models;

namespace QuanLyRungPhongHo.Controllers
{
    [Authorize(Roles = "Admin_Tinh,Admin")]
    public class QuanLyPermissionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuanLyPermissionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: QuanLyPermission
        public async Task<IActionResult> Index()
        {
            var permissions = await _context.Permissions
                .OrderBy(p => p.ModuleName)
                .ThenBy(p => p.PermissionName)
                .ToListAsync();
            return View(permissions);
        }

        // GET: QuanLyPermission/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: QuanLyPermission/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Permission permission)
        {
            if (ModelState.IsValid)
            {
                _context.Permissions.Add(permission);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã thêm quyền mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(permission);
        }

        // GET: QuanLyPermission/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
            {
                return NotFound();
            }
            return View(permission);
        }

        // POST: QuanLyPermission/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Permission permission)
        {
            if (id != permission.PermissionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(permission);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã cập nhật quyền thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PermissionExists(permission.PermissionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(permission);
        }

        // POST: QuanLyPermission/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission != null)
            {
                _context.Permissions.Remove(permission);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa quyền thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PermissionExists(int id)
        {
            return _context.Permissions.Any(e => e.PermissionId == id);
        }
    }
}
