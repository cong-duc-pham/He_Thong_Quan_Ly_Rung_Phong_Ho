// Models/ChamCong.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLRungPhongHo.Models
{
    [Table("ChamCongs")]
    public class ChamCong
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaChamCong { get; set; }

        [Required]
        [ForeignKey("LichLamViec")]
        public int MaLich { get; set; }

        [Column(TypeName = "datetime2(7)")]
        public DateTime? GioVao { get; set; }

        [Column(TypeName = "datetime2(7)")]
        public DateTime? GioRa { get; set; }

        public string? ToaDoGPS_Vao { get; set; }

        public string? ToaDoGPS_Ra { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? SoGioLam { get; set; }

        public string? GhiChu { get; set; }

        // Navigation property
        public virtual LichLamViec? LichLamViec { get; set; }
    }
}