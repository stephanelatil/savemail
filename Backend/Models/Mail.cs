using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO.Hashing;
using System.Text;
using System.Text.Json.Serialization;
using MailKit;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace Backend.Models
{
    [Index(nameof(UniqueHash), IsUnique = true)]
    [Index(nameof(FolderId), IsUnique = false)]
    public class Mail
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
        public Mail? RepliedFrom { get; set; } = null;
        [JsonIgnore]
        public ICollection<Mail> Replies { get; set; } = [];
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
        [ReadOnly(true)]
        public ICollection<Attachment> Attachments { get; set; } = [];
        [DataType(DataType.DateTime)]
        [ReadOnly(true)]
        public DateTimeOffset DateSent { get; set; } = DateTimeOffset.MinValue;
        [Required]
        [JsonIgnore]
        [ReadOnly(true)]
        public MailBox? OwnerMailBox { get; set; } = null;
        [JsonIgnore]
        public Folder? Folder { get; set; } = null;
        [JsonIgnore]
        public int FolderId { get; set; }
        
        public ulong UniqueHash
        { get 
            {
                XxHash3 xxHash= new();

                xxHash.Append(Encoding.UTF8.GetBytes(this.Subject));
                xxHash.Append(Encoding.UTF8.GetBytes(this.Body));
                xxHash.Append(BitConverter.GetBytes(this.DateSent.UtcTicks));
                if (this.Sender is not null)
                    xxHash.Append(Encoding.UTF8.GetBytes(this.Sender.Address));

                return xxHash.GetCurrentHashAsUInt64();
            }
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
            this.DateSent = msg.Date;

            MailboxAddress? from = (MailboxAddress?) msg.From.FirstOrDefault((MailboxAddress?)null);
            this.Sender = new EmailAddress(){FullName=from?.Name, Address=from?.Address ?? "UNKNOWN"};

            this.Recipients = [];
            foreach (MailboxAddress recipient in msg.To.Cast<MailboxAddress>())
                this.Recipients.Add(new EmailAddress(){Address = recipient.Address, FullName = recipient.Name});
            foreach (MailboxAddress recipient in msg.Cc.Cast<MailboxAddress>())
                this.RecipientsCc.Add(new EmailAddress(){Address = recipient.Address, FullName = recipient.Name});
        }
    }
}