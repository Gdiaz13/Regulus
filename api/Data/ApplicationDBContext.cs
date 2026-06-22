using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions dbContextOptions)
            : base(dbContextOptions) { }

        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<AssetCategory> AssetCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ConfigureStockSymbol(modelBuilder);
            ConfigureCommentStock(modelBuilder);
            ConfigureAssetCategory(modelBuilder);
            ConfigureAsset(modelBuilder);
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

        private static void ConfigureAssetCategory(ModelBuilder modelBuilder)
        {
            var category = modelBuilder.Entity<AssetCategory>();
            category.Property(value => value.Name).HasMaxLength(AssetCategory.NameMaxLength);
            category.Property(value => value.Slug).HasMaxLength(AssetCategory.SlugMaxLength);
            category.Property(value => value.AssetType).HasConversion<string>().HasMaxLength(Asset.AssetTypeMaxLength);
            category.HasIndex(value => value.Name).IsUnique();
            category.HasIndex(value => value.Slug).IsUnique();
        }

        private static void ConfigureAsset(ModelBuilder modelBuilder)
        {
            var asset = modelBuilder.Entity<Asset>();
            asset.Property(value => value.Symbol).HasMaxLength(Asset.SymbolMaxLength);
            asset.Property(value => value.AssetType).HasConversion<string>().HasMaxLength(Asset.AssetTypeMaxLength);
            asset.HasIndex(value => new { value.AssetType, value.Symbol }).IsUnique();
            ConfigureAssetCategoryLink(asset);
        }

        private static void ConfigureAssetCategoryLink(EntityTypeBuilder<Asset> asset)
        {
            asset
                .HasOne(value => value.Category)
                .WithMany(category => category.Assets)
                .HasForeignKey(value => value.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
