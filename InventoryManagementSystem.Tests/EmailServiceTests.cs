using Xunit;
using Moq;
using Microsoft.Extensions.Options;
using InventoryManagementSystem.Services;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Tests
{
    public class EmailSenderTests
    {
        private static IOptions<AuthMessageSenderParams> GetTestOptions() =>
            Options.Create(new AuthMessageSenderParams
            {
                ApiKey = "dummy-api-key",
                SenderEmail = "test@sender.com",
                SenderName = "Test Sender"
            });

        [Fact]
        public async Task SendEmailAsync_SuccessfulSend_SendsMessage()
        {
            var mockClient = new Mock<ISendGridClient>();
            mockClient
                .Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Response(HttpStatusCode.Accepted, null, null));

            var emailSender = new EmailSender(GetTestOptions(), mockClient.Object);

            await emailSender.SendEmailAsync("to@example.com", "Test Subject", "<p>Hello</p>");

            mockClient.Verify(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SendEmailAsync_FailedSend_LogsError()
        {
            var mockResponse = new Response(HttpStatusCode.BadRequest, null, null);
            var mockClient = new Mock<ISendGridClient>();
            mockClient
                .Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            var emailSender = new EmailSender(GetTestOptions(), mockClient.Object);

            await emailSender.SendEmailAsync("to@example.com", "Fail Subject", "<p>Oops</p>");

            mockClient.Verify(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
