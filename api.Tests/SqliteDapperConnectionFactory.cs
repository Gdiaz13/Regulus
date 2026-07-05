using System.Data.Common;
using api.Services;
using Dapper;
using Microsoft.Data.Sqlite;

namespace api.Tests;

internal sealed class SqliteDapperConnectionFactory : IDatabaseConnectionFactory, IDisposable
{
    private readonly string _connectionString;
    private readonly SqliteConnection _root;

    public SqliteDapperConnectionFactory()
    {
        SqlMapper.AddTypeHandler(new SqliteGuidTypeHandler());
        SqlMapper.AddTypeHandler(new SqliteDateOnlyTypeHandler());
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
        create table users (
            id text primary key,
            email text not null,
            normalized_email text not null unique,
            display_name text not null,
            password_hash text not null,
            created_at text not null,
            updated_at text null,
            last_login_at text null,
            is_active integer not null default 1,
            email_confirmed integer not null default 0,
            failed_login_count integer not null default 0,
            lockout_until text null
        );

        create table refresh_tokens (
            id text primary key,
            user_id text not null references users(id) on delete cascade,
            token_hash text not null unique,
            created_at text not null,
            expires_at text not null,
            revoked_at text null,
            replaced_by_token_hash text null,
            created_by_ip text null,
            revoked_by_ip text null
        );

        create table user_settings (
            user_id text primary key references users(id) on delete cascade,
            settings_json text not null default '{}',
            created_at text not null default current_timestamp,
            updated_at text null
        );

        create table stocks (
            id integer primary key autoincrement,
            user_id text not null,
            symbol text not null,
            company_name text not null,
            purchase_price numeric not null,
            last_dividend numeric not null,
            industry text not null,
            market_cap integer not null,
            unique(user_id, symbol)
        );

        create table comments (
            id integer primary key autoincrement,
            user_id text not null,
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
            price_type text null,
            card_condition text null,
            grade text null,
            currency text null,
            unique(asset_id, date)
        );

        create table predictions (
            id integer primary key autoincrement,
            user_id text not null,
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

        create table model_accuracy_results (
            id integer primary key autoincrement,
            prediction_id integer not null references predictions(id) on delete cascade,
            user_id text not null,
            asset_id text not null,
            asset_type text not null,
            model_name text not null,
            model_version text not null,
            predicted_percent_change real not null,
            confidence_score real not null default 0,
            risk_score real not null default 0,
            bullish_score real not null default 0,
            bearish_score real not null default 0,
            time_horizon_days integer not null default 0,
            actual_percent_change real not null,
            absolute_percent_error real not null,
            direction_matched integer not null,
            actual_price numeric not null,
            target_date text not null,
            actual_date text not null,
            is_mock integer not null default 0,
            predicted_on text not null,
            scored_at text not null,
            unique(prediction_id)
        );

        create table background_job_runs (
            id integer primary key autoincrement,
            job_name text not null,
            status text not null,
            detail text null,
            items_processed integer not null default 0,
            started_at text not null default current_timestamp,
            finished_at text null
        );
        """;

    // PostgreSQL date columns come back as DateOnly through Npgsql; SQLite stores
    // them as text, so tests need this handler to read the same row types.
    private sealed class SqliteDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override DateOnly Parse(object value)
        {
            return value switch
            {
                DateOnly date => date,
                DateTime dateTime => DateOnly.FromDateTime(dateTime),
                _ => DateOnly.FromDateTime(DateTime.Parse(value.ToString() ?? string.Empty)),
            };
        }

        public override void SetValue(System.Data.IDbDataParameter parameter, DateOnly value)
        {
            parameter.Value = value.ToDateTime(TimeOnly.MinValue);
        }
    }

    private sealed class SqliteGuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override Guid Parse(object value)
        {
            return value is Guid guid ? guid : Guid.Parse(value.ToString() ?? string.Empty);
        }

        public override void SetValue(System.Data.IDbDataParameter parameter, Guid value)
        {
            parameter.Value = value.ToString();
        }
    }
}
