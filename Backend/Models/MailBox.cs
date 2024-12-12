namespace Backend.Models;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Backend.Services;
using MailKit.Net.Imap;
using MailKit.Security;
using Microsoft.IdentityModel.Tokens;

public class MailBox
{
    public int Id { get; set; }
    public string ImapDomain { get; set; } = string.Empty;
    public short ImapPort { get; set; }
    [JsonIgnore]
    [ReadOnly(true)]
    public string OwnerId { get; set; } = string.Empty;
    [JsonIgnore]
    [ReadOnly(true)]
    public AppUser? Owner { get; set; } = null;
    public bool NeedsReauth { get; set; } = false;
    public string Username { get; set; } = string.Empty;
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    public ImapProvider Provider { get; set; } = ImapProvider.Simple;
    public OAuthCredentials? OAuthCredentials { get; set; }
    [JsonIgnore]
    public ICollection<Mail> Mails { get; set; } = [];
    public ICollection<Folder> Folders { get; set; } = [];

    public async Task ImapAuthenticateAsync(ImapClient client,
                                            ITokenEncryptionService tokenEncryptionService,
                                            IOAuthService? tokenRefreshService=null,
                                            CancellationToken cancellationToken = default)
    {
        switch (this.Provider)
        {
            case ImapProvider.Simple:
                await client.AuthenticateAsync(this.Username, tokenEncryptionService.Decrypt(this.Password, this.Id, this.OwnerId), cancellationToken);
                break;
            case ImapProvider.Google:
                ArgumentNullException.ThrowIfNull(this.OAuthCredentials, nameof(this.OAuthCredentials));
                if (this.OAuthCredentials.AccessTokenExpired)
                {
                    if (this.NeedsReauth)
                        throw new SecurityTokenExpiredException("OAuthCredentials expired");
                    //refresh here
                    if (tokenRefreshService is null || !await tokenRefreshService.RefreshToken(this.OAuthCredentials, this.OwnerId))
                        throw new AuthenticationException("Unable to refresh credentials");
                }
                await client.AuthenticateAsync(new SaslMechanismOAuth2(this.Username, tokenEncryptionService.Decrypt(this.OAuthCredentials.AccessToken, this.Id, this.OwnerId)), cancellationToken);
                break;
            default:
                await client.AuthenticateAsync(new SaslMechanismAnonymous(this.Username), cancellationToken);
                break;
        };
    }

    public static string GetImapDomainForProvider(ImapProvider provider){
        return provider switch {
            ImapProvider.Google => "imap.gmail.com",
            _ => string.Empty
        };
    }

    public static short GetImapPortForProvider(ImapProvider provider){
        return provider switch {
            ImapProvider.Google => 993,
            _ => 993
        };
    }
}


public enum ImapProvider
{
    /// <summary>
    /// Uses a simple username password for authentication with MailKit.ImapClient
    /// </summary>
    Simple,
    /// <summary>
    /// Will use an OAuth2 token obtained via the Google Authentication processor 
    /// </summary>
    Google
}