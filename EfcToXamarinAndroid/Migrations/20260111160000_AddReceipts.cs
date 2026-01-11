using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EfcToXamarinAndroid.MigrationsHelper.Migrations
{
    public partial class AddReceipts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Receipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DataItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    ShopName = table.Column<string>(type: "TEXT", nullable: true),
                    ShopInn = table.Column<string>(type: "TEXT", nullable: true),
                    Address = table.Column<string>(type: "TEXT", nullable: true),
                    ReceiptDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalSum = table.Column<float>(type: "REAL", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", nullable: true),
                    RegNumber = table.Column<string>(type: "TEXT", nullable: true),
                    CardNumber = table.Column<string>(type: "TEXT", nullable: true),
                    EripPayerNumber = table.Column<string>(type: "TEXT", nullable: true),
                    OrderNumber = table.Column<string>(type: "TEXT", nullable: true),
                    Subject = table.Column<string>(type: "TEXT", nullable: true),
                    RawData = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Receipts_Cats_DataItemId",
                        column: x => x.DataItemId,
                        principalTable: "Cats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReceiptId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Quantity = table.Column<double>(type: "REAL", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", nullable: true),
                    Price = table.Column<float>(type: "REAL", nullable: false),
                    Sum = table.Column<float>(type: "REAL", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiptItems_Receipts_ReceiptId",
                        column: x => x.ReceiptId,
                        principalTable: "Receipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_DataItemId",
                table: "Receipts",
                column: "DataItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptItems_ReceiptId",
                table: "ReceiptItems",
                column: "ReceiptId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReceiptItems");

            migrationBuilder.DropTable(
                name: "Receipts");
        }
    }
}
