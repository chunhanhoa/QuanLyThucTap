using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ABC.Models;

public partial class QlpcthucTapContext : DbContext
{
    private bool _isPostgreSql;
    
    public QlpcthucTapContext()
    {
    }

    public QlpcthucTapContext(DbContextOptions<QlpcthucTapContext> options)
        : base(options)
    {
        _isPostgreSql = Database.ProviderName?.Contains("Npgsql") == true;
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
        // Kiểm tra xem database provider có phải Npgsql không
        _isPostgreSql = Database.ProviderName?.Contains("Npgsql") == true;
        
        modelBuilder.Entity<CtDanhgium>(entity =>
        {
            entity.HasKey(e => new { e.MaPdg, e.MaTuan });

            // Nếu là PostgreSQL, đặt tên bảng là chữ thường
            entity.ToTable(_isPostgreSql ? "ct_danhgia" : "CT_DANHGIA");

            entity.Property(e => e.MaPdg).HasColumnName(_isPostgreSql ? "mapdg" : "maPDG");
            entity.Property(e => e.MaTuan).HasColumnName(_isPostgreSql ? "matuan" : "maTuan");
            entity.Property(e => e.DiemSo)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName(_isPostgreSql ? "diemso" : "diemSo");
            entity.Property(e => e.GhiChu)
                .HasMaxLength(50)
                .HasColumnName(_isPostgreSql ? "ghichu" : "ghiChu");
            entity.Property(e => e.NgayLap)
                .HasDefaultValueSql(_isPostgreSql ? "NOW()" : "(getdate())")
                .HasColumnName(_isPostgreSql ? "ngaylap" : "ngayLap");

            entity.HasOne(d => d.MaPdgNavigation).WithMany(p => p.CtDanhgia)
                .HasForeignKey(d => d.MaPdg)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName(_isPostgreSql ? "fk_ct_danhgia_mapdg" : "FK__CT_DANHGI__maPDG__619B8048");

            entity.HasOne(d => d.MaTuanNavigation).WithMany(p => p.CtDanhgia)
                .HasForeignKey(d => d.MaTuan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName(_isPostgreSql ? "fk_ct_danhgia_matuan" : "FK__CT_DANHGI__maTua__628FA481");
        });

        modelBuilder.Entity<Detai>(entity =>
        {
            entity.HasKey(e => e.MaDt);

            entity.ToTable(_isPostgreSql ? "detai" : "DETAI");

            entity.HasIndex(e => e.MaDt, _isPostgreSql ? "uq_detai_madt" : "UQ__DETAI__7A3EF41207C92CD1").IsUnique();

            entity.Property(e => e.MaDt)
                .HasMaxLength(10)
                .HasColumnName(_isPostgreSql ? "madt" : "maDT");
            entity.Property(e => e.MoTa)
                .HasMaxLength(200)
                .HasColumnName(_isPostgreSql ? "mota" : "moTa");
            entity.Property(e => e.TenDt)
                .HasMaxLength(50)
                .HasColumnName(_isPostgreSql ? "tendt" : "tenDT");
        });

        modelBuilder.Entity<Doanhnghiep>(entity =>
        {
            entity.HasKey(e => e.MaDn);

            entity.ToTable(_isPostgreSql ? "doanhnghiep" : "DOANHNGHIEP");

            entity.HasIndex(e => e.MaDn, _isPostgreSql ? "uq_doanhnghiep_madn" : "UQ__DOANHNGH__7A3EF408705A4064").IsUnique();

            entity.Property(e => e.MaDn)
                .HasMaxLength(10)
                .HasColumnName(_isPostgreSql ? "madn" : "maDN");
            entity.Property(e => e.DiaChi)
                .HasMaxLength(100)
                .HasColumnName(_isPostgreSql ? "diachi" : "diaChi");
            entity.Property(e => e.LinhVuc)
                .HasMaxLength(50)
                .HasColumnName(_isPostgreSql ? "linhvuc" : "linhVuc");
            entity.Property(e => e.TenDn)
                .HasMaxLength(50)
                .HasColumnName(_isPostgreSql ? "tendn" : "tenDN");
        });

        modelBuilder.Entity<Giangvien>(entity =>
        {
            entity.HasKey(e => e.MaGv);

            entity.ToTable(_isPostgreSql ? "giangvien" : "GIANGVIEN");

            entity.HasIndex(e => e.MaGv, _isPostgreSql ? "uq_giangvien_magv" : "UQ__GIANGVIE__7A3E2D74FF47FDD3").IsUnique();

            entity.Property(e => e.MaGv)
                .HasMaxLength(10)
                .HasColumnName(_isPostgreSql ? "magv" : "maGV");
            entity.Property(e => e.BoMon)
                .HasMaxLength(30)
                .HasColumnName(_isPostgreSql ? "bomon" : "boMon");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName(_isPostgreSql ? "email" : "email");
            entity.Property(e => e.MaKhoa)
                .HasMaxLength(10)
                .HasColumnName(_isPostgreSql ? "makhoa" : "maKhoa");
            entity.Property(e => e.Sdt)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName(_isPostgreSql ? "sdt" : "SDT");
            entity.Property(e => e.TenGv)
                .HasMaxLength(50)
                .HasColumnName(_isPostgreSql ? "tengv" : "tenGV");

            entity.HasOne(d => d.MaKhoaNavigation).WithMany(p => p.Giangviens)
                .HasForeignKey(d => d.MaKhoa)
                .HasConstraintName(_isPostgreSql ? "fk_giangvien_makhoa" : "FK__GIANGVIEN__maKho__4E88ABD4");
        });

        modelBuilder.Entity<Khoa>(entity =>
        {
            entity.HasKey(e => e.MaKhoa);

            entity.ToTable(_isPostgreSql ? "khoa" : "KHOA");

            entity.HasIndex(e => e.TenKhoa, _isPostgreSql ? "uq_khoa_tenkhoa" : "UQ__KHOA__35C7DE782B17DF6E").IsUnique();

            entity.HasIndex(e => e.MaKhoa, _isPostgreSql ? "uq_khoa_makhoa" : "UQ__KHOA__C79B8C234AD2695F").IsUnique();

            entity.Property(e => e.MaKhoa)
                .HasMaxLength(10)
                .HasColumnName(_isPostgreSql ? "makhoa" : "maKhoa");
            entity.Property(e => e.TenKhoa)
                .HasMaxLength(50)
                .HasColumnName(_isPostgreSql ? "tenkhoa" : "tenKhoa");
        });

        modelBuilder.Entity<Nguoiphutrach>(entity =>
        {
            entity.HasKey(e => e.MaNpt);

            entity.ToTable(_isPostgreSql ? "nguoiphutrach" : "NGUOIPHUTRACH");

            entity.HasIndex(e => e.MaNpt, _isPostgreSql ? "uq_nguoiphutrach_manpt" : "UQ__NGUOIPHU__269959D5383B5D51").IsUnique();

            entity.Property(e => e.MaNpt).HasColumnName(_isPostgreSql ? "manpt" : "maNPT");
            entity.Property(e => e.ChucVu)
                .HasMaxLength(50)
                .HasColumnName(_isPostgreSql ? "chucvu" : "chucVu");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName(_isPostgreSql ? "email" : "email");
            entity.Property(e => e.MaDn)
                .HasMaxLength(10)
                .HasColumnName(_isPostgreSql ? "madn" : "maDN");
            entity.Property(e => e.Sdt)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName(_isPostgreSql ? "sdt" : "SDT");
            entity.Property(e => e.TenNpt)
                .HasMaxLength(50)
                .HasColumnName(_isPostgreSql ? "tennpt" : "tenNPT");

            entity.HasOne(d => d.MaDnNavigation).WithMany(p => p.Nguoiphutraches)
                .HasForeignKey(d => d.MaDn)
                .HasConstraintName(_isPostgreSql ? "fk_nguoiphutrach_madn" : "FK__NGUOIPHUTR__maDN__46E78A0C");
        });

        modelBuilder.Entity<Phieudanhgium>(entity =>
        {
            entity.HasKey(e => e.MaPdg);

            entity.ToTable(_isPostgreSql ? "phieudanhgia" : "PHIEUDANHGIA");

            entity.HasIndex(e => e.MaPdg, _isPostgreSql ? "uq_phieudanhgia_mapdg" : "UQ__PHIEUDAN__2719D847A3B12B62").IsUnique();

            entity.Property(e => e.MaPdg).HasColumnName(_isPostgreSql ? "mapdg" : "maPDG");
            entity.Property(e => e.MaGv)
                .HasMaxLength(10)
                .HasColumnName(_isPostgreSql ? "magv" : "maGV");
            entity.Property(e => e.MaSv)
                .HasMaxLength(10)
                .HasColumnName(_isPostgreSql ? "masv" : "maSV");
            entity.Property(e => e.NgayLap)
                .HasDefaultValueSql(_isPostgreSql ? "NOW()" : "(getdate())")
                .HasColumnName(_isPostgreSql ? "ngaylap" : "ngayLap");
            entity.Property(e => e.NhanXet)
                .HasMaxLength(50)
                .HasColumnName(_isPostgreSql ? "nhanxet" : "nhanXet");

            entity.HasOne(d => d.MaGvNavigation).WithMany(p => p.Phieudanhgia)
                .HasForeignKey(d => d.MaGv)
                .HasConstraintName(_isPostgreSql ? "fk_phieudanhgia_magv" : "FK__PHIEUDANHG__maGV__5CD6CB2B");

            entity.HasOne(d => d.MaSvNavigation).WithMany(p => p.Phieudanhgia)
                .HasForeignKey(d => d.MaSv)
                .HasConstraintName(_isPostgreSql ? "fk_phieudanhgia_masv" : "FK__PHIEUDANHG__maSV__5DCAEF64");
        });

        modelBuilder.Entity<Quyen>(entity =>
        {
            entity.HasKey(e => e.MaQuyen);

            entity.ToTable(_isPostgreSql ? "quyen" : "QUYEN");

            entity.HasIndex(e => e.TenQuyen, _isPostgreSql ? "uq_quyen_tenquyen" : "UQ__QUYEN__2302FA4E6C13706C").IsUnique();

            entity.HasIndex(e => e.MaQuyen, _isPostgreSql ? "uq_quyen_maquyen" : "UQ__QUYEN__97001DA24B82C2E9").IsUnique();

            entity.Property(e => e.MaQuyen)
                .ValueGeneratedNever()
                .HasColumnName(_isPostgreSql ? "maquyen" : "maQuyen");
            entity.Property(e => e.TenQuyen)
                .HasMaxLength(50)
                .HasColumnName(_isPostgreSql ? "tenquyen" : "tenQuyen");
        });

        modelBuilder.Entity<Sinhvien>(entity =>
        {
            entity.HasKey(e => e.MaSv);

            entity.ToTable(_isPostgreSql ? "sinhvien" : "SINHVIEN");

            entity.HasIndex(e => e.MaSv, _isPostgreSql ? "uq_sinhvien_masv" : "UQ__SINHVIEN__7A227A65CA31B88B").IsUnique();

            entity.Property(e => e.MaSv)
                .HasMaxLength(10)
                .HasColumnName(_isPostgreSql ? "masv" : "maSV");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName(_isPostgreSql ? "email" : "email");
            entity.Property(e => e.Lop)
                .HasMaxLength(30)
                .HasColumnName(_isPostgreSql ? "lop" : "lop");
            entity.Property(e => e.MaDt)
                .HasMaxLength(10)
                .HasColumnName(_isPostgreSql ? "madt" : "maDT");
            entity.Property(e => e.MaGv)
                .HasMaxLength(10)
                .HasColumnName(_isPostgreSql ? "magv" : "maGV");
            entity.Property(e => e.MaKhoa)
                .HasMaxLength(10)
                .HasColumnName(_isPostgreSql ? "makhoa" : "maKhoa");
            entity.Property(e => e.MaNpt).HasColumnName(_isPostgreSql ? "manpt" : "maNPT");
            entity.Property(e => e.Sdt)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName(_isPostgreSql ? "sdt" : "SDT");
            entity.Property(e => e.TenSv)
                .HasMaxLength(50)
                .HasColumnName(_isPostgreSql ? "tensv" : "tenSV");

            entity.HasOne(d => d.MaDtNavigation).WithMany(p => p.Sinhviens)
                .HasForeignKey(d => d.MaDt)
                .HasConstraintName(_isPostgreSql ? "fk_sinhvien_madt" : "FK__SINHVIEN__maDT__5535A963");

            entity.HasOne(d => d.MaGvNavigation).WithMany(p => p.Sinhviens)
                .HasForeignKey(d => d.MaGv)
                .HasConstraintName(_isPostgreSql ? "fk_sinhvien_magv" : "FK__SINHVIEN__maGV__52593CB8");

            entity.HasOne(d => d.MaKhoaNavigation).WithMany(p => p.Sinhviens)
                .HasForeignKey(d => d.MaKhoa)
                .HasConstraintName(_isPostgreSql ? "fk_sinhvien_makhoa" : "FK__SINHVIEN__maKhoa__5441852A");

            entity.HasOne(d => d.MaNptNavigation).WithMany(p => p.Sinhviens)
                .HasForeignKey(d => d.MaNpt)
                .HasConstraintName(_isPostgreSql ? "fk_sinhvien_manpt" : "FK__SINHVIEN__maNPT__534D60F1");
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

            entity.ToTable(_isPostgreSql ? "tuan" : "TUAN");

            entity.HasIndex(e => e.MaTuan, _isPostgreSql ? "uq_tuan_matuan" : "UQ__TUAN__107FEF96AEA9E14E").IsUnique();

            entity.Property(e => e.MaTuan).HasColumnName(_isPostgreSql ? "matuan" : "maTuan");
            entity.Property(e => e.TenTuan)
                .HasMaxLength(50)
                .HasColumnName(_isPostgreSql ? "tentuan" : "tenTuan");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
