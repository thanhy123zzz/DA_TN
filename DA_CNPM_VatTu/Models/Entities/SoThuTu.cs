using System;
using System.Collections.Generic;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class SoThuTu
    {
        public int Id { get; set; }
        public DateTime Ngay { get; set; }
        public string Loai { get; set; } = null!;
        public int Stt { get; set; }
    }
}
