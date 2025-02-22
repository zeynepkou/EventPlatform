using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yazlab2.Migrations
{
    /// <inheritdoc />
    public partial class favoricozum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FavoriSayisi",
                table: "Etkinlikler",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Favoriler",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullanıcıID = table.Column<int>(type: "int", nullable: false),
                    EtkinlikID = table.Column<int>(type: "int", nullable: false),
                    FavoriEklemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favoriler", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Favoriler_Etkinlikler_EtkinlikID",
                        column: x => x.EtkinlikID,
                        principalTable: "Etkinlikler",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Favoriler_Kullanıcılar_KullanıcıID",
                        column: x => x.KullanıcıID,
                        principalTable: "Kullanıcılar",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Favoriler_EtkinlikID",
                table: "Favoriler",
                column: "EtkinlikID");

            migrationBuilder.CreateIndex(
                name: "IX_Favoriler_KullanıcıID",
                table: "Favoriler",
                column: "KullanıcıID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Favoriler");

            migrationBuilder.DropColumn(
                name: "FavoriSayisi",
                table: "Etkinlikler");
        }
    }
}
