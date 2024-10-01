using SendGrid.Helpers.Mail;
using SendGrid;

namespace ABCRetailWebApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _sendGridApiKey;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _sendGridApiKey = _configuration["SendGrid:ApiKey"];
        }

        public async Task SendEmailAsync(string toEmail, string subject, string messageBody)
        {
            var client = new SendGridClient(_sendGridApiKey);
            var from = new EmailAddress("sapphirebohemian@gmail.com", "ABC Retail");
            var to = new EmailAddress(toEmail);
            var plainTextContent = messageBody;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, plainTextContent);

            var response = await client.SendEmailAsync(msg);

            if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                var responseBody = await response.Body.ReadAsStringAsync();
                throw new Exception($"Failed to send email. Status code: {response.StatusCode}, Response body: {responseBody}");
            }
        }
    }
}
