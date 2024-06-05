using Dumpify;
using SmtpServer;
using SmtpServer.ComponentModel;

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
            try
            {
                // Instead of waiting for a key press, wait for the cancellation token to be triggered
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // Expected exception when the cancellation token is triggered, can be safely ignored
            }

            Console.WriteLine("The service has stopped");
        }
    }
}