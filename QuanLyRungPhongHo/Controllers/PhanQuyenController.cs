using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyRungPhongHo.Data;
using QuanLyRungPhongHo.Models;

namespace QuanLyRungPhongHo.Controllers
{
    [Authorize(Roles = "Admin_Tinh,Admin")]
    public class PhanQuyenController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Danh sách 3 role cố định
        private static readonly Dictionary<string, string> Roles = new()
        {
            { "Admin_Tinh", "Quản trị viên Tỉnh" },
            { "QuanLy_Xa", "Quản lý Xã" },
            { "Kiem_Lam", "Kiểm lâm viên" }
        };

        // Danh sách role hiển thị trên giao diện (bỏ Admin_Tinh)
        private static readonly Dictionary<string, string> DisplayRoles = new()
        {
            { "QuanLy_Xa", "Quản lý Xã" },
            { "Kiem_Lam", "Kiểm lâm viên" }
        };

        public PhanQuyenController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PhanQuyen
        public async Task<IActionResult> Index()
        {
            // Khởi tạo dữ liệu mẫu nếu chưa có
            await InitializePermissionsIfEmpty();

            var rolePermissions = new List<PhanQuyenViewModel>();

            // Chỉ hiển thị role QuanLy_Xa và Kiem_Lam (bỏ Admin_Tinh)
            foreach (var role in DisplayRoles)
            {
                var permissions = await (from p in _context.Permissions
                                        where p.IsActive
                                        join rp in _context.RolePermissions.Where(r => r.RoleName == role.Key)
                                        on p.PermissionId equals rp.PermissionId into rpGroup
                                        from rp in rpGroup.DefaultIfEmpty()
                                        orderby p.ModuleName, p.PermissionName
                                        select new PermissionItem
                                        {
                                            PermissionId = p.PermissionId,
                                            PermissionCode = p.PermissionCode,
                                            PermissionName = p.PermissionName,
                                            ModuleName = p.ModuleName,
                                            IsGranted = rp != null && rp.IsGranted
                                        }).ToListAsync();

                rolePermissions.Add(new PhanQuyenViewModel
                {
                    RoleName = role.Key,
                    RoleDisplayName = role.Value,
                    Permissions = permissions
                });
            }

            return View(rolePermissions);
        }

        // POST: PhanQuyen/UpdatePermission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePermission(string roleName, int permissionId, bool isGranted)
        {
            try
            {
                // Kiểm tra role hợp lệ
                if (!Roles.ContainsKey(roleName))
                {
                    return Json(new { success = false, message = "Role không hợp lệ" });
                }

                // Tìm permission
                var permission = await _context.Permissions.FindAsync(permissionId);
                if (permission == null)
                {
                    return Json(new { success = false, message = "Quyền không tồn tại" });
                }

                // Tìm hoặc tạo RolePermission
                var rolePermission = await _context.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleName == roleName && rp.PermissionId == permissionId);

                if (rolePermission == null)
                {
                    // Tạo mới
                    rolePermission = new RolePermission
                    {
                        RoleName = roleName,
                        PermissionId = permissionId,
                        IsGranted = isGranted,
                        CreatedDate = DateTime.Now
                    };
                    _context.RolePermissions.Add(rolePermission);
                }
                else
                {
                    // Cập nhật
                    rolePermission.IsGranted = isGranted;
                    rolePermission.ModifiedDate = DateTime.Now;
                    _context.RolePermissions.Update(rolePermission);
                }

                await _context.SaveChangesAsync();

                return Json(new 
                { 
                    success = true, 
                    message = $"Đã {(isGranted ? "cấp" : "thu hồi")} quyền '{permission.PermissionName}' cho {Roles[roleName]}" 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Khởi tạo dữ liệu quyền mẫu
        private async Task InitializePermissionsIfEmpty()
        {
            // Dọn dẹp các quyền cũ không còn sử dụng
            await CleanupOldPermissions();
            
            // Kiểm tra và tạo Permissions nếu chưa có
            bool hasPermissions = await _context.Permissions.AnyAsync();
            
            if (!hasPermissions)
            {
                await CreatePermissions();
            }
            
            // Luôn kiểm tra và cập nhật RolePermissions
            await InitializeRolePermissions();
        }

        private async Task CleanupOldPermissions()
        {
            // Danh sách các Permission code cũ không còn sử dụng
            var oldPermissionCodes = new[]
            {
                "TaiKhoan.Create",
                "TaiKhoan.Edit", 
                "TaiKhoan.Delete",
                "TaiKhoan.ResetPassword",
                "Report.ViewAll",
                "Report.ViewOwn",
                "Report.Export"
            };

            // Xóa các RolePermission liên quan đến Permission cũ
            var oldRolePermissions = await _context.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => oldPermissionCodes.Contains(rp.Permission!.PermissionCode))
                .ToListAsync();

            if (oldRolePermissions.Any())
            {
                _context.RolePermissions.RemoveRange(oldRolePermissions);
            }

            // Xóa các Permission cũ
            var oldPermissions = await _context.Permissions
                .Where(p => oldPermissionCodes.Contains(p.PermissionCode))
                .ToListAsync();

            if (oldPermissions.Any())
            {
                _context.Permissions.RemoveRange(oldPermissions);
                await _context.SaveChangesAsync();
            }
        }

        private async Task CreatePermissions()
        {

            var permissions = new List<Permission>
            {
                // Module: Danh mục Xã
                new() { PermissionCode = "DanhMucXa.View", PermissionName = "Xem danh sách Xã", ModuleName = "Danh mục Xã" },
                new() { PermissionCode = "DanhMucXa.Create", PermissionName = "Tạo mới Xã", ModuleName = "Danh mục Xã" },
                new() { PermissionCode = "DanhMucXa.Edit", PermissionName = "Sửa thông tin Xã", ModuleName = "Danh mục Xã" },
                new() { PermissionCode = "DanhMucXa.Delete", PermissionName = "Xóa Xã", ModuleName = "Danh mục Xã" },

                // Module: Danh mục Thôn
                new() { PermissionCode = "DanhMucThon.View", PermissionName = "Xem danh sách Thôn", ModuleName = "Danh mục Thôn" },
                new() { PermissionCode = "DanhMucThon.Create", PermissionName = "Tạo mới Thôn", ModuleName = "Danh mục Thôn" },
                new() { PermissionCode = "DanhMucThon.Edit", PermissionName = "Sửa thông tin Thôn", ModuleName = "Danh mục Thôn" },
                new() { PermissionCode = "DanhMucThon.Delete", PermissionName = "Xóa Thôn", ModuleName = "Danh mục Thôn" },

                // Module: Lô rừng
                new() { PermissionCode = "LoRung.View", PermissionName = "Xem danh sách Lô rừng", ModuleName = "Lô Rừng" },
                new() { PermissionCode = "LoRung.Create", PermissionName = "Tạo mới Lô rừng", ModuleName = "Lô Rừng" },
                new() { PermissionCode = "LoRung.Edit", PermissionName = "Sửa thông tin Lô rừng", ModuleName = "Lô Rừng" },
                new() { PermissionCode = "LoRung.Delete", PermissionName = "Xóa Lô rừng", ModuleName = "Lô Rừng" },

                // Module: Nhân Sự (bao gồm cả quản lý tài khoản)
                new() { PermissionCode = "NhanSu.View", PermissionName = "Xem danh sách Nhân sự", ModuleName = "Nhân Sự" },
                new() { PermissionCode = "NhanSu.Create", PermissionName = "Tạo mới Nhân sự & Tài khoản", ModuleName = "Nhân Sự" },
                new() { PermissionCode = "NhanSu.Edit", PermissionName = "Sửa Nhân sự & Tài khoản", ModuleName = "Nhân Sự" },
                new() { PermissionCode = "NhanSu.Delete", PermissionName = "Xóa Nhân sự & Tài khoản", ModuleName = "Nhân Sự" },

                // Module: Nhật Ký Bảo Vệ
                new() { PermissionCode = "NhatKyBaoVe.View", PermissionName = "Xem Nhật ký bảo vệ", ModuleName = "Nhật Ký Bảo Vệ" },
                new() { PermissionCode = "NhatKyBaoVe.Create", PermissionName = "Tạo Nhật ký bảo vệ", ModuleName = "Nhật Ký Bảo Vệ" },
                new() { PermissionCode = "NhatKyBaoVe.Edit", PermissionName = "Sửa Nhật ký bảo vệ", ModuleName = "Nhật Ký Bảo Vệ" },
                new() { PermissionCode = "NhatKyBaoVe.Delete", PermissionName = "Xóa Nhật ký bảo vệ", ModuleName = "Nhật Ký Bảo Vệ" },

                // Module: Sinh Vật
                new() { PermissionCode = "SinhVat.View", PermissionName = "Xem danh sách Sinh vật", ModuleName = "Sinh Vật" },
                new() { PermissionCode = "SinhVat.Create", PermissionName = "Tạo mới Sinh vật", ModuleName = "Sinh Vật" },
                new() { PermissionCode = "SinhVat.Edit", PermissionName = "Sửa thông tin Sinh vật", ModuleName = "Sinh Vật" },
                new() { PermissionCode = "SinhVat.Delete", PermissionName = "Xóa Sinh vật", ModuleName = "Sinh Vật" },

                // Module: Báo Cáo Thống Kê
                new() { PermissionCode = "BaoCaoThongKe.View", PermissionName = "Xem báo cáo thống kê", ModuleName = "Báo Cáo Thống Kê" },
                new() { PermissionCode = "BaoCaoThongKe.Export", PermissionName = "Xuất báo cáo (CSV/PDF)", ModuleName = "Báo Cáo Thống Kê" }
            };

            _context.Permissions.AddRange(permissions);
            await _context.SaveChangesAsync();
        }

        private async Task InitializeRolePermissions()
        {
            var allPermissions = await _context.Permissions.ToListAsync();
            if (!allPermissions.Any()) return;

            var rolePermissions = new List<RolePermission>();

            // 1. Phân quyền cho Admin_Tinh (Full quyền)
            await EnsureRolePermissions("Admin_Tinh", allPermissions, allPermissions.Select(p => p.PermissionCode).ToArray());

            // 2. Phân quyền cho QuanLy_Xa (Quản lý dữ liệu xã)
            var quanLyXaPermissions = new[]
            {
                // Danh mục Thôn - Full quyền
                "DanhMucThon.View", "DanhMucThon.Create", "DanhMucThon.Edit", "DanhMucThon.Delete",
                // Lô Rừng - Full quyền
                "LoRung.View", "LoRung.Create", "LoRung.Edit", "LoRung.Delete",
                // Nhân sự & Tài khoản - Full quyền
                "NhanSu.View", "NhanSu.Create", "NhanSu.Edit", "NhanSu.Delete",
                // Nhật ký bảo vệ - Full quyền
                "NhatKyBaoVe.View", "NhatKyBaoVe.Create", "NhatKyBaoVe.Edit", "NhatKyBaoVe.Delete",
                // Sinh vật - Full quyền
                "SinhVat.View", "SinhVat.Create", "SinhVat.Edit", "SinhVat.Delete",
                // Báo cáo - Xem và xuất
                "BaoCaoThongKe.View", "BaoCaoThongKe.Export"
            };
            await EnsureRolePermissions("QuanLy_Xa", allPermissions, quanLyXaPermissions);

            // 3. Phân quyền cho Kiem_Lam (Xem danh mục và ghi nhật ký)
            var kiemLamPermissions = new[]
            {
                // Xem các danh mục (chỉ xem, không sửa)
                "DanhMucXa.View", "DanhMucThon.View", "LoRung.View", "NhanSu.View", "SinhVat.View",
                // Nhật ký bảo vệ - Xem, Tạo, Sửa (không xóa)
                "NhatKyBaoVe.View", "NhatKyBaoVe.Create", "NhatKyBaoVe.Edit",
                // Báo cáo - Xem (không xuất)
                "BaoCaoThongKe.View"
            };
            await EnsureRolePermissions("Kiem_Lam", allPermissions, kiemLamPermissions);
        }

        private async Task EnsureRolePermissions(string roleName, List<Permission> allPermissions, string[] permissionCodes)
        {
            foreach (var permCode in permissionCodes)
            {
                var permission = allPermissions.FirstOrDefault(p => p.PermissionCode == permCode);
                if (permission == null) continue;

                // Kiểm tra xem đã có RolePermission chưa
                var existingRolePermission = await _context.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleName == roleName && rp.PermissionId == permission.PermissionId);

                if (existingRolePermission == null)
                {
                    // Tạo mới nếu chưa có
                    _context.RolePermissions.Add(new RolePermission
                    {
                        RoleName = roleName,
                        PermissionId = permission.PermissionId,
                        IsGranted = true,
                        CreatedDate = DateTime.Now
                    });
                }
                // Nếu đã có thì giữ nguyên (không ghi đè cài đặt của Admin)
            }

            await _context.SaveChangesAsync();
        }
    }
}
