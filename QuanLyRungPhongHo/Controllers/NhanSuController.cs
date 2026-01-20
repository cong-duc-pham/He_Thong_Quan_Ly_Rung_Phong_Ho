using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyRungPhongHo.Data;
using QuanLyRungPhongHo.Models;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyRungPhongHo.Controllers
{
    [Authorize]
    public class NhanSuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NhanSuController(ApplicationDbContext context)
        {
            _context = context;
        }

        //  Hiển thị danh sách + Search + Filter
        public async Task<IActionResult> Index(string searchString, string roleFilter, string maXaFilter)
        {
            try
            {
                var query = from ns in _context.NhanSus
                            join xa in _context.DanhMucXas on ns.MaXa equals xa.MaXa into xaGroup
                            from x in xaGroup.DefaultIfEmpty()
                            join tk in _context.TaiKhoans on ns.MaNV equals tk.MaNV into tkGroup
                            from t in tkGroup.DefaultIfEmpty()
                            select new NhanSuViewModel
                            {
                                MaNV = ns.MaNV,
                                HoTen = ns.HoTen,
                                ChucVu = ns.ChucVu ?? "",
                                SDT = ns.SDT ?? "",
                                MaXa = ns.MaXa,
                                TenXa = x != null ? x.TenXa : "Chưa phân công",
                                TenDangNhap = t != null ? t.TenDangNhap : "",
                                Quyen = t != null ? t.Quyen : ""
                            };

                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(x => x.HoTen.Contains(searchString) || 
                                             (x.SDT != null && x.SDT.Contains(searchString)));
                }
                if (!string.IsNullOrEmpty(roleFilter))
                {
                    query = query.Where(x => x.ChucVu == roleFilter);
                }
                if (!string.IsNullOrEmpty(maXaFilter))
                {
                    query = query.Where(x => x.MaXa == maXaFilter);
                }

                ViewBag.CurrentSearch = searchString;
                ViewBag.CurrentRole = roleFilter;
                ViewBag.CurrentXa = maXaFilter;
                ViewBag.DsXa = new SelectList(await _context.DanhMucXas.OrderBy(x => x.TenXa).ToListAsync(), "MaXa", "TenXa");

                var result = await query.OrderByDescending(x => x.MaNV).ToListAsync();
                return View(result);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi tải dữ liệu: {ex.Message}";
                return View(new List<NhanSuViewModel>());
            }
        }

        // lấy chi tiết 1 nhân viên
        [HttpGet]
        public async Task<JsonResult> GetById(int id)
        {
            try
            {
                var ns = await _context.NhanSus.FindAsync(id);
                if (ns == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy nhân sự" });
                }

                var tk = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaNV == id);

                return Json(new
                {
                    success = true,
                    MaNV = ns.MaNV,
                    HoTen = ns.HoTen,
                    ChucVu = ns.ChucVu,
                    SDT = ns.SDT,
                    MaXa = ns.MaXa,
                    TenDangNhap = tk?.TenDangNhap ?? "",
                    Quyen = tk?.Quyen ?? "NhanVien_Thon"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Lưu (Thêm hoặc Sửa) với validation chi tiết
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Save(NhanSuViewModel model)
        {
            try
            {
                // Validation chi tiết
                if (string.IsNullOrWhiteSpace(model.HoTen))
                {
                    return Json(new { success = false, message = "Họ tên không được để trống!" });
                }
                
                if (string.IsNullOrWhiteSpace(model.ChucVu))
                {
                    return Json(new { success = false, message = "Vui lòng chọn chức vụ!" });
                }
                
                if (string.IsNullOrWhiteSpace(model.SDT))
                {
                    return Json(new { success = false, message = "SĐT không được để trống!" });
                }
                
                // Validate SĐT Việt Nam
                if (!Regex.IsMatch(model.SDT, @"^(03|05|07|08|09|01[2|6|8|9])+([0-9]{8})$"))
                {
                    return Json(new { success = false, message = "Số điện thoại không đúng định dạng VN!" });
                }
                
                if (string.IsNullOrWhiteSpace(model.MaXa))
                {
                    return Json(new { success = false, message = "Vui lòng chọn địa bàn!" });
                }
                
                if (string.IsNullOrWhiteSpace(model.TenDangNhap))
                {
                    return Json(new { success = false, message = "Tên đăng nhập không được để trống!" });
                }
                
                // Validate username
                if (!Regex.IsMatch(model.TenDangNhap, @"^[a-zA-Z0-9_]{5,50}$"))
                {
                    return Json(new { success = false, message = "Tên đăng nhập từ 5-50 ký tự, chỉ chữ, số và gạch dưới!" });
                }
                
                // Kiểm tra mật khẩu khi thêm mới
                if (model.MaNV == 0 && string.IsNullOrWhiteSpace(model.MatKhau))
                {
                    return Json(new { success = false, message = "Mật khẩu không được để trống khi thêm mới!" });
                }
                
                if (!string.IsNullOrWhiteSpace(model.MatKhau) && model.MatKhau.Length < 6)
                {
                    return Json(new { success = false, message = "Mật khẩu phải có ít nhất 6 ký tự!" });
                }

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Kiểm tra trùng tên đăng nhập
                        var existingUsername = await _context.TaiKhoans
                            .Where(x => x.TenDangNhap == model.TenDangNhap && x.MaNV != model.MaNV)
                            .FirstOrDefaultAsync();
                            
                        if (existingUsername != null)
                        {
                            return Json(new { success = false, message = "Tên đăng nhập đã tồn tại!" });
                        }

                        if (model.MaNV == 0) // THÊM MỚI
                        {
                            var ns = new NhanSu
                            {
                                HoTen = model.HoTen.Trim(),
                                ChucVu = model.ChucVu,
                                SDT = model.SDT.Trim(),
                                MaXa = model.MaXa
                            };
                            _context.NhanSus.Add(ns);
                            await _context.SaveChangesAsync();

                            // HASH mật khẩu trước khi lưu
                            string hashedPassword = HashPassword(model.MatKhau ?? "123456");
                            
                            var tk = new TaiKhoan
                            {
                                MaNV = ns.MaNV,
                                TenDangNhap = model.TenDangNhap.Trim(),
                                MatKhau = hashedPassword, // Lưu mật khẩu đã hash
                                Quyen = model.Quyen ?? "NhanVien_Thon"
                            };
                            _context.TaiKhoans.Add(tk);
                            await _context.SaveChangesAsync();
                            
                            await transaction.CommitAsync();
                            return Json(new { success = true, message = "Thêm nhân sự thành công!" });
                        }
                        else // CẬP NHẬT
                        {
                            var ns = await _context.NhanSus.FindAsync(model.MaNV);
                            if (ns == null)
                            {
                                return Json(new { success = false, message = "Không tìm thấy nhân sự!" });
                            }

                            ns.HoTen = model.HoTen.Trim();
                            ns.ChucVu = model.ChucVu;
                            ns.SDT = model.SDT.Trim();
                            ns.MaXa = model.MaXa;
                            
                            _context.NhanSus.Update(ns);

                            var tk = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaNV == model.MaNV);
                            if (tk != null)
                            {
                                tk.TenDangNhap = model.TenDangNhap.Trim();
                                tk.Quyen = model.Quyen ?? tk.Quyen;
                                
                                // HASH mật khẩu nếu người dùng nhập mật khẩu mới
                                if (!string.IsNullOrWhiteSpace(model.MatKhau))
                                {
                                    tk.MatKhau = HashPassword(model.MatKhau);
                                }
                                
                                _context.TaiKhoans.Update(tk);
                            }
                            else
                            {
                                // Tạo tài khoản mới nếu chưa có
                                string hashedPassword = HashPassword(model.MatKhau ?? "123456");
                                
                                var newTk = new TaiKhoan
                                {
                                    MaNV = model.MaNV,
                                    TenDangNhap = model.TenDangNhap.Trim(),
                                    MatKhau = hashedPassword,
                                    Quyen = model.Quyen ?? "NhanVien_Thon"
                                };
                                _context.TaiKhoans.Add(newTk);
                            }

                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            return Json(new { success = true, message = "Cập nhật thành công!" });
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return Json(new { success = false, message = "Lỗi: " + ex.Message });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // Xóa với transaction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Delete(int id)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var ns = await _context.NhanSus.FindAsync(id);
                    if (ns == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy nhân sự!" });
                    }

                    // Xóa tài khoản trước
                    var tk = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaNV == id);
                    if (tk != null)
                    {
                        _context.TaiKhoans.Remove(tk);
                    }

                    // Sau đó xóa nhân sự
                    _context.NhanSus.Remove(ns);
                    
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    return Json(new { success = true, message = "Xóa thành công!" });
                }
                catch (DbUpdateException)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Không thể xóa! Nhân sự này đã có dữ liệu liên quan." });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Lỗi: " + ex.Message });
                }
            }
        }

        // Hàm hash mật khẩu SHA256 (giống AccountController)
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}