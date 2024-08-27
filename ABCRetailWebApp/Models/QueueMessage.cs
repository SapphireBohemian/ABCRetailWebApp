namespace ABCRetailWebApp.Models
{
    public class QueueMessage
    {
        public string MessageId { get; set; }
        public string PopReceipt { get; set; }
        public string MessageType { get; set; }
        public string Content { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
