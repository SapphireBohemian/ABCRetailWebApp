using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json;
//using System.Text.Json;
using QueueMessageModel = ABCRetailWebApp.Models.QueueMessage;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABCRetailWebApp.Services
{
    public class QueueService : IQueueService
    {
        private readonly QueueServiceClient _queueServiceClient;

        public QueueService(QueueServiceClient queueServiceClient)
        {
            _queueServiceClient = queueServiceClient;
        }

        private QueueClient GetQueueClient(string queueName) =>
            _queueServiceClient.GetQueueClient(queueName);

        public async Task AddMessageAsync(string queueName, QueueMessageModel message)
        {
            var queueClient = GetQueueClient(queueName);
            await queueClient.CreateIfNotExistsAsync();
            string messageText = JsonConvert.SerializeObject(message);
            await queueClient.SendMessageAsync(messageText);
        }



        public async Task<QueueMessageModel> PeekMessageAsync(string queueName)
        {
            var queueClient = GetQueueClient(queueName);
            var response = await queueClient.PeekMessageAsync();
            if (response.Value != null)
            {
                return JsonConvert.DeserializeObject<QueueMessageModel>(response.Value.MessageText); // Deserialize JSON to QueueMessageModel
            }
            return null;
        }

        public async Task<IEnumerable<QueueMessageModel>> PeekMessagesAsync(string queueName, int maxMessages)
        {
            var queueClient = GetQueueClient(queueName);
            var response = await queueClient.PeekMessagesAsync(maxMessages);

            return response.Value.Select(msg => new QueueMessageModel
            {
                MessageId = msg.MessageId,
                Content = msg.MessageText,
                Timestamp = DateTimeOffset.UtcNow // Set to current time if not available
            });
        }

        public async Task<QueueMessageModel> DequeueMessageAsync(string queueName)
        {
            var queueClient = GetQueueClient(queueName);
            var response = await queueClient.ReceiveMessageAsync();

            if (response.Value != null)
            {
                return new QueueMessageModel
                {
                    MessageId = response.Value.MessageId,
                    PopReceipt = response.Value.PopReceipt,
                    Content = response.Value.MessageText,
                    Timestamp = response.Value.InsertedOn
                };
            }

            return null;
        }


        public async Task DeleteMessageAsync(string queueName, string messageId, string popReceipt)
        {
            var queueClient = GetQueueClient(queueName);
            await queueClient.DeleteMessageAsync(messageId, popReceipt);
        }
    }
}
