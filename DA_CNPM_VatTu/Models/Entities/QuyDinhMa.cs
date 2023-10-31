using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class QuyDinhMa
    {
        public int Id { get; set; }
        public int? DoDai { get; set; }
        public string? TiepDauNgu { get; set; }
        public int? Idcn { get; set; }
    }
}
