using DA_CNPM_VatTu.Models.Entities;

namespace DA_CNPM_VatTu.Models.MapData
{
    public class PhieuNhapKhoMap
    {
        public int Id { get; set; }
        public string SoPn { get; set; } = null!;
        public string? SoHd { get; set; }
        public string? NgayHd { get; set; }
        public string? GhiChu { get; set; }
        public int Idnv { get; set; }
        public string? NgayTao { get; set; }
        public int? Nvsua { get; set; }
        public DateTime? NgaySua { get; set; }
        public bool? Active { get; set; }
        public int Idcn { get; set; }
        public int Idncc { get; set; }
        public virtual List<ChiTietPhieuNhapMap> ChiTietPhieuNhaps { get; set; }
    }
}
