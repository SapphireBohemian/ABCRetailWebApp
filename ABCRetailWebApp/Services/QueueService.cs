using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json;
using QueueMessageModel = ABCRetailWebApp.Models.QueueMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABCRetailWebApp.Services
{
    public class QueueService : IQueueService
    {
        private readonly QueueServiceClient _queueServiceClient;
        private readonly TimeZoneInfo _localTimeZone;

        public QueueService(QueueServiceClient queueServiceClient)
        {
            _queueServiceClient = queueServiceClient;
            _localTimeZone = TimeZoneInfo.Local; // Get the local timezone of the server
        }

        private QueueClient GetQueueClient(string queueName) =>
            _queueServiceClient.GetQueueClient(queueName);

        private DateTimeOffset ConvertToLocalTime(DateTimeOffset utcTime)
        {
            return TimeZoneInfo.ConvertTime(utcTime, _localTimeZone);
        }

        public async Task AddMessageAsync(string queueName, QueueMessageModel message)
        {
            var queueClient = GetQueueClient(queueName);
            await queueClient.CreateIfNotExistsAsync();
            string messageText = JsonConvert.SerializeObject(new { message.MessageType, message.Content });
            await queueClient.SendMessageAsync(messageText);
        }

        public async Task<QueueMessageModel> GetMessageByIdAsync(string queueName, string messageId)
        {
            var queueClient = GetQueueClient(queueName);
            var messages = await queueClient.ReceiveMessagesAsync(maxMessages: 32); 

            var message = messages.Value.FirstOrDefault(m => m.MessageId == messageId);
            if (message != null)
            {
                var messageData = JsonConvert.DeserializeObject<QueueMessageModel>(message.MessageText);
                return new QueueMessageModel
                {
                    MessageId = message.MessageId,
                    PopReceipt = message.PopReceipt,
                    MessageType = messageData.MessageType,
                    Content = messageData.Content,
                    Timestamp = ConvertToLocalTime(message.InsertedOn.Value) // Convert to local time
                };
            }
            return null;
        }

        public async Task<QueueMessageModel> PeekMessageAsync(string queueName)
        {
            var queueClient = GetQueueClient(queueName);
            var response = await queueClient.PeekMessageAsync();
            if (response.Value != null)
            {
                var messageData = JsonConvert.DeserializeObject<QueueMessageModel>(response.Value.MessageText);
                return new QueueMessageModel
                {
                    MessageId = response.Value.MessageId,
                    MessageType = messageData.MessageType,
                    Content = messageData.Content,
                    Timestamp = ConvertToLocalTime(response.Value.InsertedOn.Value) // Convert to local time
                };
            }
            return null;
        }

        public async Task<IEnumerable<QueueMessageModel>> PeekMessagesAsync(string queueName, int maxMessages)
        {
            var queueClient = GetQueueClient(queueName);
            var response = await queueClient.PeekMessagesAsync(maxMessages);

            return response.Value.Select(msg =>
            {
                var messageData = JsonConvert.DeserializeObject<QueueMessageModel>(msg.MessageText);
                return new QueueMessageModel
                {
                    MessageId = msg.MessageId,
                    MessageType = messageData.MessageType,
                    Content = messageData.Content,
                    Timestamp = ConvertToLocalTime(msg.InsertedOn.Value) // Convert to local time
                };
            });
        }

        public async Task<QueueMessageModel> DequeueMessageAsync(string queueName)
        {
            var queueClient = GetQueueClient(queueName);
            var response = await queueClient.ReceiveMessageAsync();

            if (response.Value != null)
            {
                var messageData = JsonConvert.DeserializeObject<QueueMessageModel>(response.Value.MessageText);
                return new QueueMessageModel
                {
                    MessageId = response.Value.MessageId,
                    PopReceipt = response.Value.PopReceipt,
                    MessageType = messageData.MessageType,
                    Content = messageData.Content,
                    Timestamp = ConvertToLocalTime(response.Value.InsertedOn.Value) // Convert to local time
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
