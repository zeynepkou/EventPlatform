using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yazlab2.Migrations
{
    /// <inheritdoc />
    public partial class etkinlik : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
    name: "KullanıcıID",
    table: "Etkinlikler",
    type: "int",
    nullable: false); // Bu satırı nullable olarak değiştiriyoruz


            migrationBuilder.CreateIndex(
                name: "IX_Etkinlikler_KullanıcıID",
                table: "Etkinlikler",
                column: "KullanıcıID");

            migrationBuilder.AddForeignKey(
                name: "FK_Etkinlikler_Kullanıcılar_KullanıcıID",
                table: "Etkinlikler",
                column: "KullanıcıID",
                principalTable: "Kullanıcılar",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Etkinlikler_Kullanıcılar_KullanıcıID",
                table: "Etkinlikler");

            migrationBuilder.DropIndex(
                name: "IX_Etkinlikler_KullanıcıID",
                table: "Etkinlikler");

            migrationBuilder.DropColumn(
                name: "KullanıcıID",
                table: "Etkinlikler");
        }
    }
}
