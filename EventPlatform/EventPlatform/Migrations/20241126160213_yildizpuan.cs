using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yazlab2.Migrations
{
    /// <inheritdoc />
    public partial class yildizpuan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "YildizPuan",
                table: "GeriBildirimler",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "OrtalamaPuan",
                table: "Etkinlikler",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "ToplamYildizPuani",
                table: "Etkinlikler",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "YildizPuaniSayisi",
                table: "Etkinlikler",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "YildizPuan",
                table: "GeriBildirimler");

            migrationBuilder.DropColumn(
                name: "OrtalamaPuan",
                table: "Etkinlikler");

            migrationBuilder.DropColumn(
                name: "ToplamYildizPuani",
                table: "Etkinlikler");

            migrationBuilder.DropColumn(
                name: "YildizPuaniSayisi",
                table: "Etkinlikler");
        }
    }
}
