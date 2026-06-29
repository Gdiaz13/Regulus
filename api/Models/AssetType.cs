namespace api.Models
{
    // The kind of thing Regulas tracks. Stored as a string in the database so new
    // markets (crypto, collectibles) can be added without renumbering.
    public enum AssetType
    {
        Stock,
        Etf,
        TcgCard,
        Crypto,
        Collectible,
    }
}
