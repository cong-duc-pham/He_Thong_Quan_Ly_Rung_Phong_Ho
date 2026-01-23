using QuanLyRungPhongHo.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class SinhVat
{
    [Key]
    public int MaSV { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên loài")]
    [StringLength(200, ErrorMessage = "Tên loài không quá 200 ký tự")]
    public string TenLoai { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn loại sinh vật")]
    [StringLength(50)]
    public string LoaiSV { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn mức độ quý hiếm")]
    [StringLength(50)]
    public string MucDoQuyHiem { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn lô rừng")]
    public int MaLo { get; set; }

    [ForeignKey("MaLo")]
    public virtual LoRung? LoRung { get; set; }
}