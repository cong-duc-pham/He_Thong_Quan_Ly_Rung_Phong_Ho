using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyRungPhongHo.Data;
using QuanLyRungPhongHo.Attributes;

namespace QuanLyRungPhongHo.Controllers
{
    [Authorize] // Chỉ yêu cầu đăng nhập, quyền được kiểm tra bởi CheckPermission
    public class CaLamViecController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CaLamViecController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CaLamViec
        [CheckPermission("CaLamViec.View")]
        public async Task<IActionResult> Index()
        {
            // TODO: Implement ca làm việc management
            return View();
        }
    }
}
