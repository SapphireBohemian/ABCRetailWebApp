using Azure.Data.Tables;
using Azure;

namespace ABCRetailWebApp.Models
{
    public class Customer : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Constructor
        public Customer()
        {
            PartitionKey = "CustomerProfiles"; // Set a default value or logic to determine
            RowKey = Guid.NewGuid().ToString(); // Generate a unique RowKey
        }
    }

}
