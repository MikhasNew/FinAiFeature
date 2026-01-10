using System;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EfcToXamarinAndroid.Core
{
    public class DataItemContext : DbContext
    {
        public DbSet<DataItem>? Cats { get; set; }
        public DbSet<Receipt>? Receipts { get; set; }
        public DbSet<ReceiptItem>? ReceiptItems { get; set; }

        private string DatabasePath { get; set; }

        public DataItemContext(string databasePath)
        {
            DatabasePath = databasePath;
            ApplyManualMigrations();
        }

        private void ApplyManualMigrations()
        {
            try
            {
                Database.OpenConnection();
                using (var command = Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS ""Receipts"" (
                            ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Receipts"" PRIMARY KEY AUTOINCREMENT,
                            ""DataItemId"" INTEGER NOT NULL,
                            ""ShopName"" TEXT NULL,
                            ""ShopInn"" TEXT NULL,
                            ""Address"" TEXT NULL,
                            ""ReceiptDate"" TEXT NOT NULL,
                            ""TotalSum"" REAL NOT NULL,
                            ""Currency"" TEXT NULL,
                            ""RegNumber"" TEXT NULL,
                            ""CardNumber"" TEXT NULL,
                            ""EripPayerNumber"" TEXT NULL,
                            ""OrderNumber"" TEXT NULL,
                            ""Subject"" TEXT NULL,
                            ""RawData"" TEXT NULL,
                            CONSTRAINT ""FK_Receipts_Cats_DataItemId"" FOREIGN KEY (""DataItemId"") REFERENCES ""Cats"" (""Id"") ON DELETE CASCADE
                        );
                        CREATE TABLE IF NOT EXISTS ""ReceiptItems"" (
                            ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_ReceiptItems"" PRIMARY KEY AUTOINCREMENT,
                            ""ReceiptId"" INTEGER NOT NULL,
                            ""Name"" TEXT NULL,
                            ""Quantity"" REAL NOT NULL,
                            ""Unit"" TEXT NULL,
                            ""Price"" REAL NOT NULL,
                            ""Sum"" REAL NOT NULL,
                            ""Code"" TEXT NULL,
                            CONSTRAINT ""FK_ReceiptItems_Receipts_ReceiptId"" FOREIGN KEY (""ReceiptId"") REFERENCES ""Receipts"" (""Id"") ON DELETE CASCADE
                        );
                        CREATE INDEX IF NOT EXISTS ""IX_Receipts_DataItemId"" ON ""Receipts"" (""DataItemId"");
                        CREATE INDEX IF NOT EXISTS ""IX_ReceiptItems_ReceiptId"" ON ""ReceiptItems"" (""ReceiptId"");
                    ";
                    command.ExecuteNonQuery();

                    // Manual columns check/add for existing table
                    AddColumnIfNotExists(command, "Receipts", "Currency", "TEXT NULL");
                    AddColumnIfNotExists(command, "Receipts", "RegNumber", "TEXT NULL");
                    AddColumnIfNotExists(command, "Receipts", "CardNumber", "TEXT NULL");
                    AddColumnIfNotExists(command, "Receipts", "EripPayerNumber", "TEXT NULL");
                    AddColumnIfNotExists(command, "Receipts", "OrderNumber", "TEXT NULL");
                    AddColumnIfNotExists(command, "Receipts", "Subject", "TEXT NULL");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[DataItemContext] Manual migration error: {ex.Message}");
            }
        }

        private void AddColumnIfNotExists(System.Data.Common.DbCommand command, string tableName, string columnName, string type)
        {
            try
            {
                command.CommandText = $"PRAGMA table_info({tableName})";
                bool exists = false;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["name"].ToString().Equals(columnName, StringComparison.OrdinalIgnoreCase))
                        {
                            exists = true;
                            break;
                        }
                    }
                }
                if (!exists)
                {
                    command.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {type}";
                    command.ExecuteNonQuery();
                    System.Console.WriteLine($"[DataItemContext] Added column {columnName} to {tableName}");
                }
            }
            catch (Exception ex)
            {
                 System.Console.WriteLine($"[DataItemContext] Error adding column {columnName}: {ex.Message}");
            }
        }

        // Конструктор для создания миграций (используется EF Core Tools)
        public DataItemContext()
        {
            DatabasePath = "migration_dummy.db";
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataItem>()
                .HasIndex(x => x.Date);

            modelBuilder.Entity<DataItem>()
                .HasIndex(x => x.OperacionTyp);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename={DatabasePath}");
        }
    }
}
