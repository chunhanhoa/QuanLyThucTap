using System;
using System.Collections.Generic;

namespace ABC.Models;

public partial class Detai
{
    public string MaDt { get; set; } = null!;

    public string TenDt { get; set; } = null!;

    public string? MoTa { get; set; }

    public virtual ICollection<Sinhvien> Sinhviens { get; set; } = new List<Sinhvien>();
}
