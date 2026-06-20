using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Migrations;

internal static class MigrationModelConfiguration
{
    private const int StockSymbolMaxLength = 32;

    public static void ConfigureInitial(ModelBuilder modelBuilder)
    {
        Configure(modelBuilder, ConfigureInitialCommentEntity, ConfigureInitialStockEntity, ConfigureInitialCommentNavigation);
    }

    public static void ConfigureUniqueStockSymbol(ModelBuilder modelBuilder)
    {
        Configure(modelBuilder, ConfigureInitialCommentEntity, ConfigureCurrentStockEntity, ConfigureInitialCommentNavigation);
    }

    public static void ConfigureCurrent(ModelBuilder modelBuilder)
    {
        Configure(modelBuilder, ConfigureCurrentCommentEntity, ConfigureCurrentStockEntity, ConfigureCurrentCommentNavigation);
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
}
