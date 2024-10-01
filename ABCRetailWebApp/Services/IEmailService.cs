using ABCRetailWebApp.Models;

namespace ABCRetailWebApp.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string messageBody);
    }
}
