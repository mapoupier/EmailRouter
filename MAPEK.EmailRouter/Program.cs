using System.Buffers;
using System.Text;
using Dumpify;
using SmtpServer;
using SmtpServer.ComponentModel;
using SmtpServer.Protocol;
using SmtpServer.Storage;
using MimeKit;
using SendGrid;
using SendGrid.Helpers.Mail;
using SmtpServer.Mail;

namespace EmailRouter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var options = new SmtpServerOptionsBuilder()
                .ServerName("localhost")
                .Port(25)
                .Build();

            var serviceProvider = new ServiceProvider();
            serviceProvider.Add(new MailboxFilter());
            serviceProvider.Add(new MessageStore());

            var smtpServer = new SmtpServer.SmtpServer(options, serviceProvider);

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cancellationTokenSource.Cancel();
            };

            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => { cancellationTokenSource.Cancel(); };
            
            _ = smtpServer.StartAsync(cancellationToken);
            Console.WriteLine("Press CTRL-C to stop the service");
            // Instead of waiting for a key press, wait for the cancellation token to be triggered
            await Task.Delay(Timeout.Infinite, cancellationToken);
            Console.WriteLine("The service has stopped");
        }
    }

    public class MailboxFilter : IMailboxFilter
    {
        public Task<bool> CanAcceptFromAsync(ISessionContext context, IMailbox from, int size, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task<bool> CanDeliverToAsync(ISessionContext context, IMailbox to, IMailbox from, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }

    public class MessageStore : IMessageStore
    {
        public async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction,
            ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            var text = Encoding.UTF8.GetString(buffer.ToArray());
            var message = MimeMessage.Load(new MemoryStream(buffer.ToArray()));

            await HandleMessageAsync(message);

            return SmtpResponse.Ok;
        }

        private static async Task HandleMessageAsync(MimeMessage message)
        {
            try
            {
                Console.WriteLine("Messaged received:");
                message.Dump();
                var apiKey = Environment.GetEnvironmentVariable("SG_API_KEY");
                var client = new SendGridClient(apiKey);

                var from = message.From.Mailboxes.First();

                var sendGridMessage = new SendGridMessage
                {
                    From = new EmailAddress(from.Address, from.Name),
                    Subject = message.Subject,
                    HtmlContent = message.HtmlBody ?? message.TextBody,
                    PlainTextContent = message.TextBody
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
                Console.WriteLine("response is");
                response.DumpConsole();
                Console.WriteLine($"Email sent to SendGrid: Status code {response.StatusCode}");
                Console.WriteLine(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error forwarding email: {ex.Message}");
            }
        }
    }
}