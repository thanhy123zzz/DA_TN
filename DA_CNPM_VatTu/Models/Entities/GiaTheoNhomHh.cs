using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class GiaTheoNhomHh
    {
        public int Id { get; set; }
        public int Idnhh { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
        public double? TiLeSi { get; set; }
        public double? TiLeLe { get; set; }
        public int? Nvtao { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? Nvsua { get; set; }
        public DateTime? NgaySua { get; set; }
        public bool? Active { get; set; }
        public int? Idcn { get; set; }

        public virtual NhomHangHoa IdnhhNavigation { get; set; } = null!;
    }
}
