using Microsoft.EntityFrameworkCore;

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
           //  EfcToXamarinAndroid.Core.Infrastructure.DatabaseBootstrapper.EnsureInitialized();
           //  Database.EnsureDeleted();   // удаляем бд со старой схемой
           //  Database.EnsureCreated();   // создаем бд с новой схемой
            DatabasePath = databasePath;
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
