using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using QuanLyRungPhongHo.Data;

namespace QuanLyRungPhongHo.Attributes
{
    /// <summary>
    /// Attribute kiểm tra quyền từ database
    /// Admin_Tinh luôn có full quyền, các role khác phải kiểm tra từ bảng RolePermission
    /// Hỗ trợ kiểm tra nhiều quyền - user chỉ cần có 1 trong số các quyền là được phép truy cập
    /// </summary>
    public class CheckPermissionAttribute : TypeFilterAttribute
    {
        public CheckPermissionAttribute(params string[] permissionCodes) : base(typeof(PermissionFilter))
        {
            Arguments = new object[] { permissionCodes };
        }
    }

    public class PermissionFilter : IAsyncAuthorizationFilter
    {
        private readonly string[] _permissionCodes;
        private readonly ApplicationDbContext _context;

        public PermissionFilter(ApplicationDbContext context, string[] permissionCodes)
        {
            _context = context;
            _permissionCodes = permissionCodes;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            Console.WriteLine("=== CheckPermission Filter Executing ===");
            Console.WriteLine($"Required Permissions: {string.Join(", ", _permissionCodes)}");
            
            // Kiểm tra đã đăng nhập chưa
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                Console.WriteLine("User NOT authenticated - Challenge");
                context.Result = new ChallengeResult();
                return;
            }

            Console.WriteLine($"User authenticated: {context.HttpContext.User.Identity.Name}");

            // Admin_Tinh có full quyền, không cần kiểm tra
            if (context.HttpContext.User.IsInRole("Admin_Tinh") || context.HttpContext.User.IsInRole("Admin"))
            {
                Console.WriteLine("User is Admin - ALLOWED");
                return;
            }

            // Lấy role của user hiện tại
            var userRole = context.HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;

            Console.WriteLine($"User Role: {userRole ?? "NULL"}");

            if (string.IsNullOrEmpty(userRole))
            {
                Console.WriteLine("No role found - FORBIDDEN");
                context.Result = new RedirectToActionResult("Forbidden", "Home", null);
                return;
            }

            // Kiểm tra quyền từ database - user chỉ cần có 1 trong các quyền là được phép
            var hasPermission = await _context.RolePermissions
                .AnyAsync(rp => rp.RoleName == userRole 
                    && _permissionCodes.Contains(rp.Permission!.PermissionCode)
                    && rp.IsGranted 
                    && rp.Permission.IsActive);

            Console.WriteLine($"Has Permission: {hasPermission}");

            if (!hasPermission)
            {
                Console.WriteLine($"Permission DENIED for {userRole} - Required: {string.Join(", ", _permissionCodes)}");
                // Không có quyền - redirect đến trang Forbidden
                context.Result = new RedirectToActionResult("Forbidden", "Home", null);
                return;
            }
            
            Console.WriteLine("Permission GRANTED - Access allowed");
        }
    }
}
