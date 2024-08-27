namespace ABCRetailWebApp.Models
{
    public class BlobUploadViewModel
    {
        public string ContainerName { get; set; }
        public IFormFile File { get; set; }
    }
}
