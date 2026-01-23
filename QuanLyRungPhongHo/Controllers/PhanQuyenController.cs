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
            if (await _context.Permissions.AnyAsync())
            {
                return; // Đã có dữ liệu
            }

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

                // Module: Nhân sự
                new() { PermissionCode = "NhanSu.View", PermissionName = "Xem danh sách Nhân sự", ModuleName = "Nhân Sự" },
                new() { PermissionCode = "NhanSu.Create", PermissionName = "Tạo mới Nhân sự", ModuleName = "Nhân Sự" },
                new() { PermissionCode = "NhanSu.Edit", PermissionName = "Sửa thông tin Nhân sự", ModuleName = "Nhân Sự" },
                new() { PermissionCode = "NhanSu.Delete", PermissionName = "Xóa Nhân sự", ModuleName = "Nhân Sự" },

                // Module: Tài khoản
                new() { PermissionCode = "TaiKhoan.Create", PermissionName = "Tạo tài khoản", ModuleName = "Tài Khoản" },
                new() { PermissionCode = "TaiKhoan.Edit", PermissionName = "Sửa tài khoản", ModuleName = "Tài Khoản" },
                new() { PermissionCode = "TaiKhoan.Delete", PermissionName = "Xóa tài khoản", ModuleName = "Tài Khoản" },
                new() { PermissionCode = "TaiKhoan.ResetPassword", PermissionName = "Reset mật khẩu", ModuleName = "Tài Khoản" },

                // Module: Nhật ký bảo vệ
                new() { PermissionCode = "NhatKyBaoVe.View", PermissionName = "Xem Nhật ký bảo vệ", ModuleName = "Nhật Ký Bảo Vệ" },
                new() { PermissionCode = "NhatKyBaoVe.Create", PermissionName = "Tạo Nhật ký bảo vệ", ModuleName = "Nhật Ký Bảo Vệ" },
                new() { PermissionCode = "NhatKyBaoVe.Edit", PermissionName = "Sửa Nhật ký bảo vệ", ModuleName = "Nhật Ký Bảo Vệ" },
                new() { PermissionCode = "NhatKyBaoVe.Delete", PermissionName = "Xóa Nhật ký bảo vệ", ModuleName = "Nhật Ký Bảo Vệ" },

                // Module: Sinh vật
                new() { PermissionCode = "SinhVat.View", PermissionName = "Xem danh sách Sinh vật", ModuleName = "Sinh Vật" },
                new() { PermissionCode = "SinhVat.Create", PermissionName = "Tạo mới Sinh vật", ModuleName = "Sinh Vật" },
                new() { PermissionCode = "SinhVat.Edit", PermissionName = "Sửa thông tin Sinh vật", ModuleName = "Sinh Vật" },
                new() { PermissionCode = "SinhVat.Delete", PermissionName = "Xóa Sinh vật", ModuleName = "Sinh Vật" },

                // Module: Báo cáo
                new() { PermissionCode = "Report.ViewAll", PermissionName = "Xem báo cáo toàn tỉnh", ModuleName = "Báo Cáo" },
                new() { PermissionCode = "Report.ViewOwn", PermissionName = "Xem báo cáo của mình", ModuleName = "Báo Cáo" },
                new() { PermissionCode = "Report.Export", PermissionName = "Xuất báo cáo", ModuleName = "Báo Cáo" }
            };

            _context.Permissions.AddRange(permissions);
            await _context.SaveChangesAsync();

            // Tạo quyền mặc định cho Admin_Tinh (full quyền)
            var adminPermissions = permissions.Select(p => new RolePermission
            {
                RoleName = "Admin_Tinh",
                PermissionId = p.PermissionId,
                IsGranted = true,
                CreatedDate = DateTime.Now
            });

            _context.RolePermissions.AddRange(adminPermissions);
            await _context.SaveChangesAsync();
        }
    }
}
