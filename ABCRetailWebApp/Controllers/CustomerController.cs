using ABCRetailWebApp.Models;
using ABCRetailWebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailWebApp.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ITableService _tableService;
        private readonly IQueueService _queueService;
        private readonly IEmailService _emailService;

        public CustomerController(ITableService tableService, IQueueService queueService, IEmailService emailService)
        {
            _tableService = tableService;
            _queueService = queueService;
            _emailService = emailService;
        }

        // GET: Customer/Index
        public async Task<IActionResult> Index()
        {
            var customers = await _tableService.GetAllCustomersAsync();
            return View(customers);
        }

        // GET: Customer/Details/5
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            var customer = await _tableService.GetCustomerAsync(partitionKey, rowKey);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // GET: Customer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                await _tableService.AddCustomerAsync(customer);

                //Queue message for customer creationn
                var customerQueueMessage = new QueueMessage
                {
                    Content = $"Customer Created: {customer.Name}",
                    MessageType = "CustomerCreation",
                    Timestamp = DateTimeOffset.Now
                };
                await _queueService.AddMessageAsync("your-queue-name", customerQueueMessage);

                // Send email notification if email is provided
                if (!string.IsNullOrEmpty(customer.Email))
                {
                    var subject = "Customer Profile Creation";
                    var body = $@"
                        <h3>Thank you for creating a profile with us:</h3>
                        <h4>Here are your profile details, and update if needed.<h4>
                        <p><strong>Customer Name:</strong> {customer.Name}</p>
                        <p><strong>Customer Email:</strong> {customer.Email}</p>
                        <p><strong>Customer Address:</strong> {customer.Address:C}</p>";

                    await _emailService.SendEmailAsync(customer.Email, subject, body);

                    // Set TempData message
                    TempData["EmailSent"] = "You have been sent an email please check to verify details.";
                }

                return RedirectToAction(nameof(Details), new { partitionKey = customer.PartitionKey, rowKey = customer.RowKey });
            }
            return View(customer);
        }

        // GET: Customer/Edit/5
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var customer = await _tableService.GetCustomerAsync(partitionKey, rowKey);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey, Customer customer)
        {
            if (partitionKey != customer.PartitionKey || rowKey != customer.RowKey)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                await _tableService.UpdateCustomerAsync(customer);

                //Queue message for customer update
                var customerQueueMessage = new QueueMessage
                {
                    Content = $"Customer Updated: {customer.Name}",
                    MessageType = "CustomerUpdate",
                    Timestamp = DateTimeOffset.Now
                };
                await _queueService.AddMessageAsync("your-queue-name", customerQueueMessage);

                // Send email notification if email is provided
                if (!string.IsNullOrEmpty(customer.Email))
                {
                    var subject = "Update Profile Details";
                    var body = $@"
                        <h3>Update Details:</h3>
                        <p><strong>Customer Name:</strong> {customer.Name}</p>
                        <p><strong>Customer Email:</strong> {customer.Email}</p>
                        <p><strong>Customer Address:</strong> {customer.Address:C}</p>";

                    await _emailService.SendEmailAsync(customer.Email, subject, body);

                    // Set TempData message
                    TempData["EmailSent"] = "An email has been sent.";
                }

                return RedirectToAction(nameof(Details), new { partitionKey = customer.PartitionKey, rowKey = customer.RowKey });
            }
            return View(customer);
        }

        // GET: Customer/Delete/5
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            var customer = await _tableService.GetCustomerAsync(partitionKey, rowKey);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            var customer = await _tableService.GetCustomerAsync(partitionKey, rowKey);
            await _tableService.DeleteCustomerAsync(partitionKey, rowKey);

            //Queue message for customer deletion
            var customerQueueMessage = new QueueMessage
            {
                Content = $"Customer Deleted: {customer.Name}",
                MessageType = "CustomerDeletion",
                Timestamp = DateTimeOffset.Now
            };
            await _queueService.AddMessageAsync("your-queue-name", customerQueueMessage);

            // Send email notification
            if (!string.IsNullOrEmpty(customer.Email))
            {
                var subject = "Profile Deleted";
                var body = $@" 
                    <h3>Your Profile has been deleted:</h3>
                    <p><strong>Name:</strong> {customer.Name}</p>";

                await _emailService.SendEmailAsync(customer.Email, subject, body);

                // Set TempData message
                TempData["EmailSent"] = "A notification email has been sent, check email.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
