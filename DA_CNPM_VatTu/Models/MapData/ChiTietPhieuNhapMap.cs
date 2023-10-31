namespace DA_CNPM_VatTu.Models.MapData
{
    public class ChiTietPhieuNhapMap
    {
        public int Id { get; set; }
        public int Idpn { get; set; }
        public int Idhh { get; set; }
        public int? Idbh { get; set; }
        public double Sl { get; set; }
        public double DonGia { get; set; }
        public double? Cktm { get; set; }
        public double? Thue { get; set; }
        public string? SoLo { get; set; } = null!;
        public string? Nsx { get; set; }
        public string? Hsd { get; set; }
        public int? Tgbh { get; set; }
        public string? GhiChu { get; set; }
        public int? Nvtao { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? Nvsua { get; set; }
        public DateTime? NgaySua { get; set; }
        public bool? Active { get; set; }
    }
}
