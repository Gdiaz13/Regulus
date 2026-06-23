using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class RequireCommentStock : Migration
    {
        private const string DeleteOrphanCommentsSql = """
            DELETE FROM [Comments] WHERE [StockId] IS NULL;
            """;

        private const string RequireStockIdSql = """
            ALTER TABLE [Comments] ALTER COLUMN [StockId] int NOT NULL;
            """;

        private const string AllowNullableStockIdSql = """
            ALTER TABLE [Comments] ALTER COLUMN [StockId] int NULL;
            """;

        // SQL Server will not alter a column's nullability while an index depends on
        // it, so the index is dropped around the ALTER and rebuilt afterwards.
        private const string DropStockIdIndexSql = """
            DROP INDEX [IX_Comments_StockId] ON [Comments];
            """;

        private const string CreateStockIdIndexSql = """
            CREATE INDEX [IX_Comments_StockId] ON [Comments] ([StockId]);
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            DropCommentForeignKey(migrationBuilder);
            DeleteOrphanComments(migrationBuilder);
            DropStockIdIndex(migrationBuilder);
            RequireStockId(migrationBuilder);
            CreateStockIdIndex(migrationBuilder);
            AddRequiredForeignKey(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DropCommentForeignKey(migrationBuilder);
            DropStockIdIndex(migrationBuilder);
            AllowNullableStockId(migrationBuilder);
            CreateStockIdIndex(migrationBuilder);
            AddOptionalForeignKey(migrationBuilder);
        }

        private static void DeleteOrphanComments(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(DeleteOrphanCommentsSql);
        }

        private static void RequireStockId(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(RequireStockIdSql);
        }

        private static void AllowNullableStockId(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(AllowNullableStockIdSql);
        }

        private static void DropStockIdIndex(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(DropStockIdIndexSql);
        }

        private static void CreateStockIdIndex(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(CreateStockIdIndexSql);
        }

        private static void DropCommentForeignKey(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Comments_Stocks_StockId", table: "Comments");
        }

        private static void AddRequiredForeignKey(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Stocks_StockId",
                table: "Comments",
                column: "StockId",
                principalTable: "Stocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        private static void AddOptionalForeignKey(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Stocks_StockId",
                table: "Comments",
                column: "StockId",
                principalTable: "Stocks",
                principalColumn: "Id"
            );
        }
    }
}
