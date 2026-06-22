using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    // A tracked thing of any kind: stock, ETF, TCG card, crypto, or collectible.
    // AssetType keeps the table flexible so a new market never needs a new schema.
    public class Asset
    {
        public const int SymbolMaxLength = 32;
        public const int AssetTypeMaxLength = 32;

        public int Id { get; set; }

        // Ticker for stocks/ETFs/crypto, card code for TCG, etc.
        [MaxLength(SymbolMaxLength)]
        public string Symbol { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public AssetType AssetType { get; set; }

        // Optional so an asset can exist before it is sorted into a category.
        public int? CategoryId { get; set; }
        public AssetCategory? Category { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
