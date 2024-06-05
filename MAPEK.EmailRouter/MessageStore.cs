using System.Buffers;
using System.Text;
using MimeKit;
using SendGrid;
using SendGrid.Helpers.Mail;
using SmtpServer;
using SmtpServer.Protocol;
using SmtpServer.Storage;

namespace EmailRouter;

public class MessageStore : IMessageStore
{
    public async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction,
        ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
    {
        var text = Encoding.UTF8.GetString(buffer.ToArray());
        var message = await MimeMessage.LoadAsync(new MemoryStream(buffer.ToArray()), cancellationToken);

        await HandleMessageAsync(message);

        return SmtpResponse.Ok;
    }

    private static async Task HandleMessageAsync(MimeMessage message)
    {
        try
        {
            var apiKey = Environment.GetEnvironmentVariable("SG_API_KEY");
            var client = new SendGridClient(apiKey);

            var from = message.From.Mailboxes.First();

            var sendGridMessage = new SendGridMessage
            {
                From = new EmailAddress(from.Address, from.Name),
                Subject = message.Subject,
                HtmlContent = message.HtmlBody ?? message.TextBody ?? "No Content Provided",
                PlainTextContent = message.TextBody ?? "No Content Provided"
            };

            foreach (var to in message.To.Mailboxes)
                sendGridMessage.AddTo(new EmailAddress(to.Address, to.Name));

            foreach (var cc in message.Cc.Mailboxes)
                sendGridMessage.AddCc(new EmailAddress(cc.Address, cc.Name));

            foreach (var attachment in message.Attachments)
            {
                using var memoryStream = new MemoryStream();
                if (attachment is not MimePart part) continue;

                await part.Content.DecodeToAsync(memoryStream);
                var bytes = memoryStream.ToArray();
                sendGridMessage.AddAttachment(part.FileName, Convert.ToBase64String(bytes),
                    part.ContentType.MimeType);
            }

            var response = await client.SendEmailAsync(sendGridMessage);
            Console.WriteLine($"Email sent to SendGrid: Status code {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error forwarding email: {ex.Message}");
        }
    }
}