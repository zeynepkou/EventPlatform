using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yazlab2.Migrations
{
    /// <inheritdoc />
    public partial class geribildirim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Etkinlikler_Kullanıcılar_KullanıcıID",
                table: "Etkinlikler");

            migrationBuilder.AlterColumn<int>(
                name: "KullanıcıID",
                table: "Etkinlikler",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "GeriBildirimler",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GondericiID = table.Column<int>(type: "int", nullable: false),
                    GeriBidirimMetni = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GonderimZamani = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GeriBildirimDurum = table.Column<int>(type: "int", nullable: false),
                    EtkinlikID = table.Column<int>(type: "int", nullable: true),
                    AliciID = table.Column<int>(type: "int", nullable: true),
                    KullanıcıID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeriBildirimler", x => x.ID);
                    table.ForeignKey(
                        name: "FK_GeriBildirimler_Etkinlikler_EtkinlikID",
                        column: x => x.EtkinlikID,
                        principalTable: "Etkinlikler",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GeriBildirimler_Kullanıcılar_AliciID",
                        column: x => x.AliciID,
                        principalTable: "Kullanıcılar",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_GeriBildirimler_Kullanıcılar_GondericiID",
                        column: x => x.GondericiID,
                        principalTable: "Kullanıcılar",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GeriBildirimler_Kullanıcılar_KullanıcıID",
                        column: x => x.KullanıcıID,
                        principalTable: "Kullanıcılar",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeriBildirimler_AliciID",
                table: "GeriBildirimler",
                column: "AliciID");

            migrationBuilder.CreateIndex(
                name: "IX_GeriBildirimler_EtkinlikID",
                table: "GeriBildirimler",
                column: "EtkinlikID");

            migrationBuilder.CreateIndex(
                name: "IX_GeriBildirimler_GondericiID",
                table: "GeriBildirimler",
                column: "GondericiID");

            migrationBuilder.CreateIndex(
                name: "IX_GeriBildirimler_KullanıcıID",
                table: "GeriBildirimler",
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

            migrationBuilder.DropTable(
                name: "GeriBildirimler");

            migrationBuilder.AlterColumn<int>(
                name: "KullanıcıID",
                table: "Etkinlikler",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Etkinlikler_Kullanıcılar_KullanıcıID",
                table: "Etkinlikler",
                column: "KullanıcıID",
                principalTable: "Kullanıcılar",
                principalColumn: "ID");
        }
    }
}
