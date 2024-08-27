using ABCRetailWebApp.Models;
using System.Threading.Tasks;

namespace ABCRetailWebApp.Services
{
    public interface ITableService
    {
        // Order methods
        Task<Order> GetOrderAsync(string partitionKey, string rowKey);
        Task AddOrderAsync(Order order);
        Task UpdateOrderAsync(Order order);
        Task DeleteOrderAsync(string partitionKey, string rowKey);
        Task<IEnumerable<Order>> GetAllOrdersAsync(); // Add this method

        // Product methods
        Task<Product> GetProductAsync(string partitionKey, string rowKey);
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(string partitionKey, string rowKey);
        Task<IEnumerable<Product>> GetAllProductsAsync(); // Add this method

        // Customer methods
        Task<Customer> GetCustomerAsync(string partitionKey, string rowKey);
        Task AddCustomerAsync(Customer customer);
        Task UpdateCustomerAsync(Customer customer);
        Task DeleteCustomerAsync(string partitionKey, string rowKey);
        Task<IEnumerable<Customer>> GetAllCustomersAsync(); // Add this method
    }
}
