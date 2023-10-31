using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class Hhdvt
    {
        public int Id { get; set; }
        public int Idcn { get; set; }
        public int Idhh { get; set; }
        public int Iddvt { get; set; }
        public double? SlquyDoi { get; set; }
        public double? TiLeSi { get; set; }
        public double? TiLeLe { get; set; }
        public double? GiaBanSi { get; set; }
        public double? GiaBanLe { get; set; }
        public int? Nvtao { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? Nvsua { get; set; }
        public DateTime? NgaySua { get; set; }
        public bool? Active { get; set; }

        public virtual ChiNhanh IdcnNavigation { get; set; } = null!;
        public virtual DonViTinh IddvtNavigation { get; set; } = null!;
        public virtual HangHoa IdhhNavigation { get; set; } = null!;
    }
}
