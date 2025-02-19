﻿using Azure;
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

        public async Task<Product> GetProductByRowKeyAsync(string rowKey)
        {
            var tableClient = _tableServiceClient.GetTableClient("Products"); 
            var response = await tableClient.GetEntityAsync<Product>("PartitionKey", rowKey); 
            return response.Value;
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

            // Retrieve the existing product to get its ETag
            var existingProduct = await tableClient.GetEntityAsync<Product>(product.PartitionKey, product.RowKey);

            // Ensure the product has a valid ETag before updating
            if (string.IsNullOrEmpty(existingProduct.Value.ETag.ToString()))
            {
                throw new ArgumentException("ETag cannot be null or empty", nameof(product.ETag));
            }

            // Set the ETag of the existing product to the one being updated
            product.ETag = existingProduct.Value.ETag;

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

            // Retrieve the existing customer to get its ETag
            var existingCustomerResponse = await tableClient.GetEntityAsync<Customer>(customer.PartitionKey, customer.RowKey);

            if (existingCustomerResponse.HasValue)
            {
                // Ensure the customer has a valid ETag before updating
                var existingCustomer = existingCustomerResponse.Value;

                // Set the ETag of the existing customer to the one being updated
                customer.ETag = existingCustomer.ETag;

                // Perform the update operation
                await tableClient.UpdateEntityAsync(customer, customer.ETag, TableUpdateMode.Replace);
            }
            else
            {
                throw new ArgumentException("Customer not found for update.");
            }
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
