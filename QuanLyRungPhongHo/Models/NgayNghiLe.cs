// Models/NgayNghiLe.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyRungPhongHo.Models
{
    [Table("NgayNghiLes")]
    public class NgayNghiLe
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaNgayNghi { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên ngày nghỉ lễ")]
        [MaxLength(200, ErrorMessage = "Tên ngày nghỉ không được vượt quá 200 ký tự")]
        public string TenNgayNghi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        [Column(TypeName = "date")]
        public DateTime NgayBatDau { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
        [Column(TypeName = "date")]
        public DateTime NgayKetThuc { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại ngày nghỉ")]
        [MaxLength(50, ErrorMessage = "Loại ngày nghỉ không được vượt quá 50 ký tự")]
        public string LoaiNgayNghi { get; set; } = string.Empty;

        public string? GhiChu { get; set; }
    }
}