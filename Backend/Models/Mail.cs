using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MailKit;
using MimeKit;

namespace Backend.Models
{
    public class Mail
    {
        [Key]
        public long Id { get; set; }
        public UniqueId ImapMailUID{ get; set; }
        [JsonIgnore]
        public string ImapMailId { get; set; }= string.Empty;
        [NotMapped]
        [JsonIgnore]
        public string? ImapReplyFromId { get; } = null;
        [JsonIgnore]
        public Mail? RepliedFrom { get; set; } = null;
        [ReadOnly(true)]
        public EmailAddress? Sender {get; set; } = null;
        [ReadOnly(true)]
        public List<EmailAddress> Recipients { get; set; } = [];
        [ReadOnly(true)]
        public List<EmailAddress> RecipientsCc { get; set; } = [];
        [ReadOnly(true)]
        public string Subject { get; set; } = string.Empty;
        [ReadOnly(true)]
        public string Body { get; set; } = string.Empty;
        [ReadOnly(true)]
        public List<Attachment> Attachments { get; set; } = [];
        [DataType(DataType.DateTime)]
        [ReadOnly(true)]
        public DateTimeOffset DateReceived { get; set; } = DateTimeOffset.Now;
        [Required]
        [JsonIgnore]
        [ReadOnly(true)]
        public MailBox? OwnerMailBox { get; set; } = null;
        [JsonIgnore]
        public Folder? Folder { get; set; } = null;

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
            this.DateReceived = msg.Date;

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