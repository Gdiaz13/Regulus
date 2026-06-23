namespace api.Models
{
    // The kind of thing Regulas tracks. Stored as a string in the database so new
    // markets (crypto, collectibles) can be added without renumbering and so the
    // column stays readable in SQL Server or a future PostgreSQL move.
    public enum AssetType
    {
        Stock,
        Etf,
        TcgCard,
        Crypto,
        Collectible,
    }
}
