using Azure.Data.Tables;
using Azure;

namespace ABCRetailWebApp.Models
{
    public class Inventory
    {
        // Identifier for the product
        public string ProductID { get; set; }

        // Quantity of the product in stock
        public int Quantity { get; set; }
    }

}
