using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions dbContextOptions)
            : base(dbContextOptions) { }

        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ConfigureStockSymbol(modelBuilder);
            ConfigureCommentStock(modelBuilder);
        }

        private static void ConfigureStockSymbol(ModelBuilder modelBuilder)
        {
            var stock = modelBuilder.Entity<Stock>();
            stock.Property(value => value.Symbol).HasMaxLength(Stock.SymbolMaxLength);
            stock.HasIndex(value => value.Symbol).IsUnique();
        }

        private static void ConfigureCommentStock(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Comment>()
                .HasOne(comment => comment.Stock)
                .WithMany(stock => stock.Comments)
                .HasForeignKey(comment => comment.StockId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        }
    }
}
