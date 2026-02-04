using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyRungPhongHo.Models
{
    [Table("LichLamViecs")]
    public class LichLamViec
    {
        [Key]
        public int MaLich { get; set; }

        [Required]
        public int MaNV { get; set; }

        [Required]
        public int MaCa { get; set; }

        [Required]
        public DateTime NgayLamViec { get; set; }

        public int? MaLo { get; set; }

        [StringLength(50)]
        public string TrangThai { get; set; } = "Đã phân công";

        public string GhiChu { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public int? NguoiTao { get; set; }

        public DateTime? NgayCapNhat { get; set; }

        // Navigation Properties - Đặt tên khớp với ApplicationDbContext
        [ForeignKey("MaNV")]
        public virtual NhanSu NhanVien { get; set; }

        [ForeignKey("MaCa")]
        public virtual CaLamViec CaLamViec { get; set; }

        [ForeignKey("MaLo")]
        public virtual LoRung LoRung { get; set; }

        [ForeignKey("NguoiTao")]
        public virtual NhanSu NguoiTaoLich { get; set; }

        // Collection cho ChamCong
        public virtual ICollection<ChamCong> ChamCongs { get; set; }
    }
}