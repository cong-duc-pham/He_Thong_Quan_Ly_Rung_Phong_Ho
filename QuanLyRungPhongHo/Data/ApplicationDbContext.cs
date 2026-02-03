using Microsoft.EntityFrameworkCore;
using QLRungPhongHo.Models;
using QuanLyRungPhongHo.Models;

namespace QuanLyRungPhongHo.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<DanhMucXa> DanhMucXas => Set<DanhMucXa>();
        public DbSet<DanhMucThon> DanhMucThons => Set<DanhMucThon>();
        public DbSet<LoRung> LoRungs => Set<LoRung>();
        public DbSet<NhanSu> NhanSus => Set<NhanSu>();
        public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
        public DbSet<SinhVat> SinhVats => Set<SinhVat>();
        public DbSet<NhatKyBaoVe> NhatKyBaoVes => Set<NhatKyBaoVe>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

        public DbSet<CaLamViec> CaLamViecs { get; set; }
        public DbSet<LichLamViec> LichLamViecs { get; set; }
        public DbSet<DonXinNghi> DonXinNghis { get; set; }
        public DbSet<NgayNghiLe> NgayNghiLes { get; set; }
        public DbSet<ChamCong> ChamCongs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- 1. Cấu hình Khóa Chính (Primary Keys) ---
            modelBuilder.Entity<DanhMucXa>().HasKey(x => x.MaXa);
            modelBuilder.Entity<DanhMucThon>().HasKey(t => t.MaThon);
            modelBuilder.Entity<LoRung>().HasKey(l => l.MaLo);
            modelBuilder.Entity<NhanSu>().HasKey(n => n.MaNV);
            modelBuilder.Entity<TaiKhoan>().HasKey(t => t.MaTK);
            modelBuilder.Entity<SinhVat>().HasKey(s => s.MaSV);
            modelBuilder.Entity<NhatKyBaoVe>().HasKey(nk => nk.MaNK);

            // --- 2. Cấu hình Quan Hệ (Relationships) & Khóa Ngoại (Foreign Keys) ---

            
            modelBuilder.Entity<NhanSu>()
                .HasOne(n => n.DanhMucXa)
                .WithMany(x => x.NhanSus) 
                .HasForeignKey(n => n.MaXa) 
                .OnDelete(DeleteBehavior.SetNull); 

            modelBuilder.Entity<DanhMucThon>()
                .HasOne(t => t.DanhMucXa)
                .WithMany(x => x.DanhMucThons) 
                .HasForeignKey(t => t.MaXa) 
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LoRung>()
                .HasOne(l => l.DanhMucThon)
                .WithMany(t => t.LoRungs)
                .HasForeignKey(l => l.MaThon)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TaiKhoan>()
                .HasOne(tk => tk.NhanSu)
                .WithOne(ns => ns.TaiKhoan)
                .HasForeignKey<TaiKhoan>(tk => tk.MaNV)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<NhatKyBaoVe>()
                .HasOne(nk => nk.LoRung)
                .WithMany(l => l.NhatKyBaoVes)
                .HasForeignKey(nk => nk.MaLo)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<NhatKyBaoVe>()
                .HasOne(nk => nk.NhanSu)
                .WithMany(ns => ns.NhatKyBaoVes)
                .HasForeignKey(nk => nk.MaNV_GhiNhan)
                .OnDelete(DeleteBehavior.SetNull);

            // --- 3. Cấu hình Permission & RolePermission ---
            modelBuilder.Entity<Permission>().HasKey(p => p.PermissionId);
            modelBuilder.Entity<RolePermission>().HasKey(rp => rp.RolePermissionId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: Mỗi role chỉ có 1 bản ghi cho mỗi permission
            modelBuilder.Entity<RolePermission>()
                .HasIndex(rp => new { rp.RoleName, rp.PermissionId })
                .IsUnique();


            modelBuilder.Entity<SinhVat>()
                .HasOne(sv => sv.LoRung)
                .WithMany(l => l.SinhVats)
                .HasForeignKey(sv => sv.MaLo)
                .OnDelete(DeleteBehavior.SetNull);


            // Cấu hình quan hệ cho LichLamViec (2 FK đến cùng bảng NhanSus)
            modelBuilder.Entity<LichLamViec>()
                .HasOne(l => l.NhanVien)
                .WithMany()
                .HasForeignKey(l => l.MaNV)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LichLamViec>()
                .HasOne(l => l.NguoiTaoLich)
                .WithMany()
                .HasForeignKey(l => l.NguoiTao)
                .OnDelete(DeleteBehavior.NoAction);

            // Cấu hình quan hệ cho DonXinNghi (2 FK đến cùng bảng NhanSus)
            modelBuilder.Entity<DonXinNghi>()
                .HasOne(d => d.NhanVien)
                .WithMany()
                .HasForeignKey(d => d.MaNV)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DonXinNghi>()
                .HasOne(d => d.NguoiDuyetDon)
                .WithMany()
                .HasForeignKey(d => d.NguoiDuyet)
                .OnDelete(DeleteBehavior.NoAction);

            // Cấu hình unique constraint
            modelBuilder.Entity<LichLamViec>()
                .HasIndex(l => new { l.MaNV, l.NgayLamViec })
                .IsUnique();

            // Cấu hình quan hệ 1:1 giữa LichLamViec và ChamCong
            modelBuilder.Entity<ChamCong>()
                .HasIndex(c => c.MaLich)
                .IsUnique();

            // Seed data cho CaLamViecs
            modelBuilder.Entity<CaLamViec>().HasData(
                new CaLamViec { MaCa = 1, TenCa = "Ca Sáng", GioBatDau = new TimeSpan(7, 0, 0), GioKetThuc = new TimeSpan(11, 0, 0), MoTa = "Ca làm việc buổi sáng", TrangThai = true },
                new CaLamViec { MaCa = 2, TenCa = "Ca Chiều", GioBatDau = new TimeSpan(13, 0, 0), GioKetThuc = new TimeSpan(17, 0, 0), MoTa = "Ca làm việc buổi chiều", TrangThai = true },
                new CaLamViec { MaCa = 3, TenCa = "Ca Tối", GioBatDau = new TimeSpan(18, 0, 0), GioKetThuc = new TimeSpan(22, 0, 0), MoTa = "Ca làm việc buổi tối", TrangThai = true },
                new CaLamViec { MaCa = 4, TenCa = "Ca Hành Chính", GioBatDau = new TimeSpan(8, 0, 0), GioKetThuc = new TimeSpan(17, 0, 0), MoTa = "Ca hành chính cả ngày", TrangThai = true }
            );

            // Seed data cho NgayNghiLes
            modelBuilder.Entity<NgayNghiLe>().HasData(
                new NgayNghiLe { MaNgayNghi = 1, TenNgayNghi = "Tết Nguyên Đán 2026", NgayBatDau = new DateTime(2026, 1, 28), NgayKetThuc = new DateTime(2026, 2, 3), LoaiNgayNghi = "Tết" },
                new NgayNghiLe { MaNgayNghi = 2, TenNgayNghi = "Giỗ Tổ Hùng Vương", NgayBatDau = new DateTime(2026, 4, 10), NgayKetThuc = new DateTime(2026, 4, 10), LoaiNgayNghi = "Lễ" },
                new NgayNghiLe { MaNgayNghi = 3, TenNgayNghi = "Ngày Giải phóng miền Nam", NgayBatDau = new DateTime(2026, 4, 30), NgayKetThuc = new DateTime(2026, 4, 30), LoaiNgayNghi = "Lễ" },
                new NgayNghiLe { MaNgayNghi = 4, TenNgayNghi = "Ngày Quốc tế Lao động", NgayBatDau = new DateTime(2026, 5, 1), NgayKetThuc = new DateTime(2026, 5, 1), LoaiNgayNghi = "Lễ" },
                new NgayNghiLe { MaNgayNghi = 5, TenNgayNghi = "Quốc Khánh", NgayBatDau = new DateTime(2026, 9, 2), NgayKetThuc = new DateTime(2026, 9, 2), LoaiNgayNghi = "Lễ" }
            );

        }
    }
}