using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class ChiTietPhieuXuat
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

        public virtual ChiTietPhieuNhap IdctpnNavigation { get; set; } = null!;
        public virtual DonViTinh IddvtNavigation { get; set; } = null!;
        public virtual HangHoa IdhhNavigation { get; set; } = null!;
        public virtual PhieuXuatKho IdpxNavigation { get; set; } = null!;
    }
}
