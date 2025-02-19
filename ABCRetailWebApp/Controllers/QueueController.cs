﻿using Microsoft.AspNetCore.Mvc;
using ABCRetailWebApp.Models;
using ABCRetailWebApp.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; 

namespace ABCRetailWebApp.Controllers
{
    public class QueueController : Controller
    {
        private readonly IQueueService _queueService;
        private readonly ILogger<QueueController> _logger;

        public QueueController(IQueueService queueService, ILogger<QueueController> logger)
        {
            _queueService = queueService;
            _logger = logger;
        }

        // GET: Queue
        public async Task<IActionResult> Index()
        {
            return View();
        }

        // GET: Queue/Peek
        public async Task<IActionResult> Peek()
        {
            var queueName = "your-queue-name"; 
            var messages = await _queueService.PeekMessagesAsync(queueName, 20); // Adjust the number of messages as needed
            return View(messages);
        }

        
        // POST: Queue/Dequeue
        [HttpPost]
        public async Task<IActionResult> Dequeue()
        {
            var queueName = "your-queue-name"; 
            var dequeuedMessage = await _queueService.DequeueMessageAsync(queueName);

            if (dequeuedMessage == null)
            {
                return NotFound("No messages to dequeue.");
            }

            // Render the dequeued message with the popReceipt and messageId
            return View("Dequeue", dequeuedMessage);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        // POST: Queue/Add
        [HttpPost]
        public async Task<IActionResult> Add(QueueMessage message)
        {
            if (!ModelState.IsValid)
            {
                var queueName = "your-queue-name"; 
                await _queueService.AddMessageAsync(queueName, message);
                return RedirectToAction("Peek");
            }
            return View(message);
        }

        // POST: Queue/Delete
        [HttpPost]
        public async Task<IActionResult> Delete(string messageId, string popReceipt)
        {
            if (string.IsNullOrEmpty(popReceipt) || string.IsNullOrEmpty(messageId))
            {
                return BadRequest("Invalid messageId or popReceipt.");
            }

            var queueName = "your-queue-name"; 
            await _queueService.DeleteMessageAsync(queueName, messageId, popReceipt);
            return RedirectToAction("Peek");
        }
    }
}
