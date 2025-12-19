using System.ComponentModel.DataAnnotations;

namespace QuanLyRungPhongHo.Models
{
    public class DanhMucThon
    {
        public string MaThon { get; set; } = null!;
        public string TenThon { get; set; } = null!;
        public string MaXa { get; set; } = null!;

        public DanhMucXa DanhMucXa { get; set; } = null!;
        public ICollection<LoRung> LoRungs { get; set; } = new List<LoRung>();
    }
}