using api.Contracts;
using api.Models;
using Dapper;
using Npgsql;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Text.Json;

namespace api.Services;

// Persists opaque trainer artifacts and metrics without coupling PostgreSQL to
// one model implementation. Only honest, real training responses belong here.
public sealed class TrainedModelVersionStore
{
    public const int MaxArtifactBytes = 256 * 1024;
    public const int MaxMetricsBytes = 64 * 1024;
    public const int VersionsRetainedPerModel = 100;
    private const string ExpectedCategory = "Technology";
    private const int SerializationAttempts = 3;
    private readonly IDatabaseConnectionFactory _factory;

    public TrainedModelVersionStore(IDatabaseConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task SaveAsync(
        string category,
        int seriesCount,
        AiTrainResponse response,
        CancellationToken token = default)
    {
        var payload = Prepare(category, seriesCount, response);
        var parameters = Parameters(category, seriesCount, response, payload);
        await PersistWithRetryAsync(parameters, response.ModelName, category, token);
    }

    private async Task PersistWithRetryAsync(
        object parameters, string modelName, string category, CancellationToken token)
    {
        for (var attempt = 1; ; attempt++)
        {
            if (await TryPersistAsync(parameters, modelName, category, attempt, token))
            {
                return;
            }
        }
    }

    private async Task<bool> TryPersistAsync(
        object parameters, string modelName, string category, int attempt, CancellationToken token)
    {
        try
        {
            await PersistAsync(parameters, modelName, category, token);
            return true;
        }
        catch (PostgresException exception) when (
            exception.SqlState == PostgresErrorCodes.SerializationFailure && attempt < SerializationAttempts)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(25), token);
            return false;
        }
    }

    private async Task PersistAsync(
        object parameters, string modelName, string category, CancellationToken token)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync(token);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable, token);
        await connection.ExecuteAsync(Command(Sql.Insert, parameters, token, transaction));
        var prune = new { ModelName = modelName, Category = category, Keep = VersionsRetainedPerModel };
        await connection.ExecuteAsync(Command(Sql.Prune, prune, token, transaction));
        await transaction.CommitAsync(token);
    }

    public async Task<List<TrainedModelVersion>> ListRecentAsync(
        int take,
        CancellationToken token = default)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync(token);
        var rows = await connection.QueryAsync<TrainedModelVersion>(
            Command(Sql.ListRecent, new { Take = Math.Clamp(take, 1, 100) }, token));
        return rows.ToList();
    }

    private static PreparedPayload Prepare(string category, int seriesCount, AiTrainResponse response)
    {
        ValidateEnvelope(category, seriesCount, response);
        var artifact = RequireObject(response.Artifact, "artifact");
        var metrics = RequireObject(response.Metrics, "metrics");
        var (testMae, baselineMae) = RequireMaes(metrics);
        return new(BoundedJson(artifact, MaxArtifactBytes, "artifact"),
            BoundedJson(metrics, MaxMetricsBytes, "metrics"), testMae < baselineMae);
    }

    private static void ValidateEnvelope(string category, int seriesCount, AiTrainResponse response)
    {
        StockTechTrainingProtocol.Validate(response);
        if (!response.Trained || category != ExpectedCategory || seriesCount < 1)
        {
            throw new InvalidOperationException("The trainer returned an unsupported or unsuccessful response.");
        }
    }

    private static JsonElement RequireObject(JsonElement? value, string field)
    {
        if (value is not { ValueKind: JsonValueKind.Object })
        {
            throw new InvalidOperationException($"The trainer {field} must be a JSON object.");
        }
        return value.Value;
    }

    private static (double TestMae, double BaselineMae) RequireMaes(JsonElement metrics)
    {
        if (!Number(metrics, "testMae", out var testMae) || testMae < 0
            || !Number(metrics, "baselineMae", out var baselineMae) || baselineMae < 0)
        {
            throw new InvalidOperationException("The trainer metrics require finite non-negative MAEs.");
        }
        return (testMae, baselineMae);
    }

    private static string BoundedJson(JsonElement value, int maxBytes, string field)
    {
        var json = value.GetRawText();
        if (Encoding.UTF8.GetByteCount(json) > maxBytes)
        {
            throw new InvalidOperationException($"The trainer {field} exceeds its storage limit.");
        }
        return json;
    }


    private static object Parameters(string category, int seriesCount, AiTrainResponse response, PreparedPayload payload)
    {
        return new
        {
            response.ModelName,
            response.ModelVersion,
            Category = category,
            response.ContractVersion,
            SeriesCount = seriesCount,
            payload.ArtifactJson,
            payload.MetricsJson,
            payload.PromotionEligible,
            CreatedAt = DateTime.UtcNow,
        };
    }

    private static bool Number(JsonElement metrics, string name, out double result)
    {
        result = 0;
        return metrics.ValueKind == JsonValueKind.Object
            && metrics.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.Number
            && value.TryGetDouble(out result)
            && double.IsFinite(result);
    }

    private static CommandDefinition Command(
        string sql, object parameters, CancellationToken token, DbTransaction? transaction = null)
    {
        return new CommandDefinition(sql, parameters, transaction, cancellationToken: token);
    }

    private sealed record PreparedPayload(
        string ArtifactJson, string MetricsJson, bool PromotionEligible);

    private static class Sql
    {
        public const string Insert = """
            insert into trained_model_versions
                (model_name, model_version, category, contract_version, series_count,
                 artifact_json, metrics_json, promotion_eligible, created_at)
            values
                (@ModelName, @ModelVersion, @Category, @ContractVersion, @SeriesCount,
                 @ArtifactJson, @MetricsJson, @PromotionEligible, @CreatedAt);
            """;

        public const string ListRecent = """
            select id as "Id", model_name as "ModelName", model_version as "ModelVersion",
                   category as "Category", contract_version as "ContractVersion",
                   series_count as "SeriesCount", artifact_json as "ArtifactJson",
                   metrics_json as "MetricsJson", promotion_eligible as "PromotionEligible",
                   created_at as "CreatedAt"
            from trained_model_versions
            order by created_at desc, id desc
            limit @Take;
            """;

        public const string Prune = """
            delete from trained_model_versions
            where model_name = @ModelName and category = @Category
              and id not in (
                  select id from trained_model_versions
                  where model_name = @ModelName and category = @Category
                  order by created_at desc, id desc
                  limit @Keep
              );
            """;
    }
}
