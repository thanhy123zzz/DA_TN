using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class ChiTietPhieuXuatTam
    {
        public int Id { get; set; }
        public int Idhh { get; set; }
        public int Iddvt { get; set; }
        public double? Sl { get; set; }
        public double? DonGia { get; set; }
        public int Idctpn { get; set; }
        public double? Cktm { get; set; }
        public double? Thue { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? Nvsua { get; set; }
        public DateTime? NgaySua { get; set; }
        public string? TenHh { get; set; }
        public string? TenDvt { get; set; }
        public string Host { get; set; } = null!;
    }
}
