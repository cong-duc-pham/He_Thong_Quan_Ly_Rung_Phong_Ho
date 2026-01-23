using System.ComponentModel.DataAnnotations;

namespace QuanLyRungPhongHo.Models
{
    /// <summary>
    /// Bảng trung gian lưu quyền của từng Role
    /// Admin có thể bật/tắt quyền cho từng role
    /// </summary>
    public class RolePermission
    {
        [Key]
        public int RolePermissionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; } = null!; // "Admin_Tinh", "QuanLy_Xa", "Kiem_Lam"

        [Required]
        public int PermissionId { get; set; }

        public bool IsGranted { get; set; } = true; // Admin có thể bật/tắt

        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }

        // Navigation
        public Permission? Permission { get; set; }
    }
}
