using ABCRetailWebApp.Models;
using ABCRetailWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ABCRetailWebApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly ITableService _tableService;
        private readonly IBlobService _blobService;
        private readonly IQueueService _queueService;

        public ProductController(ITableService tableService, IBlobService blobService, IQueueService queueService)
        {
            _tableService = tableService;
            _blobService = blobService;
            _queueService = queueService;
        }

        // GET: Product/Index
        public async Task<IActionResult> Index()
        {
            var products = await _tableService.GetAllProductsAsync();
            return View(products);
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            var product = await _tableService.GetProductAsync(partitionKey, rowKey);
            if (product == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                ViewBag.ImageUrl = await _blobService.DownloadProductImageAsync(product.ImageUrl);
            }
            else
            {
                ViewBag.ImageUrl = null;
            }

            return View(product);
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
            {
                product.PartitionKey = "Products";
                product.RowKey = Guid.NewGuid().ToString();

                if (imageFile != null && imageFile.Length > 0)
                {
                    var imageUrl = await _blobService.UploadProductImageAsync(imageFile, product.RowKey);
                    product.ImageUrl = imageUrl;

                    // Queue message for image upload
                    var imageQueueMessage = new QueueMessage
                    {
                        Content = $"Image Uploaded: {imageFile.FileName}",
                        MessageType = "ImageUpload",
                        Timestamp = DateTimeOffset.Now
                    };
                    await _queueService.AddMessageAsync("your-queue-name", imageQueueMessage);
                }

                await _tableService.AddProductAsync(product);

                // Queue message for product creation
                var productQueueMessage = new QueueMessage
                {
                    Content = $"Product Added: {product.Name}",
                    MessageType = "ProductCreation",
                    Timestamp = DateTimeOffset.Now
                };
                await _queueService.AddMessageAsync("your-queue-name", productQueueMessage);

                return RedirectToAction(nameof(Details), new { partitionKey = product.PartitionKey, rowKey = product.RowKey });
            }
            return View(product);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var product = await _tableService.GetProductAsync(partitionKey, rowKey);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey, Product product, IFormFile newImageFile)
        {
            if (partitionKey != product.PartitionKey || rowKey != product.RowKey)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                // Only delete the old image if a new image is uploaded and the old ImageUrl is not null or empty
                if (newImageFile != null && newImageFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        await _blobService.DeleteProductImageAsync(product.ImageUrl);
                    }

                    var newImageUrl = await _blobService.UploadProductImageAsync(newImageFile, product.RowKey);
                    product.ImageUrl = newImageUrl;

                    // Queue message for image update
                    var imageQueueMessage = new QueueMessage
                    {
                        Content = $"Image Updated: {newImageFile.FileName}",
                        MessageType = "ImageUpdate",
                        Timestamp = DateTimeOffset.Now
                    };
                    await _queueService.AddMessageAsync("your-queue-name", imageQueueMessage);
                }

                await _tableService.UpdateProductAsync(product);

                // Queue message for product update
                var productQueueMessage = new QueueMessage
                {
                    Content = $"Product Updated: {product.Name}",
                    MessageType = "ProductUpdate",
                    Timestamp = DateTimeOffset.Now
                };
                await _queueService.AddMessageAsync("your-queue-name", productQueueMessage);

                return RedirectToAction(nameof(Details), new { partitionKey = product.PartitionKey, rowKey = product.RowKey });
            }

            return View(product);
        }


        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            var product = await _tableService.GetProductAsync(partitionKey, rowKey);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            var product = await _tableService.GetProductAsync(partitionKey, rowKey);
            if (product != null)
            {
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    await _blobService.DeleteProductImageAsync(product.ImageUrl);
                }
                await _tableService.DeleteProductAsync(partitionKey, rowKey);

                // Queue message for product deletion
                var productQueueMessage = new QueueMessage
                {
                    Content = $"Product Deleted: {product.Name}",
                    MessageType = "ProductDeletion",
                    Timestamp = DateTimeOffset.Now
                };
                await _queueService.AddMessageAsync("your-queue-name", productQueueMessage);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
