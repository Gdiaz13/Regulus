using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceHistory : Migration
    {
        private const string CreatePriceHistorySql = """
            CREATE TABLE [PriceHistories] (
                [Id] int NOT NULL IDENTITY,
                [AssetId] int NOT NULL,
                [Date] date NOT NULL,
                [Open] decimal(18,4) NOT NULL,
                [High] decimal(18,4) NOT NULL,
                [Low] decimal(18,4) NOT NULL,
                [Close] decimal(18,4) NOT NULL,
                [Volume] bigint NOT NULL,
                [Source] nvarchar(32) NOT NULL,
                [CreatedOn] datetime2 NOT NULL,
                CONSTRAINT [PK_PriceHistories] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_PriceHistories_Assets_AssetId]
                    FOREIGN KEY ([AssetId]) REFERENCES [Assets] ([Id]) ON DELETE CASCADE
            );
            """;

        private const string CreatePriceHistoryIndexSql = """
            CREATE UNIQUE INDEX [IX_PriceHistories_AssetId_Date] ON [PriceHistories] ([AssetId], [Date]);
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            CreatePriceHistoryTable(migrationBuilder);
            CreatePriceHistoryIndex(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PriceHistories");
        }

        private static void CreatePriceHistoryTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(CreatePriceHistorySql);
        }

        private static void CreatePriceHistoryIndex(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(CreatePriceHistoryIndexSql);
        }
    }
}
