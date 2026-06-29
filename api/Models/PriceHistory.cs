using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    // One end-of-day price point for an asset. History is stored so the AI layer
    // has real data to learn from and so past predictions can be scored against
    // what actually happened. Linked to Assets so any market reuses the table.
    public class PriceHistory
    {
        public const int SourceMaxLength = 32;

        public int Id { get; set; }

        public int AssetId { get; set; }
        public Asset? Asset { get; set; }

        public DateOnly Date { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Open { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal High { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Low { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Close { get; set; }

        public long Volume { get; set; }

        [MaxLength(SourceMaxLength)]
        public string Source { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
