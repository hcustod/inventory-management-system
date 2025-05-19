using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Services
{
    public class AuthMessageSenderParams
    {
        public string ApiKey { get; set; }
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
    }

    public class EmailSender : IEmailSender
    {
        private readonly AuthMessageSenderParams _options;
        private readonly ISendGridClient _client;

        public EmailSender(IOptions<AuthMessageSenderParams> options, ISendGridClient client)
        {
            _options = options.Value;
            _client = client;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var from = new EmailAddress(_options.SenderEmail, _options.SenderName);
            var to = new EmailAddress(email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlMessage);
            var response = await _client.SendEmailAsync(msg);

            if ((int)response.StatusCode >= 400)
            {
                // Optional logging could go here
                System.Console.WriteLine($"SendGrid Error: {response.StatusCode}");
            }
        }
    }
}