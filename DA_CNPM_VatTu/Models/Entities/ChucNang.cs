using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class ChucNang
    {
        public ChucNang()
        {
            PhanQuyenChucNangs = new HashSet<PhanQuyenChucNang>();
        }

        public int Id { get; set; }
        public string MaChucNang { get; set; } = null!;
        public string? TenChucNang { get; set; }
        public string? Url { get; set; }

        public virtual ICollection<PhanQuyenChucNang> PhanQuyenChucNangs { get; set; }
    }
}
