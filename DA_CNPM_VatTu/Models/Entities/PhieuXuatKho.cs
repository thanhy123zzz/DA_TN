using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class PhieuXuatKho
    {
        public PhieuXuatKho()
        {
            ChiTietPhieuXuats = new HashSet<ChiTietPhieuXuat>();
        }

        public int Id { get; set; }
        public string? SoPx { get; set; }
        public int? Idkh { get; set; }
        public int? Idcn { get; set; }
        public int? Idnv { get; set; }
        public string? SoHd { get; set; }
        public DateTime? NgayHd { get; set; }
        public string? GhiChu { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? Nvsua { get; set; }
        public DateTime? NgaySua { get; set; }
        public bool? Active { get; set; }
        public int? Idtt { get; set; }

        public virtual ChiNhanh? IdcnNavigation { get; set; }
        public virtual KhachHang? IdkhNavigation { get; set; }
        public virtual NhanVien? IdnvNavigation { get; set; }
        public virtual ICollection<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; }
    }
}
