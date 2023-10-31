using DA_CNPM_VatTu.Models.Entities;

namespace DA_CNPM_VatTu.Models.MapData
{
    public class MapNhanVien
    {
        public IFormFile FormFile { get; set; }
        public NhanVien NhanVien { get; set; }
        public Account Account { get; set; }
        public string NgaySinh { get; set; }
    }
}
