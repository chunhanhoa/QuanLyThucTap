using System;
using System.Collections.Generic;

namespace ABC.Models;

public partial class Sinhvien
{
    public string MaSv { get; set; } = null!;

    public string TenSv { get; set; } = null!;

    public string? Lop { get; set; }

    public string? Sdt { get; set; }

    public string? Email { get; set; }

    public string? MaGv { get; set; }

    public int? MaNpt { get; set; }

    public string? MaKhoa { get; set; }

    public string? MaDt { get; set; }

    public virtual Detai? MaDtNavigation { get; set; }

    public virtual Giangvien? MaGvNavigation { get; set; }

    public virtual Khoa? MaKhoaNavigation { get; set; }

    public virtual Nguoiphutrach? MaNptNavigation { get; set; }

    public virtual ICollection<Phieudanhgium> Phieudanhgia { get; set; } = new List<Phieudanhgium>();
}
