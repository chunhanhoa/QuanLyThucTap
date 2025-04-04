using System;
using System.Collections.Generic;

namespace ABC.Models;

public partial class CtDanhgium
{
    public int MaPdg { get; set; }

    public int MaTuan { get; set; }

    public DateOnly? NgayLap { get; set; }

    public decimal? DiemSo { get; set; }

    public string? GhiChu { get; set; }

    public virtual Phieudanhgium MaPdgNavigation { get; set; } = null!;

    public virtual Tuan MaTuanNavigation { get; set; } = null!;
}
