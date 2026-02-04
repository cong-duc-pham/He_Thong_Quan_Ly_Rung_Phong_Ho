using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyRungPhongHo.Data;
using QuanLyRungPhongHo.Models;
using QuanLyRungPhongHo.Validators;
using System.ComponentModel.DataAnnotations;
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
                                Email = ns.Email ?? "",
                                MaXa = ns.MaXa,
                                TenXa = x != null ? x.TenXa : "Chưa phân công",
                                TenDangNhap = t != null ? t.TenDangNhap : "",
                                Quyen = t != null ? t.Quyen : ""
                            };

                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(x => x.HoTen.Contains(searchString) ||
                                             (x.SDT != null && x.SDT.Contains(searchString)) ||
                                             (x.Email != null && x.Email.Contains(searchString)));
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
                    Email = ns.Email,
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

        // Lưu (Thêm hoặc Sửa) với validation chi tiết chuyên nghiệp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Save(NhanSuViewModel model)
        {
            try
            {
                // 1. VALIDATION HỌ TÊN
                var hoTenValidation = NhanSuValidator.ValidateHoTen(model.HoTen);
                if (!hoTenValidation.IsValid)
                    return Json(new { success = false, message = hoTenValidation.ErrorMessage, errorField = "HoTen" });

                // 2. VALIDATION CHỨC VỤ
                var chucVuValidation = NhanSuValidator.ValidateChucVu(model.ChucVu);
                if (!chucVuValidation.IsValid)
                    return Json(new { success = false, message = chucVuValidation.ErrorMessage, errorField = "ChucVu" });

                // 3. VALIDATION SỐ ĐIỆN THOẠI
                var sdtValidation = NhanSuValidator.ValidateSDT(model.SDT);
                if (!sdtValidation.IsValid)
                    return Json(new { success = false, message = sdtValidation.ErrorMessage, errorField = "SDT" });

                // 4. VALIDATION EMAIL
                var emailValidation = NhanSuValidator.ValidateEmail(model.Email);
                if (!emailValidation.IsValid)
                    return Json(new { success = false, message = emailValidation.ErrorMessage, errorField = "Email" });

                // 5. VALIDATION MÃ XÃ
                var maXaValidation = NhanSuValidator.ValidateMaXa(model.MaXa);
                if (!maXaValidation.IsValid)
                    return Json(new { success = false, message = maXaValidation.ErrorMessage, errorField = "MaXa" });

                // 6. VALIDATION TÊN ĐĂNG NHẬP
                var tenDangNhapValidation = NhanSuValidator.ValidateTenDangNhap(model.TenDangNhap);
                if (!tenDangNhapValidation.IsValid)
                    return Json(new { success = false, message = tenDangNhapValidation.ErrorMessage, errorField = "TenDangNhap" });

                // 7. VALIDATION MẬT KHẨU
                bool isPasswordRequired = (model.MaNV == 0); // Bắt buộc khi thêm mới
                var matKhauValidation = NhanSuValidator.ValidateMatKhau(model.MatKhau, isPasswordRequired);
                if (!matKhauValidation.IsValid)
                    return Json(new { success = false, message = matKhauValidation.ErrorMessage, errorField = "MatKhau" });

                // 8. VALIDATION QUYỀN
                var quyenValidation = NhanSuValidator.ValidateQuyen(model.Quyen);
                if (!quyenValidation.IsValid)
                    return Json(new { success = false, message = quyenValidation.ErrorMessage, errorField = "Quyen" });

                // 9. VALIDATION NGHIỆP VỤ: Quyền phù hợp với chức vụ
                var quyenChucVuValidation = NhanSuValidator.ValidateQuyenVsChucVu(model.Quyen, model.ChucVu);
                if (!quyenChucVuValidation.IsValid)
                    return Json(new { success = false, message = quyenChucVuValidation.ErrorMessage });

                // 10. CHUẨN HÓA DỮ LIỆU
                model.HoTen = NhanSuValidator.NormalizeHoTen(model.HoTen);
                model.SDT = NhanSuValidator.NormalizeSDT(model.SDT!);
                model.Email = NhanSuValidator.NormalizeEmail(model.Email);
                model.TenDangNhap = model.TenDangNhap?.Trim();

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 11. KIỂM TRA TRÙNG TÊN ĐĂNG NHẬP
                        var existingUsername = await _context.TaiKhoans
                            .Where(x => x.TenDangNhap == model.TenDangNhap && x.MaNV != model.MaNV)
                            .FirstOrDefaultAsync();

                        if (existingUsername != null)
                        {
                            return Json(new { success = false, message = "Tên đăng nhập đã tồn tại! Vui lòng chọn tên khác.", errorField = "TenDangNhap" });
                        }

                        // 12. KIỂM TRA TRÙNG SỐ ĐIỆN THOẠI
                        var existingSDT = await _context.NhanSus
                            .Where(x => x.SDT == model.SDT && x.MaNV != model.MaNV)
                            .FirstOrDefaultAsync();

                        if (existingSDT != null)
                        {
                            return Json(new { success = false, message = "Số điện thoại đã được sử dụng bởi nhân sự khác!", errorField = "SDT" });
                        }

                        // 13. KIỂM TRA TRÙNG EMAIL (nếu có)
                        if (!string.IsNullOrWhiteSpace(model.Email))
                        {
                            var existingEmail = await _context.NhanSus
                                .Where(x => x.Email == model.Email && x.MaNV != model.MaNV)
                                .FirstOrDefaultAsync();

                            if (existingEmail != null)
                            {
                                return Json(new { success = false, message = "Email đã được sử dụng bởi nhân sự khác!", errorField = "Email" });
                            }
                        }

                        // 14. KIỂM TRA MÃ XÃ TỒN TẠI
                        var xaExists = await _context.DanhMucXas.AnyAsync(x => x.MaXa == model.MaXa);
                        if (!xaExists)
                        {
                            return Json(new { success = false, message = "Mã xã không tồn tại trong hệ thống!" });
                        }

                        if (model.MaNV == 0) // THÊM MỚI
                        {
                            var ns = new NhanSu
                            {
                                HoTen = model.HoTen,
                                ChucVu = model.ChucVu,
                                SDT = model.SDT,
                                Email = !string.IsNullOrWhiteSpace(model.Email) ? model.Email : null,
                                MaXa = model.MaXa
                            };
                            _context.NhanSus.Add(ns);
                            await _context.SaveChangesAsync();

                            // HASH mật khẩu trước khi lưu
                            string hashedPassword = HashPassword(model.MatKhau ?? "123456");

                            var tk = new TaiKhoan
                            {
                                MaNV = ns.MaNV,
                                TenDangNhap = model.TenDangNhap,
                                MatKhau = hashedPassword,
                                Quyen = model.Quyen ?? "Kiem_Lam",
                                TrangThai = true
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

                            ns.HoTen = model.HoTen;
                            ns.ChucVu = model.ChucVu;
                            ns.SDT = model.SDT;
                            ns.Email = !string.IsNullOrWhiteSpace(model.Email) ? model.Email : null;
                            ns.MaXa = model.MaXa;

                            _context.NhanSus.Update(ns);

                            var tk = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaNV == model.MaNV);
                            if (tk != null)
                            {
                                tk.TenDangNhap = model.TenDangNhap;
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
                                    TenDangNhap = model.TenDangNhap,
                                    MatKhau = hashedPassword,
                                    Quyen = model.Quyen ?? "Kiem_Lam",
                                    TrangThai = true
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

        // Khóa/Mở khóa tài khoản (thay vì xóa để tránh ảnh hưởng dữ liệu liên quan)
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

                    // Tìm tài khoản của nhân sự
                    var tk = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaNV == id);
                    if (tk == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy tài khoản của nhân sự này!" });
                    }

                    // Kiểm tra xem tài khoản có dữ liệu liên quan không
                    var hasRelatedData = await _context.NhatKyBaoVes.AnyAsync(nk => nk.MaNV_GhiNhan == id) ||
                                        await _context.LichLamViecs.AnyAsync(ll => ll.MaNV == id);

                    if (hasRelatedData)
                    {
                        // Nếu có dữ liệu liên quan, chỉ khóa tài khoản
                        tk.TrangThai = false; // Khóa tài khoản
                        _context.TaiKhoans.Update(tk);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return Json(new
                        {
                            success = true,
                            message = "Đã khóa tài khoản nhân sự thành công! (Không xóa do có dữ liệu liên quan)",
                            isLocked = true
                        });
                    }
                    else
                    {
                        // Nếu không có dữ liệu liên quan, vẫn khóa thay vì xóa (an toàn hơn)
                        tk.TrangThai = false;
                        _context.TaiKhoans.Update(tk);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return Json(new
                        {
                            success = true,
                            message = "Đã khóa tài khoản nhân sự thành công!",
                            isLocked = true
                        });
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Lỗi: " + ex.Message });
                }
            }
        }

        // Mở khóa tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UnlockAccount(int id)
        {
            try
            {
                var tk = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.MaNV == id);
                if (tk == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy tài khoản!" });
                }

                if (tk.TrangThai)
                {
                    return Json(new { success = false, message = "Tài khoản đang ở trạng thái hoạt động!" });
                }

                tk.TrangThai = true;
                _context.TaiKhoans.Update(tk);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Mở khóa tài khoản thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
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

        // API Search real-time (JSON response)
        [HttpGet]
        public async Task<JsonResult> SearchRealtime(string searchString, string roleFilter, string maXaFilter)
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
                                Email = ns.Email ?? "",
                                MaXa = ns.MaXa,
                                TenXa = x != null ? x.TenXa : "Chưa phân công",
                                TenDangNhap = t != null ? t.TenDangNhap : "",
                                Quyen = t != null ? t.Quyen : ""
                            };

                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(x => x.HoTen.Contains(searchString) ||
                                             (x.SDT != null && x.SDT.Contains(searchString)) ||
                                             (x.Email != null && x.Email.Contains(searchString)));
                }
                if (!string.IsNullOrEmpty(roleFilter))
                {
                    query = query.Where(x => x.ChucVu == roleFilter);
                }
                if (!string.IsNullOrEmpty(maXaFilter))
                {
                    query = query.Where(x => x.MaXa == maXaFilter);
                }

                var result = await query.OrderByDescending(x => x.MaNV).ToListAsync();

                return Json(new
                {
                    success = true,
                    items = result,
                    totalRecords = result.Count,
                    message = result.Count == 0 ? "Không tìm thấy dữ liệu" :
                              result.Count == 1 ? "Tìm thấy 1 bản ghi" :
                              $"Tìm thấy {result.Count} bản ghi"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}