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
            // ApplyManualMigrations() removed - now using standard migrations
        }

        // Manual migrations removed


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
