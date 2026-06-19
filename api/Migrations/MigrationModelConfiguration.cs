using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api.Migrations;

internal static class MigrationModelConfiguration
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        ConfigureAnnotations(modelBuilder);
        ConfigureCommentModel(modelBuilder);
        ConfigureStockModel(modelBuilder);
        ConfigureNavigations(modelBuilder);
    }

    private static void ConfigureAnnotations(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "9.0.10")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);
        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);
    }

    private static void ConfigureCommentModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity("api.Models.Comment", ConfigureCommentEntity);
    }

    private static void ConfigureStockModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity("api.Models.Stock", ConfigureStockEntity);
    }

    private static void ConfigureNavigations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity("api.Models.Comment", ConfigureCommentNavigation);
        modelBuilder.Entity("api.Models.Stock", ConfigureStockNavigation);
    }

    private static void ConfigureCommentEntity(EntityTypeBuilder builder)
    {
        ConfigureCommentId(builder);
        ConfigureCommentFields(builder);
        builder.HasKey("Id");
        builder.HasIndex("StockId");
        builder.ToTable("Comments");
    }

    private static void ConfigureCommentId(EntityTypeBuilder builder)
    {
        var id = builder.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
        SqlServerPropertyBuilderExtensions.UseIdentityColumn(id);
    }

    private static void ConfigureCommentFields(EntityTypeBuilder builder)
    {
        builder.Property<string>("Content").IsRequired().HasColumnType("nvarchar(max)");
        builder.Property<DateTime>("CreatedOn").HasColumnType("datetime2");
        builder.Property<int?>("StockId").HasColumnType("int");
        builder.Property<string>("Title").IsRequired().HasColumnType("nvarchar(max)");
    }

    private static void ConfigureStockEntity(EntityTypeBuilder builder)
    {
        ConfigureStockId(builder);
        ConfigureStockFields(builder);
        builder.HasKey("Id");
        builder.ToTable("Stocks");
    }

    private static void ConfigureStockId(EntityTypeBuilder builder)
    {
        var id = builder.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
        SqlServerPropertyBuilderExtensions.UseIdentityColumn(id);
    }

    private static void ConfigureStockFields(EntityTypeBuilder builder)
    {
        builder.Property<string>("CompanyName").IsRequired().HasColumnType("nvarchar(max)");
        builder.Property<string>("Industry").IsRequired().HasColumnType("nvarchar(max)");
        builder.Property<decimal>("LastDividend").HasColumnType("decimal(18,2)");
        builder.Property<long>("MarketCap").HasColumnType("bigint");
        builder.Property<decimal>("PurchasePrice").HasColumnType("decimal(18,2)");
        builder.Property<string>("Symbol").IsRequired().HasColumnType("nvarchar(max)");
    }

    private static void ConfigureCommentNavigation(EntityTypeBuilder builder)
    {
        builder.HasOne("api.Models.Stock", "Stock").WithMany("Comments").HasForeignKey("StockId");
        builder.Navigation("Stock");
    }

    private static void ConfigureStockNavigation(EntityTypeBuilder builder)
    {
        builder.Navigation("Comments");
    }
}
