using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    // Groups assets under a market segment, e.g. "Technology" stocks or "Pokemon"
    // cards. Categories line up with the Category-AI layer (StockAI, TCGAI) later.
    public class AssetCategory
    {
        public const int NameMaxLength = 64;
        public const int SlugMaxLength = 64;

        public int Id { get; set; }

        [MaxLength(NameMaxLength)]
        public string Name { get; set; } = string.Empty;

        // URL- and code-friendly key, e.g. "technology" or "pokemon".
        [MaxLength(SlugMaxLength)]
        public string Slug { get; set; } = string.Empty;

        public AssetType AssetType { get; set; }

        public List<Asset> Assets { get; set; } = new List<Asset>();
    }
}
