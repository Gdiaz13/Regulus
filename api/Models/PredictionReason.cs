using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    // One line of "why" attached to a prediction. Reasons and warnings share this
    // table and are told apart by Kind, so the prediction always carries both the
    // opportunity and the risk side, never one without the other.
    public class PredictionReason
    {
        public int Id { get; set; }

        public int PredictionId { get; set; }
        public Prediction? Prediction { get; set; }

        public PredictionReasonKind Kind { get; set; }

        [MaxLength(512)]
        public string Text { get; set; } = string.Empty;
    }

    public enum PredictionReasonKind
    {
        Reason,
        Warning,
    }
}
