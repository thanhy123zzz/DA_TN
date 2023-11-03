using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class PhieuNhapKho
    {
        public PhieuNhapKho()
        {
            ChiTietPhieuNhaps = new HashSet<ChiTietPhieuNhap>();
        }

        public int Id { get; set; }
        public string? SoPn { get; set; }
        public string? SoHd { get; set; }
        public DateTime? NgayHd { get; set; }
        public string? GhiChu { get; set; }
        public int? Idnv { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? Nvsua { get; set; }
        public DateTime? NgaySua { get; set; }
        public bool? Active { get; set; }
        public int? Idcn { get; set; }
        public int? Idncc { get; set; }

        public virtual ChiNhanh? IdcnNavigation { get; set; }
        public virtual NhaCungCap? IdnccNavigation { get; set; }
        public virtual NhanVien? IdnvNavigation { get; set; }
        public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; }
    }
}
