using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EfcToXamarinAndroid.MigrationsHelper.Migrations
{
    /// <inheritdoc />
    public partial class AddIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Cats_Date",
                table: "Cats",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Cats_OperacionTyp",
                table: "Cats",
                column: "OperacionTyp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cats_Date",
                table: "Cats");

            migrationBuilder.DropIndex(
                name: "IX_Cats_OperacionTyp",
                table: "Cats");
        }
    }
}
