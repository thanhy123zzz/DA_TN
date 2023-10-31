using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class ChiNhanh
    {
        public ChiNhanh()
        {
            Hhdvts = new HashSet<Hhdvt>();
            PhanQuyens = new HashSet<PhanQuyen>();
            PhieuNhapKhos = new HashSet<PhieuNhapKho>();
            PhieuXuatKhos = new HashSet<PhieuXuatKho>();
        }

        public int Id { get; set; }
        public string MaCn { get; set; } = null!;
        public string? TenCn { get; set; }
        public string? Sdt { get; set; }
        public string? DiaChi { get; set; }
        public string? Email { get; set; }
        public string? GhiChu { get; set; }
        public int? Nvtao { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? Nvsua { get; set; }
        public DateTime? NgaySua { get; set; }
        public bool? Active { get; set; }

        public virtual ICollection<Hhdvt> Hhdvts { get; set; }
        public virtual ICollection<PhanQuyen> PhanQuyens { get; set; }
        public virtual ICollection<PhieuNhapKho> PhieuNhapKhos { get; set; }
        public virtual ICollection<PhieuXuatKho> PhieuXuatKhos { get; set; }
    }
}
