using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class Account
    {
        public Account()
        {
            NhanViens = new HashSet<NhanVien>();
        }

        public int Id { get; set; }
        public string TaiKhoan { get; set; } = null!;
        public string MatKhau { get; set; } = null!;

        public virtual ICollection<NhanVien> NhanViens { get; set; }
    }
}
