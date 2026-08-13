namespace api.Services;

public static class RegulasAuthDefaults
{
    public const string Scheme = "RegulasBearer";
    public const string TokenHashClaim = "regulas:token_hash";

    // The claim is only ever issued to users the database says are admins, and
    // it is re-read from the row on every request because the bearer token
    // carries no rights of its own. Demoting a user takes effect immediately.
    public const string AdminClaim = "regulas:admin";
    public const string AdminPolicy = "RegulasAdmin";
}
