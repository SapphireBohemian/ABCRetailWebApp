using System.IO;
using System.Threading.Tasks;
using ABCRetailWebApp.Models;
using Microsoft.AspNetCore.Http;

namespace ABCRetailWebApp.Services
{
    public interface IBlobService
    {
        Task<string> UploadBlobAsync(IFormFile file, string containerName);
        Task<Stream> DownloadBlobAsync(string blobName, string containerName);
        Task DeleteBlobAsync(string blobName, string containerName);
        Task<string> UploadProductImageAsync(IFormFile file, string productId);
        Task<Stream> DownloadProductImageAsync(string imageUrl);
        Task DeleteProductImageAsync(string imageUrl);
        //Task<IEnumerable<string>> ListBlobsAsync(string containerName);
        Task<IEnumerable<BlobModel>> ListBlobsAsync(string containerName); // Update this line

    }
}
