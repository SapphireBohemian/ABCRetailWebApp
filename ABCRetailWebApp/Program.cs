using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Files.Shares;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ABCRetailWebApp.Services; 

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
builder.Services.AddScoped<ITableService, TableService>(); 
builder.Services.AddScoped<IBlobService, BlobService>(); 
builder.Services.AddScoped<IQueueService, QueueService>(); 
builder.Services.AddScoped<IFileService, FileService>(); 
builder.Services.AddScoped<IEmailService, EmailService>();

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
