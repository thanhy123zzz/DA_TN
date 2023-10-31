using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class NhomNhanVien
    {
        public NhomNhanVien()
        {
            NhanViens = new HashSet<NhanVien>();
        }

        public int Id { get; set; }
        public string MaNnv { get; set; } = null!;
        public string? TenNnv { get; set; }
        public int? Nvtao { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? Nvsua { get; set; }
        public DateTime? NgaySua { get; set; }
        public bool? Active { get; set; }
        public int? Idcn { get; set; }

        public virtual ICollection<NhanVien> NhanViens { get; set; }
    }
}
