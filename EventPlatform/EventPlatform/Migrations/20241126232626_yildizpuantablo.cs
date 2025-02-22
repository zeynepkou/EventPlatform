using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yazlab2.Migrations
{
    /// <inheritdoc />
    public partial class yildizpuantablo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "YildizPuan",
                table: "GeriBildirimler");

            migrationBuilder.CreateTable(
                name: "YildizPuanlar",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullanıcıID = table.Column<int>(type: "int", nullable: false),
                    Puan = table.Column<int>(type: "int", nullable: false),
                    EtkinlikID = table.Column<int>(type: "int", nullable: false),
                    GonderimZamani = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YildizPuanlar", x => x.ID);
                    table.ForeignKey(
                        name: "FK_YildizPuanlar_Etkinlikler_EtkinlikID",
                        column: x => x.EtkinlikID,
                        principalTable: "Etkinlikler",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YildizPuanlar_Kullanıcılar_KullanıcıID",
                        column: x => x.KullanıcıID,
                        principalTable: "Kullanıcılar",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_YildizPuanlar_EtkinlikID",
                table: "YildizPuanlar",
                column: "EtkinlikID");

            migrationBuilder.CreateIndex(
                name: "IX_YildizPuanlar_KullanıcıID",
                table: "YildizPuanlar",
                column: "KullanıcıID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YildizPuanlar");

            migrationBuilder.AddColumn<int>(
                name: "YildizPuan",
                table: "GeriBildirimler",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
