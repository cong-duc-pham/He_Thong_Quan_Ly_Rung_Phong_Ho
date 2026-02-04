// Models/DonXinNghi.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyRungPhongHo.Models
{
    [Table("DonXinNghis")]
    public class DonXinNghi
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaDon { get; set; }

        [Required]
        [ForeignKey("NhanVien")]
        public int MaNV { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime NgayBatDau { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime NgayKetThuc { get; set; }

        [Required]
        [MaxLength(50)]
        public string LoaiNghi { get; set; } = string.Empty;

        public string? LyDo { get; set; }

        [Required]
        [MaxLength(50)]
        public string TrangThai { get; set; } = "Chờ duyệt";

        [ForeignKey("NguoiDuyetDon")]
        public int? NguoiDuyet { get; set; }

        [Column(TypeName = "datetime2(7)")]
        public DateTime? NgayDuyet { get; set; }

        public string? GhiChuDuyet { get; set; }

        [Required]
        [Column(TypeName = "datetime2(7)")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual NhanSu? NhanVien { get; set; }
        public virtual NhanSu? NguoiDuyetDon { get; set; }
    }
}