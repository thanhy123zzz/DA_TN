using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DA_CNPM_VatTu.Models.Entities
{
    public partial class DACNPMContext : DbContext
    {
        public DACNPMContext()
        {
        }

        public DACNPMContext(DbContextOptions<DACNPMContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; } = null!;
        public virtual DbSet<BaoHanh> BaoHanhs { get; set; } = null!;
        public virtual DbSet<CachXuat> CachXuats { get; set; } = null!;
        public virtual DbSet<ChiNhanh> ChiNhanhs { get; set; } = null!;
        public virtual DbSet<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = null!;
        public virtual DbSet<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; } = null!;
        public virtual DbSet<ChucNang> ChucNangs { get; set; } = null!;
        public virtual DbSet<DonViTinh> DonViTinhs { get; set; } = null!;
        public virtual DbSet<GiaTheoKhachHang> GiaTheoKhachHangs { get; set; } = null!;
        public virtual DbSet<GiaTheoNhomHh> GiaTheoNhomHhs { get; set; } = null!;
        public virtual DbSet<HangHoa> HangHoas { get; set; } = null!;
        public virtual DbSet<HangSanXuat> HangSanXuats { get; set; } = null!;
        public virtual DbSet<HangTonKho> HangTonKhos { get; set; } = null!;
        public virtual DbSet<Hhdvt> Hhdvts { get; set; } = null!;
        public virtual DbSet<KhachHang> KhachHangs { get; set; } = null!;
        public virtual DbSet<NhaCungCap> NhaCungCaps { get; set; } = null!;
        public virtual DbSet<NhanVien> NhanViens { get; set; } = null!;
        public virtual DbSet<NhomHangHoa> NhomHangHoas { get; set; } = null!;
        public virtual DbSet<NhomNhanVien> NhomNhanViens { get; set; } = null!;
        public virtual DbSet<NuocSanXuat> NuocSanXuats { get; set; } = null!;
        public virtual DbSet<PhanQuyen> PhanQuyens { get; set; } = null!;
        public virtual DbSet<PhanQuyenChucNang> PhanQuyenChucNangs { get; set; } = null!;
        public virtual DbSet<PhanQuyenNhanVien> PhanQuyenNhanViens { get; set; } = null!;
        public virtual DbSet<PhieuNhapKho> PhieuNhapKhos { get; set; } = null!;
        public virtual DbSet<PhieuXuatKho> PhieuXuatKhos { get; set; } = null!;
        public virtual DbSet<QuyDinhMa> QuyDinhMas { get; set; } = null!;
        public virtual DbSet<SoThuTu> SoThuTus { get; set; } = null!;
        public virtual DbSet<ThongTinBaoHanh> ThongTinBaoHanhs { get; set; } = null!;
        public virtual DbSet<ThongTinDoanhNghiep> ThongTinDoanhNghieps { get; set; } = null!;
        public virtual DbSet<TiLeCanhBao> TiLeCanhBaos { get; set; } = null!;
        public virtual DbSet<TinhGiaXuat> TinhGiaXuats { get; set; } = null!;
        public virtual DbSet<VaiTro> VaiTros { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Data Source=DESKTOP-HRCK5CV\\SQLEXPRESS;Initial Catalog=DACNPM;Integrated Security=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasIndex(e => new { e.TaiKhoan, e.MatKhau }, "UQ__Accounts__D56FD66EE2928392")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.MatKhau).HasMaxLength(50);

                entity.Property(e => e.TaiKhoan).HasMaxLength(50);
            });

            modelBuilder.Entity<BaoHanh>(entity =>
            {
                entity.ToTable("BaoHanh");

                entity.HasIndex(e => e.MaBh, "UQ__BaoHanh__272475A2C6BC3184")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.MaBh)
                    .HasMaxLength(50)
                    .HasColumnName("MaBH")
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.NgaySua).HasColumnType("datetime");

                entity.Property(e => e.NgayTao).HasColumnType("datetime");

                entity.Property(e => e.Nvsua).HasColumnName("NVSua");

                entity.Property(e => e.Nvtao).HasColumnName("NVTao");

                entity.Property(e => e.TenBh)
                    .HasMaxLength(500)
                    .HasColumnName("TenBH")
                    .UseCollation("Vietnamese_CI_AS");
            });

            modelBuilder.Entity<CachXuat>(entity =>
            {
                entity.ToTable("CachXuat");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.TheoHsd).HasColumnName("TheoHSD");

                entity.Property(e => e.TheoTgnhap).HasColumnName("TheoTGNhap");
            });

            modelBuilder.Entity<ChiNhanh>(entity =>
            {
                entity.ToTable("ChiNhanh");

                entity.HasIndex(e => e.MaCn, "UQ__ChiNhanh__27258E0F907AA819")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.DiaChi)
                    .HasMaxLength(500)
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.GhiChu)
                    .HasMaxLength(500)
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.MaCn)
                    .HasMaxLength(50)
                    .HasColumnName("MaCN")
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.NgaySua).HasColumnType("datetime");

                entity.Property(e => e.NgayTao).HasColumnType("datetime");

                entity.Property(e => e.Nvsua).HasColumnName("NVSua");

                entity.Property(e => e.Nvtao).HasColumnName("NVTao");

                entity.Property(e => e.Sdt)
                    .HasMaxLength(50)
                    .HasColumnName("SDT")
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.TenCn)
                    .HasMaxLength(500)
                    .HasColumnName("TenCN")
                    .UseCollation("Vietnamese_CI_AS");
            });

            modelBuilder.Entity<ChiTietPhieuNhap>(entity =>
            {
                entity.ToTable("ChiTietPhieuNhap");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Cktm).HasColumnName("CKTM");

                entity.Property(e => e.GhiChu)
                    .HasMaxLength(2000)
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.Hsd)
                    .HasColumnType("datetime")
                    .HasColumnName("HSD");

                entity.Property(e => e.Idbh).HasColumnName("IDBH");

                entity.Property(e => e.Iddvtnhap).HasColumnName("IDDVTNhap");

                entity.Property(e => e.Idhh).HasColumnName("IDHH");

                entity.Property(e => e.Idpn).HasColumnName("IDPN");

                entity.Property(e => e.NgaySua).HasColumnType("datetime");

                entity.Property(e => e.NgayTao).HasColumnType("datetime");

                entity.Property(e => e.Nsx)
                    .HasColumnType("datetime")
                    .HasColumnName("NSX");

                entity.Property(e => e.Nvsua).HasColumnName("NVSua");

                entity.Property(e => e.Nvtao).HasColumnName("NVTao");

                entity.Property(e => e.Sl).HasColumnName("SL");

                entity.Property(e => e.Slqd).HasColumnName("SLQD");

                entity.Property(e => e.SoLo)
                    .HasMaxLength(50)
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.Tgbh).HasColumnName("TGBH");

                entity.HasOne(d => d.IdbhNavigation)
                    .WithMany(p => p.ChiTietPhieuNhaps)
                    .HasForeignKey(d => d.Idbh)
                    .HasConstraintName("FK__ChiTietPhi__IDBH__43D61337");

                entity.HasOne(d => d.IdhhNavigation)
                    .WithMany(p => p.ChiTietPhieuNhaps)
                    .HasForeignKey(d => d.Idhh)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChiTietPhi__IDHH__42E1EEFE");

                entity.HasOne(d => d.IdpnNavigation)
                    .WithMany(p => p.ChiTietPhieuNhaps)
                    .HasForeignKey(d => d.Idpn)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChiTietPhi__IDPN__41EDCAC5");
            });

            modelBuilder.Entity<ChiTietPhieuXuat>(entity =>
            {
                entity.ToTable("ChiTietPhieuXuat");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.Cktm).HasColumnName("CKTM");

                entity.Property(e => e.Idctpn).HasColumnName("IDCTPN");

                entity.Property(e => e.Iddvt).HasColumnName("IDDVT");

                entity.Property(e => e.Idhh).HasColumnName("IDHH");

                entity.Property(e => e.Idpx).HasColumnName("IDPX");

                entity.Property(e => e.NgaySua).HasColumnType("datetime");

                entity.Property(e => e.NgayTao).HasColumnType("datetime");

                entity.Property(e => e.Nvsua).HasColumnName("NVSua");

                entity.Property(e => e.Nvtao).HasColumnName("NVTao");

                entity.Property(e => e.Sl).HasColumnName("SL");

                entity.HasOne(d => d.IdctpnNavigation)
                    .WithMany(p => p.ChiTietPhieuXuats)
                    .HasForeignKey(d => d.Idctpn)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChiTietPh__IDCTP__6DCC4D03");

                entity.HasOne(d => d.IddvtNavigation)
                    .WithMany(p => p.ChiTietPhieuXuats)
                    .HasForeignKey(d => d.Iddvt)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChiTietPh__IDDVT__6CD828CA");

                entity.HasOne(d => d.IdhhNavigation)
                    .WithMany(p => p.ChiTietPhieuXuats)
                    .HasForeignKey(d => d.Idhh)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChiTietPhi__IDHH__6BE40491");

                entity.HasOne(d => d.IdpxNavigation)
                    .WithMany(p => p.ChiTietPhieuXuats)
                    .HasForeignKey(d => d.Idpx)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChiTietPhi__IDPX__6AEFE058");
            });

            modelBuilder.Entity<ChucNang>(entity =>
            {
                entity.ToTable("ChucNang");

                entity.HasIndex(e => e.MaChucNang, "UQ__ChucNang__B26DC2563B2BBDAB")
                    .IsUnique();

                entity.HasIndex(e => e.MaChucNang, "UQ__ChucNang__B26DC2563FC5CDD0")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.MaChucNang).HasMaxLength(50);

                entity.Property(e => e.TenChucNang).HasMaxLength(50);

                entity.Property(e => e.Url).HasMaxLength(50);
            });

            modelBuilder.Entity<DonViTinh>(entity =>
            {
                entity.ToTable("DonViTinh");

                entity.HasIndex(e => e.MaDvt, "UQ__DonViTin__3D895AFF12F05779")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.Idcn).HasColumnName("IDCN");

                entity.Property(e => e.MaDvt)
                    .HasMaxLength(50)
                    .HasColumnName("MaDVT")
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.NgaySua).HasColumnType("datetime");

                entity.Property(e => e.NgayTao).HasColumnType("datetime");

                entity.Property(e => e.Nvsua).HasColumnName("NVSua");

                entity.Property(e => e.Nvtao).HasColumnName("NVTao");

                entity.Property(e => e.TenDvt)
                    .HasMaxLength(500)
                    .HasColumnName("TenDVT")
                    .UseCollation("Vietnamese_CI_AS");
            });

            modelBuilder.Entity<GiaTheoKhachHang>(entity =>
            {
                entity.ToTable("GiaTheoKhachHang");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.Idcn).HasColumnName("IDCN");

                entity.Property(e => e.Iddvt).HasColumnName("IDDVT");

                entity.Property(e => e.Idhh).HasColumnName("IDHH");

                entity.Property(e => e.Idkh).HasColumnName("IDKH");

                entity.Property(e => e.NgaySua).HasColumnType("datetime");

                entity.Property(e => e.NgayTao).HasColumnType("datetime");

                entity.Property(e => e.Nvsua).HasColumnName("NVSua");

                entity.Property(e => e.Nvtao).HasColumnName("NVTao");

                entity.HasOne(d => d.IddvtNavigation)
                    .WithMany(p => p.GiaTheoKhachHangs)
                    .HasForeignKey(d => d.Iddvt)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__GiaTheoKh__IDDVT__59C55456");

                entity.HasOne(d => d.IdhhNavigation)
                    .WithMany(p => p.GiaTheoKhachHangs)
                    .HasForeignKey(d => d.Idhh)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__GiaTheoKha__IDHH__5BAD9CC8");

                entity.HasOne(d => d.IdkhNavigation)
                    .WithMany(p => p.GiaTheoKhachHangs)
                    .HasForeignKey(d => d.Idkh)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__GiaTheoKha__IDKH__5AB9788F");
            });

            modelBuilder.Entity<GiaTheoNhomHh>(entity =>
            {
                entity.ToTable("GiaTheoNhomHH");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.Idcn).HasColumnName("IDCN");

                entity.Property(e => e.Idnhh).HasColumnName("IDNHH");

                entity.Property(e => e.NgaySua).HasColumnType("datetime");

                entity.Property(e => e.NgayTao).HasColumnType("datetime");

                entity.Property(e => e.Nvsua).HasColumnName("NVSua");

                entity.Property(e => e.Nvtao).HasColumnName("NVTao");

                entity.HasOne(d => d.IdnhhNavigation)
                    .WithMany(p => p.GiaTheoNhomHhs)
                    .HasForeignKey(d => d.Idnhh)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__GiaTheoNh__IDNHH__73852659");
            });

            modelBuilder.Entity<HangHoa>(entity =>
            {
                entity.ToTable("HangHoa");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.Avatar)
                    .HasMaxLength(250)
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.IdbaoHanh).HasColumnName("IDBaoHanh");

                entity.Property(e => e.Idcn).HasColumnName("IDCN");

                entity.Property(e => e.Iddvtchinh).HasColumnName("IDDVTChinh");

                entity.Property(e => e.Idhsx).HasColumnName("IDHSX");

                entity.Property(e => e.Idnhh).HasColumnName("IDNHH");

                entity.Property(e => e.Idnsx).HasColumnName("IDNSX");

                entity.Property(e => e.MaHh)
                    .HasMaxLength(50)
                    .HasColumnName("MaHH")
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.NgaySua).HasColumnType("datetime");

                entity.Property(e => e.NgayTao).HasColumnType("datetime");

                entity.Property(e => e.Nvsua).HasColumnName("NVSua");

                entity.Property(e => e.Nvtao).HasColumnName("NVTao");

                entity.Property(e => e.TenHh)
                    .HasMaxLength(500)
                    .HasColumnName("TenHH")
                    .UseCollation("Vietnamese_CI_AS");

                entity.HasOne(d => d.IdbaoHanhNavigation)
                    .WithMany(p => p.HangHoas)
                    .HasForeignKey(d => d.IdbaoHanh)
                    .HasConstraintName("FK__HangHoa__IDBaoHa__15DA3E5D");

                entity.HasOne(d => d.IddvtchinhNavigation)
                    .WithMany(p => p.HangHoas)
                    .HasForeignKey(d => d.Iddvtchinh)
                    .HasConstraintName("FK__HangHoa__IDDVTCh__74AE54BC");

                entity.HasOne(d => d.IdhsxNavigation)
                    .WithMany(p => p.HangHoas)
                    .HasForeignKey(d => d.Idhsx)
                    .HasConstraintName("FK__HangHoa__IDHSX__73BA3083");

                entity.HasOne(d => d.IdnhhNavigation)
                    .WithMany(p => p.HangHoas)
                    .HasForeignKey(d => d.Idnhh)
                    .HasConstraintName("FK__HangHoa__IDNHH__71D1E811");

                entity.HasOne(d => d.IdnsxNavigation)
                    .WithMany(p => p.HangHoas)
                    .HasForeignKey(d => d.Idnsx)
                    .HasConstraintName("FK__HangHoa__IDNSX__72C60C4A");
            });

            modelBuilder.Entity<HangSanXuat>(entity =>
            {
                entity.ToTable("HangSanXuat");

                entity.HasIndex(e => e.MaHsx, "UQ__HangSanX__3C90113D04945F94")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.Idcn).HasColumnName("IDCN");

                entity.Property(e => e.MaHsx)
                    .HasMaxLength(50)
                    .HasColumnName("MaHSX")
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.NgaySua).HasColumnType("datetime");

                entity.Property(e => e.NgayTao).HasColumnType("datetime");

                entity.Property(e => e.Nvsua).HasColumnName("NVSua");

                entity.Property(e => e.Nvtao).HasColumnName("NVTao");

                entity.Property(e => e.TenHsx)
                    .HasMaxLength(500)
                    .HasColumnName("TenHSX")
                    .UseCollation("Vietnamese_CI_AS");
            });

            modelBuilder.Entity<HangTonKho>(entity =>
            {
                entity.ToTable("HangTonKho");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Cktm).HasColumnName("CKTM");

                entity.Property(e => e.Hsd)
                    .HasColumnType("datetime")
                    .HasColumnName("HSD");

                entity.Property(e => e.Idcn).HasColumnName("IDCN");

                entity.Property(e => e.Idctpn).HasColumnName("IDCTPN");

                entity.Property(e => e.Idhh).HasColumnName("IDHH");

                entity.Property(e => e.NgayNhap).HasColumnType("datetime");

                entity.Property(e => e.Slcon).HasColumnName("SLCon");

                entity.HasOne(d => d.IdctpnNavigation)
                    .WithMany(p => p.HangTonKhos)
                    .HasForeignKey(d => d.Idctpn)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__HangTonKh__IDCTP__4F47C5E3");

                entity.HasOne(d => d.IdhhNavigation)
                    .WithMany(p => p.HangTonKhos)
                    .HasForeignKey(d => d.Idhh)
                    .HasConstraintName("FK__HangTonKho__IDHH__16CE6296");
            });

            modelBuilder.Entity<Hhdvt>(entity =>
            {
                entity.ToTable("HHDVT");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.Idcn).HasColumnName("IDCN");

                entity.Property(e => e.Iddvt).HasColumnName("IDDVT");

                entity.Property(e => e.Idhh).HasColumnName("IDHH");

                entity.Property(e => e.NgaySua).HasColumnType("datetime");

                entity.Property(e => e.NgayTao).HasColumnType("datetime");

                entity.Property(e => e.Nvsua).HasColumnName("NVSua");

                entity.Property(e => e.Nvtao).HasColumnName("NVTao");

                entity.Property(e => e.SlquyDoi).HasColumnName("SLQuyDoi");

                entity.HasOne(d => d.IdcnNavigation)
                    .WithMany(p => p.Hhdvts)
                    .HasForeignKey(d => d.Idcn)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__HHDVT__IDCN__2CF2ADDF");

                entity.HasOne(d => d.IddvtNavigation)
                    .WithMany(p => p.Hhdvts)
                    .HasForeignKey(d => d.Iddvt)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__HHDVT__IDDVT__2DE6D218");

                entity.HasOne(d => d.IdhhNavigation)
                    .WithMany(p => p.Hhdvts)
                    .HasForeignKey(d => d.Idhh)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__HHDVT__IDHH__2BFE89A6");
            });

            modelBuilder.Entity<KhachHang>(entity =>
            {
                entity.ToTable("KhachHang");

                entity.HasIndex(e => e.MaKh, "UQ__KhachHan__2725CF1F3F77B0B3")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.DiaChi)
                    .HasMaxLength(500)
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.GhiChu)
                    .HasMaxLength(500)
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.Idnvsale).HasColumnName("IDNVSale");

                entity.Property(e => e.LoaiKh).HasColumnName("LoaiKH");

                entity.Property(e => e.MaKh)
                    .HasMaxLength(50)
                    .HasColumnName("MaKH")
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.MaSoThue)
                    .HasMaxLength(50)
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.NgaySua).HasColumnType("datetime");

                entity.Property(e => e.NgayTao).HasColumnType("datetime");

                entity.Property(e => e.Sdt)
                    .HasMaxLength(50)
                    .HasColumnName("SDT")
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.TenKh)
                    .HasMaxLength(500)
                    .HasColumnName("TenKH")
                    .UseCollation("Vietnamese_CI_AS");

                entity.HasOne(d => d.IdnvsaleNavigation)
                    .WithMany(p => p.KhachHangs)
                    .HasForeignKey(d => d.Idnvsale)
                    .HasConstraintName("FK__KhachHang__IDNVS__797309D9");
            });

            modelBuilder.Entity<NhaCungCap>(entity =>
            {
                entity.ToTable("NhaCungCap");

                entity.HasIndex(e => e.MaNcc, "UQ__NhaCungC__3A185DEA397DBD88")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.DiaChi)
                    .HasMaxLength(500)
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.GhiChu)
                    .HasMaxLength(500)
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.Idcn).HasColumnName("IDCN");

                entity.Property(e => e.MaNcc)
                    .HasMaxLength(50)
                    .HasColumnName("MaNCC")
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.NgaySua).HasColumnType("datetime");

                entity.Property(e => e.NgayTao).HasColumnType("datetime");

                entity.Property(e => e.Nvsua).HasColumnName("NVSua");

                entity.Property(e => e.Nvtao).HasColumnName("NVTao");

                entity.Property(e => e.Sdt)
                    .HasMaxLength(50)
                    .HasColumnName("SDT")
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.TenNcc)
                    .HasMaxLength(500)
                    .HasColumnName("TenNCC")
                    .UseCollation("Vietnamese_CI_AS");
            });

            modelBuilder.Entity<NhanVien>(entity =>
            {
                entity.ToTable("NhanVien");

                entity.HasIndex(e => e.MaNv, "UQ__NhanVien__2725D70B09A0B6DA")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.Avatar)
                    .HasMaxLength(250)
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.Cccd)
                    .HasMaxLength(50)
                    .HasColumnName("CCCD")
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.DiaChi)
                    .HasMaxLength(500)
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.Idnnv).HasColumnName("IDNNV");

                entity.Property(e => e.Idtk).HasColumnName("IDTK");

                entity.Property(e => e.MaNv)
                    .HasMaxLength(50)
                    .HasColumnName("MaNV")
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.NgaySinh).HasColumnType("date");

                entity.Property(e => e.NgaySua).HasColumnType("datetime");

                entity.Property(e => e.NgayTao).HasColumnType("datetime");

                entity.Property(e => e.Nvsua).HasColumnName("NVSua");

                entity.Property(e => e.Nvtao).HasColumnName("NVTao");

                entity.Property(e => e.QueQuan)
                    .HasMaxLength(500)
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.Sdt)
                    .HasMaxLength(50)
                    .HasColumnName("SDT")
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.TenNv)
                    .HasMaxLength(500)
                    .HasColumnName("TenNV")
                    .UseCollation("Vietnamese_CI_AS");

                entity.HasOne(d => d.IdnnvNavigation)
                    .WithMany(p => p.NhanViens)
                    .HasForeignKey(d => d.Idnnv)
                    .HasConstraintName("FK__NhanVien__IDNNV__14270015");

                entity.HasOne(d => d.IdtkNavigation)
                    .WithMany(p => p.NhanViens)
                    .HasForeignKey(d => d.Idtk)
                    .HasConstraintName("FK__NhanVien__IDTK__1332DBDC");
            });

            modelBuilder.Entity<NhomHangHoa>(entity =>
            {
                entity.ToTable("NhomHangHoa");

                entity.HasIndex(e => e.MaNhh, "UQ__NhomHang__3A1BF20FCB8C9E09")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.Idcn).HasColumnName("IDCN");

                entity.Property(e => e.MaNhh)
                    .HasMaxLength(50)
                    .HasColumnName("MaNHH")
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.NgaySua).HasColumnType("datetime");

                entity.Property(e => e.NgayTao).HasColumnType("datetime");

                entity.Property(e => e.Nvsua).HasColumnName("NVSua");

                entity.Property(e => e.Nvtao).HasColumnName("NVTao");

                entity.Property(e => e.TenNhh)
                    .HasMaxLength(500)
                    .HasColumnName("TenNHH")
                    .UseCollation("Vietnamese_CI_AS");
            });

            modelBuilder.Entity<NhomNhanVien>(entity =>
            {
                entity.ToTable("NhomNhanVien");

                entity.HasIndex(e => e.MaNnv, "UQ__NhomNhan__3A1823B1D5237842")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.Idcn).HasColumnName("IDCN");

                entity.Property(e => e.MaNnv)
                    .HasMaxLength(50)
                    .HasColumnName("MaNNV");

                entity.Property(e => e.NgaySua).HasColumnType("datetime");

                entity.Property(e => e.NgayTao).HasColumnType("datetime");

                entity.Property(e => e.Nvsua).HasColumnName("NVSua");

                entity.Property(e => e.Nvtao).HasColumnName("NVTao");

                entity.Property(e => e.TenNnv)
                    .HasMaxLength(50)
                    .HasColumnName("TenNNV");
            });

            modelBuilder.Entity<NuocSanXuat>(entity =>
            {
                entity.ToTable("NuocSanXuat");

                entity.HasIndex(e => e.MaNsx, "UQ__NuocSanX__3A1BDBD3C9C594AF")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.Idcn).HasColumnName("IDCN");

                entity.Property(e => e.MaNsx)
                    .HasMaxLength(50)
                    .HasColumnName("MaNSX")
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.NgaySua).HasColumnType("datetime");

                entity.Property(e => e.NgayTao).HasColumnType("datetime");

                entity.Property(e => e.Nvsua).HasColumnName("NVSua");

                entity.Property(e => e.Nvtao).HasColumnName("NVTao");

                entity.Property(e => e.TenNsx)
                    .HasMaxLength(500)
                    .HasColumnName("TenNSX")
                    .UseCollation("Vietnamese_CI_AS");
            });

            modelBuilder.Entity<PhanQuyen>(entity =>
            {
                entity.ToTable("PhanQuyen");

                entity.HasIndex(e => new { e.Idcn, e.Idvt }, "UQ__PhanQuye__03FA40192109B250")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Idcn).HasColumnName("IDCN");

                entity.Property(e => e.Idvt).HasColumnName("IDVT");

                entity.Property(e => e.NgaySua).HasColumnType("datetime");

                entity.Property(e => e.NgayTao).HasColumnType("datetime");

                entity.Property(e => e.Nvsua).HasColumnName("NVSua");

                entity.Property(e => e.Nvtao).HasColumnName("NVTao");

                entity.HasOne(d => d.IdcnNavigation)
                    .WithMany(p => p.PhanQuyens)
                    .HasForeignKey(d => d.Idcn)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__PhanQuyen__IDCN__5AEE82B9");

                entity.HasOne(d => d.IdvtNavigation)
                    .WithMany(p => p.PhanQuyens)
                    .HasForeignKey(d => d.Idvt)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__PhanQuyen__IDVT__5BE2A6F2");
            });

            modelBuilder.Entity<PhanQuyenChucNang>(entity =>
            {
                entity.ToTable("PhanQuyenChucNang");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.IdchucNang).HasColumnName("IDChucNang");

                entity.Property(e => e.Idpq).HasColumnName("IDPQ");

                entity.Property(e => e.NgaySua).HasColumnType("datetime");

                entity.Property(e => e.NgayTao).HasColumnType("datetime");

                entity.Property(e => e.Nvsua).HasColumnName("NVSua");

                entity.Property(e => e.Nvtao).HasColumnName("NVTao");

                entity.HasOne(d => d.IdchucNangNavigation)
                    .WithMany(p => p.PhanQuyenChucNangs)
                    .HasForeignKey(d => d.IdchucNang)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__PhanQuyen__IDChu__5EBF139D");

                entity.HasOne(d => d.IdpqNavigation)
                    .WithMany(p => p.PhanQuyenChucNangs)
                    .HasForeignKey(d => d.Idpq)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__PhanQuyenC__IDPQ__5FB337D6");
            });

            modelBuilder.Entity<PhanQuyenNhanVien>(entity =>
            {
                entity.ToTable("PhanQuyenNhanVien");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Idnv).HasColumnName("IDNV");

                entity.Property(e => e.Idpq).HasColumnName("IDPQ");

                entity.Property(e => e.NgaySua).HasColumnType("datetime");

                entity.Property(e => e.NgayTao).HasColumnType("datetime");

                entity.Property(e => e.Nvsua).HasColumnName("NVSua");

                entity.Property(e => e.Nvtao).HasColumnName("NVTao");

                entity.HasOne(d => d.IdnvNavigation)
                    .WithMany(p => p.PhanQuyenNhanViens)
                    .HasForeignKey(d => d.Idnv)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__PhanQuyenN__IDNV__5CD6CB2B");

                entity.HasOne(d => d.IdpqNavigation)
                    .WithMany(p => p.PhanQuyenNhanViens)
                    .HasForeignKey(d => d.Idpq)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__PhanQuyenN__IDPQ__5DCAEF64");
            });

            modelBuilder.Entity<PhieuNhapKho>(entity =>
            {
                entity.ToTable("PhieuNhapKho");

                entity.HasIndex(e => e.SoPn, "UQ__PhieuNha__BC3C6A726F8BFF7E")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.GhiChu)
                    .HasMaxLength(2000)
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.Idcn).HasColumnName("IDCN");

                entity.Property(e => e.Idncc).HasColumnName("IDNCC");

                entity.Property(e => e.Idnv).HasColumnName("IDNV");

                entity.Property(e => e.NgayHd)
                    .HasColumnType("date")
                    .HasColumnName("NgayHD");

                entity.Property(e => e.NgaySua).HasColumnType("datetime");

                entity.Property(e => e.NgayTao).HasColumnType("datetime");

                entity.Property(e => e.Nvsua).HasColumnName("NVSua");

                entity.Property(e => e.SoHd)
                    .HasMaxLength(10)
                    .HasColumnName("SoHD")
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.SoPn)
                    .HasMaxLength(50)
                    .HasColumnName("SoPN")
                    .UseCollation("Vietnamese_CI_AS");

                entity.HasOne(d => d.IdcnNavigation)
                    .WithMany(p => p.PhieuNhapKhos)
                    .HasForeignKey(d => d.Idcn)
                    .HasConstraintName("FK__PhieuNhapK__IDCN__40058253");

                entity.HasOne(d => d.IdnccNavigation)
                    .WithMany(p => p.PhieuNhapKhos)
                    .HasForeignKey(d => d.Idncc)
                    .HasConstraintName("FK__PhieuNhap__IDNCC__32AB8735");

                entity.HasOne(d => d.IdnvNavigation)
                    .WithMany(p => p.PhieuNhapKhos)
                    .HasForeignKey(d => d.Idnv)
                    .HasConstraintName("FK__PhieuNhapK__IDNV__339FAB6E");
            });

            modelBuilder.Entity<PhieuXuatKho>(entity =>
            {
                entity.ToTable("PhieuXuatKho");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.GhiChu)
                    .HasMaxLength(10)
                    .IsFixedLength()
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.Idcn).HasColumnName("IDCN");

                entity.Property(e => e.Idkh).HasColumnName("IDKH");

                entity.Property(e => e.Idnv).HasColumnName("IDNV");

                entity.Property(e => e.Idtt).HasColumnName("IDTT");

                entity.Property(e => e.NgayHd)
                    .HasColumnType("datetime")
                    .HasColumnName("NgayHD");

                entity.Property(e => e.NgaySua).HasColumnType("datetime");

                entity.Property(e => e.NgayTao).HasColumnType("datetime");

                entity.Property(e => e.Nvsua).HasColumnName("NVSua");

                entity.Property(e => e.SoHd)
                    .HasMaxLength(50)
                    .HasColumnName("SoHD")
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.SoPx)
                    .HasMaxLength(50)
                    .HasColumnName("SoPX")
                    .UseCollation("Vietnamese_CI_AS");

                entity.HasOne(d => d.IdcnNavigation)
                    .WithMany(p => p.PhieuXuatKhos)
                    .HasForeignKey(d => d.Idcn)
                    .HasConstraintName("FK__PhieuXuatK__IDCN__625A9A57");

                entity.HasOne(d => d.IdkhNavigation)
                    .WithMany(p => p.PhieuXuatKhos)
                    .HasForeignKey(d => d.Idkh)
                    .HasConstraintName("FK__PhieuXuatK__IDKH__6166761E");

                entity.HasOne(d => d.IdnvNavigation)
                    .WithMany(p => p.PhieuXuatKhos)
                    .HasForeignKey(d => d.Idnv)
                    .HasConstraintName("FK__PhieuXuatK__IDNV__634EBE90");
            });

            modelBuilder.Entity<QuyDinhMa>(entity =>
            {
                entity.ToTable("QuyDinhMa");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Idcn).HasColumnName("IDCN");

                entity.Property(e => e.TiepDauNgu)
                    .HasMaxLength(1)
                    .UseCollation("Vietnamese_CI_AS");
            });

            modelBuilder.Entity<SoThuTu>(entity =>
            {
                entity.ToTable("SoThuTu");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Loai)
                    .HasMaxLength(50)
                    .UseCollation("Vietnamese_CI_AS");

                entity.Property(e => e.Ngay).HasColumnType("datetime");

                entity.Property(e => e.Stt).HasColumnName("STT");
            });

            modelBuilder.Entity<ThongTinBaoHanh>(entity =>
            {
                entity.ToTable("ThongTinBaoHanh");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Idctpx).HasColumnName("IDCTPX");

                entity.Property(e => e.IdnvbaoHanh).HasColumnName("IDNVBaoHanh");

                entity.Property(e => e.NgayBaoHanh).HasColumnType("date");

                entity.HasOne(d => d.IdctpxNavigation)
                    .WithMany(p => p.ThongTinBaoHanhs)
                    .HasForeignKey(d => d.Idctpx)
                    .HasConstraintName("FK__ThongTinB__IDCTP__55BFB948");

                entity.HasOne(d => d.IdnvbaoHanhNavigation)
                    .WithMany(p => p.ThongTinBaoHanhs)
                    .HasForeignKey(d => d.IdnvbaoHanh)
                    .HasConstraintName("FK__ThongTinB__IDNVB__51EF2864");
            });

            modelBuilder.Entity<ThongTinDoanhNghiep>(entity =>
            {
                entity.ToTable("ThongTinDoanhNghiep");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("ID");

                entity.Property(e => e.ChuTk).HasMaxLength(200);

                entity.Property(e => e.DiaChi).HasMaxLength(500);

                entity.Property(e => e.DienThoai).HasMaxLength(50);

                entity.Property(e => e.Email).HasMaxLength(50);

                entity.Property(e => e.Mst)
                    .HasMaxLength(50)
                    .HasColumnName("MST");

                entity.Property(e => e.NganHang).HasMaxLength(50);

                entity.Property(e => e.SoTk).HasMaxLength(50);

                entity.Property(e => e.TenDoanhNghiep).HasMaxLength(500);
            });

            modelBuilder.Entity<TiLeCanhBao>(entity =>
            {
                entity.ToTable("TiLeCanhBao");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("ID");

                entity.Property(e => e.TenTiLe).HasMaxLength(50);
            });

            modelBuilder.Entity<TinhGiaXuat>(entity =>
            {
                entity.ToTable("TinhGiaXuat");

                entity.HasIndex(e => e.MaCach, "UQ__TinhGiaX__20E4AE2D93240B13")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.MaCach).HasMaxLength(50);

                entity.Property(e => e.TenCach).HasMaxLength(50);
            });

            modelBuilder.Entity<VaiTro>(entity =>
            {
                entity.ToTable("VaiTro");

                entity.HasIndex(e => e.MaVaiTro, "UQ__VaiTro__C24C41CE5B3E3A10")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Active).HasDefaultValueSql("((1))");

                entity.Property(e => e.MaVaiTro).HasMaxLength(50);

                entity.Property(e => e.NgaySua).HasColumnType("datetime");

                entity.Property(e => e.NgayTao).HasColumnType("datetime");

                entity.Property(e => e.Nvsua).HasColumnName("NVSua");

                entity.Property(e => e.Nvtao).HasColumnName("NVTao");

                entity.Property(e => e.TenVaiTro).HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
