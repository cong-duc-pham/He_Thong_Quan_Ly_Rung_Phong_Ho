using System.ComponentModel.DataAnnotations;

namespace QuanLyRungPhongHo.Models
{
    /// <summary>
    /// Bảng lưu danh sách các quyền (Permission) trong hệ thống
    /// </summary>
    public class Permission
    {
        [Key]
        public int PermissionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PermissionCode { get; set; } = null!; // VD: "LoRung.Create", "NhanSu.Edit"

        [Required]
        [MaxLength(200)]
        public string PermissionName { get; set; } = null!; // VD: "Tạo lô rừng", "Sửa nhân sự"

        [MaxLength(100)]
        public string? ModuleName { get; set; } // VD: "Lô Rừng", "Nhân Sự", "Nhật Ký"

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
