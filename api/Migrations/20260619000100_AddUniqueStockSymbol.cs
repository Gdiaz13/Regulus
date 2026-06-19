using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueStockSymbol : Migration
    {
        private const string LimitSymbolSql = """
            ALTER TABLE [Stocks] ALTER COLUMN [Symbol] nvarchar(32) NOT NULL;
            """;

        private const string RemoveSymbolLimitSql = """
            ALTER TABLE [Stocks] ALTER COLUMN [Symbol] nvarchar(max) NOT NULL;
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            LimitSymbolLength(migrationBuilder);
            CreateUniqueSymbolIndex(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DropUniqueSymbolIndex(migrationBuilder);
            RemoveSymbolLimit(migrationBuilder);
        }

        private static void LimitSymbolLength(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(LimitSymbolSql);
        }

        private static void CreateUniqueSymbolIndex(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Stocks_Symbol",
                table: "Stocks",
                column: "Symbol",
                unique: true
            );
        }

        private static void DropUniqueSymbolIndex(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Stocks_Symbol", table: "Stocks");
        }

        private static void RemoveSymbolLimit(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(RemoveSymbolLimitSql);
        }
    }
}
