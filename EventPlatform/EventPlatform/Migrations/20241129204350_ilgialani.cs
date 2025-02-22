using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yazlab2.Migrations
{
    /// <inheritdoc />
    public partial class ilgialani : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IlgiAlanlari",
                table: "Kullanıcılar");



            migrationBuilder.CreateTable(
                name: "IlgiAlanlari",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KullanıcıID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IlgiAlanlari", x => x.ID);
                    table.ForeignKey(
                        name: "FK_IlgiAlanlari_Kullanıcılar_KullanıcıID",
                        column: x => x.KullanıcıID,
                        principalTable: "Kullanıcılar",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Kategoriler",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IlgiAlaniID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kategoriler", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Kategoriler_IlgiAlanlari_IlgiAlaniID",
                        column: x => x.IlgiAlaniID,
                        principalTable: "IlgiAlanlari",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KullaniciIlgiAlanlari",
                columns: table => new
                {
                    KullanıcıID = table.Column<int>(type: "int", nullable: false),
                    IlgiAlaniID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KullaniciIlgiAlanlari", x => new { x.KullanıcıID, x.IlgiAlaniID });
                    table.ForeignKey(
                        name: "FK_KullaniciIlgiAlanlari_IlgiAlanlari_IlgiAlaniID",
                        column: x => x.IlgiAlaniID,
                        principalTable: "IlgiAlanlari",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KullaniciIlgiAlanlari_Kullanıcılar_KullanıcıID",
                        column: x => x.KullanıcıID,
                        principalTable: "Kullanıcılar",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Etkinlikler_KategoriID",
                table: "Etkinlikler",
                column: "KategoriID");

            migrationBuilder.CreateIndex(
                name: "IX_IlgiAlanlari_KullanıcıID",
                table: "IlgiAlanlari",
                column: "KullanıcıID");

            migrationBuilder.CreateIndex(
                name: "IX_Kategoriler_IlgiAlaniID",
                table: "Kategoriler",
                column: "IlgiAlaniID");

            migrationBuilder.CreateIndex(
                name: "IX_KullaniciIlgiAlanlari_IlgiAlaniID",
                table: "KullaniciIlgiAlanlari",
                column: "IlgiAlaniID");

            migrationBuilder.AddForeignKey(
                name: "FK_Etkinlikler_Kategoriler_KategoriID",
                table: "Etkinlikler",
                column: "KategoriID",
                principalTable: "Kategoriler",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Etkinlikler_Kategoriler_KategoriID",
                table: "Etkinlikler");

            migrationBuilder.DropTable(
                name: "Kategoriler");

            migrationBuilder.DropTable(
                name: "KullaniciIlgiAlanlari");

            migrationBuilder.DropTable(
                name: "IlgiAlanlari");

            migrationBuilder.DropIndex(
                name: "IX_Etkinlikler_KategoriID",
                table: "Etkinlikler");

            migrationBuilder.DropColumn(
                name: "KategoriID",
                table: "Etkinlikler");

            migrationBuilder.AddColumn<string>(
                name: "IlgiAlanlari",
                table: "Kullanıcılar",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Kategori",
                table: "Etkinlikler",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
