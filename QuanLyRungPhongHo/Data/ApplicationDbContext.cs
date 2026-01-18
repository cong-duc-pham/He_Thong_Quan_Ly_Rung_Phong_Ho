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

            // Cấu hình relationship 1-1 giữa TaiKhoan và NhanSu
            // TaiKhoan là dependent (có foreign key MaNV)
            modelBuilder.Entity<TaiKhoan>()
                .HasOne(tk => tk.NhanSu)
                .WithOne(ns => ns.TaiKhoan)
                .HasForeignKey<TaiKhoan>(tk => tk.MaNV)
                .OnDelete(DeleteBehavior.SetNull);

            // Cấu hình các primary keys
            modelBuilder.Entity<DanhMucXa>()
                .HasKey(x => x.MaXa);

            modelBuilder.Entity<DanhMucThon>()
                .HasKey(t => t.MaThon);

            modelBuilder.Entity<LoRung>()
                .HasKey(l => l.MaLo);

            modelBuilder.Entity<NhanSu>()
                .HasKey(n => n.MaNV);

            modelBuilder.Entity<TaiKhoan>()
                .HasKey(t => t.MaTK);

            modelBuilder.Entity<SinhVat>()
                .HasKey(s => s.MaSV);

            modelBuilder.Entity<NhatKyBaoVe>()
                .HasKey(nk => nk.MaNK);
        }
    }
}