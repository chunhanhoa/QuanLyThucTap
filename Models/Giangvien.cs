using System;
using System.Collections.Generic;

namespace ABC.Models;

public partial class Giangvien
{
    public string MaGv { get; set; } = null!;

    public string TenGv { get; set; } = null!;

    public string? BoMon { get; set; }

    public string? Sdt { get; set; }

    public string? Email { get; set; }

    public string? MaKhoa { get; set; }

    public virtual Khoa? MaKhoaNavigation { get; set; }

    public virtual ICollection<Phieudanhgium> Phieudanhgia { get; set; } = new List<Phieudanhgium>();

    public virtual ICollection<Sinhvien> Sinhviens { get; set; } = new List<Sinhvien>();
}
