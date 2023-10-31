using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class VaiTro
    {
        public VaiTro()
        {
            PhanQuyens = new HashSet<PhanQuyen>();
        }

        public int Id { get; set; }
        public string MaVaiTro { get; set; } = null!;
        public string? TenVaiTro { get; set; }
        public int? Nvtao { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? Nvsua { get; set; }
        public DateTime? NgaySua { get; set; }
        public bool? Active { get; set; }

        public virtual ICollection<PhanQuyen> PhanQuyens { get; set; }
    }
}
