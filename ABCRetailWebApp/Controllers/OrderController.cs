using ABCRetailWebApp.Models;
using ABCRetailWebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailWebApp.Controllers
{
    public class OrderController : Controller
    {
        private readonly ITableService _tableService;
        private readonly IQueueService _queueService;

        public OrderController(ITableService tableService, IQueueService queueService)
        {
            _tableService = tableService;
            _queueService = queueService;
        }

        // GET: Order/Index
        public async Task<IActionResult> Index()
        {
            var orders = await _tableService.GetAllOrdersAsync();
            return View(orders);
        }

        // GET: Order/Details/5
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            var order = await _tableService.GetOrderAsync(partitionKey, rowKey);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // GET: Order/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order)
        {
            if (ModelState.IsValid)
            {
                await _tableService.AddOrderAsync(order);

                // Add message to queue for processing
                var message = $"Processing Order: {order.OrderId}";
              //  await _queueService.AddMessageAsync("order-queue", message);

                return RedirectToAction(nameof(Details), new { partitionKey = order.PartitionKey, rowKey = order.RowKey });
            }
            return View(order);
        }

        // GET: Order/Edit/5
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var order = await _tableService.GetOrderAsync(partitionKey, rowKey);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: Order/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey, Order order)
        {
            if (partitionKey != order.PartitionKey || rowKey != order.RowKey)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                await _tableService.UpdateOrderAsync(order);
                return RedirectToAction(nameof(Details), new { partitionKey = order.PartitionKey, rowKey = order.RowKey });
            }
            return View(order);
        }

        // GET: Order/Delete/5
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            var order = await _tableService.GetOrderAsync(partitionKey, rowKey);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: Order/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            var order = await _tableService.GetOrderAsync(partitionKey, rowKey);
            if (order == null)
            {
                return NotFound();
            }

            await _tableService.DeleteOrderAsync(partitionKey, rowKey);

            // Add message to queue for deletion
            var message = $"Deleting Order: {order.OrderId}";
           // await _queueService.AddMessageAsync("order-queue", message);

            return RedirectToAction(nameof(Index));
        }
    }
}
