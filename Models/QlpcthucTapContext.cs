using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ABC.Models;

public partial class QlpcthucTapContext : DbContext
{
    private readonly ILogger<QlpcthucTapContext> _logger;
    private bool _isPostgreSql => Database.ProviderName?.Contains("Npgsql") == true;
    
    public QlpcthucTapContext()
    {
    }

    public QlpcthucTapContext(DbContextOptions<QlpcthucTapContext> options, ILogger<QlpcthucTapContext> logger = null)
        : base(options)
    {
        _logger = logger;
    }

    public virtual DbSet<CtDanhgium> CtDanhgia { get; set; }
    public virtual DbSet<Detai> Detais { get; set; }
    public virtual DbSet<Doanhnghiep> Doanhnghieps { get; set; }
    public virtual DbSet<Giangvien> Giangviens { get; set; }
    public virtual DbSet<Khoa> Khoas { get; set; }
    public virtual DbSet<Nguoiphutrach> Nguoiphutraches { get; set; }
    public virtual DbSet<Phieudanhgium> Phieudanhgia { get; set; }
    public virtual DbSet<Quyen> Quyens { get; set; }
    public virtual DbSet<Sinhvien> Sinhviens { get; set; }
    public virtual DbSet<Taikhoan> Taikhoans { get; set; }
    public virtual DbSet<Tuan> Tuans { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=LAPTOP-96K2E9GI\\HOA;Database=QLPCThucTap;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _logger?.LogInformation("OnModelCreating được gọi. Provider: {Provider}, IsPostgreSql: {IsPostgreSql}", 
            Database.ProviderName, _isPostgreSql);
            
        // Kiểm tra xem database provider có phải Npgsql không
        var isPostgres = _isPostgreSql;

        // Cấu hình bảng Taikhoan và Quyen trước tiên vì chúng quan trọng
        modelBuilder.Entity<Taikhoan>(entity =>
        {
            entity.HasKey(e => e.MaTk);
            entity.ToTable(isPostgres ? "taikhoan" : "TAIKHOAN");

            // Điều chỉnh tên cột theo provider
            entity.Property(e => e.MaTk)
                .HasColumnName(isPostgres ? "matk" : "maTK");
            
            entity.Property(e => e.MaQuyen)
                .HasColumnName(isPostgres ? "maquyen" : "maQuyen");
            
            entity.Property(e => e.MatKhau)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName(isPostgres ? "matkhau" : "matKhau");
            
            entity.Property(e => e.TaiKhoan)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName(isPostgres ? "taikhoan" : "taiKhoan");

            entity.HasOne(d => d.MaQuyenNavigation)
                .WithMany(p => p.Taikhoans)
                .HasForeignKey(d => d.MaQuyen)
                .HasConstraintName(isPostgres ? "fk_taikhoan_quyen" : "FK_TAIKHOAN_QUYEN");
        });

        modelBuilder.Entity<Quyen>(entity =>
        {
            entity.HasKey(e => e.MaQuyen);
            entity.ToTable(isPostgres ? "quyen" : "QUYEN");

            entity.Property(e => e.MaQuyen)
                .ValueGeneratedNever()
                .HasColumnName(isPostgres ? "maquyen" : "maQuyen");
            
            entity.Property(e => e.TenQuyen)
                .HasMaxLength(50)
                .HasColumnName(isPostgres ? "tenquyen" : "tenQuyen");
        });

        // Cấu hình các entity khác
        modelBuilder.Entity<CtDanhgium>(entity =>
        {
            entity.HasKey(e => new { e.MaPdg, e.MaTuan });
            entity.ToTable(isPostgres ? "ct_danhgia" : "CT_DANHGIA");

            entity.Property(e => e.MaPdg).HasColumnName(isPostgres ? "mapdg" : "maPDG");
            entity.Property(e => e.MaTuan).HasColumnName(isPostgres ? "matuan" : "maTuan");
            entity.Property(e => e.DiemSo)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName(isPostgres ? "diemso" : "diemSo");
            entity.Property(e => e.GhiChu)
                .HasMaxLength(50)
                .HasColumnName(isPostgres ? "ghichu" : "ghiChu");
            entity.Property(e => e.NgayLap)
                .HasDefaultValueSql(isPostgres ? "NOW()" : "(getdate())")
                .HasColumnName(isPostgres ? "ngaylap" : "ngayLap");

            entity.HasOne(d => d.MaPdgNavigation).WithMany(p => p.CtDanhgia)
                .HasForeignKey(d => d.MaPdg)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName(isPostgres ? "fk_ct_danhgia_mapdg" : "FK__CT_DANHGI__maPDG__619B8048");

            entity.HasOne(d => d.MaTuanNavigation).WithMany(p => p.CtDanhgia)
                .HasForeignKey(d => d.MaTuan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName(isPostgres ? "fk_ct_danhgia_matuan" : "FK__CT_DANHGI__maTua__628FA481");
        });

        modelBuilder.Entity<Detai>(entity =>
        {
            entity.HasKey(e => e.MaDt);

            entity.ToTable(isPostgres ? "detai" : "DETAI");

            entity.HasIndex(e => e.MaDt, isPostgres ? "uq_detai_madt" : "UQ__DETAI__7A3EF41207C92CD1").IsUnique();

            entity.Property(e => e.MaDt)
                .HasMaxLength(10)
                .HasColumnName(isPostgres ? "madt" : "maDT");
            entity.Property(e => e.MoTa)
                .HasMaxLength(200)
                .HasColumnName(isPostgres ? "mota" : "moTa");
            entity.Property(e => e.TenDt)
                .HasMaxLength(50)
                .HasColumnName(isPostgres ? "tendt" : "tenDT");
        });

        modelBuilder.Entity<Doanhnghiep>(entity =>
        {
            entity.HasKey(e => e.MaDn);

            entity.ToTable(isPostgres ? "doanhnghiep" : "DOANHNGHIEP");

            entity.HasIndex(e => e.MaDn, isPostgres ? "uq_doanhnghiep_madn" : "UQ__DOANHNGH__7A3EF408705A4064").IsUnique();

            entity.Property(e => e.MaDn)
                .HasMaxLength(10)
                .HasColumnName(isPostgres ? "madn" : "maDN");
            entity.Property(e => e.DiaChi)
                .HasMaxLength(100)
                .HasColumnName(isPostgres ? "diachi" : "diaChi");
            entity.Property(e => e.LinhVuc)
                .HasMaxLength(50)
                .HasColumnName(isPostgres ? "linhvuc" : "linhVuc");
            entity.Property(e => e.TenDn)
                .HasMaxLength(50)
                .HasColumnName(isPostgres ? "tendn" : "tenDN");
        });

        modelBuilder.Entity<Giangvien>(entity =>
        {
            entity.HasKey(e => e.MaGv);

            entity.ToTable(isPostgres ? "giangvien" : "GIANGVIEN");

            entity.HasIndex(e => e.MaGv, isPostgres ? "uq_giangvien_magv" : "UQ__GIANGVIE__7A3E2D74FF47FDD3").IsUnique();

            entity.Property(e => e.MaGv)
                .HasMaxLength(10)
                .HasColumnName(isPostgres ? "magv" : "maGV");
            entity.Property(e => e.BoMon)
                .HasMaxLength(30)
                .HasColumnName(isPostgres ? "bomon" : "boMon");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName(isPostgres ? "email" : "email");
            entity.Property(e => e.MaKhoa)
                .HasMaxLength(10)
                .HasColumnName(isPostgres ? "makhoa" : "maKhoa");
            entity.Property(e => e.Sdt)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName(isPostgres ? "sdt" : "SDT");
            entity.Property(e => e.TenGv)
                .HasMaxLength(50)
                .HasColumnName(isPostgres ? "tengv" : "tenGV");

            entity.HasOne(d => d.MaKhoaNavigation).WithMany(p => p.Giangviens)
                .HasForeignKey(d => d.MaKhoa)
                .HasConstraintName(isPostgres ? "fk_giangvien_makhoa" : "FK__GIANGVIEN__maKho__4E88ABD4");
        });

        modelBuilder.Entity<Khoa>(entity =>
        {
            entity.HasKey(e => e.MaKhoa);

            entity.ToTable(isPostgres ? "khoa" : "KHOA");

            entity.HasIndex(e => e.TenKhoa, isPostgres ? "uq_khoa_tenkhoa" : "UQ__KHOA__35C7DE782B17DF6E").IsUnique();

            entity.HasIndex(e => e.MaKhoa, isPostgres ? "uq_khoa_makhoa" : "UQ__KHOA__C79B8C234AD2695F").IsUnique();

            entity.Property(e => e.MaKhoa)
                .HasMaxLength(10)
                .HasColumnName(isPostgres ? "makhoa" : "maKhoa");
            entity.Property(e => e.TenKhoa)
                .HasMaxLength(50)
                .HasColumnName(isPostgres ? "tenkhoa" : "tenKhoa");
        });

        modelBuilder.Entity<Nguoiphutrach>(entity =>
        {
            entity.HasKey(e => e.MaNpt);

            entity.ToTable(isPostgres ? "nguoiphutrach" : "NGUOIPHUTRACH");

            entity.HasIndex(e => e.MaNpt, isPostgres ? "uq_nguoiphutrach_manpt" : "UQ__NGUOIPHU__269959D5383B5D51").IsUnique();

            entity.Property(e => e.MaNpt).HasColumnName(isPostgres ? "manpt" : "maNPT");
            entity.Property(e => e.ChucVu)
                .HasMaxLength(50)
                .HasColumnName(isPostgres ? "chucvu" : "chucVu");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName(isPostgres ? "email" : "email");
            entity.Property(e => e.MaDn)
                .HasMaxLength(10)
                .HasColumnName(isPostgres ? "madn" : "maDN");
            entity.Property(e => e.Sdt)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName(isPostgres ? "sdt" : "SDT");
            entity.Property(e => e.TenNpt)
                .HasMaxLength(50)
                .HasColumnName(isPostgres ? "tennpt" : "tenNPT");

            entity.HasOne(d => d.MaDnNavigation).WithMany(p => p.Nguoiphutraches)
                .HasForeignKey(d => d.MaDn)
                .HasConstraintName(isPostgres ? "fk_nguoiphutrach_madn" : "FK__NGUOIPHUTR__maDN__46E78A0C");
        });

        modelBuilder.Entity<Phieudanhgium>(entity =>
        {
            entity.HasKey(e => e.MaPdg);

            entity.ToTable(isPostgres ? "phieudanhgia" : "PHIEUDANHGIA");

            entity.HasIndex(e => e.MaPdg, isPostgres ? "uq_phieudanhgia_mapdg" : "UQ__PHIEUDAN__2719D847A3B12B62").IsUnique();

            entity.Property(e => e.MaPdg).HasColumnName(isPostgres ? "mapdg" : "maPDG");
            entity.Property(e => e.MaGv)
                .HasMaxLength(10)
                .HasColumnName(isPostgres ? "magv" : "maGV");
            entity.Property(e => e.MaSv)
                .HasMaxLength(10)
                .HasColumnName(isPostgres ? "masv" : "maSV");
            entity.Property(e => e.NgayLap)
                .HasDefaultValueSql(isPostgres ? "NOW()" : "(getdate())")
                .HasColumnName(isPostgres ? "ngaylap" : "ngayLap");
            entity.Property(e => e.NhanXet)
                .HasMaxLength(50)
                .HasColumnName(isPostgres ? "nhanxet" : "nhanXet");

            entity.HasOne(d => d.MaGvNavigation).WithMany(p => p.Phieudanhgia)
                .HasForeignKey(d => d.MaGv)
                .HasConstraintName(isPostgres ? "fk_phieudanhgia_magv" : "FK__PHIEUDANHG__maGV__5CD6CB2B");

            entity.HasOne(d => d.MaSvNavigation).WithMany(p => p.Phieudanhgia)
                .HasForeignKey(d => d.MaSv)
                .HasConstraintName(isPostgres ? "fk_phieudanhgia_masv" : "FK__PHIEUDANHG__maSV__5DCAEF64");
        });

        modelBuilder.Entity<Quyen>(entity =>
        {
            entity.HasKey(e => e.MaQuyen);

            entity.ToTable(isPostgres ? "quyen" : "QUYEN");

            entity.HasIndex(e => e.TenQuyen, isPostgres ? "uq_quyen_tenquyen" : "UQ__QUYEN__2302FA4E6C13706C").IsUnique();

            entity.HasIndex(e => e.MaQuyen, isPostgres ? "uq_quyen_maquyen" : "UQ__QUYEN__97001DA24B82C2E9").IsUnique();

            entity.Property(e => e.MaQuyen)
                .ValueGeneratedNever()
                .HasColumnName(isPostgres ? "maquyen" : "maQuyen");
            entity.Property(e => e.TenQuyen)
                .HasMaxLength(50)
                .HasColumnName(isPostgres ? "tenquyen" : "tenQuyen");
        });

        modelBuilder.Entity<Sinhvien>(entity =>
        {
            entity.HasKey(e => e.MaSv);

            entity.ToTable(isPostgres ? "sinhvien" : "SINHVIEN");

            entity.HasIndex(e => e.MaSv, isPostgres ? "uq_sinhvien_masv" : "UQ__SINHVIEN__7A227A65CA31B88B").IsUnique();

            entity.Property(e => e.MaSv)
                .HasMaxLength(10)
                .HasColumnName(isPostgres ? "masv" : "maSV");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName(isPostgres ? "email" : "email");
            entity.Property(e => e.Lop)
                .HasMaxLength(30)
                .HasColumnName(isPostgres ? "lop" : "lop");
            entity.Property(e => e.MaDt)
                .HasMaxLength(10)
                .HasColumnName(isPostgres ? "madt" : "maDT");
            entity.Property(e => e.MaGv)
                .HasMaxLength(10)
                .HasColumnName(isPostgres ? "magv" : "maGV");
            entity.Property(e => e.MaKhoa)
                .HasMaxLength(10)
                .HasColumnName(isPostgres ? "makhoa" : "maKhoa");
            entity.Property(e => e.MaNpt).HasColumnName(isPostgres ? "manpt" : "maNPT");
            entity.Property(e => e.Sdt)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName(isPostgres ? "sdt" : "SDT");
            entity.Property(e => e.TenSv)
                .HasMaxLength(50)
                .HasColumnName(isPostgres ? "tensv" : "tenSV");

            entity.HasOne(d => d.MaDtNavigation).WithMany(p => p.Sinhviens)
                .HasForeignKey(d => d.MaDt)
                .HasConstraintName(isPostgres ? "fk_sinhvien_madt" : "FK__SINHVIEN__maDT__5535A963");

            entity.HasOne(d => d.MaGvNavigation).WithMany(p => p.Sinhviens)
                .HasForeignKey(d => d.MaGv)
                .HasConstraintName(isPostgres ? "fk_sinhvien_magv" : "FK__SINHVIEN__maGV__52593CB8");

            entity.HasOne(d => d.MaKhoaNavigation).WithMany(p => p.Sinhviens)
                .HasForeignKey(d => d.MaKhoa)
                .HasConstraintName(isPostgres ? "fk_sinhvien_makhoa" : "FK__SINHVIEN__maKhoa__5441852A");

            entity.HasOne(d => d.MaNptNavigation).WithMany(p => p.Sinhviens)
                .HasForeignKey(d => d.MaNpt)
                .HasConstraintName(isPostgres ? "fk_sinhvien_manpt" : "FK__SINHVIEN__maNPT__534D60F1");
        });

        modelBuilder.Entity<Taikhoan>(entity =>
        {
            entity.HasKey(e => e.MaTk);

            // Sử dụng tên bảng theo provider
            entity.ToTable(_isPostgreSql ? "taikhoan" : "TAIKHOAN");

            // Điều chỉnh tên cột theo provider
            entity.Property(e => e.MaTk).HasColumnName(_isPostgreSql ? "matk" : "maTK");
            entity.Property(e => e.MaQuyen).HasColumnName(_isPostgreSql ? "maquyen" : "maQuyen");
            entity.Property(e => e.MatKhau)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName(_isPostgreSql ? "matkhau" : "matKhau");
            entity.Property(e => e.TaiKhoan)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName(_isPostgreSql ? "taikhoan" : "taiKhoan");

            entity.HasOne(d => d.MaQuyenNavigation).WithMany(p => p.Taikhoans)
                .HasForeignKey(d => d.MaQuyen)
                .HasConstraintName(_isPostgreSql ? "fk_taikhoan_quyen" : "FK_TAIKHOAN_QUYEN");
        });

        modelBuilder.Entity<Tuan>(entity =>
        {
            entity.HasKey(e => e.MaTuan);

            entity.ToTable(isPostgres ? "tuan" : "TUAN");

            entity.HasIndex(e => e.MaTuan, isPostgres ? "uq_tuan_matuan" : "UQ__TUAN__107FEF96AEA9E14E").IsUnique();

            entity.Property(e => e.MaTuan).HasColumnName(isPostgres ? "matuan" : "maTuan");
            entity.Property(e => e.TenTuan)
                .HasMaxLength(50)
                .HasColumnName(isPostgres ? "tentuan" : "tenTuan");
        });

        OnModelCreatingPartial(modelBuilder);
    }
    
    // Method này giúp tạo dữ liệu mẫu khi cần
    public void EnsureSeeded()
    {
        if (_isPostgreSql && Database.CanConnect())
        {
            _logger?.LogInformation("Bắt đầu kiểm tra và tạo dữ liệu mẫu...");
            
            try
            {
                // Tạo bảng Quyen và Taikhoan trước tiên nếu cần
                Database.ExecuteSqlRaw(@"
                    CREATE TABLE IF NOT EXISTS quyen (
                        maquyen INTEGER PRIMARY KEY,
                        tenquyen VARCHAR(50)
                    );
                    
                    CREATE TABLE IF NOT EXISTS taikhoan (
                        matk SERIAL PRIMARY KEY,
                        taikhoan VARCHAR(50) NOT NULL,
                        matkhau VARCHAR(50),
                        maquyen INTEGER REFERENCES quyen(maquyen)
                    );
                ");
                
                _logger?.LogInformation("Đã tạo bảng quyen và taikhoan (nếu chưa tồn tại)");
                
                // Kiểm tra và tạo dữ liệu mẫu cho Quyen
                var quyenCount = Quyens.Count();
                if (quyenCount == 0)
                {
                    _logger?.LogInformation("Thêm dữ liệu mẫu cho bảng quyen");
                    
                    // Thêm dữ liệu quyền cơ bản
                    Database.ExecuteSqlRaw(@"
                        INSERT INTO quyen (maquyen, tenquyen) VALUES 
                        (1, 'Admin'), 
                        (2, 'Giảng viên'), 
                        (3, 'Sinh viên')
                        ON CONFLICT (maquyen) DO NOTHING;
                    ");
                }
                
                // Kiểm tra và tạo tài khoản admin mặc định
                var adminExists = Taikhoans.Any(t => t.TaiKhoan == "admin1");
                if (!adminExists)
                {
                    _logger?.LogInformation("Thêm tài khoản admin mặc định");
                    
                    Database.ExecuteSqlRaw(@"
                        INSERT INTO taikhoan (taikhoan, matkhau, maquyen) VALUES 
                        ('admin1', '123456', 1)
                        ON CONFLICT (taikhoan) DO NOTHING;
                    ");
                }
                
                _logger?.LogInformation("Hoàn thành kiểm tra và tạo dữ liệu mẫu");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Lỗi khi tạo và seed dữ liệu ban đầu");
                throw;
            }
        }
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
