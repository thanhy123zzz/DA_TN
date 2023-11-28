using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class HangHoa
    {
        public HangHoa()
        {
            ChiTietPhieuNhaps = new HashSet<ChiTietPhieuNhap>();
            ChiTietPhieuXuats = new HashSet<ChiTietPhieuXuat>();
            GiaTheoKhachHangs = new HashSet<GiaTheoKhachHang>();
            HangTonKhos = new HashSet<HangTonKho>();
            Hhdvts = new HashSet<Hhdvt>();
        }

        public int Id { get; set; }
        public string MaHh { get; set; } = null!;
        public string? TenHh { get; set; }
        public string? Avatar { get; set; }
        public int? Idnhh { get; set; }
        public int? Idhsx { get; set; }
        public int? Idnsx { get; set; }
        public double? TiLeSi { get; set; }
        public double? TiLeLe { get; set; }
        public double? GiaBanSi { get; set; }
        public double? GiaBanLe { get; set; }
        public int? Nvtao { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? Nvsua { get; set; }
        public DateTime? NgaySua { get; set; }
        public bool? Active { get; set; }
        public int? Idcn { get; set; }
        public int? Iddvtchinh { get; set; }
        public int? IdbaoHanh { get; set; }

        public virtual BaoHanh? IdbaoHanhNavigation { get; set; }
        public virtual DonViTinh? IddvtchinhNavigation { get; set; }
        public virtual HangSanXuat? IdhsxNavigation { get; set; }
        public virtual NhomHangHoa? IdnhhNavigation { get; set; }
        public virtual NuocSanXuat? IdnsxNavigation { get; set; }
        public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; }
        public virtual ICollection<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; }
        public virtual ICollection<GiaTheoKhachHang> GiaTheoKhachHangs { get; set; }
        public virtual ICollection<HangTonKho> HangTonKhos { get; set; }
        public virtual ICollection<Hhdvt> Hhdvts { get; set; }
    }
}
