using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLRungPhongHo.Models
{
    [Table("CaLamViecs")]
    public class CaLamViec
    {
        [Key]
        public int MaCa { get; set; }

        [Required]
        [StringLength(100)]
        public string TenCa { get; set; }

        [Required]
        public TimeSpan GioBatDau { get; set; }

        [Required]
        public TimeSpan GioKetThuc { get; set; }

        public string MoTa { get; set; }

        public bool TrangThai { get; set; } = true;
    }
}