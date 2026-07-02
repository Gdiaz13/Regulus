using api.Models;
using Dapper;

namespace api.Services;

// Dapper-backed auth storage. Raw tokens are never saved, only their hashes.
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

    public async Task<RegulasUser?> FindByNormalizedEmailAsync(string normalizedEmail)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<RegulasUser>(Sql.UserByEmail, new { NormalizedEmail = normalizedEmail });
    }

    public async Task<RegulasUser?> FindByIdAsync(Guid id)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<RegulasUser>(Sql.UserById, new { Id = id });
    }

    public async Task<RefreshToken> CreateRefreshTokenAsync(Guid userId, string tokenHash, DateTime expiresAt)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        var token = new { Id = Guid.NewGuid(), UserId = userId, TokenHash = tokenHash, CreatedAt = DateTime.UtcNow, ExpiresAt = expiresAt };
        return await connection.QuerySingleAsync<RefreshToken>(Sql.InsertRefreshToken, token);
    }

    public async Task<RegulasUser?> FindUserByTokenHashAsync(string tokenHash)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<RegulasUser>(Sql.UserByTokenHash, new { TokenHash = tokenHash, Now = DateTime.UtcNow });
    }

    public async Task<bool> RevokeRefreshTokenAsync(string tokenHash)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        var parameters = new { TokenHash = tokenHash, RevokedAt = DateTime.UtcNow };
        return await connection.ExecuteAsync(Sql.RevokeRefreshToken, parameters) > 0;
    }

    public async Task MarkLoginAsync(Guid userId)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        await connection.ExecuteAsync(Sql.MarkLogin, new { Id = userId, LastLoginAt = DateTime.UtcNow });
    }

    public async Task UpdatePasswordHashAsync(Guid userId, string passwordHash)
    {
        await using var connection = await _factory.OpenDatabaseConnectionAsync();
        var parameters = new { Id = userId, PasswordHash = passwordHash, UpdatedAt = DateTime.UtcNow };
        await connection.ExecuteAsync(Sql.UpdatePasswordHash, parameters);
    }

    private static class Sql
    {
        private const string UserColumns = """
            id as "Id", email as "Email", normalized_email as "NormalizedEmail",
            display_name as "DisplayName", password_hash as "PasswordHash",
            created_at as "CreatedAt", updated_at as "UpdatedAt",
            last_login_at as "LastLoginAt", is_active as "IsActive",
            email_confirmed as "EmailConfirmed", failed_login_count as "FailedLoginCount",
            lockout_until as "LockoutUntil"
            """;

        private const string JoinedUserColumns = """
            u.id as "Id", u.email as "Email", u.normalized_email as "NormalizedEmail",
            u.display_name as "DisplayName", u.password_hash as "PasswordHash",
            u.created_at as "CreatedAt", u.updated_at as "UpdatedAt",
            u.last_login_at as "LastLoginAt", u.is_active as "IsActive",
            u.email_confirmed as "EmailConfirmed", u.failed_login_count as "FailedLoginCount",
            u.lockout_until as "LockoutUntil"
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

        public const string InsertRefreshToken = """
            insert into refresh_tokens (id, user_id, token_hash, created_at, expires_at)
            values (@Id, @UserId, @TokenHash, @CreatedAt, @ExpiresAt)
            returning id as "Id", user_id as "UserId", token_hash as "TokenHash",
                      created_at as "CreatedAt", expires_at as "ExpiresAt",
                      revoked_at as "RevokedAt",
                      replaced_by_token_hash as "ReplacedByTokenHash";
            """;

        public const string UserByTokenHash = $"""
            select {JoinedUserColumns}
            from refresh_tokens t
            join users u on u.id = t.user_id
            where t.token_hash = @TokenHash and t.revoked_at is null
              and t.expires_at > @Now and u.is_active = true;
            """;

        public const string RevokeRefreshToken = """
            update refresh_tokens
            set revoked_at = @RevokedAt
            where token_hash = @TokenHash and revoked_at is null;
            """;

        public const string MarkLogin = """
            update users
            set last_login_at = @LastLoginAt
            where id = @Id;
            """;

        public const string UpdatePasswordHash = """
            update users
            set password_hash = @PasswordHash, updated_at = @UpdatedAt
            where id = @Id;
            """;
    }
}
