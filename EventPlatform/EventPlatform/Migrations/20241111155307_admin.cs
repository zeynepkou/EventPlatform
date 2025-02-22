using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yazlab2.Migrations
{
    /// <inheritdoc />
    public partial class admin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EtkinlikDurum",
                table: "Etkinlikler",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EtkinlikDurum",
                table: "Etkinlikler");
        }
    }
}
