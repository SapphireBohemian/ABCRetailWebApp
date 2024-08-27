using ABCRetailWebApp.Models;
using ABCRetailWebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailWebApp.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ITableService _tableService;

        public CustomerController(ITableService tableService)
        {
            _tableService = tableService;
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
            await _tableService.DeleteCustomerAsync(partitionKey, rowKey);
            return RedirectToAction(nameof(Index));
        }
    }
}
