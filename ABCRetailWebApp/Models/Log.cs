namespace ABCRetailWebApp.Models
{
    public class Log
    {
        public string Id { get; set; }       // Unique identifier for the log entry
        public string Message { get; set; }  // Log message content
        public DateTime Timestamp { get; set; } // When the log entry was created
    }

}
