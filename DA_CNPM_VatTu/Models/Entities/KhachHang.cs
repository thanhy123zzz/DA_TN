using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class KhachHang
    {
        public KhachHang()
        {
            GiaTheoKhachHangs = new HashSet<GiaTheoKhachHang>();
            PhieuXuatKhos = new HashSet<PhieuXuatKho>();
        }

        public int Id { get; set; }
        public string MaKh { get; set; } = null!;
        public string? TenKh { get; set; }
        public string? DiaChi { get; set; }
        public string? Sdt { get; set; }
        public string? Email { get; set; }
        public string? MaSoThue { get; set; }
        public bool? LoaiKh { get; set; }
        public string? GhiChu { get; set; }
        public int? Idnvsale { get; set; }
        public DateTime? NgayTao { get; set; }
        public DateTime? NgaySua { get; set; }
        public bool? Active { get; set; }

        public virtual NhanVien? IdnvsaleNavigation { get; set; }
        public virtual ICollection<GiaTheoKhachHang> GiaTheoKhachHangs { get; set; }
        public virtual ICollection<PhieuXuatKho> PhieuXuatKhos { get; set; }
    }
}
