using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO.Hashing;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using MailKit;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using NpgsqlTypes;

namespace Backend.Models
{
    [Index(nameof(UniqueHash), IsUnique = false)]
    [Index(nameof(FolderId), IsUnique = false)]
    public partial class Mail
    {
        [Key]
        public long Id { get; set; }
        public UniqueId ImapMailUID { get; set; }
        [JsonIgnore]
        public string ImapMailId { get; set; }= string.Empty;
        [NotMapped]
        [JsonIgnore]
        public readonly string? ImapReplyFromId;
        [JsonIgnore]
        public long? RepliedFromId { get; set; }
        public bool IsAReply => this.RepliedFromId.HasValue && this.RepliedFromId.Value > 0;
        public Mail? RepliedFrom { get; set; } = null;
        [JsonIgnore]
        public long? ReplyId { get; set; } = null;
        public Mail? Reply { get; set; } = null;
        public bool HasReply { get; set; } = false;
        [ReadOnly(true)]
        public EmailAddress? Sender {get; set; } = null;
        [ReadOnly(true)]
        public ICollection<EmailAddress> Recipients { get; set; } = [];
        [ReadOnly(true)]
        public ICollection<EmailAddress> RecipientsCc { get; set; } = [];
        [ReadOnly(true)]
        public string Subject { get; set; } = string.Empty;
        [ReadOnly(true)]
        public string Body { get; set; } = string.Empty;
        public string BodyText { get {
            HtmlAgilityPack.HtmlDocument doc = new();
            doc.LoadHtml(this.Body);
            string text = RmRedundantNewLinesRegex().Replace(doc.DocumentNode.InnerText.Trim(), "\n");
            return RmRedundantSpacesRegex().Replace(text, " ");
        } 
        private set {} }
        public NpgsqlTsVector? SearchVector { get; set; }
        [ReadOnly(true)]
        public ICollection<Attachment> Attachments { get; set; } = [];
        [ReadOnly(true)]
        public DateTime DateSent { get; set; } = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Unspecified);
        [Required]
        [JsonIgnore]
        [ReadOnly(true)]
        public MailBox? OwnerMailBox { get; set; } = null;
        [Required]
        [JsonIgnore]
        [ReadOnly(true)]
        public int? OwnerMailBoxId { get; set; } = null;
        [JsonIgnore]
        public Folder? Folder { get; set; } = null;
        [JsonIgnore]
        public int FolderId { get; set; }
        
        private ulong _uniqueHash = 0;
        public ulong UniqueHash
        { get 
            {
                if (this._uniqueHash == 0){
                    XxHash3 xxHash= new();

                    xxHash.Append(Encoding.UTF8.GetBytes(this.Subject));
                    xxHash.Append(Encoding.UTF8.GetBytes(this.Body));
                    xxHash.Append(BitConverter.GetBytes(this.DateSent.Ticks));
                    if (this.Sender is not null)
                        xxHash.Append(Encoding.UTF8.GetBytes(this.Sender.Address));

                    this._uniqueHash = xxHash.GetCurrentHashAsUInt64();
                }
                return this._uniqueHash;
            }
            private set { _ = value; }
        }

        public Mail(){}

        public Mail(MimeMessage msg, UniqueId uid, Folder folder)
        {
            this.Id = 0;
            this.ImapReplyFromId = msg.InReplyTo;
            this.ImapMailId = msg.MessageId;
            this.ImapMailUID = uid;
            this.Subject = msg.Subject;
            this.Body = msg.HtmlBody;
            this.Folder = folder;
            this.OwnerMailBox = folder.MailBox;
            this.DateSent = DateTime.SpecifyKind(msg.Date.ToUniversalTime().DateTime, DateTimeKind.Unspecified);


            MailboxAddress? from = msg.From.Count > 0 ? (MailboxAddress?) msg.From[0] : null;
            this.Sender = new EmailAddress(){FullName=from?.Name, Address=from?.Address ?? "UNKNOWN"};

            this.Recipients = [];
            foreach (MailboxAddress recipient in msg.To.Cast<MailboxAddress>())
                this.Recipients.Add(new EmailAddress(){Address = recipient.Address, FullName = recipient.Name});
            foreach (MailboxAddress recipient in msg.Cc.Cast<MailboxAddress>())
                this.RecipientsCc.Add(new EmailAddress(){Address = recipient.Address, FullName = recipient.Name});
        }

        [GeneratedRegex(@"\s+\n")]
        private static partial Regex RmRedundantNewLinesRegex();
        [GeneratedRegex(@"\s+[ ]")]
        private static partial Regex RmRedundantSpacesRegex();
    }
}