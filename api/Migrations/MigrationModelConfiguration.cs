using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Migrations;

internal static class MigrationModelConfiguration
{
    private const int StockSymbolMaxLength = 32;
    private const int AssetSymbolMaxLength = 32;
    private const int AssetTypeMaxLength = 32;
    private const int CategoryNameMaxLength = 64;
    private const int CategorySlugMaxLength = 64;
    private const int PredictionAssetIdMaxLength = 64;
    private const int ModelNameMaxLength = 64;
    private const int ReasonKindMaxLength = 16;
    private const int ReasonTextMaxLength = 512;

    public static void ConfigureInitial(ModelBuilder modelBuilder)
    {
        Configure(modelBuilder, ConfigureInitialCommentEntity, ConfigureInitialStockEntity, ConfigureInitialCommentNavigation);
    }

    public static void ConfigureUniqueStockSymbol(ModelBuilder modelBuilder)
    {
        Configure(modelBuilder, ConfigureInitialCommentEntity, ConfigureCurrentStockEntity, ConfigureInitialCommentNavigation);
    }

    public static void ConfigureRequireCommentStock(ModelBuilder modelBuilder)
    {
        Configure(modelBuilder, ConfigureCurrentCommentEntity, ConfigureCurrentStockEntity, ConfigureCurrentCommentNavigation);
    }

    public static void ConfigureAddAssets(ModelBuilder modelBuilder)
    {
        ConfigureRequireCommentStock(modelBuilder);
        ConfigureAssetModels(modelBuilder);
    }

    public static void ConfigureCurrent(ModelBuilder modelBuilder)
    {
        ConfigureAddAssets(modelBuilder);
        ConfigurePredictionModels(modelBuilder);
    }

    private static void ConfigureAnnotations(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "9.0.10")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);
        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);
    }

    private static void ConfigureCommentModel(ModelBuilder modelBuilder, Action<EntityTypeBuilder> configureComment)
    {
        modelBuilder.Entity("api.Models.Comment", configureComment);
    }

    private static void ConfigureStockModel(ModelBuilder modelBuilder, Action<EntityTypeBuilder> configureStock)
    {
        modelBuilder.Entity("api.Models.Stock", configureStock);
    }

    private static void ConfigureNavigations(
        ModelBuilder modelBuilder,
        Action<EntityTypeBuilder> configureCommentNavigation
    )
    {
        modelBuilder.Entity("api.Models.Comment", configureCommentNavigation);
        modelBuilder.Entity("api.Models.Stock", ConfigureStockNavigation);
    }

    private static void ConfigureInitialCommentEntity(EntityTypeBuilder builder)
    {
        ConfigureCommentEntity(builder, ConfigureOptionalCommentStockId);
    }

    private static void ConfigureCurrentCommentEntity(EntityTypeBuilder builder)
    {
        ConfigureCommentEntity(builder, ConfigureRequiredCommentStockId);
    }

    private static void ConfigureCommentEntity(
        EntityTypeBuilder builder,
        Action<EntityTypeBuilder> configureStockId
    )
    {
        ConfigureCommentId(builder);
        ConfigureCommentTextFields(builder);
        configureStockId(builder);
        builder.HasKey("Id");
        builder.HasIndex("StockId");
        builder.ToTable("Comments");
    }

    private static void Configure(
        ModelBuilder modelBuilder,
        Action<EntityTypeBuilder> configureComment,
        Action<EntityTypeBuilder> configureStock,
        Action<EntityTypeBuilder> configureCommentNavigation
    )
    {
        ConfigureAnnotations(modelBuilder);
        ConfigureCommentModel(modelBuilder, configureComment);
        ConfigureStockModel(modelBuilder, configureStock);
        ConfigureNavigations(modelBuilder, configureCommentNavigation);
    }

    private static void ConfigureCommentId(EntityTypeBuilder builder)
    {
        var id = builder.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
        SqlServerPropertyBuilderExtensions.UseIdentityColumn(id);
    }

    private static void ConfigureCommentTextFields(EntityTypeBuilder builder)
    {
        builder.Property<string>("Content").IsRequired().HasColumnType("nvarchar(max)");
        builder.Property<DateTime>("CreatedOn").HasColumnType("datetime2");
        builder.Property<string>("Title").IsRequired().HasColumnType("nvarchar(max)");
    }

    private static void ConfigureOptionalCommentStockId(EntityTypeBuilder builder)
    {
        builder.Property<int?>("StockId").HasColumnType("int");
    }

    private static void ConfigureRequiredCommentStockId(EntityTypeBuilder builder)
    {
        builder.Property<int>("StockId").HasColumnType("int");
    }

    private static void ConfigureInitialStockEntity(EntityTypeBuilder builder)
    {
        ConfigureStockId(builder);
        ConfigureStockFields(builder, null);
        builder.HasKey("Id");
        builder.ToTable("Stocks");
    }

    private static void ConfigureCurrentStockEntity(EntityTypeBuilder builder)
    {
        ConfigureStockId(builder);
        ConfigureStockFields(builder, StockSymbolMaxLength);
        builder.HasKey("Id");
        builder.HasIndex("Symbol").IsUnique();
        builder.ToTable("Stocks");
    }

    private static void ConfigureStockId(EntityTypeBuilder builder)
    {
        var id = builder.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
        SqlServerPropertyBuilderExtensions.UseIdentityColumn(id);
    }

    private static void ConfigureStockFields(EntityTypeBuilder builder, int? symbolMaxLength)
    {
        builder.Property<string>("CompanyName").IsRequired().HasColumnType("nvarchar(max)");
        builder.Property<string>("Industry").IsRequired().HasColumnType("nvarchar(max)");
        builder.Property<decimal>("LastDividend").HasColumnType("decimal(18,2)");
        builder.Property<long>("MarketCap").HasColumnType("bigint");
        builder.Property<decimal>("PurchasePrice").HasColumnType("decimal(18,2)");
        ConfigureStockSymbol(builder, symbolMaxLength);
    }

    private static void ConfigureStockSymbol(EntityTypeBuilder builder, int? maxLength)
    {
        var symbol = builder.Property<string>("Symbol").IsRequired();
        if (maxLength.HasValue)
        {
            symbol.HasMaxLength(maxLength.Value).HasColumnType("nvarchar(32)");
            return;
        }
        symbol.HasColumnType("nvarchar(max)");
    }

    private static void ConfigureInitialCommentNavigation(EntityTypeBuilder builder)
    {
        builder.HasOne("api.Models.Stock", "Stock").WithMany("Comments").HasForeignKey("StockId");
        builder.Navigation("Stock");
    }

    private static void ConfigureCurrentCommentNavigation(EntityTypeBuilder builder)
    {
        var relation = builder.HasOne("api.Models.Stock", "Stock").WithMany("Comments").HasForeignKey("StockId");
        relation.OnDelete(DeleteBehavior.Cascade).IsRequired();
        builder.Navigation("Stock");
    }

    private static void ConfigureStockNavigation(EntityTypeBuilder builder)
    {
        builder.Navigation("Comments");
    }

    private static void ConfigureAssetModels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity("api.Models.AssetCategory", ConfigureAssetCategoryEntity);
        modelBuilder.Entity("api.Models.Asset", ConfigureAssetEntity);
        modelBuilder.Entity("api.Models.Asset", ConfigureAssetNavigation);
        modelBuilder.Entity("api.Models.AssetCategory", ConfigureAssetCategoryNavigation);
    }

    private static void ConfigureAssetCategoryEntity(EntityTypeBuilder builder)
    {
        ConfigureIdentityId(builder);
        builder.Property<string>("Name").IsRequired().HasMaxLength(CategoryNameMaxLength).HasColumnType("nvarchar(64)");
        builder.Property<string>("Slug").IsRequired().HasMaxLength(CategorySlugMaxLength).HasColumnType("nvarchar(64)");
        builder.Property<string>("AssetType").IsRequired().HasMaxLength(AssetTypeMaxLength).HasColumnType("nvarchar(32)");
        builder.HasKey("Id");
        builder.HasIndex("Name").IsUnique();
        builder.HasIndex("Slug").IsUnique();
        builder.ToTable("AssetCategories");
    }

    private static void ConfigureAssetEntity(EntityTypeBuilder builder)
    {
        ConfigureIdentityId(builder);
        builder.Property<string>("Symbol").IsRequired().HasMaxLength(AssetSymbolMaxLength).HasColumnType("nvarchar(32)");
        builder.Property<string>("Name").IsRequired().HasColumnType("nvarchar(max)");
        builder.Property<string>("AssetType").IsRequired().HasMaxLength(AssetTypeMaxLength).HasColumnType("nvarchar(32)");
        builder.Property<int?>("CategoryId").HasColumnType("int");
        builder.Property<DateTime>("CreatedOn").HasColumnType("datetime2");
        builder.HasKey("Id");
        builder.HasIndex("CategoryId");
        builder.HasIndex("AssetType", "Symbol").IsUnique();
        builder.ToTable("Assets");
    }

    private static void ConfigureAssetNavigation(EntityTypeBuilder builder)
    {
        builder
            .HasOne("api.Models.AssetCategory", "Category")
            .WithMany("Assets")
            .HasForeignKey("CategoryId")
            .OnDelete(DeleteBehavior.SetNull);
        builder.Navigation("Category");
    }

    private static void ConfigureAssetCategoryNavigation(EntityTypeBuilder builder)
    {
        builder.Navigation("Assets");
    }

    private static void ConfigureIdentityId(EntityTypeBuilder builder)
    {
        var id = builder.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
        SqlServerPropertyBuilderExtensions.UseIdentityColumn(id);
    }

    private static void ConfigurePredictionModels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity("api.Models.Prediction", ConfigurePredictionEntity);
        modelBuilder.Entity("api.Models.PredictionReason", ConfigurePredictionReasonEntity);
        modelBuilder.Entity("api.Models.PredictionReason", ConfigurePredictionReasonNavigation);
        modelBuilder.Entity("api.Models.Prediction", ConfigurePredictionNavigation);
    }

    private static void ConfigurePredictionEntity(EntityTypeBuilder builder)
    {
        ConfigureIdentityId(builder);
        ConfigurePredictionTextFields(builder);
        ConfigurePredictionNumberFields(builder);
        ConfigurePredictionMetaFields(builder);
        builder.HasKey("Id");
        builder.HasIndex("AssetId", "CreatedOn");
        builder.ToTable("Predictions");
    }

    private static void ConfigurePredictionTextFields(EntityTypeBuilder builder)
    {
        builder.Property<string>("AssetId").IsRequired().HasMaxLength(PredictionAssetIdMaxLength).HasColumnType("nvarchar(64)");
        builder.Property<string>("AssetName").IsRequired().HasColumnType("nvarchar(max)");
        builder.Property<string>("AssetType").IsRequired().HasMaxLength(AssetTypeMaxLength).HasColumnType("nvarchar(32)");
        builder.Property<string>("Category").IsRequired().HasColumnType("nvarchar(max)");
        builder.Property<string>("ModelName").IsRequired().HasMaxLength(ModelNameMaxLength).HasColumnType("nvarchar(64)");
        builder.Property<string>("ModelVersion").IsRequired().HasMaxLength(ModelNameMaxLength).HasColumnType("nvarchar(64)");
    }

    private static void ConfigurePredictionNumberFields(EntityTypeBuilder builder)
    {
        builder.Property<decimal>("CurrentPrice").HasColumnType("decimal(18,2)");
        builder.Property<decimal>("PredictedPrice").HasColumnType("decimal(18,2)");
        builder.Property<double>("PredictedPercentChange").HasColumnType("float");
        builder.Property<double>("ConfidenceScore").HasColumnType("float");
        builder.Property<double>("RiskScore").HasColumnType("float");
        builder.Property<double>("BullishScore").HasColumnType("float");
        builder.Property<double>("BearishScore").HasColumnType("float");
        builder.Property<int>("TimeHorizonDays").HasColumnType("int");
    }

    private static void ConfigurePredictionMetaFields(EntityTypeBuilder builder)
    {
        builder.Property<bool>("IsMock").HasColumnType("bit");
        builder.Property<DateTime>("CreatedOn").HasColumnType("datetime2");
    }

    private static void ConfigurePredictionNavigation(EntityTypeBuilder builder)
    {
        builder.Navigation("Reasons");
    }

    private static void ConfigurePredictionReasonEntity(EntityTypeBuilder builder)
    {
        ConfigureIdentityId(builder);
        builder.Property<int>("PredictionId").HasColumnType("int");
        builder.Property<string>("Kind").IsRequired().HasMaxLength(ReasonKindMaxLength).HasColumnType("nvarchar(16)");
        builder.Property<string>("Text").IsRequired().HasMaxLength(ReasonTextMaxLength).HasColumnType("nvarchar(512)");
        builder.HasKey("Id");
        builder.HasIndex("PredictionId");
        builder.ToTable("PredictionReasons");
    }

    private static void ConfigurePredictionReasonNavigation(EntityTypeBuilder builder)
    {
        builder
            .HasOne("api.Models.Prediction", "Prediction")
            .WithMany("Reasons")
            .HasForeignKey("PredictionId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation("Prediction");
    }
}
