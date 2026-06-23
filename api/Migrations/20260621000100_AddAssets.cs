using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddAssets : Migration
    {
        private const string CreateAssetCategoriesSql = """
            CREATE TABLE [AssetCategories] (
                [Id] int NOT NULL IDENTITY,
                [Name] nvarchar(64) NOT NULL,
                [Slug] nvarchar(64) NOT NULL,
                [AssetType] nvarchar(32) NOT NULL,
                CONSTRAINT [PK_AssetCategories] PRIMARY KEY ([Id])
            );
            """;

        private const string CreateAssetsSql = """
            CREATE TABLE [Assets] (
                [Id] int NOT NULL IDENTITY,
                [Symbol] nvarchar(32) NOT NULL,
                [Name] nvarchar(max) NOT NULL,
                [AssetType] nvarchar(32) NOT NULL,
                [CategoryId] int NULL,
                [CreatedOn] datetime2 NOT NULL,
                CONSTRAINT [PK_Assets] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_Assets_AssetCategories_CategoryId]
                    FOREIGN KEY ([CategoryId]) REFERENCES [AssetCategories] ([Id]) ON DELETE SET NULL
            );
            """;

        private const string CreateAssetIndexesSql = """
            CREATE UNIQUE INDEX [IX_AssetCategories_Name] ON [AssetCategories] ([Name]);
            CREATE UNIQUE INDEX [IX_AssetCategories_Slug] ON [AssetCategories] ([Slug]);
            CREATE INDEX [IX_Assets_CategoryId] ON [Assets] ([CategoryId]);
            CREATE UNIQUE INDEX [IX_Assets_AssetType_Symbol] ON [Assets] ([AssetType], [Symbol]);
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            CreateAssetCategoriesTable(migrationBuilder);
            CreateAssetsTable(migrationBuilder);
            CreateAssetIndexes(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DropAssetsTable(migrationBuilder);
            DropAssetCategoriesTable(migrationBuilder);
        }

        private static void CreateAssetCategoriesTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(CreateAssetCategoriesSql);
        }

        private static void CreateAssetsTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(CreateAssetsSql);
        }

        private static void CreateAssetIndexes(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(CreateAssetIndexesSql);
        }

        private static void DropAssetsTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Assets");
        }

        private static void DropAssetCategoriesTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AssetCategories");
        }
    }
}
