using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class TiLeCanhBao
    {
        public long Id { get; set; }
        public string? TenTiLe { get; set; }
        public double? TiLe { get; set; }
    }
}
