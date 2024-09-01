using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public interface IOAuthCredentialsService
{
    public Task<OAuthCredentials?> GetCredentialsById(int id);
    public Task<OAuthCredentials> CreateNewCredentials(ImapProvider provider, string accessToken, string refreshToken, int mailboxId);
    public Task<MailBox> CreateNewMailboxWithCredentials(ImapProvider provider, string accessToken, string refreshToken, AppUser owner);
    public Task<OAuthCredentials?> RefreshCredentials(OAuthCredentials credentials);
}

public class OAuthCredentialsService : IOAuthCredentialsService
{
    private readonly IOAuthService _oAuthService;
    private readonly ApplicationDBContext _context;

    public OAuthCredentialsService(IOAuthService oAuthService, ApplicationDBContext context)
    {
        this._oAuthService = oAuthService;
        this._context = context;
    }

    public async Task<MailBox> CreateNewMailboxWithCredentials(ImapProvider provider, string accessToken, string refreshToken, AppUser owner)
    {
        var mailbox = this._context.MailBox.Add(new MailBox(){
            Owner = owner,
            Provider = provider,
            ImapPort = MailBox.GetImapPortForProvider(provider)
        });


        var credentials = this._context.OAuthCredentials.Add(new(){
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Provider = provider,
            OwnerMailbox = mailbox.Entity
        });

        mailbox.Entity.ImapDomain = OAuthCredentials.ImapUrl(provider);
        mailbox.Entity.OAuthCredentials = credentials.Entity;
        mailbox.Entity.Username = await this._oAuthService.GetEmail(credentials.Entity);
        await this._context.SaveChangesAsync();
        return mailbox.Entity;
    }

    public async Task<OAuthCredentials> CreateNewCredentials(ImapProvider provider, string accessToken, string refreshToken, int mailboxId)
    {
        MailBox mailbox = await this._context.MailBox.Where(mb => mb.Id == mailboxId)
                                                 .Include(mb=> mb.OAuthCredentials)
                                                 .FirstOrDefaultAsync() ??
                                                    throw new ArgumentException("The provided mailboxId does not exist", nameof(mailboxId));
        if (mailbox.OAuthCredentials is not null)
        {
            var oldCreds = mailbox.OAuthCredentials;
            mailbox.OAuthCredentials = null;
            this._context.OAuthCredentials.Remove(oldCreds);
        }

        OAuthCredentials credentials = new(){
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Provider = provider,
            OwnerMailboxId = mailboxId,
            OwnerMailbox = mailbox
        };
        var entity = this._context.OAuthCredentials.Add(credentials);
        mailbox.ImapPort = MailBox.GetImapPortForProvider(provider);
        mailbox.ImapDomain = OAuthCredentials.ImapUrl(provider);

        mailbox.Username = await this._oAuthService.GetEmail(credentials);
        this._context.Update(mailbox);
        await this._context.SaveChangesAsync();

        return entity.Entity;
    }

    public async Task<OAuthCredentials?> GetCredentialsById(int id)
    {
        return await this._context.OAuthCredentials.Where(c =>c.Id == id).FirstOrDefaultAsync();
    }

    public async Task<OAuthCredentials?> RefreshCredentials(OAuthCredentials credentials)
    {
        if (!await this._oAuthService.RefreshToken(credentials))
            return null;
        return credentials;
    }
}