using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class BaoHanh
    {
        public BaoHanh()
        {
            ChiTietPhieuNhaps = new HashSet<ChiTietPhieuNhap>();
            HangHoas = new HashSet<HangHoa>();
        }

        public int Id { get; set; }
        public string MaBh { get; set; } = null!;
        public string? TenBh { get; set; }
        public int? SoNgay { get; set; }
        public int? Nvtao { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? Nvsua { get; set; }
        public DateTime? NgaySua { get; set; }
        public bool? Active { get; set; }

        public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; }
        public virtual ICollection<HangHoa> HangHoas { get; set; }
    }
}
