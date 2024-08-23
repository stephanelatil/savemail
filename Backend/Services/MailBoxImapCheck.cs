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
    AuthenticationError,
    InvalidSaslMethod
}

public interface IMailBoxImapCheck
{
    public Task<ImapCheckResult> CheckConnection(UpdateMailBox mailbox, CancellationToken cancellationToken=default);
    public Task<List<ImapProvider>> GetValidProviders(UpdateMailBox mailbox, CancellationToken cancellationToken=default);
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

            if (mailbox.Provider != ImapProvider.Simple && !mailbox.Provider.IsValidProvider(client.AuthenticationMechanisms))
                return ImapCheckResult.InvalidSaslMethod;
            
            await MailBox.ImapAuthenticateAsync(
                                    client,
                                    mailbox,
                                    cancellationToken);
        }
        catch(ArgumentException){ return ImapCheckResult.InvalidValue;}
        catch(IOException){ return ImapCheckResult.ConnectionToServerError;}
        catch(ImapProtocolException){ return ImapCheckResult.ConnectionToServerError;}
        catch(AuthenticationException){ return ImapCheckResult.AuthenticationError;}

        return ImapCheckResult.Success;
    }

    public async Task<List<ImapProvider>> GetValidProviders(UpdateMailBox mailbox, CancellationToken cancellationToken = default)
    {
        using var client = new ImapClient();
        List<ImapProvider> validProviders = [];
        ArgumentNullException.ThrowIfNull(mailbox.ImapPort);

        await client.ConnectAsync(mailbox.ImapDomain,
                                mailbox.ImapPort.Value,
                                SecureSocketOptions.Auto,
                                cancellationToken);

        HashSet<string> authMethods = client.AuthenticationMechanisms;
        foreach (ImapProvider provider in Enum.GetValues(typeof(ImapProvider)))
            if (provider.IsValidProvider(authMethods))
                validProviders.Add(provider);
        if (validProviders.Contains(ImapProvider.Plain))
            validProviders.Add(ImapProvider.Simple);

        return validProviders;
    }
}