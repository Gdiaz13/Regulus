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

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            DropCommentForeignKey(migrationBuilder);
            DeleteOrphanComments(migrationBuilder);
            RequireStockId(migrationBuilder);
            AddRequiredForeignKey(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DropCommentForeignKey(migrationBuilder);
            AllowNullableStockId(migrationBuilder);
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
