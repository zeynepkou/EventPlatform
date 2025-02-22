using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yazlab2.Migrations
{
    /// <inheritdoc />
    public partial class sifre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResetCodeExpiration",
                table: "Kullanıcılar",
                newName: "ResetTokenExpireDate");

            migrationBuilder.RenameColumn(
                name: "PasswordResetCode",
                table: "Kullanıcılar",
                newName: "ResetToken");

           


           

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
           
            migrationBuilder.RenameColumn(
                name: "ResetTokenExpireDate",
                table: "Kullanıcılar",
                newName: "ResetCodeExpiration");

            migrationBuilder.RenameColumn(
                name: "ResetToken",
                table: "Kullanıcılar",
                newName: "PasswordResetCode");
        }
    }
}
