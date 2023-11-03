namespace DA_CNPM_VatTu.Models.MapData
{
    public class PhieuXuatKhoMap
    {
        public int Id { get; set; }
        public string? SoPx { get; set; }
        public int? Idkh { get; set; }
        public int? Idcn { get; set; }
        public int? Idnv { get; set; }
        public string? SoHd { get; set; }
        public string? NgayHd { get; set; }
        public string? GhiChu { get; set; }
        public string? NgayTao { get; set; }
        public int? Nvsua { get; set; }
        public DateTime? NgaySua { get; set; }
        public bool? Active { get; set; }
        public int? Idtt { get; set; }
        public virtual List<ChiTietPhieuXuatMap> ChiTietPhieuXuatMaps { get; set; }
    }
}
