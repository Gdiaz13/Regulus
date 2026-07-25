using api.Contracts;
using Dapper;

namespace api.Services;

// Builds bounded training inputs from assets that have both saved predictions
// and stored closes. Predictions provide the specialist category; prices supply
// the chronological observations.
public sealed class ModelTrainingDataStore
{
    private readonly IDatabaseConnectionFactory _factory;

    public ModelTrainingDataStore(IDatabaseConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<List<AiTrainSeries>> ListSeriesAsync(
        string category,
        int maxSeries,
        int pointsPerSeries,
        CancellationToken token = default
    )
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync(token);
        var parameters = Parameters(category, maxSeries, pointsPerSeries);
        var command = new CommandDefinition(Sql.ListSeries, parameters, cancellationToken: token);
        var rows = await connection.QueryAsync<TrainingRow>(command);
        return ToSeries(rows);
    }

    private static object Parameters(string category, int maxSeries, int pointsPerSeries)
    {
        return new
        {
            Category = category.Trim(),
            MaxSeries = Math.Clamp(maxSeries, 1, 50),
            PointsPerSeries = Math.Clamp(pointsPerSeries, 30, 1000),
        };
    }

    private static List<AiTrainSeries> ToSeries(IEnumerable<TrainingRow> rows)
    {
        return rows
            .GroupBy(row => row.Symbol, StringComparer.OrdinalIgnoreCase)
            .Select(group => new AiTrainSeries(group.Key, group.Select(row => row.Close).ToList()))
            .ToList();
    }

    private sealed class TrainingRow
    {
        public string Symbol { get; init; } = string.Empty;
        public decimal Close { get; init; }
    }

    private static class Sql
    {
        public const string ListSeries = """
            with latest_predictions as (
                select pr.asset_id, pr.asset_type, pr.category,
                       row_number() over (
                           partition by upper(pr.asset_id), pr.asset_type
                           order by pr.created_on desc, pr.id desc
                       ) as prediction_rank
                from predictions pr
                where not pr.is_mock
            ),
            eligible as (
                select a.id, a.symbol,
                       (select max(ph.date) from price_history ph where ph.asset_id = a.id) as latest_date
                from assets a
                join latest_predictions pr on upper(pr.asset_id) = upper(a.symbol)
                                          and pr.asset_type = a.asset_type
                                          and pr.prediction_rank = 1
                where a.asset_type = 'Stock'
                  and lower(pr.category) = lower(@Category)
                  and exists (select 1 from price_history ph where ph.asset_id = a.id)
                order by latest_date desc, a.symbol
                limit @MaxSeries
            ),
            ranked as (
                select e.symbol as "Symbol", ph.close_price as "Close", ph.date,
                       row_number() over (partition by e.id order by ph.date desc) as row_number
                from eligible e
                join price_history ph on ph.asset_id = e.id
            )
            select "Symbol", "Close"
            from ranked
            where row_number <= @PointsPerSeries
            order by "Symbol", date;
            """;
    }
}
