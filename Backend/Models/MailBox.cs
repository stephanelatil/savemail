using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using Backend.Models.DTO;
using MailKit.Net.Imap;
using MailKit.Security;

namespace Backend.Models
{
    public class MailBox
    {
        public int Id { get; set; }
        public string ImapDomain { get; set; } = string.Empty;
        public short ImapPort { get; set; }
        public SecureSocketOptions SecureSocketOptions { get; set; } = SecureSocketOptions.Auto;
        [JsonIgnore]
        [ReadOnly(true)]
        public string OwnerId { get; set; } = string.Empty;
        [JsonIgnore]
        [ReadOnly(true)]
        public AppUser? Owner { get; set; } = null;
        public string Username {get ; set;} = string.Empty;
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        public ImapProvider Provider { get; set; } = ImapProvider.Plain;
        [JsonIgnore]
        public ICollection<Mail> Mails { get;set; } = [];
        public ICollection<Folder> Folders { get; set;} = [];

        public static async Task ImapAuthenticateAsync(ImapClient client, UpdateMailBox mb,
                                                        CancellationToken cancellationToken = default)
        {
            switch (mb.Provider)
            {
                case ImapProvider.Simple:
                    await client.AuthenticateAsync(mb.Username, mb.Password, cancellationToken);
                    break;
                case ImapProvider.Plain:
                    await client.AuthenticateAsync(new SaslMechanismPlain(mb.Username, mb.Password), cancellationToken);
                    break;
                case ImapProvider.SaslLogin:
                    await client.AuthenticateAsync(new SaslMechanismLogin(mb.Username, mb.Password), cancellationToken);
                    break;
                case ImapProvider.Cram_MD5:
                    await client.AuthenticateAsync(new SaslMechanismCramMd5(mb.Username, mb.Password), cancellationToken);
                    break;
                case ImapProvider.Gmail:
                    await client.AuthenticateAsync(new SaslMechanismOAuth2(mb.Username, mb.Password), cancellationToken);
                    break;
                default:
                    await client.AuthenticateAsync(new SaslMechanismAnonymous(mb.Username), cancellationToken);
                    break;
            };
        }

        public async Task ImapAuthenticateAsync(ImapClient client, CancellationToken cancellationToken = default)
        {
            switch (this.Provider)
            {
                case ImapProvider.Simple:
                    await client.AuthenticateAsync(this.Username, this.Password, cancellationToken);
                    break;
                case ImapProvider.Plain:
                    await client.AuthenticateAsync(new SaslMechanismPlain(this.Username, this.Password), cancellationToken);
                    break;
                case ImapProvider.SaslLogin:
                    await client.AuthenticateAsync(new SaslMechanismLogin(this.Username, this.Password), cancellationToken);
                    break;
                case ImapProvider.Cram_MD5:
                    await client.AuthenticateAsync(new SaslMechanismCramMd5(this.Username, this.Password), cancellationToken);
                    break;
                case ImapProvider.Gmail:
                    await client.AuthenticateAsync(new SaslMechanismOAuth2(this.Username, this.Password), cancellationToken);
                    break;
                default:
                    await client.AuthenticateAsync(new SaslMechanismAnonymous(this.Username), cancellationToken);
                    break;
            };
        }
    }

    public enum ImapProvider
    {
        /// <summary>
        /// Uses a simple username password for authentication with MailKit.ImapClient
        /// </summary>
        Simple=0,
        Plain=1,
        SaslLogin=2,
        Cram_MD5=3,
        /// <summary>
        /// Will use an OAuth2 token obtained via the Google Authentication processor 
        /// </summary>
        Gmail=4
    }

    public static class ImapProviderExtensions
    {
        public static string GetAssociatedAuthMethod(this ImapProvider provider){
            return provider switch
            {
                ImapProvider.Plain => "PLAIN",
                ImapProvider.SaslLogin => "LOGIN",
                ImapProvider.Cram_MD5 => "CramMD5",
                ImapProvider.Gmail => "XOAUTH2",
                _ => string.Empty
            };
        }

        public static bool IsValidProvider(this ImapProvider provider, HashSet<string> authMethods)
        {
            return authMethods.Contains(provider.GetAssociatedAuthMethod());
        }

    }
}