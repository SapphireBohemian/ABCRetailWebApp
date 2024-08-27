namespace ABCRetailWebApp.Models
{
    public class FileModel
    {
        // Unique identifier for the file
        public string FileID { get; set; }

        // Name of the file
        public string FileName { get; set; }

        // URL or path where the file is stored
        public string FileUrl { get; set; }

        // Date when the file was uploaded
        public DateTime UploadDate { get; set; }
    }

}
