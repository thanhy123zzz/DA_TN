using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class ChiTietPhieuNhap
    {
        public ChiTietPhieuNhap()
        {
            ChiTietPhieuXuats = new HashSet<ChiTietPhieuXuat>();
            HangTonKhos = new HashSet<HangTonKho>();
        }

        public int Id { get; set; }
        public int Idpn { get; set; }
        public int Idhh { get; set; }
        public int? Idbh { get; set; }
        public int? Iddvtnhap { get; set; }
        public double? Slqd { get; set; }
        public double Sl { get; set; }
        public double DonGia { get; set; }
        public double? Cktm { get; set; }
        public double? Thue { get; set; }
        public string? SoLo { get; set; }
        public DateTime? Nsx { get; set; }
        public DateTime? Hsd { get; set; }
        public int? Tgbh { get; set; }
        public string? GhiChu { get; set; }
        public int? Nvtao { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? Nvsua { get; set; }
        public DateTime? NgaySua { get; set; }
        public bool? Active { get; set; }

        public virtual BaoHanh? IdbhNavigation { get; set; }
        public virtual HangHoa IdhhNavigation { get; set; } = null!;
        public virtual PhieuNhapKho IdpnNavigation { get; set; } = null!;
        public virtual ICollection<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; }
        public virtual ICollection<HangTonKho> HangTonKhos { get; set; }
    }
}
