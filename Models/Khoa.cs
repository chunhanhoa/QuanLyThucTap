using System;
using System.Collections.Generic;

namespace ABC.Models;

public partial class Khoa
{
    public string MaKhoa { get; set; } = null!;

    public string TenKhoa { get; set; } = null!;

    public virtual ICollection<Giangvien> Giangviens { get; set; } = new List<Giangvien>();

    public virtual ICollection<Sinhvien> Sinhviens { get; set; } = new List<Sinhvien>();
}
