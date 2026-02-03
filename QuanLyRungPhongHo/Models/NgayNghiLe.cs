// Models/NgayNghiLe.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLRungPhongHo.Models
{
    [Table("NgayNghiLes")]
    public class NgayNghiLe
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaNgayNghi { get; set; }

        [Required]
        [MaxLength(200)]
        public string TenNgayNghi { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "date")]
        public DateTime NgayBatDau { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime NgayKetThuc { get; set; }

        [Required]
        [MaxLength(50)]
        public string LoaiNgayNghi { get; set; } = string.Empty;

        public string? GhiChu { get; set; }
    }
}