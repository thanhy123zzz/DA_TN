using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class NhaCungCap
    {
        public NhaCungCap()
        {
            PhieuNhapKhos = new HashSet<PhieuNhapKho>();
        }

        public int Id { get; set; }
        public string MaNcc { get; set; } = null!;
        public string? TenNcc { get; set; }
        public string? DiaChi { get; set; }
        public string? Sdt { get; set; }
        public string? Email { get; set; }
        public string? GhiChu { get; set; }
        public int? Nvtao { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? Nvsua { get; set; }
        public DateTime? NgaySua { get; set; }
        public bool? Active { get; set; }
        public int? Idcn { get; set; }

        public virtual ICollection<PhieuNhapKho> PhieuNhapKhos { get; set; }
    }
}
