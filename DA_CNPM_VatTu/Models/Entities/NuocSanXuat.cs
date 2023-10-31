using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class NuocSanXuat
    {
        public NuocSanXuat()
        {
            HangHoas = new HashSet<HangHoa>();
        }

        public int Id { get; set; }
        public string MaNsx { get; set; } = null!;
        public string? TenNsx { get; set; }
        public int? Nvtao { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? Nvsua { get; set; }
        public DateTime? NgaySua { get; set; }
        public bool? Active { get; set; }
        public int? Idcn { get; set; }

        public virtual ICollection<HangHoa> HangHoas { get; set; }
    }
}
