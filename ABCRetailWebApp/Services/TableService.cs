using Azure;
using Azure.Data.Tables;
using System;
using System.Threading.Tasks;
using ABCRetailWebApp.Models;

namespace ABCRetailWebApp.Services
{
    public class TableService : ITableService
    {
        private readonly TableServiceClient _tableServiceClient;

        public TableService(TableServiceClient tableServiceClient)
        {
            _tableServiceClient = tableServiceClient;
        }

        // Helper method to get table client
        private TableClient GetTableClient(string tableName)
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);

            // Ensure the table exists
            tableClient.CreateIfNotExists();

            return tableClient;
        }

        // Order methods
        public async Task<Order> GetOrderAsync(string partitionKey, string rowKey)
        {
            var tableClient = GetTableClient("orders");
            try
            {
                var response = await tableClient.GetEntityAsync<Order>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // Handle not found
                return null;
            }
        }

        public async Task AddOrderAsync(Order order)
        {
            var tableClient = GetTableClient("orders");
            await tableClient.AddEntityAsync(order);
        }

        public async Task UpdateOrderAsync(Order order)
        {
            var tableClient = GetTableClient("orders");
            await tableClient.UpdateEntityAsync(order, order.ETag, TableUpdateMode.Replace);
        }

        public async Task DeleteOrderAsync(string partitionKey, string rowKey)
        {
            var tableClient = GetTableClient("orders");
            await tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            var tableClient = GetTableClient("orders");
            var entities = new List<Order>();
            await foreach (var entity in tableClient.QueryAsync<Order>())
            {
                entities.Add(entity);
            }
            return entities;
        }

        // Product methods
        public async Task<Product> GetProductAsync(string partitionKey, string rowKey)
        {
            var tableClient = GetTableClient("products");
            try
            {
                var response = await tableClient.GetEntityAsync<Product>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // Handle not found
                return null;
            }
        }

        public async Task AddProductAsync(Product product)
        {
            var tableClient = GetTableClient("products");
            await tableClient.AddEntityAsync(product);
        }

        public async Task UpdateProductAsync(Product product)
        {
            var tableClient = GetTableClient("products");
            await tableClient.UpdateEntityAsync(product, product.ETag, TableUpdateMode.Replace);
        }

        public async Task DeleteProductAsync(string partitionKey, string rowKey)
        {
            var tableClient = GetTableClient("products");
            await tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            var tableClient = GetTableClient("products");
            var entities = new List<Product>();
            await foreach (var entity in tableClient.QueryAsync<Product>())
            {
                entities.Add(entity);
            }
            return entities;
        }

        // Customer methods
        public async Task<Customer> GetCustomerAsync(string partitionKey, string rowKey)
        {
            var tableClient = GetTableClient("customers");
            try
            {
                var response = await tableClient.GetEntityAsync<Customer>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // Handle not found
                return null;
            }
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            var tableClient = GetTableClient("customers");
            await tableClient.AddEntityAsync(customer);
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            var tableClient = GetTableClient("customers");
            await tableClient.UpdateEntityAsync(customer, customer.ETag, TableUpdateMode.Replace);
        }

        public async Task DeleteCustomerAsync(string partitionKey, string rowKey)
        {
            var tableClient = GetTableClient("customers");
            await tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            var tableClient = GetTableClient("customers");
            var entities = new List<Customer>();
            await foreach (var entity in tableClient.QueryAsync<Customer>())
            {
                entities.Add(entity);
            }
            return entities;
        }
    }
}
