using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class HangTonKho
    {
        public int Id { get; set; }
        public int Idctpn { get; set; }
        public DateTime? NgayNhap { get; set; }
        public double? Slcon { get; set; }
        public int? Idcn { get; set; }
        public double? GiaNhap { get; set; }
        public double? Thue { get; set; }
        public double? Cktm { get; set; }
        public DateTime? Hsd { get; set; }
        public int? Idhh { get; set; }

        public virtual ChiTietPhieuNhap IdctpnNavigation { get; set; } = null!;
        public virtual HangHoa? IdhhNavigation { get; set; }
    }
}
