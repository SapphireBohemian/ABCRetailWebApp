namespace ABCRetailWebApp.Services
{
    public interface IFileService
    {
        Task UploadFileAsync(IFormFile file, string directoryName, string fileName);
        Task<Stream> DownloadFileAsync(string directoryName, string fileName);
        Task DeleteFileAsync(string directoryName, string fileName);
        Task<IEnumerable<string>> ListFilesAsync(string directoryName);
    }


}
