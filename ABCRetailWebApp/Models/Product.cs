using Azure.Data.Tables;
using Azure;

namespace ABCRetailWebApp.Models
{
    public class Product : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string ImageUrl { get; set; } // URL to the image in Blob Storage
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
