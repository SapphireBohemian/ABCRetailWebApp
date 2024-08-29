using ABCRetailWebApp.Models;
using ABCRetailWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ABCRetailWebApp.Controllers
{
    public class BlobController : Controller
    {
        private readonly IBlobService _blobService;
        private readonly IQueueService _queueService;
        private readonly ITableService _tableService;

        public BlobController(IBlobService blobService, IQueueService queueService, ITableService tableService)
        {
            _blobService = blobService;
            _queueService = queueService;
            _tableService = tableService;
        }

        [HttpGet]
        public async Task<IActionResult> ListBlobs(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                // Handle error or provide a default container name
                return View("Error");
            }

            var blobs = await _blobService.ListBlobsAsync(containerName);
            ViewData["ContainerName"] = containerName;
            return View(blobs);
        }

        public async Task<IActionResult> ProductImages()
        {
            var blobs = await _blobService.ListBlobsAsync("product-images");
            return View("ProductImages", blobs);
        }

        public async Task<IActionResult> MediaContent()
        {
            var blobs = await _blobService.ListBlobsAsync("media-content");
            return View("MediaContent", blobs);
        }

        public async Task<IActionResult> DeleteBlob(string containerName, string blobName)
        {
            // Ensure containerName and blobName are valid
            if (string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(blobName))
            {
                return BadRequest("Container name or blob name cannot be null or empty.");
            }

            // Call DeleteBlobAsync with the correct parameters
            await _blobService.DeleteBlobAsync(containerName, blobName);

            // Redirect or return appropriate response
            if (containerName.Equals("media-content", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("MediaContent");
            }
            return RedirectToAction("Index"); // or another appropriate action
        }




        // Action to view blobs in a specified container
        public async Task<IActionResult> Gallery(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                return BadRequest("Container name cannot be null or empty.");
            }

            var blobs = await _blobService.ListBlobsAsync(containerName);
            ViewBag.ContainerName = containerName;
            return View(blobs);
        }

        public async Task<IActionResult> GetImage(string blobName, string containerName)
        {
            if (string.IsNullOrEmpty(blobName) || string.IsNullOrEmpty(containerName))
            {
                return NotFound();
            }

            try
            {
                var stream = await _blobService.DownloadBlobAsync(blobName, containerName);

                var fileExtension = Path.GetExtension(blobName).ToLowerInvariant();
                var provider = new FileExtensionContentTypeProvider();
                provider.TryGetContentType(fileExtension, out var contentType);

                return File(stream, contentType ?? "application/octet-stream");
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> GetProductImage(string rowKey)
        {
            if (string.IsNullOrEmpty(rowKey))
            {
                return NotFound();
            }

            try
            {
                // Retrieve product details using RowKey
                var product = await _tableService.GetProductByRowKeyAsync(rowKey);

                if (product != null && !string.IsNullOrEmpty(product.ImageUrl))
                {
                    var imageStream = await _blobService.DownloadBlobAsync("product-images", product.ImageUrl);
                    var contentType = "image/jpeg"; // You might want to determine this dynamically

                    return File(imageStream, contentType);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and log errors
                return NotFound();
            }
        }



        public async Task<IActionResult> DownloadBlob(string containerName, string blobName)
        {
            try
            {
                // Sanitize the blob name
                blobName = Uri.EscapeDataString(blobName);

                var blobStream = await _blobService.DownloadBlobAsync(blobName, containerName);
                return File(blobStream, "application/octet-stream", blobName);
            }
            catch (InvalidOperationException ex)
            {
                // Handle specific exceptions, such as invalid blob or container names
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // GET: Blob/Download/{blobName}
        public async Task<IActionResult> Download(string blobName, string containerName)
        {
            var blobStream = await _blobService.DownloadBlobAsync(blobName, containerName);
            if (blobStream == null)
            {
                return NotFound();
            }

            return File(blobStream, "application/octet-stream", blobName);
        }

        // GET: Blob/Upload
        public IActionResult Upload(string containerName)
        {
            var model = new BlobUploadViewModel
            {
                ContainerName = containerName
            };
            return View(model);
        }

        // POST: Blob/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(BlobUploadViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.File != null && model.File.Length > 0 && !string.IsNullOrEmpty(model.ContainerName))
                {
                    try
                    {
                        // Log container name and file details
                        Console.WriteLine($"Uploading file: {model.File.FileName} to container: {model.ContainerName}");

                        await _blobService.UploadBlobAsync(model.File, model.ContainerName);

                        // Queue message for image upload
                        var imageQueueMessage = new QueueMessage
                        {
                            Content = $"Image Uploaded: {model.File.FileName}",
                            MessageType = "ImageUpload",
                            Timestamp = DateTimeOffset.Now
                        };
                        await _queueService.AddMessageAsync("your-queue-name", imageQueueMessage);

                        ViewBag.Message = "Upload successful!";
                        return RedirectToAction("List", new { containerName = model.ContainerName });
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"An error occurred while uploading the file: {ex.Message}");
                        Console.WriteLine($"Error during upload: {ex.Message}");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "File or container name is missing.");
                    Console.WriteLine("File or container name is missing.");
                }
            }

            return View(model);
        }



        // GET: Blob/Index
        public IActionResult Index(string containerName)
        {
            return RedirectToAction("List", new { containerName = containerName ?? "media-content" });
        }

        // GET: Blob/List
        public async Task<IActionResult> List(string containerName)
        {
            containerName ??= "media-content";

            if (string.IsNullOrEmpty(containerName))
            {
                return BadRequest("Container name cannot be null or empty.");
            }

            var blobList = await _blobService.ListBlobsAsync(containerName);

            if (!blobList.Any())
            {
                ModelState.AddModelError("", $"No blobs found in container: {containerName}");
            }

            ViewData["ContainerName"] = containerName;
            return View(blobList);
        }

    }
}
