using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class GiaTheoKhachHang
    {
        public int Id { get; set; }
        public int Idkh { get; set; }
        public int Idhh { get; set; }
        public int Iddvt { get; set; }
        public double? TiLeSi { get; set; }
        public double? TiLeLe { get; set; }
        public double? GiaBanSi { get; set; }
        public double? GiaBanLe { get; set; }
        public int? Nvtao { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? Nvsua { get; set; }
        public DateTime? NgaySua { get; set; }
        public bool? Active { get; set; }
        public int? Idcn { get; set; }

        public virtual DonViTinh IddvtNavigation { get; set; } = null!;
        public virtual HangHoa IdhhNavigation { get; set; } = null!;
        public virtual KhachHang IdkhNavigation { get; set; } = null!;
    }
}
