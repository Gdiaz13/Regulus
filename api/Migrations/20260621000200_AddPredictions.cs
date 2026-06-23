using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddPredictions : Migration
    {
        private const string CreatePredictionsSql = """
            CREATE TABLE [Predictions] (
                [Id] int NOT NULL IDENTITY,
                [AssetId] nvarchar(64) NOT NULL,
                [AssetName] nvarchar(max) NOT NULL,
                [AssetType] nvarchar(32) NOT NULL,
                [Category] nvarchar(max) NOT NULL,
                [CurrentPrice] decimal(18,2) NOT NULL,
                [PredictedPrice] decimal(18,2) NOT NULL,
                [PredictedPercentChange] float NOT NULL,
                [ConfidenceScore] float NOT NULL,
                [RiskScore] float NOT NULL,
                [BullishScore] float NOT NULL,
                [BearishScore] float NOT NULL,
                [TimeHorizonDays] int NOT NULL,
                [ModelName] nvarchar(64) NOT NULL,
                [ModelVersion] nvarchar(64) NOT NULL,
                [IsMock] bit NOT NULL,
                [CreatedOn] datetime2 NOT NULL,
                CONSTRAINT [PK_Predictions] PRIMARY KEY ([Id])
            );
            """;

        private const string CreatePredictionReasonsSql = """
            CREATE TABLE [PredictionReasons] (
                [Id] int NOT NULL IDENTITY,
                [PredictionId] int NOT NULL,
                [Kind] nvarchar(16) NOT NULL,
                [Text] nvarchar(512) NOT NULL,
                CONSTRAINT [PK_PredictionReasons] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_PredictionReasons_Predictions_PredictionId]
                    FOREIGN KEY ([PredictionId]) REFERENCES [Predictions] ([Id]) ON DELETE CASCADE
            );
            """;

        private const string CreatePredictionIndexesSql = """
            CREATE INDEX [IX_Predictions_AssetId_CreatedOn] ON [Predictions] ([AssetId], [CreatedOn]);
            CREATE INDEX [IX_PredictionReasons_PredictionId] ON [PredictionReasons] ([PredictionId]);
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            CreatePredictionsTable(migrationBuilder);
            CreatePredictionReasonsTable(migrationBuilder);
            CreatePredictionIndexes(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DropPredictionReasonsTable(migrationBuilder);
            DropPredictionsTable(migrationBuilder);
        }

        private static void CreatePredictionsTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(CreatePredictionsSql);
        }

        private static void CreatePredictionReasonsTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(CreatePredictionReasonsSql);
        }

        private static void CreatePredictionIndexes(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(CreatePredictionIndexesSql);
        }

        private static void DropPredictionReasonsTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PredictionReasons");
        }

        private static void DropPredictionsTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Predictions");
        }
    }
}
