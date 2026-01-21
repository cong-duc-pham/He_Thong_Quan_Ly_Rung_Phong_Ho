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
        }
    }
}