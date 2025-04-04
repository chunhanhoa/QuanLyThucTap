using System;
using System.Collections.Generic;

namespace ABC.Models;

public partial class Quyen
{
    public int MaQuyen { get; set; }

    public string TenQuyen { get; set; } = null!;

    public virtual ICollection<Taikhoan> Taikhoans { get; set; } = new List<Taikhoan>();
}
