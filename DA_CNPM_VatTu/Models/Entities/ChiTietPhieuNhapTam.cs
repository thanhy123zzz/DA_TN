using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class ChiTietPhieuNhapTam
    {
        public int Id { get; set; }
        public int? Idpn { get; set; }
        public string? TenHh { get; set; }
        public int? Idhh { get; set; }
        public int? Idbh { get; set; }
        public string? Host { get; set; }
        public double? Sl { get; set; }
        public double? DonGia { get; set; }
        public double? Cktm { get; set; }
        public double? Thue { get; set; }
        public string? SoLo { get; set; }
        public DateTime? Nsx { get; set; }
        public DateTime? Hsd { get; set; }
        public int? Tgbh { get; set; }
        public string? GhiChu { get; set; }
        public string? Dvt { get; set; }
    }
}
