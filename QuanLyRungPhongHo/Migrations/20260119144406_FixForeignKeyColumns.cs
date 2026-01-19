using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyRungPhongHo.Migrations
{
    /// <inheritdoc />
    public partial class FixForeignKeyColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ==========================================================================================
            // BƯỚC 1: CẮT ĐỨT MỌI LIÊN KẾT (DROP FOREIGN KEYS & INDEXES)
            // Phải làm việc này ĐẦU TIÊN để tránh lỗi "Dependent Constraint"
            // ==========================================================================================

            // 1.1. Cắt liên kết bảng LoRungs (Con của DanhMucThons)
            migrationBuilder.DropForeignKey(
                name: "FK_LoRungs_DanhMucThons_DanhMucThonMaThon",
                table: "LoRungs");

            migrationBuilder.DropIndex(
                name: "IX_LoRungs_DanhMucThonMaThon",
                table: "LoRungs");

            migrationBuilder.DropColumn(
                name: "DanhMucThonMaThon",
                table: "LoRungs");

            // 1.2. Cắt liên kết bảng DanhMucThons (Con của DanhMucXas)
            migrationBuilder.DropForeignKey(
                name: "FK_DanhMucThons_DanhMucXas_DanhMucXaMaXa",
                table: "DanhMucThons");

            migrationBuilder.DropIndex(
                name: "IX_DanhMucThons_DanhMucXaMaXa",
                table: "DanhMucThons");

            migrationBuilder.DropColumn(
                name: "DanhMucXaMaXa",
                table: "DanhMucThons");

            // 1.3. Cắt liên kết bảng NhanSus (Con của DanhMucXas)
            migrationBuilder.DropForeignKey(
                name: "FK_NhanSus_DanhMucXas_DanhMucXaMaXa",
                table: "NhanSus");

            migrationBuilder.DropIndex(
                name: "IX_NhanSus_DanhMucXaMaXa",
                table: "NhanSus");

            migrationBuilder.DropColumn(
                name: "DanhMucXaMaXa",
                table: "NhanSus");

            // 1.4. Cắt liên kết bảng NhatKyBaoVes (Con của LoRungs & NhanSus)
            migrationBuilder.DropForeignKey(
                name: "FK_NhatKyBaoVes_LoRungs_LoRungMaLo",
                table: "NhatKyBaoVes");

            migrationBuilder.DropForeignKey(
                name: "FK_NhatKyBaoVes_NhanSus_NhanSuMaNV",
                table: "NhatKyBaoVes");

            migrationBuilder.DropIndex(
                name: "IX_NhatKyBaoVes_LoRungMaLo",
                table: "NhatKyBaoVes");

            migrationBuilder.DropIndex(
                name: "IX_NhatKyBaoVes_NhanSuMaNV",
                table: "NhatKyBaoVes");

            migrationBuilder.DropColumn(
                name: "LoRungMaLo",
                table: "NhatKyBaoVes");

            migrationBuilder.DropColumn(
                name: "NhanSuMaNV",
                table: "NhatKyBaoVes");

            // 1.5. Cắt liên kết bảng SinhVats (Con của LoRungs)
            migrationBuilder.DropForeignKey(
                name: "FK_SinhVats_LoRungs_LoRungMaLo",
                table: "SinhVats");

            migrationBuilder.DropIndex(
                name: "IX_SinhVats_LoRungMaLo",
                table: "SinhVats");

            migrationBuilder.DropColumn(
                name: "LoRungMaLo",
                table: "SinhVats");

            // ==========================================================================================
            // BƯỚC 2: SỬA ĐỔI CẤU TRÚC CÁC BẢNG (DROP PK -> ALTER -> ADD PK)
            // ==========================================================================================

            // --- 2.1. Sửa Bảng Cha Cao Nhất: DanhMucXas ---
            migrationBuilder.DropPrimaryKey(
                name: "PK_DanhMucXas",
                table: "DanhMucXas");

            migrationBuilder.AlterColumn<string>(
                name: "MaXa",
                table: "DanhMucXas",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DanhMucXas",
                table: "DanhMucXas",
                column: "MaXa");

            // --- 2.2. Sửa Bảng Con Cấp 1: DanhMucThons ---
            migrationBuilder.DropPrimaryKey(
                name: "PK_DanhMucThons",
                table: "DanhMucThons");

            // Sửa cột PK (MaThon)
            migrationBuilder.AlterColumn<string>(
                name: "MaThon",
                table: "DanhMucThons",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            // Sửa cột FK (MaXa) để khớp với bảng cha
            migrationBuilder.AlterColumn<string>(
                name: "MaXa",
                table: "DanhMucThons",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DanhMucThons",
                table: "DanhMucThons",
                column: "MaThon");

            // --- 2.3. Sửa Bảng NhanSus ---
            migrationBuilder.AlterColumn<string>(
                name: "MaXa",
                table: "NhanSus",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            // --- 2.4. Sửa Bảng LoRungs ---
            migrationBuilder.AlterColumn<string>(
                name: "MaThon",
                table: "LoRungs",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            // ==========================================================================================
            // BƯỚC 3: THIẾT LẬP LẠI LIÊN KẾT (CREATE INDEX & ADD FOREIGN KEY)
            // ==========================================================================================

            // 3.1. DanhMucThons -> DanhMucXas
            migrationBuilder.CreateIndex(
                name: "IX_DanhMucThons_MaXa",
                table: "DanhMucThons",
                column: "MaXa");

            migrationBuilder.AddForeignKey(
                name: "FK_DanhMucThons_DanhMucXas_MaXa",
                table: "DanhMucThons",
                column: "MaXa",
                principalTable: "DanhMucXas",
                principalColumn: "MaXa",
                onDelete: ReferentialAction.Cascade);

            // 3.2. NhanSus -> DanhMucXas
            migrationBuilder.CreateIndex(
                name: "IX_NhanSus_MaXa",
                table: "NhanSus",
                column: "MaXa");

            migrationBuilder.AddForeignKey(
                name: "FK_NhanSus_DanhMucXas_MaXa",
                table: "NhanSus",
                column: "MaXa",
                principalTable: "DanhMucXas",
                principalColumn: "MaXa",
                onDelete: ReferentialAction.SetNull);

            // 3.3. LoRungs -> DanhMucThons
            migrationBuilder.CreateIndex(
                name: "IX_LoRungs_MaThon",
                table: "LoRungs",
                column: "MaThon");

            migrationBuilder.AddForeignKey(
                name: "FK_LoRungs_DanhMucThons_MaThon",
                table: "LoRungs",
                column: "MaThon",
                principalTable: "DanhMucThons",
                principalColumn: "MaThon",
                onDelete: ReferentialAction.SetNull);

            // 3.4. NhatKyBaoVes -> LoRungs & NhanSus
            migrationBuilder.CreateIndex(
                name: "IX_NhatKyBaoVes_MaLo",
                table: "NhatKyBaoVes",
                column: "MaLo");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyBaoVes_MaNV_GhiNhan",
                table: "NhatKyBaoVes",
                column: "MaNV_GhiNhan");

            migrationBuilder.AddForeignKey(
                name: "FK_NhatKyBaoVes_LoRungs_MaLo",
                table: "NhatKyBaoVes",
                column: "MaLo",
                principalTable: "LoRungs",
                principalColumn: "MaLo",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_NhatKyBaoVes_NhanSus_MaNV_GhiNhan",
                table: "NhatKyBaoVes",
                column: "MaNV_GhiNhan",
                principalTable: "NhanSus",
                principalColumn: "MaNV",
                onDelete: ReferentialAction.SetNull);

            // 3.5. SinhVats -> LoRungs
            migrationBuilder.CreateIndex(
                name: "IX_SinhVats_MaLo",
                table: "SinhVats",
                column: "MaLo");

            migrationBuilder.AddForeignKey(
                name: "FK_SinhVats_LoRungs_MaLo",
                table: "SinhVats",
                column: "MaLo",
                principalTable: "LoRungs",
                principalColumn: "MaLo",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Code rollback nếu cần, tạm thời để trống hoặc tự sinh
        }
    }
}