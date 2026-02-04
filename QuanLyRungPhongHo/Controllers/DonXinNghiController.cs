using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyRungPhongHo.Data;
using QuanLyRungPhongHo.Attributes;

namespace QuanLyRungPhongHo.Controllers
{
    [Authorize]
    public class DonXinNghiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DonXinNghiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DonXinNghi
        [CheckPermission("DonXinNghi.View")]
        public async Task<IActionResult> Index()
        {
            // TODO: Implement đơn xin nghỉ management
            return View();
        }
    }
}
