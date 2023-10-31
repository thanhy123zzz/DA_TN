using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class PhanQuyenNhanVien
    {
        public int Id { get; set; }
        public int Idnv { get; set; }
        public int Idpq { get; set; }
        public int? Nvtao { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? Nvsua { get; set; }
        public DateTime? NgaySua { get; set; }
        public bool? Active { get; set; }

        public virtual NhanVien IdnvNavigation { get; set; } = null!;
        public virtual PhanQuyen IdpqNavigation { get; set; } = null!;
    }
}
