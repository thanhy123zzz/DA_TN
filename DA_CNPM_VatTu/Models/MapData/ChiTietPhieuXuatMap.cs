namespace DA_CNPM_VatTu.Models.MapData
{
    public class ChiTietPhieuXuatMap
    {
        public int Id { get; set; }
        public int Idpx { get; set; }
        public int Idhh { get; set; }
        public int Iddvt { get; set; }
        public double? Sl { get; set; }
        public double? DonGia { get; set; }
        public int Idctpn { get; set; }
        public double? Cktm { get; set; }
        public double? Thue { get; set; }
        public int? Nvtao { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? Nvsua { get; set; }
        public DateTime? NgaySua { get; set; }
        public bool? Active { get; set; }
    }
}
