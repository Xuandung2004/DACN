using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace DACN_Web_API.Models;

public partial class CsdlFinal1Context : DbContext
{
    public CsdlFinal1Context()
    {
    }

    public CsdlFinal1Context(DbContextOptions<CsdlFinal1Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Anh> Anhs { get; set; }

    public virtual DbSet<Danhgium> Danhgia { get; set; }

    public virtual DbSet<Danhmuc> Danhmucs { get; set; }

    public virtual DbSet<Dondattruoc> Dondattruocs { get; set; }

    public virtual DbSet<Donhang> Donhangs { get; set; }

    public virtual DbSet<DonhangChitiet> DonhangChitiets { get; set; }

    public virtual DbSet<Giohang> Giohangs { get; set; }

    public virtual DbSet<Kichthuoc> Kichthuocs { get; set; }

    public virtual DbSet<Ngayhen> Ngayhens { get; set; }

    public virtual DbSet<Nguoidung> Nguoidungs { get; set; }

    public virtual DbSet<Sanpham> Sanphams { get; set; }

    public virtual DbSet<Thanhtoanon> Thanhtoanons { get; set; }

    public virtual DbSet<Thongtinnhan> Thongtinnhans { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=webtest;user=root;password=abc123", Microsoft.EntityFrameworkCore.ServerVersion.Parse("9.4.0-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Anh>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("anh");

            entity.HasIndex(e => e.Id, "ID").IsUnique();

            entity.HasIndex(e => e.SanPhamId, "SanPham_ID");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.SanPhamId).HasColumnName("SanPham_ID");
            entity.Property(e => e.Url)
                .HasMaxLength(255)
                .HasColumnName("URL");

            entity.HasOne(d => d.SanPham).WithMany(p => p.Anhs)
                .HasForeignKey(d => d.SanPhamId)
                .HasConstraintName("anh_ibfk_1");
        });

        modelBuilder.Entity<Danhgium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("danhgia");

            entity.HasIndex(e => e.Id, "ID").IsUnique();

            entity.HasIndex(e => e.NguoiDungId, "NguoiDung_ID");

            entity.HasIndex(e => e.SanPhamId, "SanPham_ID");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.NgayDanhGia).HasColumnType("datetime");
            entity.Property(e => e.NguoiDungId).HasColumnName("NguoiDung_ID");
            entity.Property(e => e.NoiDung).HasColumnType("mediumtext");
            entity.Property(e => e.SanPhamId).HasColumnName("SanPham_ID");

            entity.HasOne(d => d.NguoiDung).WithMany(p => p.Danhgia)
                .HasForeignKey(d => d.NguoiDungId)
                .HasConstraintName("danhgia_ibfk_1");

            entity.HasOne(d => d.SanPham).WithMany(p => p.Danhgia)
                .HasForeignKey(d => d.SanPhamId)
                .HasConstraintName("danhgia_ibfk_2");
        });

        modelBuilder.Entity<Danhmuc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("danhmuc");

            entity.HasIndex(e => e.Id, "ID").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.MoTa).HasColumnType("mediumtext");
            entity.Property(e => e.Slug).HasMaxLength(255);
            entity.Property(e => e.TenDm)
                .HasMaxLength(50)
                .HasColumnName("TenDM");
        });

        modelBuilder.Entity<Dondattruoc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("dondattruoc");

            entity.HasIndex(e => e.Id, "ID").IsUnique();

            entity.HasIndex(e => e.NguoiDungId, "NguoiDung_ID");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.GiaDuKien).HasPrecision(10, 2);
            entity.Property(e => e.MoTa).HasColumnType("mediumtext");
            entity.Property(e => e.NgayHt)
                .HasColumnType("datetime")
                .HasColumnName("NgayHT");
            entity.Property(e => e.NguoiDungId).HasColumnName("NguoiDung_ID");
            entity.Property(e => e.TrangThai)
                .HasDefaultValueSql("'đang xử lý'")
                .HasColumnType("enum('đang xử lý','đã duyệt','đã hủy','đợi đặt cọc','hoàn tất','hoàn thiện sản phẩm')");

            entity.HasOne(d => d.NguoiDung).WithMany(p => p.Dondattruocs)
                .HasForeignKey(d => d.NguoiDungId)
                .HasConstraintName("dondattruoc_ibfk_1");
        });

        modelBuilder.Entity<Donhang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("donhang");

            entity.HasIndex(e => e.DiaChiId, "DiaChi_ID");

            entity.HasIndex(e => e.Id, "ID").IsUnique();

            entity.HasIndex(e => e.NguoiDungId, "NguoiDung_ID");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DiaChiId).HasColumnName("DiaChi_ID");
            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.NgayDat).HasColumnType("datetime");
            entity.Property(e => e.NguoiDungId).HasColumnName("NguoiDung_ID");
            entity.Property(e => e.TrangThai)
                .HasDefaultValueSql("'đang xử lý'")
                .HasColumnType("enum('đang xử lý','đã giao','đã vận chuyển','đã hủy')");

            entity.HasOne(d => d.DiaChi).WithMany(p => p.Donhangs)
                .HasForeignKey(d => d.DiaChiId)
                .HasConstraintName("donhang_ibfk_2");

            entity.HasOne(d => d.NguoiDung).WithMany(p => p.Donhangs)
                .HasForeignKey(d => d.NguoiDungId)
                .HasConstraintName("donhang_ibfk_1");
        });

        modelBuilder.Entity<DonhangChitiet>(entity =>
        {
            entity.HasKey(e => new { e.DonHangId, e.SanPhamId, e.KichThuocId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0, 0 });

            entity.ToTable("donhang_chitiet");

            entity.HasIndex(e => e.KichThuocId, "KichThuoc_ID");

            entity.HasIndex(e => e.SanPhamId, "SanPham_ID");

            entity.Property(e => e.DonHangId).HasColumnName("DonHang_ID");
            entity.Property(e => e.SanPhamId).HasColumnName("SanPham_ID");
            entity.Property(e => e.KichThuocId).HasColumnName("KichThuoc_ID");
            entity.Property(e => e.Gia).HasPrecision(10, 2);

            entity.HasOne(d => d.DonHang).WithMany(p => p.DonhangChitiets)
                .HasForeignKey(d => d.DonHangId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("donhang_chitiet_ibfk_2");

            entity.HasOne(d => d.KichThuoc).WithMany(p => p.DonhangChitiets)
                .HasForeignKey(d => d.KichThuocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("donhang_chitiet_ibfk_3");

            entity.HasOne(d => d.SanPham).WithMany(p => p.DonhangChitiets)
                .HasForeignKey(d => d.SanPhamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("donhang_chitiet_ibfk_1");
        });

        modelBuilder.Entity<Giohang>(entity =>
        {
            entity.HasKey(e => new { e.SanPhamId, e.NguoiDungId, e.KichThuocId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0, 0 });

            entity.ToTable("giohang");

            entity.HasIndex(e => e.KichThuocId, "KichThuoc_ID");

            entity.HasIndex(e => e.NguoiDungId, "NguoiDung_ID");

            entity.Property(e => e.SanPhamId).HasColumnName("SanPham_ID");
            entity.Property(e => e.NguoiDungId).HasColumnName("NguoiDung_ID");
            entity.Property(e => e.KichThuocId).HasColumnName("KichThuoc_ID");

            entity.HasOne(d => d.KichThuoc).WithMany(p => p.Giohangs)
                .HasForeignKey(d => d.KichThuocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("giohang_ibfk_3");

            entity.HasOne(d => d.NguoiDung).WithMany(p => p.Giohangs)
                .HasForeignKey(d => d.NguoiDungId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("giohang_ibfk_2");

            entity.HasOne(d => d.SanPham).WithMany(p => p.Giohangs)
                .HasForeignKey(d => d.SanPhamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("giohang_ibfk_1");
        });

        modelBuilder.Entity<Kichthuoc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("kichthuoc");

            entity.HasIndex(e => e.Id, "ID").IsUnique();

            entity.HasIndex(e => e.SanPhamId, "SanPham_ID");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.SanPhamId).HasColumnName("SanPham_ID");

            entity.HasOne(d => d.SanPham).WithMany(p => p.Kichthuocs)
                .HasForeignKey(d => d.SanPhamId)
                .HasConstraintName("kichthuoc_ibfk_1");
        });

        modelBuilder.Entity<Ngayhen>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ngayhen");

            entity.HasIndex(e => e.Id, "ID").IsUnique();

            entity.HasIndex(e => e.NguoiDungId, "NguoiDung_ID");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Ngay).HasColumnType("datetime");
            entity.Property(e => e.NguoiDungId).HasColumnName("NguoiDung_ID");

            entity.HasOne(d => d.NguoiDung).WithMany(p => p.Ngayhens)
                .HasForeignKey(d => d.NguoiDungId)
                .HasConstraintName("ngayhen_ibfk_1");
        });

        modelBuilder.Entity<Nguoidung>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("nguoidung");

            entity.HasIndex(e => e.Id, "ID").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MatKhau).HasMaxLength(255);
            entity.Property(e => e.NgayTao).HasColumnType("datetime");
            entity.Property(e => e.Sdt)
                .HasMaxLength(20)
                .HasColumnName("SDT");
            entity.Property(e => e.TenDn)
                .HasMaxLength(50)
                .HasColumnName("TenDN");
            entity.Property(e => e.TrangThai).HasColumnType("enum('đang hoạt động','ngừng hoạt động')");
            entity.Property(e => e.ViTri)
                .HasDefaultValueSql("'khachhang'")
                .HasColumnType("enum('khachhang','nhan vien','admin')");
        });

        modelBuilder.Entity<Sanpham>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("sanpham");

            entity.HasIndex(e => e.DanhMucId, "DanhMuc_ID");

            entity.HasIndex(e => e.Id, "ID").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CapNhat).HasColumnType("datetime");
            entity.Property(e => e.DanhMucId).HasColumnName("DanhMuc_ID");
            entity.Property(e => e.MoTa).HasColumnType("mediumtext");
            entity.Property(e => e.NgayTao).HasColumnType("datetime");
            entity.Property(e => e.Slug).HasMaxLength(255);
            entity.Property(e => e.TenSp)
                .HasMaxLength(255)
                .HasColumnName("TenSP");

            entity.HasOne(d => d.DanhMuc).WithMany(p => p.Sanphams)
                .HasForeignKey(d => d.DanhMucId)
                .HasConstraintName("sanpham_ibfk_1");
        });

        modelBuilder.Entity<Thanhtoanon>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("thanhtoanon");

            entity.HasIndex(e => e.DonHangId, "DonHang_ID");

            entity.HasIndex(e => e.Id, "ID").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DonHangId).HasColumnName("DonHang_ID");
            entity.Property(e => e.MaGiaoDich).HasMaxLength(255);
            entity.Property(e => e.NoiDung).HasColumnType("mediumtext");
            entity.Property(e => e.PhuongThuc).HasMaxLength(255);
            entity.Property(e => e.SoTien).HasPrecision(10, 2);
            entity.Property(e => e.ThoiGian).HasColumnType("datetime");
            entity.Property(e => e.TrangThai)
                .HasDefaultValueSql("'Đang chờ'")
                .HasColumnType("enum('Thành công','Thất bại','Đang chờ')");

            entity.HasOne(d => d.DonHang).WithMany(p => p.Thanhtoanons)
                .HasForeignKey(d => d.DonHangId)
                .HasConstraintName("thanhtoanon_ibfk_1");
        });

        modelBuilder.Entity<Thongtinnhan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("thongtinnhan");

            entity.HasIndex(e => e.Id, "ID").IsUnique();

            entity.HasIndex(e => e.NguoiDungId, "NguoiDung_ID");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DiaChiNhan).HasMaxLength(255);
            entity.Property(e => e.NguoiDungId).HasColumnName("NguoiDung_ID");
            entity.Property(e => e.Sdtnn)
                .HasMaxLength(255)
                .HasColumnName("SDTNN");
            entity.Property(e => e.TenNguoiNhan).HasMaxLength(255);

            entity.HasOne(d => d.NguoiDung).WithMany(p => p.Thongtinnhans)
                .HasForeignKey(d => d.NguoiDungId)
                .HasConstraintName("thongtinnhan_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
