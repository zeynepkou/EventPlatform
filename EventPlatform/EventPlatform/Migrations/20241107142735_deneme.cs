using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yazlab2.Migrations
{
    /// <inheritdoc />
    public partial class deneme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Etkinlikler",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EtkinlikAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Saat = table.Column<TimeSpan>(type: "time", nullable: false),
                    EtkinlikSuresi = table.Column<TimeSpan>(type: "time", nullable: false),
                    Konum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kategori = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Etkinlikler", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Kullanıcılar",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sifre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Eposta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Konum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IlgiAlanlari = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Soyad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DogumTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cinsiyet = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TelefonNumarasi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfilFotografiYolu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KullaniciTuru = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanıcılar", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Katılımcılar",
                columns: table => new
                {
                    KullanıcıID = table.Column<int>(type: "int", nullable: false),
                    EtkinlikID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Katılımcılar", x => new { x.KullanıcıID, x.EtkinlikID });
                    table.ForeignKey(
                        name: "FK_Katılımcılar_Etkinlikler_EtkinlikID",
                        column: x => x.EtkinlikID,
                        principalTable: "Etkinlikler",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Katılımcılar_Kullanıcılar_KullanıcıID",
                        column: x => x.KullanıcıID,
                        principalTable: "Kullanıcılar",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Mesajlar",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GondericiID = table.Column<int>(type: "int", nullable: false),
                    AliciID = table.Column<int>(type: "int", nullable: true),
                    MesajMetni = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GonderimZamani = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EtkinlikID = table.Column<int>(type: "int", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mesajlar", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Mesajlar_Kullanıcılar_AliciID",
                        column: x => x.AliciID,
                        principalTable: "Kullanıcılar",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Mesajlar_Kullanıcılar_GondericiID",
                        column: x => x.GondericiID,
                        principalTable: "Kullanıcılar",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Puanlar",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullanıcıID = table.Column<int>(type: "int", nullable: false),
                    Puanlar = table.Column<int>(type: "int", nullable: false),
                    KazanilanTarih = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Puanlar", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Puanlar_Kullanıcılar_KullanıcıID",
                        column: x => x.KullanıcıID,
                        principalTable: "Kullanıcılar",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Katılımcılar_EtkinlikID",
                table: "Katılımcılar",
                column: "EtkinlikID");

            migrationBuilder.CreateIndex(
                name: "IX_Mesajlar_AliciID",
                table: "Mesajlar",
                column: "AliciID");

            migrationBuilder.CreateIndex(
                name: "IX_Mesajlar_GondericiID",
                table: "Mesajlar",
                column: "GondericiID");

            migrationBuilder.CreateIndex(
                name: "IX_Puanlar_KullanıcıID",
                table: "Puanlar",
                column: "KullanıcıID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Katılımcılar");

            migrationBuilder.DropTable(
                name: "Mesajlar");

            migrationBuilder.DropTable(
                name: "Puanlar");

            migrationBuilder.DropTable(
                name: "Etkinlikler");

            migrationBuilder.DropTable(
                name: "Kullanıcılar");
        }
    }
}
