using System.Linq.Expressions;
using Backend.Controllers;
using Backend.Models;
using Backend.Models.DTO;
using MailKit.Net.Imap;
using MailKit.Security;
using Microsoft.CodeAnalysis;

namespace Backend.Services;

public enum ImapCheckResult
{
    Success,
    NullValue,
    InvalidValue,
    ConnectionToServerError,
    AuthenticationError
}

public interface IMailBoxImapCheck
{
    public Task<ImapCheckResult> CheckConnection(UpdateMailBox mailbox, CancellationToken cancellationToken=default);
}

public class MailboxImapCheck : IMailBoxImapCheck
{

    public async Task<ImapCheckResult> CheckConnection(UpdateMailBox mailbox,
                                                       CancellationToken cancellationToken=default)
    {
        if (mailbox is null || mailbox.ImapDomain is null
                            || !mailbox.ImapPort.HasValue
                            || mailbox.Username is null
                            || mailbox.Password is null)
            return ImapCheckResult.NullValue;

        using var client = new ImapClient();

        try{
            await client.ConnectAsync(mailbox.ImapDomain,
                                    mailbox.ImapPort.Value,
                                    SecureSocketOptions.Auto,
                                    cancellationToken);
            
            await client.AuthenticateAsync(mailbox.Username, mailbox.Password, cancellationToken);
        }
        catch(ArgumentException){ return ImapCheckResult.InvalidValue;}
        catch(IOException){ return ImapCheckResult.ConnectionToServerError;}
        catch(ImapProtocolException){ return ImapCheckResult.ConnectionToServerError;}
        catch(AuthenticationException){ return ImapCheckResult.AuthenticationError;}

        return ImapCheckResult.Success;
    }
}