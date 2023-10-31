using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class DonViTinh
    {
        public DonViTinh()
        {
            ChiTietPhieuXuats = new HashSet<ChiTietPhieuXuat>();
            GiaTheoKhachHangs = new HashSet<GiaTheoKhachHang>();
            HangHoas = new HashSet<HangHoa>();
            Hhdvts = new HashSet<Hhdvt>();
        }

        public int Id { get; set; }
        public string MaDvt { get; set; } = null!;
        public string? TenDvt { get; set; }
        public int? Nvtao { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? Nvsua { get; set; }
        public DateTime? NgaySua { get; set; }
        public bool? Active { get; set; }
        public int? Idcn { get; set; }

        public virtual ICollection<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; }
        public virtual ICollection<GiaTheoKhachHang> GiaTheoKhachHangs { get; set; }
        public virtual ICollection<HangHoa> HangHoas { get; set; }
        public virtual ICollection<Hhdvt> Hhdvts { get; set; }
    }
}
