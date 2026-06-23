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
        public DbSet<Prediction> Predictions { get; set; }
        public DbSet<PredictionReason> PredictionReasons { get; set; }
        public DbSet<PriceHistory> PriceHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ConfigureStockSymbol(modelBuilder);
            ConfigureCommentStock(modelBuilder);
            ConfigureAssetCategory(modelBuilder);
            ConfigureAsset(modelBuilder);
            ConfigurePrediction(modelBuilder);
            ConfigurePriceHistory(modelBuilder);
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

        private static void ConfigurePrediction(ModelBuilder modelBuilder)
        {
            var prediction = modelBuilder.Entity<Prediction>();
            prediction.Property(value => value.AssetId).HasMaxLength(Prediction.AssetIdMaxLength);
            prediction.Property(value => value.AssetType).HasConversion<string>().HasMaxLength(Asset.AssetTypeMaxLength);
            prediction.HasIndex(value => new { value.AssetId, value.CreatedOn });
            ConfigurePredictionReasons(prediction);
            ConfigurePredictionReasonKind(modelBuilder);
        }

        private static void ConfigurePredictionReasons(EntityTypeBuilder<Prediction> prediction)
        {
            prediction
                .HasMany(value => value.Reasons)
                .WithOne(reason => reason.Prediction)
                .HasForeignKey(reason => reason.PredictionId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigurePredictionReasonKind(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<PredictionReason>()
                .Property(value => value.Kind)
                .HasConversion<string>()
                .HasMaxLength(16);
        }

        private static void ConfigurePriceHistory(ModelBuilder modelBuilder)
        {
            var price = modelBuilder.Entity<PriceHistory>();
            price.Property(value => value.Source).HasMaxLength(PriceHistory.SourceMaxLength);
            price.HasIndex(value => new { value.AssetId, value.Date }).IsUnique();
            ConfigurePriceHistoryAsset(price);
        }

        private static void ConfigurePriceHistoryAsset(EntityTypeBuilder<PriceHistory> price)
        {
            price
                .HasOne(value => value.Asset)
                .WithMany()
                .HasForeignKey(value => value.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
