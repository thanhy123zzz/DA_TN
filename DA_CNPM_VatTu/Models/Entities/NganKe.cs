using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class NganKe
    {
        public NganKe()
        {
            HangHoas = new HashSet<HangHoa>();
        }

        public int Id { get; set; }
        public string? TenNganKe { get; set; }
        public int? Idcn { get; set; }
        public int? Nvtao { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? Nvsua { get; set; }
        public DateTime? NgaySua { get; set; }
        public bool? Active { get; set; }

        public virtual ICollection<HangHoa> HangHoas { get; set; }
    }
}
