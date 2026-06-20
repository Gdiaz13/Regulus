namespace api.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public int StockId { get; set; }

        // EF uses StockId to connect each note back to one portfolio stock.
        public Stock Stock { get; set; } = null!;
    }
}
