using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        private const string CreateStocksSql = """
            CREATE TABLE [Stocks] (
                [Id] int NOT NULL IDENTITY,
                [Symbol] nvarchar(max) NOT NULL,
                [CompanyName] nvarchar(max) NOT NULL,
                [PurchasePrice] decimal(18,2) NOT NULL,
                [LastDividend] decimal(18,2) NOT NULL,
                [Industry] nvarchar(max) NOT NULL,
                [MarketCap] bigint NOT NULL,
                CONSTRAINT [PK_Stocks] PRIMARY KEY ([Id])
            );
            """;

        private const string CreateCommentsSql = """
            CREATE TABLE [Comments] (
                [Id] int NOT NULL IDENTITY,
                [Title] nvarchar(max) NOT NULL,
                [Content] nvarchar(max) NOT NULL,
                [CreatedOn] datetime2 NOT NULL,
                [StockId] int NULL,
                CONSTRAINT [PK_Comments] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_Comments_Stocks_StockId]
                    FOREIGN KEY ([StockId]) REFERENCES [Stocks] ([Id])
            );
            """;

        private const string CreateCommentIndexSql = """
            CREATE INDEX [IX_Comments_StockId] ON [Comments] ([StockId]);
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            CreateStocksTable(migrationBuilder);
            CreateCommentsTable(migrationBuilder);
            CreateCommentIndex(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DropCommentsTable(migrationBuilder);
            DropStocksTable(migrationBuilder);
        }

        private static void CreateStocksTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(CreateStocksSql);
        }

        private static void CreateCommentsTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(CreateCommentsSql);
        }

        private static void CreateCommentIndex(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(CreateCommentIndexSql);
        }

        private static void DropCommentsTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Comments");
        }

        private static void DropStocksTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Stocks");
        }
    }
}
