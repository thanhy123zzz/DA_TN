using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class NhanVien
    {
        public NhanVien()
        {
            KhachHangs = new HashSet<KhachHang>();
            PhanQuyenNhanViens = new HashSet<PhanQuyenNhanVien>();
            PhieuNhapKhos = new HashSet<PhieuNhapKho>();
            PhieuXuatKhos = new HashSet<PhieuXuatKho>();
        }

        public int Id { get; set; }
        public string MaNv { get; set; } = null!;
        public string? TenNv { get; set; }
        public bool? GioiTinh { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? DiaChi { get; set; }
        public string? QueQuan { get; set; }
        public string? Sdt { get; set; }
        public string? Email { get; set; }
        public string? Cccd { get; set; }
        public string? Avatar { get; set; }
        public int? Idnnv { get; set; }
        public int? Idtk { get; set; }
        public int? Nvtao { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? Nvsua { get; set; }
        public DateTime? NgaySua { get; set; }
        public bool? Active { get; set; }

        public virtual NhomNhanVien? IdnnvNavigation { get; set; }
        public virtual Account? IdtkNavigation { get; set; }
        public virtual ICollection<KhachHang> KhachHangs { get; set; }
        public virtual ICollection<PhanQuyenNhanVien> PhanQuyenNhanViens { get; set; }
        public virtual ICollection<PhieuNhapKho> PhieuNhapKhos { get; set; }
        public virtual ICollection<PhieuXuatKho> PhieuXuatKhos { get; set; }
    }
}
