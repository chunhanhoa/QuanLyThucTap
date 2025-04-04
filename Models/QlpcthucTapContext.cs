using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ABC.Models;

public partial class QlpcthucTapContext : DbContext
{
    public QlpcthucTapContext()
    {
    }

    public QlpcthucTapContext(DbContextOptions<QlpcthucTapContext> options)
        : base(options)
    {
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
        bool isPostgreSql = Database.ProviderName?.Contains("Npgsql") == true;

        if (isPostgreSql)
        {
            // Khi sử dụng PostgreSQL, cần đặt tên bảng và cột là chữ thường
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Đổi tên bảng sang chữ thường nếu đang sử dụng PostgreSQL
                entity.SetTableName(entity.GetTableName().ToLower());

                // Đổi tên tất cả các cột sang chữ thường
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.GetColumnName().ToLower());
                }

                // Đổi tên các khóa và ràng buộc
                foreach (var key in entity.GetKeys())
                {
                    if (key.GetName() != null)
                        key.SetName(key.GetName().ToLower());
                }

                foreach (var fk in entity.GetForeignKeys())
                {
                    if (fk.GetConstraintName() != null)
                        fk.SetConstraintName(fk.GetConstraintName().ToLower());
                }

                foreach (var index in entity.GetIndexes())
                {
                    if (index.GetDatabaseName() != null)
                        index.SetDatabaseName(index.GetDatabaseName().ToLower());
                }
            }
        }

        modelBuilder.Entity<CtDanhgium>(entity =>
        {
            entity.HasKey(e => new { e.MaPdg, e.MaTuan }).HasName("PK__CT_DANHG__461E26BF0645D3F8");

            entity.ToTable("CT_DANHGIA");

            entity.Property(e => e.MaPdg).HasColumnName("maPDG");
            entity.Property(e => e.MaTuan).HasColumnName("maTuan");
            entity.Property(e => e.DiemSo)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("diemSo");
            entity.Property(e => e.GhiChu)
                .HasMaxLength(50)
                .HasColumnName("ghiChu");
            entity.Property(e => e.NgayLap)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("ngayLap");

            entity.HasOne(d => d.MaPdgNavigation).WithMany(p => p.CtDanhgia)
                .HasForeignKey(d => d.MaPdg)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CT_DANHGI__maPDG__619B8048");

            entity.HasOne(d => d.MaTuanNavigation).WithMany(p => p.CtDanhgia)
                .HasForeignKey(d => d.MaTuan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CT_DANHGI__maTua__628FA481");
        });

        modelBuilder.Entity<Detai>(entity =>
        {
            entity.HasKey(e => e.MaDt);

            entity.ToTable("DETAI");

            entity.HasIndex(e => e.MaDt, "UQ__DETAI__7A3EF41207C92CD1").IsUnique();

            entity.Property(e => e.MaDt)
                .HasMaxLength(10)
                .HasColumnName("maDT");
            entity.Property(e => e.MoTa)
                .HasMaxLength(200)
                .HasColumnName("moTa");
            entity.Property(e => e.TenDt)
                .HasMaxLength(50)
                .HasColumnName("tenDT");
        });

        modelBuilder.Entity<Doanhnghiep>(entity =>
        {
            entity.HasKey(e => e.MaDn);

            entity.ToTable("DOANHNGHIEP");

            entity.HasIndex(e => e.MaDn, "UQ__DOANHNGH__7A3EF408705A4064").IsUnique();

            entity.Property(e => e.MaDn)
                .HasMaxLength(10)
                .HasColumnName("maDN");
            entity.Property(e => e.DiaChi)
                .HasMaxLength(100)
                .HasColumnName("diaChi");
            entity.Property(e => e.LinhVuc)
                .HasMaxLength(50)
                .HasColumnName("linhVuc");
            entity.Property(e => e.TenDn)
                .HasMaxLength(50)
                .HasColumnName("tenDN");
        });

        modelBuilder.Entity<Giangvien>(entity =>
        {
            entity.HasKey(e => e.MaGv);

            entity.ToTable("GIANGVIEN");

            entity.HasIndex(e => e.MaGv, "UQ__GIANGVIE__7A3E2D74FF47FDD3").IsUnique();

            entity.Property(e => e.MaGv)
                .HasMaxLength(10)
                .HasColumnName("maGV");
            entity.Property(e => e.BoMon)
                .HasMaxLength(30)
                .HasColumnName("boMon");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.MaKhoa)
                .HasMaxLength(10)
                .HasColumnName("maKhoa");
            entity.Property(e => e.Sdt)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("SDT");
            entity.Property(e => e.TenGv)
                .HasMaxLength(50)
                .HasColumnName("tenGV");

            entity.HasOne(d => d.MaKhoaNavigation).WithMany(p => p.Giangviens)
                .HasForeignKey(d => d.MaKhoa)
                .HasConstraintName("FK__GIANGVIEN__maKho__4E88ABD4");
        });

        modelBuilder.Entity<Khoa>(entity =>
        {
            entity.HasKey(e => e.MaKhoa);

            entity.ToTable("KHOA");

            entity.HasIndex(e => e.TenKhoa, "UQ__KHOA__35C7DE782B17DF6E").IsUnique();

            entity.HasIndex(e => e.MaKhoa, "UQ__KHOA__C79B8C234AD2695F").IsUnique();

            entity.Property(e => e.MaKhoa)
                .HasMaxLength(10)
                .HasColumnName("maKhoa");
            entity.Property(e => e.TenKhoa)
                .HasMaxLength(50)
                .HasColumnName("tenKhoa");
        });

        modelBuilder.Entity<Nguoiphutrach>(entity =>
        {
            entity.HasKey(e => e.MaNpt);

            entity.ToTable("NGUOIPHUTRACH");

            entity.HasIndex(e => e.MaNpt, "UQ__NGUOIPHU__269959D5383B5D51").IsUnique();

            entity.Property(e => e.MaNpt).HasColumnName("maNPT");
            entity.Property(e => e.ChucVu)
                .HasMaxLength(50)
                .HasColumnName("chucVu");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.MaDn)
                .HasMaxLength(10)
                .HasColumnName("maDN");
            entity.Property(e => e.Sdt)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("SDT");
            entity.Property(e => e.TenNpt)
                .HasMaxLength(50)
                .HasColumnName("tenNPT");

            entity.HasOne(d => d.MaDnNavigation).WithMany(p => p.Nguoiphutraches)
                .HasForeignKey(d => d.MaDn)
                .HasConstraintName("FK__NGUOIPHUTR__maDN__46E78A0C");
        });

        modelBuilder.Entity<Phieudanhgium>(entity =>
        {
            entity.HasKey(e => e.MaPdg);

            entity.ToTable("PHIEUDANHGIA");

            entity.HasIndex(e => e.MaPdg, "UQ__PHIEUDAN__2719D847A3B12B62").IsUnique();

            entity.Property(e => e.MaPdg).HasColumnName("maPDG");
            entity.Property(e => e.MaGv)
                .HasMaxLength(10)
                .HasColumnName("maGV");
            entity.Property(e => e.MaSv)
                .HasMaxLength(10)
                .HasColumnName("maSV");
            entity.Property(e => e.NgayLap)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("ngayLap");
            entity.Property(e => e.NhanXet)
                .HasMaxLength(50)
                .HasColumnName("nhanXet");

            entity.HasOne(d => d.MaGvNavigation).WithMany(p => p.Phieudanhgia)
                .HasForeignKey(d => d.MaGv)
                .HasConstraintName("FK__PHIEUDANHG__maGV__5CD6CB2B");

            entity.HasOne(d => d.MaSvNavigation).WithMany(p => p.Phieudanhgia)
                .HasForeignKey(d => d.MaSv)
                .HasConstraintName("FK__PHIEUDANHG__maSV__5DCAEF64");
        });

        modelBuilder.Entity<Quyen>(entity =>
        {
            entity.HasKey(e => e.MaQuyen);

            entity.ToTable("QUYEN");

            entity.HasIndex(e => e.TenQuyen, "UQ__QUYEN__2302FA4E6C13706C").IsUnique();

            entity.HasIndex(e => e.MaQuyen, "UQ__QUYEN__97001DA24B82C2E9").IsUnique();

            entity.Property(e => e.MaQuyen)
                .ValueGeneratedNever()
                .HasColumnName("maQuyen");
            entity.Property(e => e.TenQuyen)
                .HasMaxLength(50)
                .HasColumnName("tenQuyen");
        });

        modelBuilder.Entity<Sinhvien>(entity =>
        {
            entity.HasKey(e => e.MaSv);

            entity.ToTable("SINHVIEN");

            entity.HasIndex(e => e.MaSv, "UQ__SINHVIEN__7A227A65CA31B88B").IsUnique();

            entity.Property(e => e.MaSv)
                .HasMaxLength(10)
                .HasColumnName("maSV");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Lop)
                .HasMaxLength(30)
                .HasColumnName("lop");
            entity.Property(e => e.MaDt)
                .HasMaxLength(10)
                .HasColumnName("maDT");
            entity.Property(e => e.MaGv)
                .HasMaxLength(10)
                .HasColumnName("maGV");
            entity.Property(e => e.MaKhoa)
                .HasMaxLength(10)
                .HasColumnName("maKhoa");
            entity.Property(e => e.MaNpt).HasColumnName("maNPT");
            entity.Property(e => e.Sdt)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("SDT");
            entity.Property(e => e.TenSv)
                .HasMaxLength(50)
                .HasColumnName("tenSV");

            entity.HasOne(d => d.MaDtNavigation).WithMany(p => p.Sinhviens)
                .HasForeignKey(d => d.MaDt)
                .HasConstraintName("FK__SINHVIEN__maDT__5535A963");

            entity.HasOne(d => d.MaGvNavigation).WithMany(p => p.Sinhviens)
                .HasForeignKey(d => d.MaGv)
                .HasConstraintName("FK__SINHVIEN__maGV__52593CB8");

            entity.HasOne(d => d.MaKhoaNavigation).WithMany(p => p.Sinhviens)
                .HasForeignKey(d => d.MaKhoa)
                .HasConstraintName("FK__SINHVIEN__maKhoa__5441852A");

            entity.HasOne(d => d.MaNptNavigation).WithMany(p => p.Sinhviens)
                .HasForeignKey(d => d.MaNpt)
                .HasConstraintName("FK__SINHVIEN__maNPT__534D60F1");
        });

        modelBuilder.Entity<Taikhoan>(entity =>
        {
            entity.HasKey(e => e.MaTk);

            entity.ToTable("TAIKHOAN");

            entity.HasIndex(e => e.MaTk, "UQ__TAIKHOAN__7A22625F0B9F8383").IsUnique();

            entity.HasIndex(e => e.TaiKhoan, "UQ__TAIKHOAN__B4C45318ACC3DD47").IsUnique();

            entity.Property(e => e.MaTk).HasColumnName("maTK");
            entity.Property(e => e.MaQuyen).HasColumnName("maQuyen");
            entity.Property(e => e.MatKhau)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("matKhau");
            entity.Property(e => e.TaiKhoan)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("taiKhoan");

            entity.HasOne(d => d.MaQuyenNavigation).WithMany(p => p.Taikhoans)
                .HasForeignKey(d => d.MaQuyen)
                .HasConstraintName("FK_TAIKHOAN_QUYEN");
        });

        modelBuilder.Entity<Tuan>(entity =>
        {
            entity.HasKey(e => e.MaTuan);

            entity.ToTable("TUAN");

            entity.HasIndex(e => e.MaTuan, "UQ__TUAN__107FEF96AEA9E14E").IsUnique();

            entity.Property(e => e.MaTuan).HasColumnName("maTuan");
            entity.Property(e => e.TenTuan)
                .HasMaxLength(50)
                .HasColumnName("tenTuan");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
