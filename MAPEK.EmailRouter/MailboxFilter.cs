using SmtpServer;
using SmtpServer.Mail;
using SmtpServer.Storage;

namespace EmailRouter;

public class MailboxFilter : IMailboxFilter
{
    public Task<bool> CanAcceptFromAsync(ISessionContext context, IMailbox from, int size,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    public Task<bool> CanDeliverToAsync(ISessionContext context, IMailbox to, IMailbox from,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}