using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class TinhGiaXuat
    {
        public int Id { get; set; }
        public string MaCach { get; set; } = null!;
        public string TenCach { get; set; } = null!;
        public bool GiaTri { get; set; }
    }
}
