using System.Collections.Generic;
using System.Threading.Tasks;
using QueueMessageModel = ABCRetailWebApp.Models.QueueMessage;
using QueueMessageAzure = Azure.Storage.Queues.Models.QueueMessage;

namespace ABCRetailWebApp.Services
{
    public interface IQueueService
    {
        Task AddMessageAsync(string queueName, QueueMessageModel message);
        Task<QueueMessageModel> PeekMessageAsync(string queueName);
        Task<IEnumerable<QueueMessageModel>> PeekMessagesAsync(string queueName, int maxMessages);
        Task<QueueMessageModel> DequeueMessageAsync(string queueName);
        Task DeleteMessageAsync(string queueName, string messageId, string popReceipt); // Updated method
    }
}
