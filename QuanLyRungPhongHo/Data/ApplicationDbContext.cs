using Microsoft.EntityFrameworkCore;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === Định nghĩa Primary Key bằng Fluent API ===
            modelBuilder.Entity<DanhMucXa>()
                .HasKey(x => x.MaXa);

            modelBuilder.Entity<DanhMucThon>()
                .HasKey(t => t.MaThon);

            modelBuilder.Entity<LoRung>()
                .HasKey(l => l.MaLo);

            modelBuilder.Entity<NhanSu>()
                .HasKey(ns => ns.MaNV);

            modelBuilder.Entity<TaiKhoan>()
                .HasKey(tk => tk.MaTK);

            modelBuilder.Entity<SinhVat>()
                .HasKey(sv => sv.MaSV);

            modelBuilder.Entity<NhatKyBaoVe>()
                .HasKey(nk => nk.MaNK);

            // === Fix quan hệ one-to-one NhanSu - TaiKhoan ===
            modelBuilder.Entity<NhanSu>()
                .HasOne(ns => ns.TaiKhoan)
                .WithOne(tk => tk.NhanSu)
                .HasForeignKey<TaiKhoan>(tk => tk.MaNV)
                .OnDelete(DeleteBehavior.Cascade);

            // === Thêm unique cho TenDangNhap ===
            modelBuilder.Entity<TaiKhoan>()
                .HasIndex(tk => tk.TenDangNhap)
                .IsUnique();
        }
    }
}