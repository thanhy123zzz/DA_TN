using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class PhanQuyenChucNang
    {
        public int Id { get; set; }
        public int IdchucNang { get; set; }
        public int Idpq { get; set; }
        public bool? Them { get; set; }
        public bool? Sua { get; set; }
        public bool? Xoa { get; set; }
        public bool? TimKiem { get; set; }
        public bool? In { get; set; }
        public bool? Xuat { get; set; }
        public int? Nvtao { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? Nvsua { get; set; }
        public DateTime? NgaySua { get; set; }

        public virtual ChucNang IdchucNangNavigation { get; set; } = null!;
        public virtual PhanQuyen IdpqNavigation { get; set; } = null!;
    }
}
