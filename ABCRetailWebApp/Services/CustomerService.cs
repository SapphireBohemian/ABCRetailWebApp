using ABCRetailWebApp.Models;
using Azure.Data.Tables;

namespace ABCRetailWebApp.Services
{
    public class CustomerService
    {
        private readonly TableServiceClient _tableServiceClient;
        private readonly TableClient _tableClient;

        public CustomerService(TableServiceClient tableServiceClient)
        {
            _tableServiceClient = tableServiceClient;
            _tableClient = _tableServiceClient.GetTableClient("Customers");
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            await _tableClient.CreateIfNotExistsAsync();
            await _tableClient.AddEntityAsync(customer);
        }

        public async Task<Customer> GetCustomerAsync(string partitionKey, string rowKey)
        {
            return await _tableClient.GetEntityAsync<Customer>(partitionKey, rowKey);
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            await _tableClient.UpdateEntityAsync(customer, customer.ETag, TableUpdateMode.Replace);
        }

        public async Task DeleteCustomerAsync(string partitionKey, string rowKey)
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }
    }
}
