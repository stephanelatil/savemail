using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Backend.Services;

public interface IOAuthCredentialsService
{
    public Task<OAuthCredentials?> GetCredentialsById(int id);
    public Task<OAuthCredentials> CreateNewCredentials(ImapProvider provider, string accessToken, DateTime validityEnd, string refreshToken, int mailboxId, string? email=null);
    public Task<MailBox> CreateNewMailboxWithCredentials(ImapProvider provider, string accessToken, DateTime validityEnd, string refreshToken, AppUser owner, string? email=null);
    public Task<OAuthCredentials?> RefreshCredentials(OAuthCredentials credentials, string ownerId);
}

public class OAuthCredentialsService : IOAuthCredentialsService
{
    private readonly IOAuthService _oAuthService;
    private readonly ApplicationDBContext _context;
    private readonly TokenEncryptionService _tokenEncryptionService;
    private readonly ILogger _logger;

    public OAuthCredentialsService(IOAuthService oAuthService,
                                   TokenEncryptionService tokenEncryptionService,
                                   ApplicationDBContext context,
                                   ILogger<OAuthCredentialsService> logger)
    {
        this._oAuthService = oAuthService;
        this._context = context;
        this._tokenEncryptionService = tokenEncryptionService;
        this._logger = logger;
    }

    public async Task<MailBox> CreateNewMailboxWithCredentials(ImapProvider provider, string accessToken, DateTime validityEnd, string refreshToken, AppUser owner, string? email=null)
    {
        var mailbox = this._context.MailBox.Add(new MailBox(){
            Owner = owner,
            Provider = provider,
            ImapPort = MailBox.GetImapPortForProvider(provider)
        });


        var credentials = this._context.OAuthCredentials.Add(new(){
            AccessToken = accessToken,
            AccessTokenValidity = validityEnd.ToUniversalTime(),
            RefreshToken = refreshToken,
            Provider = provider,
            OwnerMailbox = mailbox.Entity
        });

        mailbox.Entity.ImapDomain = OAuthCredentials.ImapUrl(provider);
        mailbox.Entity.OAuthCredentials = credentials.Entity;
        mailbox.Entity.Username = email ?? await this._oAuthService.GetEmail(credentials.Entity, owner.Id);
        await this._context.SaveChangesAsync();
        credentials.Entity.AccessToken = this._tokenEncryptionService.Encrypt(
                                                        credentials.Entity.AccessToken,
                                                        mailbox.Entity.Id,
                                                        owner.Id);
        credentials.Entity.RefreshToken = this._tokenEncryptionService.Encrypt(
                                                        credentials.Entity.RefreshToken,
                                                        mailbox.Entity.Id,
                                                        owner.Id);
        this._context.OAuthCredentials.Update(credentials.Entity);
        await this._context.SaveChangesAsync();
        return mailbox.Entity;
    }

    public async Task<OAuthCredentials> CreateNewCredentials(ImapProvider provider, string accessToken, DateTime validityEnd,
                                                             string refreshToken, int mailboxId, string? email=null)
    {
        MailBox mailbox = await this._context.MailBox.Where(mb => mb.Id == mailboxId)
                                                 .Include(mb=> mb.OAuthCredentials)
                                                 .AsSplitQuery()
                                                 .FirstOrDefaultAsync() ??
                                                    throw new ArgumentException("The provided mailboxId does not exist", nameof(mailboxId));
        EntityEntry<OAuthCredentials>? entry = null;
        mailbox.NeedsReauth = false;
        if (mailbox.OAuthCredentials is null)
        {
            entry = this._context.OAuthCredentials.Add(new(){
                AccessToken = this._tokenEncryptionService.Encrypt(accessToken, mailbox.Id, mailbox.OwnerId),
                AccessTokenValidity = validityEnd.ToUniversalTime(),
                RefreshToken = this._tokenEncryptionService.Encrypt(refreshToken, mailbox.Id, mailbox.OwnerId),
                Provider = provider,
                OwnerMailboxId = mailboxId,
                OwnerMailbox = mailbox
            });
            mailbox.ImapPort = MailBox.GetImapPortForProvider(provider);
            mailbox.ImapDomain = OAuthCredentials.ImapUrl(provider);
            mailbox.Username = email ?? await this._oAuthService.GetEmail(entry.Entity, mailbox.OwnerId);
            mailbox.OAuthCredentials = entry.Entity;
            this._context.MailBox.Update(mailbox);
        }
        else
        {
            var oauthCredentials = mailbox.OAuthCredentials;
            oauthCredentials.AccessToken = this._tokenEncryptionService.Encrypt(accessToken, mailbox.Id, mailbox.OwnerId);
            oauthCredentials.RefreshToken = this._tokenEncryptionService.Encrypt(refreshToken, mailbox.Id, mailbox.OwnerId);
            oauthCredentials.AccessTokenValidity = validityEnd;
            oauthCredentials.OwnerMailboxId = mailboxId;
            oauthCredentials.OwnerMailbox = mailbox;
            mailbox.OAuthCredentials = oauthCredentials;
            if ((email ??=await this._oAuthService.GetEmail(oauthCredentials, mailbox.OwnerId)) == mailbox.Username){
                entry = this._context.OAuthCredentials.Update(oauthCredentials);
                this._context.MailBox.Update(mailbox);
            }
            else{
                //attempting to set access tokens for wrong email
                this._logger.LogWarning("Cannot set access tokens for email {} to mailbox with email {}", email, mailbox.Username);
                throw new InvalidDataException($"Cannot set access tokens for email {email} to mailbox with email {mailbox.Username}");
            }
        }

        await this._context.SaveChangesAsync();

        return entry.Entity;
    }

    public async Task<OAuthCredentials?> GetCredentialsById(int id)
    {
        return await this._context.OAuthCredentials.Where(c =>c.Id == id).FirstOrDefaultAsync();
    }

    public async Task<OAuthCredentials?> RefreshCredentials(OAuthCredentials credentials, string ownerId)
    {
        if (!await this._oAuthService.RefreshToken(credentials, ownerId))
            return null;
        return credentials;
    }
}