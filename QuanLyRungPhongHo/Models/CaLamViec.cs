
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLRungPhongHo.Models
{
    [Table("CaLamViecs")]
    public class CaLamViec
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaCa { get; set; }

        [Required]
        [MaxLength(100)]
        public string TenCa { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "time(7)")]
        public TimeSpan GioBatDau { get; set; }

        [Required]
        [Column(TypeName = "time(7)")]
        public TimeSpan GioKetThuc { get; set; }

        public string? MoTa { get; set; }

        [Required]
        public bool TrangThai { get; set; } = true;

        // Navigation property
        public virtual ICollection<LichLamViec> LichLamViecs { get; set; } = new List<LichLamViec>();
    }
}