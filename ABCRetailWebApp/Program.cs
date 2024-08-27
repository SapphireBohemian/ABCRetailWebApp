using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Files.Shares;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ABCRetailWebApp.Services; // Ensure this namespace is correct for your services

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Retrieve connection strings from configuration
var tableConnectionString = builder.Configuration["AzureStorage:TableConnectionString"];
var blobConnectionString = builder.Configuration["AzureStorage:BlobConnectionString"];
var queueConnectionString = builder.Configuration["AzureStorage:QueueConnectionString"];
var fileConnectionString = builder.Configuration["AzureStorage:FileConnectionString"];

// Register service clients
builder.Services.AddSingleton(new TableServiceClient(tableConnectionString));
builder.Services.AddSingleton(new BlobServiceClient(blobConnectionString));
builder.Services.AddSingleton(new QueueServiceClient(queueConnectionString));
builder.Services.AddSingleton(new ShareServiceClient(fileConnectionString));

// Register application services
builder.Services.AddScoped<ITableService, TableService>(); // Assuming you have TableService class implementing ITableService
builder.Services.AddScoped<IBlobService, BlobService>(); // Assuming you have BlobService class implementing IBlobService
builder.Services.AddScoped<IQueueService, QueueService>(); // Assuming you have QueueService class implementing IQueueService
builder.Services.AddScoped<IFileService, FileService>(); // Assuming you have FileService class implementing IFileService

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
