using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyRungPhongHo.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_Clean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DanhMucXas",
                columns: table => new
                {
                    MaXa = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenXa = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMucXas", x => x.MaXa);
                });

            migrationBuilder.CreateTable(
                name: "DanhMucThons",
                columns: table => new
                {
                    MaThon = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenThon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaXa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DanhMucXaMaXa = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMucThons", x => x.MaThon);
                    table.ForeignKey(
                        name: "FK_DanhMucThons_DanhMucXas_DanhMucXaMaXa",
                        column: x => x.DanhMucXaMaXa,
                        principalTable: "DanhMucXas",
                        principalColumn: "MaXa",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NhanSus",
                columns: table => new
                {
                    MaNV = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChucVu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SDT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaXa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DanhMucXaMaXa = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanSus", x => x.MaNV);
                    table.ForeignKey(
                        name: "FK_NhanSus_DanhMucXas_DanhMucXaMaXa",
                        column: x => x.DanhMucXaMaXa,
                        principalTable: "DanhMucXas",
                        principalColumn: "MaXa");
                });

            migrationBuilder.CreateTable(
                name: "LoRungs",
                columns: table => new
                {
                    MaLo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SoTieuKhu = table.Column<int>(type: "int", nullable: true),
                    SoKhoanh = table.Column<int>(type: "int", nullable: true),
                    SoLo = table.Column<int>(type: "int", nullable: true),
                    MaThon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DienTich = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LoaiRung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DanhMucThonMaThon = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoRungs", x => x.MaLo);
                    table.ForeignKey(
                        name: "FK_LoRungs_DanhMucThons_DanhMucThonMaThon",
                        column: x => x.DanhMucThonMaThon,
                        principalTable: "DanhMucThons",
                        principalColumn: "MaThon");
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoans",
                columns: table => new
                {
                    MaTK = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quyen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaNV = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoans", x => x.MaTK);
                    table.ForeignKey(
                        name: "FK_TaiKhoans_NhanSus_MaNV",
                        column: x => x.MaNV,
                        principalTable: "NhanSus",
                        principalColumn: "MaNV",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NhatKyBaoVes",
                columns: table => new
                {
                    MaNK = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NgayGhi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LoaiSuViec = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaLo = table.Column<int>(type: "int", nullable: true),
                    MaNV_GhiNhan = table.Column<int>(type: "int", nullable: true),
                    ToaDoGPS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoRungMaLo = table.Column<int>(type: "int", nullable: true),
                    NhanSuMaNV = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyBaoVes", x => x.MaNK);
                    table.ForeignKey(
                        name: "FK_NhatKyBaoVes_LoRungs_LoRungMaLo",
                        column: x => x.LoRungMaLo,
                        principalTable: "LoRungs",
                        principalColumn: "MaLo");
                    table.ForeignKey(
                        name: "FK_NhatKyBaoVes_NhanSus_NhanSuMaNV",
                        column: x => x.NhanSuMaNV,
                        principalTable: "NhanSus",
                        principalColumn: "MaNV");
                });

            migrationBuilder.CreateTable(
                name: "SinhVats",
                columns: table => new
                {
                    MaSV = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLoai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoaiSV = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MucDoQuyHiem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaLo = table.Column<int>(type: "int", nullable: true),
                    LoRungMaLo = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SinhVats", x => x.MaSV);
                    table.ForeignKey(
                        name: "FK_SinhVats_LoRungs_LoRungMaLo",
                        column: x => x.LoRungMaLo,
                        principalTable: "LoRungs",
                        principalColumn: "MaLo");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DanhMucThons_DanhMucXaMaXa",
                table: "DanhMucThons",
                column: "DanhMucXaMaXa");

            migrationBuilder.CreateIndex(
                name: "IX_LoRungs_DanhMucThonMaThon",
                table: "LoRungs",
                column: "DanhMucThonMaThon");

            migrationBuilder.CreateIndex(
                name: "IX_NhanSus_DanhMucXaMaXa",
                table: "NhanSus",
                column: "DanhMucXaMaXa");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyBaoVes_LoRungMaLo",
                table: "NhatKyBaoVes",
                column: "LoRungMaLo");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyBaoVes_NhanSuMaNV",
                table: "NhatKyBaoVes",
                column: "NhanSuMaNV");

            migrationBuilder.CreateIndex(
                name: "IX_SinhVats_LoRungMaLo",
                table: "SinhVats",
                column: "LoRungMaLo");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoans_MaNV",
                table: "TaiKhoans",
                column: "MaNV",
                unique: true,
                filter: "[MaNV] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoans_TenDangNhap",
                table: "TaiKhoans",
                column: "TenDangNhap",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NhatKyBaoVes");

            migrationBuilder.DropTable(
                name: "SinhVats");

            migrationBuilder.DropTable(
                name: "TaiKhoans");

            migrationBuilder.DropTable(
                name: "LoRungs");

            migrationBuilder.DropTable(
                name: "NhanSus");

            migrationBuilder.DropTable(
                name: "DanhMucThons");

            migrationBuilder.DropTable(
                name: "DanhMucXas");
        }
    }
}
