using api.Models;
using Dapper;

namespace api.Services;

// Stores users and bearer sessions without taking a dependency on EF Identity.
public sealed class AuthStore
{
    private readonly IDatabaseConnectionFactory _factory;

    public AuthStore(IDatabaseConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<RegulasUser> CreateUserAsync(RegulasUser user)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.QuerySingleAsync<RegulasUser>(Sql.InsertUser, user);
    }

    public async Task<RegulasUser?> FindByEmailAsync(string normalizedEmail)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<RegulasUser>(Sql.UserByEmail, new { NormalizedEmail = normalizedEmail });
    }

    public async Task<RegulasUser?> FindByIdAsync(Guid id)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<RegulasUser>(Sql.UserById, new { Id = id });
    }

    public async Task<AuthSession> CreateSessionAsync(Guid userId, string tokenHash, DateTime expiresAt)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        var session = new { TokenHash = tokenHash, UserId = userId, CreatedAt = DateTime.UtcNow, ExpiresAt = expiresAt };
        return await connection.QuerySingleAsync<AuthSession>(Sql.InsertSession, session);
    }

    public async Task<RegulasUser?> FindUserByTokenHashAsync(string tokenHash)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<RegulasUser>(Sql.UserBySession, new { TokenHash = tokenHash, Now = DateTime.UtcNow });
    }

    public async Task<bool> RevokeSessionAsync(string tokenHash)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.ExecuteAsync(Sql.RevokeSession, new { TokenHash = tokenHash, RevokedAt = DateTime.UtcNow }) > 0;
    }

    public async Task UpdatePasswordHashAsync(Guid id, string passwordHash)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        await connection.ExecuteAsync(Sql.UpdatePasswordHash, new { Id = id, PasswordHash = passwordHash, UpdatedAt = DateTime.UtcNow });
    }

    public async Task MarkLoginAsync(Guid id)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        await connection.ExecuteAsync(Sql.MarkLogin, new { Id = id, LastLoginAt = DateTime.UtcNow });
    }

    private static class Sql
    {
        private const string UserColumns = """
            id as "Id", email as "Email", normalized_email as "NormalizedEmail",
            display_name as "DisplayName", password_hash as "PasswordHash",
            created_at as "CreatedAt", updated_at as "UpdatedAt",
            last_login_at as "LastLoginAt", is_active as "IsActive"
            """;

        public const string InsertUser = $"""
            insert into users
                (id, email, normalized_email, display_name, password_hash, created_at, is_active)
            values
                (@Id, @Email, @NormalizedEmail, @DisplayName, @PasswordHash, @CreatedAt, @IsActive)
            returning {UserColumns};
            """;

        public const string UserByEmail = $"""
            select {UserColumns}
            from users
            where normalized_email = @NormalizedEmail;
            """;

        public const string UserById = $"""
            select {UserColumns}
            from users
            where id = @Id;
            """;

        public const string InsertSession = """
            insert into auth_sessions (token_hash, user_id, created_at, expires_at)
            values (@TokenHash, @UserId, @CreatedAt, @ExpiresAt)
            returning token_hash as "TokenHash", user_id as "UserId",
                      created_at as "CreatedAt", expires_at as "ExpiresAt",
                      revoked_at as "RevokedAt";
            """;

        public const string UserBySession = $"""
            select {UserColumns}
            from auth_sessions s
            join users u on u.id = s.user_id
            where s.token_hash = @TokenHash and s.revoked_at is null
              and s.expires_at > @Now and u.is_active = true;
            """;

        public const string RevokeSession = """
            update auth_sessions
            set revoked_at = @RevokedAt
            where token_hash = @TokenHash and revoked_at is null;
            """;

        public const string UpdatePasswordHash = """
            update users
            set password_hash = @PasswordHash, updated_at = @UpdatedAt
            where id = @Id;
            """;

        public const string MarkLogin = """
            update users
            set last_login_at = @LastLoginAt
            where id = @Id;
            """;
    }
}
