using System.IdentityModel.Tokens.Jwt;
using Microsoft.Build.Framework;

namespace Backend.Models;

public class OAuthCredentials
{
    public int Id { get; set; }
    public bool NeedReAuth { get; set; } = false;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    [Required]
    public required MailBox OwnerMailbox { get; set; }
    public int OwnerMailboxId { get; set; }
    public ImapProvider Provider { get; set; } 
    public static string RefreshUrl(ImapProvider provider) => provider switch
            {
                ImapProvider.Google => "https://oauth2.googleapis.com/token",
                _ => string.Empty,
            };
    public static string UserProfileUrl(ImapProvider provider) => provider switch
            {
                ImapProvider.Google => "https://www.googleapis.com/oauth2/v2/userinfo",
                _ => string.Empty,
            };
    public static string ImapUrl(ImapProvider provider) => provider switch
            {
                ImapProvider.Google => "imap.google.com",
                _ => string.Empty,
            };
    public bool AccessTokenExpired => IsExpired(this.AccessToken);
    public bool RefreshTokenExpired => IsExpired(this.RefreshToken);

    private static bool IsExpired(string token){
        return new JwtSecurityTokenHandler().ReadToken(token) is not JwtSecurityToken jwtToken 
                        || jwtToken.ValidTo.ToUniversalTime().AddMinutes(15) < DateTime.UtcNow;
    }
}