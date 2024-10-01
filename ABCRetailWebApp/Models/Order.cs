using Azure.Data.Tables;
using Azure;

namespace ABCRetailWebApp.Models
{
    public class Order : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string OrderId { get; set; }
        public string ProductId { get; set; }
        public string CustomerName { get; set; }  
        public string ProductName { get; set; }   
        public int Quantity { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }

}
