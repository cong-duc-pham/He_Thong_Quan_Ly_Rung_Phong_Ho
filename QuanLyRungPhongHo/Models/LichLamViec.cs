// Models/LichLamViec.cs
using QuanLyRungPhongHo.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLRungPhongHo.Models
{
    [Table("LichLamViecs")]
    public class LichLamViec
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaLich { get; set; }

        [Required]
        [ForeignKey("NhanVien")]
        public int MaNV { get; set; }

        [Required]
        [ForeignKey("CaLamViec")]
        public int MaCa { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime NgayLamViec { get; set; }

        [ForeignKey("LoRung")]
        public int? MaLo { get; set; }

        [Required]
        [MaxLength(50)]
        public string TrangThai { get; set; } = "Đã phân công";

        public string? GhiChu { get; set; }

        [Required]
        [Column(TypeName = "datetime2(7)")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [ForeignKey("NguoiTaoLich")]
        public int? NguoiTao { get; set; }

        [Column(TypeName = "datetime2(7)")]
        public DateTime? NgayCapNhat { get; set; }

        // Navigation properties
        public virtual NhanSu? NhanVien { get; set; }
        public virtual NhanSu? NguoiTaoLich { get; set; }
        public virtual CaLamViec? CaLamViec { get; set; }
        public virtual LoRung? LoRung { get; set; }
        public virtual ChamCong? ChamCong { get; set; }
    }
}