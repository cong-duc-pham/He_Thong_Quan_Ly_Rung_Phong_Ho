using System.ComponentModel.DataAnnotations;

namespace QuanLyRungPhongHo.Models
{
    public class DanhMucXa
    {
        public string MaXa { get; set; } = null!;
        public string TenXa { get; set; } = null!;

        public ICollection<DanhMucThon> DanhMucThons { get; set; } = new List<DanhMucThon>();
        public ICollection<NhanSu> NhanSus { get; set; } = new List<NhanSu>();
    }
}