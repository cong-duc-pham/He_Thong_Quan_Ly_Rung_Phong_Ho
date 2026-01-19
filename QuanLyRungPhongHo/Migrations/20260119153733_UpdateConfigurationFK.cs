using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyRungPhongHo.Migrations
{
    /// <inheritdoc />
    public partial class UpdateConfigurationFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_DanhMucThons_DanhMucXas_DanhMucXaMaXa",
            //    table: "DanhMucThons");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_LoRungs_DanhMucThons_DanhMucThonMaThon",
            //    table: "LoRungs");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_NhanSus_DanhMucXas_DanhMucXaMaXa",
            //    table: "NhanSus");

            //migrationBuilder.DropIndex(
            //    name: "IX_NhanSus_DanhMucXaMaXa",
            //    table: "NhanSus");

            //migrationBuilder.DropIndex(
            //    name: "IX_LoRungs_DanhMucThonMaThon",
            //    table: "LoRungs");

            //migrationBuilder.DropIndex(
            //    name: "IX_DanhMucThons_DanhMucXaMaXa",
            //    table: "DanhMucThons");

            //migrationBuilder.DropColumn(
            //    name: "DanhMucXaMaXa",
            //    table: "NhanSus");

            //migrationBuilder.DropColumn(
            //    name: "DanhMucThonMaThon",
            //    table: "LoRungs");

            //migrationBuilder.DropColumn(
            //    name: "DanhMucXaMaXa",
            //    table: "DanhMucThons");

            migrationBuilder.AlterColumn<string>(
                name: "MaXa",
                table: "NhanSus",
                type: "nvarchar(10)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_NhanSus_MaXa",
            //    table: "NhanSus",
            //    column: "MaXa");

            //migrationBuilder.CreateIndex(
            //    name: "IX_LoRungs_MaThon",
            //    table: "LoRungs",
            //    column: "MaThon");

            //migrationBuilder.CreateIndex(
            //    name: "IX_DanhMucThons_MaXa",
            //    table: "DanhMucThons",
            //    column: "MaXa");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_DanhMucThons_DanhMucXas_MaXa",
            //    table: "DanhMucThons",
            //    column: "MaXa",
            //    principalTable: "DanhMucXas",
            //    principalColumn: "MaXa",
            //    onDelete: ReferentialAction.Cascade);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_LoRungs_DanhMucThons_MaThon",
            //    table: "LoRungs",
            //    column: "MaThon",
            //    principalTable: "DanhMucThons",
            //    principalColumn: "MaThon",
            //    onDelete: ReferentialAction.SetNull);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_NhanSus_DanhMucXas_MaXa",
            //    table: "NhanSus",
            //    column: "MaXa",
            //    principalTable: "DanhMucXas",
            //    principalColumn: "MaXa",
            //    onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DanhMucThons_DanhMucXas_MaXa",
                table: "DanhMucThons");

            migrationBuilder.DropForeignKey(
                name: "FK_LoRungs_DanhMucThons_MaThon",
                table: "LoRungs");

            migrationBuilder.DropForeignKey(
                name: "FK_NhanSus_DanhMucXas_MaXa",
                table: "NhanSus");

            migrationBuilder.DropIndex(
                name: "IX_NhanSus_MaXa",
                table: "NhanSus");

            migrationBuilder.DropIndex(
                name: "IX_LoRungs_MaThon",
                table: "LoRungs");

            migrationBuilder.DropIndex(
                name: "IX_DanhMucThons_MaXa",
                table: "DanhMucThons");

            migrationBuilder.AlterColumn<string>(
                name: "MaXa",
                table: "NhanSus",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DanhMucXaMaXa",
                table: "NhanSus",
                type: "nvarchar(10)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DanhMucThonMaThon",
                table: "LoRungs",
                type: "nvarchar(10)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DanhMucXaMaXa",
                table: "DanhMucThons",
                type: "nvarchar(10)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_NhanSus_DanhMucXaMaXa",
                table: "NhanSus",
                column: "DanhMucXaMaXa");

            migrationBuilder.CreateIndex(
                name: "IX_LoRungs_DanhMucThonMaThon",
                table: "LoRungs",
                column: "DanhMucThonMaThon");

            migrationBuilder.CreateIndex(
                name: "IX_DanhMucThons_DanhMucXaMaXa",
                table: "DanhMucThons",
                column: "DanhMucXaMaXa");

            migrationBuilder.AddForeignKey(
                name: "FK_DanhMucThons_DanhMucXas_DanhMucXaMaXa",
                table: "DanhMucThons",
                column: "DanhMucXaMaXa",
                principalTable: "DanhMucXas",
                principalColumn: "MaXa",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LoRungs_DanhMucThons_DanhMucThonMaThon",
                table: "LoRungs",
                column: "DanhMucThonMaThon",
                principalTable: "DanhMucThons",
                principalColumn: "MaThon");

            migrationBuilder.AddForeignKey(
                name: "FK_NhanSus_DanhMucXas_DanhMucXaMaXa",
                table: "NhanSus",
                column: "DanhMucXaMaXa",
                principalTable: "DanhMucXas",
                principalColumn: "MaXa");
        }
    }
}
