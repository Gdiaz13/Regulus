using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    // A saved AI prediction. Every prediction is stored so accuracy can be
    // checked later - that is how Regulas tells which models actually perform.
    public class Prediction
    {
        public const int AssetIdMaxLength = 64;
        public const int ModelNameMaxLength = 64;

        public int Id { get; set; }

        [MaxLength(AssetIdMaxLength)]
        public string AssetId { get; set; } = string.Empty;

        public string AssetName { get; set; } = string.Empty;
        public AssetType AssetType { get; set; }
        public string Category { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PredictedPrice { get; set; }

        public double PredictedPercentChange { get; set; }
        public double ConfidenceScore { get; set; }
        public double RiskScore { get; set; }
        public double BullishScore { get; set; }
        public double BearishScore { get; set; }
        public int TimeHorizonDays { get; set; }

        [MaxLength(ModelNameMaxLength)]
        public string ModelName { get; set; } = string.Empty;

        [MaxLength(ModelNameMaxLength)]
        public string ModelVersion { get; set; } = string.Empty;

        // True while the model behind this prediction is still a mock placeholder.
        public bool IsMock { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public List<PredictionReason> Reasons { get; set; } = new List<PredictionReason>();
    }
}
