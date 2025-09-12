using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Exchange_API.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public int? StockId { get; set; }
        public Stock? Stock { get; set; }
    }
}
