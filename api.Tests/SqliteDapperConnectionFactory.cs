using System.Data.Common;
using api.Services;
using Microsoft.Data.Sqlite;

namespace api.Tests;

internal sealed class SqliteDapperConnectionFactory : IDatabaseConnectionFactory, IDisposable
{
    private readonly string _connectionString;
    private readonly SqliteConnection _root;

    public SqliteDapperConnectionFactory()
    {
        _connectionString = $"Data Source=regulas-{Guid.NewGuid()};Mode=Memory;Cache=Shared";
        _root = new SqliteConnection(_connectionString);
        _root.Open();
        CreateSchema();
    }

    public async Task<DbConnection> OpenDatabaseConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    public void Dispose()
    {
        _root.Dispose();
    }

    private void CreateSchema()
    {
        using var command = _root.CreateCommand();
        command.CommandText = SchemaSql;
        command.ExecuteNonQuery();
    }

    private const string SchemaSql = """
        create table stocks (
            id integer primary key autoincrement,
            symbol text not null unique,
            company_name text not null,
            purchase_price numeric not null,
            last_dividend numeric not null,
            industry text not null,
            market_cap integer not null
        );

        create table comments (
            id integer primary key autoincrement,
            title text not null,
            content text not null,
            created_on text not null,
            stock_id integer not null references stocks(id) on delete cascade
        );

        create table asset_categories (
            id integer primary key autoincrement,
            name text not null unique,
            slug text not null unique,
            asset_type text not null
        );

        create table assets (
            id integer primary key autoincrement,
            symbol text not null,
            name text not null,
            asset_type text not null,
            category_id integer null references asset_categories(id) on delete set null,
            created_on text not null default current_timestamp,
            unique(asset_type, symbol)
        );

        create table price_history (
            id integer primary key autoincrement,
            asset_id integer not null references assets(id) on delete cascade,
            date text not null,
            open_price numeric not null,
            high_price numeric not null,
            low_price numeric not null,
            close_price numeric not null,
            volume integer not null,
            source text not null,
            unique(asset_id, date)
        );

        create table predictions (
            id integer primary key autoincrement,
            asset_id text not null,
            asset_name text not null,
            asset_type text not null,
            category text not null,
            current_price numeric not null,
            predicted_price numeric not null,
            predicted_percent_change real not null,
            confidence_score real not null,
            risk_score real not null,
            bullish_score real not null,
            bearish_score real not null,
            time_horizon_days integer not null,
            model_name text not null,
            model_version text not null,
            is_mock integer not null,
            created_on text not null
        );

        create table prediction_reasons (
            id integer primary key autoincrement,
            prediction_id integer not null references predictions(id) on delete cascade,
            kind text not null,
            text text not null
        );
        """;
}
