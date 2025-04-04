using System;
using System.Collections.Generic;

namespace ABC.Models;

public partial class Nguoiphutrach
{
    public int MaNpt { get; set; }

    public string TenNpt { get; set; } = null!;

    public string? ChucVu { get; set; }

    public string? Sdt { get; set; }

    public string? Email { get; set; }

    public string? MaDn { get; set; }

    public virtual Doanhnghiep? MaDnNavigation { get; set; }

    public virtual ICollection<Sinhvien> Sinhviens { get; set; } = new List<Sinhvien>();
}
