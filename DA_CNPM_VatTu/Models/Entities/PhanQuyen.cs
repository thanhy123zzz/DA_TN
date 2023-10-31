using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class PhanQuyen
    {
        public PhanQuyen()
        {
            PhanQuyenChucNangs = new HashSet<PhanQuyenChucNang>();
            PhanQuyenNhanViens = new HashSet<PhanQuyenNhanVien>();
        }

        public int Id { get; set; }
        public int Idcn { get; set; }
        public int Idvt { get; set; }
        public int? Nvtao { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? Nvsua { get; set; }
        public DateTime? NgaySua { get; set; }
        public bool? Active { get; set; }

        public virtual ChiNhanh IdcnNavigation { get; set; } = null!;
        public virtual VaiTro IdvtNavigation { get; set; } = null!;
        public virtual ICollection<PhanQuyenChucNang> PhanQuyenChucNangs { get; set; }
        public virtual ICollection<PhanQuyenNhanVien> PhanQuyenNhanViens { get; set; }
    }
}
