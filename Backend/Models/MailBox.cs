using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
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
        public ImapProvider Provider { get; set; } = ImapProvider.Simple;
        [JsonIgnore]
        public ICollection<Mail> Mails { get;set; } = [];
        public ICollection<Folder> Folders { get; set;} = [];

        public SaslMechanism GetSaslMechanism()
        {
            return this.Provider switch
            {
                ImapProvider.Gmail => new SaslMechanismOAuth2(this.Username, this.Password),
                _ => new SaslMechanismLogin(this.Username, this.Password),
            };
        }
    }

    public enum ImapProvider
    {
        /// <summary>
        /// Uses a simple username password for authentication with MailKit.ImapClient
        /// </summary>
        Simple = 0,
        /// <summary>
        /// Will use an OAuth2 token obtained via the Google Authentication processor 
        /// </summary>
        Gmail = 1
    }
}