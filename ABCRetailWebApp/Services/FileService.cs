using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using System.IO;
using System.Threading.Tasks;

namespace ABCRetailWebApp.Services
{

    public class FileService : IFileService
    {
        private readonly ShareServiceClient _shareServiceClient;

        public FileService(ShareServiceClient shareServiceClient)
        {
            _shareServiceClient = shareServiceClient;
        }

        private ShareDirectoryClient GetDirectoryClient(string shareName, string directoryName)
        {
            var shareClient = _shareServiceClient.GetShareClient(shareName);
            var directoryClient = shareClient.GetDirectoryClient(directoryName);
            return directoryClient;
        }



        public async Task UploadFileAsync(IFormFile file, string directoryName, string fileName)
        {
            try
            {
                var directoryClient = GetDirectoryClient("files", directoryName);
                // Ensure the directory exists
                if (!await directoryClient.ExistsAsync())
                {
                    await directoryClient.CreateAsync(); // Create the directory if it does not exist
                }

                var fileClient = directoryClient.GetFileClient(fileName);
                using (var stream = file.OpenReadStream())
                {
                    await fileClient.CreateAsync(stream.Length); // Create the file with specified length
                    await fileClient.UploadAsync(stream); // Upload the file content
                }
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"Error uploading file: {ex.Message}"); // Log the exception
                throw;
            }
        }



        public async Task<Stream> DownloadFileAsync(string directoryName, string fileName)
        {
            var directoryClient = GetDirectoryClient("files", directoryName);

            // Ensure the directory exists
            if (!await directoryClient.ExistsAsync())
            {
                throw new FileNotFoundException($"Directory '{directoryName}' does not exist.");
            }

            var fileClient = directoryClient.GetFileClient(fileName);

            // Ensure the file exists
            if (!await fileClient.ExistsAsync())
            {
                throw new FileNotFoundException($"File '{fileName}' does not exist in directory '{directoryName}'.");
            }

            ShareFileDownloadInfo download = await fileClient.DownloadAsync();
            var memoryStream = new MemoryStream();
            await download.Content.CopyToAsync(memoryStream);
            memoryStream.Position = 0; // Reset the stream position for reading

            return memoryStream;
        }


        public async Task DeleteFileAsync(string directoryName, string fileName)
        {
            var fileClient = GetDirectoryClient("files", directoryName).GetFileClient(fileName);
            await fileClient.DeleteIfExistsAsync();
        }
        public async Task<IEnumerable<string>> ListFilesAsync(string directoryName)
        {
            var directoryClient = GetDirectoryClient("files", directoryName);

            // Check if the directory exists before listing files
            var directoryExists = await directoryClient.ExistsAsync();
            if (!directoryExists)
            {
                // Log the issue and throw a FileNotFoundException
                Console.WriteLine($"Directory not found: {directoryName}");
                throw new FileNotFoundException("The specified directory does not exist.", directoryName);
            }

            var fileList = new List<string>();
            await foreach (var fileItem in directoryClient.GetFilesAndDirectoriesAsync())
            {
                if (!fileItem.IsDirectory)
                {
                    fileList.Add(fileItem.Name);
                }
            }

            return fileList.AsEnumerable();
        }


    }

}
