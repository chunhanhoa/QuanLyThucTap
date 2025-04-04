using System;
using System.Collections.Generic;

namespace ABC.Models;

public partial class Phieudanhgium
{
    public int MaPdg { get; set; }

    public DateOnly? NgayLap { get; set; }

    public string? NhanXet { get; set; }

    public string? MaGv { get; set; }

    public string? MaSv { get; set; }

    public virtual ICollection<CtDanhgium> CtDanhgia { get; set; } = new List<CtDanhgium>();

    public virtual Giangvien? MaGvNavigation { get; set; }

    public virtual Sinhvien? MaSvNavigation { get; set; }
}
