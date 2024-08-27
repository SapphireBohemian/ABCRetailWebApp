using ABCRetailWebApp.Models;
using ABCRetailWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ABCRetailWebApp.Controllers
{
    public class BlobController : Controller
    {
        private readonly IBlobService _blobService;
        private readonly IQueueService _queueService;

        public BlobController(IBlobService blobService, IQueueService queueService)
        {
            _blobService = blobService;
            _queueService = queueService;
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
                        await _blobService.UploadBlobAsync(model.File, model.ContainerName);

                        // Queue message for image upload
                        var imageQueueMessage = new QueueMessage
                        {
                            Content = $"Image Uploaded: {model.File.FileName}",
                            MessageType = "ImageUpload",
                            Timestamp = DateTimeOffset.Now
                        };
                        await _queueService.AddMessageAsync("your-queue-name", imageQueueMessage);

                        return RedirectToAction("List", new { containerName = model.ContainerName });
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"An error occurred while uploading the file: {ex.Message}");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "File or container name is missing.");
                }
            }

            return View(model);
        }



        // GET: Blob/Index
        public IActionResult Index(string containerName)
        {
            return RedirectToAction("List", new { containerName = containerName ?? "media" });
        }

        // GET: Blob/List
        public async Task<IActionResult> List(string containerName)
        {
            containerName ??= "media-container";

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

        // GET: Blob/Gallery
        public async Task<IActionResult> Gallery(string containerName)
        {
            containerName ??= "media-container";

            var blobList = await _blobService.ListBlobsAsync(containerName);

            ViewData["ContainerName"] = containerName;
            return View(blobList);
        }
    }
}
