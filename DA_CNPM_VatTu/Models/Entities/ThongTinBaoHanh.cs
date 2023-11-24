using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class ThongTinBaoHanh
    {
        public int Id { get; set; }
        public int? IdnvbaoHanh { get; set; }
        public DateTime? NgayBaoHanh { get; set; }
        public int? Idctpx { get; set; }

        public virtual ChiTietPhieuXuat? IdctpxNavigation { get; set; }
        public virtual NhanVien? IdnvbaoHanhNavigation { get; set; }
    }
}
