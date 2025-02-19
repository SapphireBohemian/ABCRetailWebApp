﻿using ABCRetailWebApp.Models;
using ABCRetailWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ABCRetailWebApp.Controllers
{
    public class FileController : Controller
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        // GET: File/Upload
        public IActionResult Upload()
        {
            return View(new FileUploadViewModel());
        }

        // POST: File/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(FileUploadViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.File != null && model.File.Length > 0)
                {
                    Console.WriteLine("File uploaded: " + model.File.FileName); 
                    await _fileService.UploadFileAsync(model.File, model.DirectoryName, model.File.FileName);
                    return RedirectToAction("List", new { directoryName = model.DirectoryName });
                }
                ModelState.AddModelError("", "File is required.");
            }
            return View(model);
        }


        // GET: File/Index
        public IActionResult Index()
        {
            return View();
        }

        // FileController.cs
        public async Task<IActionResult> Download(string directoryName, string fileName)
        {
            if (string.IsNullOrWhiteSpace(directoryName) || string.IsNullOrWhiteSpace(fileName))
            {
                return NotFound("Invalid directory or file name.");
            }

            var fileStream = await _fileService.DownloadFileAsync(directoryName, fileName);
            if (fileStream == null)
            {
                return NotFound($"File '{fileName}' does not exist in directory '{directoryName}'.");
            }

            return File(fileStream, "application/octet-stream", fileName);
        }





        // GET: File/List
        public async Task<IActionResult> List(string directoryName)
        {
            Console.WriteLine($"Requested directory: {directoryName}");

            try
            {
                var fileList = await _fileService.ListFilesAsync(directoryName);
                var fileModels = fileList.Select(fileName => new FileModel { FileName = fileName }).ToList();
                return View(fileModels);
            }
            catch (FileNotFoundException ex)
            {
                // Log the exception and handle the error gracefully
                Console.WriteLine($"Error: {ex.Message}");
                return NotFound(ex.Message);
            }
        }

        // FileController.cs
        public async Task<IActionResult> Delete(string directoryName, string fileName)
        {
            if (string.IsNullOrWhiteSpace(directoryName) || string.IsNullOrWhiteSpace(fileName))
            {
                return NotFound("Invalid directory or file name.");
            }

            try
            {
                await _fileService.DeleteFileAsync(directoryName, fileName);
                return RedirectToAction("List", new { directoryName = directoryName });
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
