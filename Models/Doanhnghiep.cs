using System;
using System.Collections.Generic;

namespace ABC.Models;

public partial class Doanhnghiep
{
    public string MaDn { get; set; } = null!;

    public string TenDn { get; set; } = null!;

    public string? DiaChi { get; set; }

    public string? LinhVuc { get; set; }

    public virtual ICollection<Nguoiphutrach> Nguoiphutraches { get; set; } = new List<Nguoiphutrach>();
}
