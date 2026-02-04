using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyRungPhongHo.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkScheduleManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tạo bảng CaLamViecs
            migrationBuilder.CreateTable(
                name: "CaLamViecs",
                columns: table => new
                {
                    MaCa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenCa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GioBatDau = table.Column<TimeSpan>(type: "time", nullable: false),
                    GioKetThuc = table.Column<TimeSpan>(type: "time", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaLamViecs", x => x.MaCa);
                });

            // Tạo bảng NgayNghiLes
            migrationBuilder.CreateTable(
                name: "NgayNghiLes",
                columns: table => new
                {
                    MaNgayNghi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenNgayNghi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "date", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "date", nullable: false),
                    LoaiNgayNghi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NgayNghiLes", x => x.MaNgayNghi);
                });

            // Tạo bảng LichLamViecs
            migrationBuilder.CreateTable(
                name: "LichLamViecs",
                columns: table => new
                {
                    MaLich = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNV = table.Column<int>(type: "int", nullable: false),
                    MaCa = table.Column<int>(type: "int", nullable: false),
                    NgayLamViec = table.Column<DateTime>(type: "date", nullable: false),
                    MaLo = table.Column<int>(type: "int", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Đã phân công"),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    NguoiTao = table.Column<int>(type: "int", nullable: true),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichLamViecs", x => x.MaLich);
                    table.ForeignKey(
                        name: "FK_LichLamViecs_CaLamViecs_MaCa",
                        column: x => x.MaCa,
                        principalTable: "CaLamViecs",
                        principalColumn: "MaCa",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LichLamViecs_LoRungs_MaLo",
                        column: x => x.MaLo,
                        principalTable: "LoRungs",
                        principalColumn: "MaLo",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LichLamViecs_NhanSus_MaNV",
                        column: x => x.MaNV,
                        principalTable: "NhanSus",
                        principalColumn: "MaNV",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LichLamViecs_NhanSus_NguoiTao",
                        column: x => x.NguoiTao,
                        principalTable: "NhanSus",
                        principalColumn: "MaNV",
                        onDelete: ReferentialAction.NoAction);
                });

            // Tạo bảng DonXinNghis
            migrationBuilder.CreateTable(
                name: "DonXinNghis",
                columns: table => new
                {
                    MaDon = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNV = table.Column<int>(type: "int", nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "date", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "date", nullable: false),
                    LoaiNghi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Chờ duyệt"),
                    NguoiDuyet = table.Column<int>(type: "int", nullable: true),
                    NgayDuyet = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GhiChuDuyet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonXinNghis", x => x.MaDon);
                    table.ForeignKey(
                        name: "FK_DonXinNghis_NhanSus_MaNV",
                        column: x => x.MaNV,
                        principalTable: "NhanSus",
                        principalColumn: "MaNV",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DonXinNghis_NhanSus_NguoiDuyet",
                        column: x => x.NguoiDuyet,
                        principalTable: "NhanSus",
                        principalColumn: "MaNV",
                        onDelete: ReferentialAction.NoAction);
                });

            // Tạo bảng ChamCongs
            migrationBuilder.CreateTable(
                name: "ChamCongs",
                columns: table => new
                {
                    MaChamCong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaLich = table.Column<int>(type: "int", nullable: false),
                    GioVao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GioRa = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ToaDoGPS_Vao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToaDoGPS_Ra = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoGioLam = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChamCongs", x => x.MaChamCong);
                    table.ForeignKey(
                        name: "FK_ChamCongs_LichLamViecs_MaLich",
                        column: x => x.MaLich,
                        principalTable: "LichLamViecs",
                        principalColumn: "MaLich",
                        onDelete: ReferentialAction.Cascade);
                });

            // Tạo indexes
            migrationBuilder.CreateIndex(
                name: "IX_LichLamViecs_MaNV",
                table: "LichLamViecs",
                column: "MaNV");

            migrationBuilder.CreateIndex(
                name: "IX_LichLamViecs_MaCa",
                table: "LichLamViecs",
                column: "MaCa");

            migrationBuilder.CreateIndex(
                name: "IX_LichLamViecs_MaLo",
                table: "LichLamViecs",
                column: "MaLo");

            migrationBuilder.CreateIndex(
                name: "IX_LichLamViecs_NguoiTao",
                table: "LichLamViecs",
                column: "NguoiTao");

            migrationBuilder.CreateIndex(
                name: "IX_LichLamViecs_NgayLamViec",
                table: "LichLamViecs",
                column: "NgayLamViec");

            migrationBuilder.CreateIndex(
                name: "IX_LichLamViecs_MaNV_NgayLamViec",
                table: "LichLamViecs",
                columns: new[] { "MaNV", "NgayLamViec" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonXinNghis_MaNV",
                table: "DonXinNghis",
                column: "MaNV");

            migrationBuilder.CreateIndex(
                name: "IX_DonXinNghis_NguoiDuyet",
                table: "DonXinNghis",
                column: "NguoiDuyet");

            migrationBuilder.CreateIndex(
                name: "IX_ChamCongs_MaLich",
                table: "ChamCongs",
                column: "MaLich",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ChamCongs");
            migrationBuilder.DropTable(name: "DonXinNghis");
            migrationBuilder.DropTable(name: "LichLamViecs");
            migrationBuilder.DropTable(name: "NgayNghiLes");
            migrationBuilder.DropTable(name: "CaLamViecs");
        }
    }
}