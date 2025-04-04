using System;
using System.Collections.Generic;

namespace ABC.Models;

public partial class Tuan
{
    public int MaTuan { get; set; }

    public string TenTuan { get; set; } = null!;

    public virtual ICollection<CtDanhgium> CtDanhgia { get; set; } = new List<CtDanhgium>();
}
