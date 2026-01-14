using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EfcToXamarinAndroid.MigrationsHelper.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingQrCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PendingQrCode",
                table: "Cats",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PendingQrCode",
                table: "Cats");
        }
    }
}
