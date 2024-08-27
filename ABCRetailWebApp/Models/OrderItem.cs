namespace ABCRetailWebApp.Models
{
    public class OrderItem
    {
        // Identifier for the product
        public string ProductID { get; set; }

        // Quantity of the product ordered
        public int Quantity { get; set; }

        // Price of the product
        public decimal Price { get; set; }
    }
}
