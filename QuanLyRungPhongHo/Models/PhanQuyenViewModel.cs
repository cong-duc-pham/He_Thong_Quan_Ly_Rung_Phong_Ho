namespace QuanLyRungPhongHo.Models
{
    /// <summary>
    /// ViewModel cho trang quản lý phân quyền
    /// </summary>
    public class PhanQuyenViewModel
    {
        public string RoleName { get; set; } = null!;
        public string RoleDisplayName { get; set; } = null!;
        public List<PermissionItem> Permissions { get; set; } = new List<PermissionItem>();
    }

    public class PermissionItem
    {
        public int PermissionId { get; set; }
        public string PermissionCode { get; set; } = null!;
        public string PermissionName { get; set; } = null!;
        public string? ModuleName { get; set; }
        public bool IsGranted { get; set; }
    }
}
