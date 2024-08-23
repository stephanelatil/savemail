using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public interface IOAuthCredentialsService
{
    public Task<OAuthCredentials?> GetCredentialsById(int id);
    public Task<OAuthCredentials> CreateNewCredentials(OAuthCredentials.OAuthProvider provider, string accessToken, string refreshToken, int mailboxId);
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

    public async Task<OAuthCredentials> CreateNewCredentials(OAuthCredentials.OAuthProvider provider, string accessToken, string refreshToken, int mailboxId)
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
        await this._context.SaveChangesAsync();

        return entity.Entity;
    }

    public async Task<OAuthCredentials?> GetCredentialsById(int id)
    {
        return await this._context.OAuthCredentials.Where(c =>c.Id == id).FirstOrDefaultAsync();
    }

    public async Task<OAuthCredentials?> RefreshCredentials(OAuthCredentials credentials)
    {
        var newTokens = await this._oAuthService.RefreshToken(credentials);

        if (newTokens is null)
            return null;

        if (newTokens.RefreshToken?.Length > 0)
            credentials.RefreshToken =  newTokens.RefreshToken;
        credentials.AccessToken = newTokens.AccessToken;
        
        var entry = this._context.OAuthCredentials.Update(credentials);
        await this._context.SaveChangesAsync();

        return entry.Entity;
    }
}