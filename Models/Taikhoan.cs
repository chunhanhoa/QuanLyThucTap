using System;
using System.Collections.Generic;

namespace ABC.Models;

public partial class Taikhoan
{
    public int MaTk { get; set; }

    public string TaiKhoan { get; set; } = null!;

    public string? MatKhau { get; set; }

    public int? MaQuyen { get; set; }

    public virtual Quyen? MaQuyenNavigation { get; set; }
}
