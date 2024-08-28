﻿using ABCRetailWebApp.Models;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ABCRetailWebApp.Services
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
        }

        private BlobContainerClient GetContainerClient(string containerName)
        {
            if (string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentException("Container name cannot be null or empty.", nameof(containerName));

            try
            {
                return _blobServiceClient.GetBlobContainerClient(containerName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get container client for container: {containerName}", ex);
            }
        }

        public async Task<string> UploadBlobAsync(IFormFile file, string containerName)
        {
            var containerClient = GetContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            Console.WriteLine($"Uploading to container: {containerName}");

            var blobClient = containerClient.GetBlobClient(file.FileName);
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            Console.WriteLine($"Uploaded blob: {blobClient.Uri}");

            return blobClient.Uri.ToString();
        }


        public async Task<Stream> DownloadBlobAsync(string blobName, string containerName)
        {
            try
            {
                var containerClient = GetContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);
                var response = await blobClient.DownloadAsync();
                return response.Value.Content;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to download blob: {blobName} from container: {containerName}", ex);
            }
        }

        public async Task DeleteBlobAsync(string blobName, string containerName)
        {
            try
            {
                var containerClient = GetContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);
                await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete blob: {blobName} from container: {containerName}", ex);
            }
        }

        public async Task<IEnumerable<BlobModel>> ListBlobsAsync(string containerName)
        {
            var containerClient = GetContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();  // Ensure the container exists

            Console.WriteLine($"Listing blobs in container: {containerName}");

            var blobs = new List<BlobModel>();
            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                Console.WriteLine($"Found blob: {blobItem.Name}");
                blobs.Add(new BlobModel
                {
                    BlobName = blobItem.Name,
                    UploadDate = blobItem.Properties.LastModified?.DateTime ?? DateTime.MinValue,
                    ContainerName = containerName
                });
            }

            Console.WriteLine($"Total blobs found: {blobs.Count}");

            return blobs;
        }






        public async Task<string> UploadProductImageAsync(IFormFile file, string productId)
        {
            var containerClient = GetContainerClient("product-images");
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient($"{productId}/{file.FileName}");
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            return blobClient.Uri.ToString();
        }

        public async Task<Stream> DownloadProductImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                throw new ArgumentNullException(nameof(imageUrl), "The image URL cannot be null or empty.");
            }
            try
            {
                var blobClient = new BlobClient(new Uri(imageUrl), new StorageSharedKeyCredential("azurestorage420", "NGbalqfmF7D5nD5ylimKpoEwwI0I1GPK4DRSs1PHijwMKDd2MR1YWA3Tt+UDHP6blv1U5mlv3Zq9+ASt4+hFPg=="));
                var response = await blobClient.DownloadAsync();
                return response.Value.Content;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to download product image from URL: {imageUrl}", ex);
            }
        }



        public async Task DeleteProductImageAsync(string imageUrl)
        {
            try
            {
                // Extract the container name and blob name from the image URL
                Uri blobUri = new Uri(imageUrl);
                string containerName = blobUri.Segments[1].TrimEnd('/');
                string blobName = string.Join("", blobUri.Segments.Skip(2));

                // Get the container client
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                // Get the blob client
                var blobClient = containerClient.GetBlobClient(blobName);

                // Delete the blob if it exists
                await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete product image from URL: {imageUrl}", ex);
            }
        }

    }
}
