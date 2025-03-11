using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO.Hashing;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using NpgsqlTypes;

namespace Backend.Models
{
    [Index(nameof(UniqueHash), IsUnique = false)]
    public partial class Mail
    {
        [Key]
        public long Id { get; set; }
        public MailKit.UniqueId ImapMailUID { get; set; }
        [JsonIgnore]
        public string ImapMailId { get; set; } = string.Empty;
        [NotMapped]
        [JsonIgnore]
        public readonly string? ImapReplyFromId;
        [NotMapped]
        [JsonIgnore]
        public MimeMessage? MimeMessage;
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
        public NpgsqlTsVector SearchVector { get; set; }
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

        //split into uniqueId1 and 2 to have a 128 bit hash to minimize collisions
        private byte[] _uniqueHash = new byte[16];
        public ulong UniqueHash
        { 
            get 
            {
                //it's all zeroes: not set
                if (this._uniqueHash.All(x=> x == 0)){
                    XxHash128 xxHash = new();

                    xxHash.Append(Encoding.UTF8.GetBytes(this.Subject));
                    xxHash.Append(Encoding.UTF8.GetBytes(this.Body));
                    xxHash.Append(BitConverter.GetBytes(this.DateSent.Ticks));
                    if (this.Sender is not null)
                        xxHash.Append(Encoding.UTF8.GetBytes(this.Sender.Address));
                    xxHash.Append(Encoding.UTF8.GetBytes(
                            string.Join(";", this.Recipients.OrderBy(x=>x.Address).Select(x=>x.Address))));
                    xxHash.Append(Encoding.UTF8.GetBytes(
                            string.Join(";", this.RecipientsCc.OrderBy(x=>x.Address).Select(x=>x.Address))));
                    xxHash.Append(BitConverter.GetBytes(this.OwnerMailBoxId??this.OwnerMailBox?.Id??0));

                    this._uniqueHash = xxHash.GetCurrentHash();
                }
                return BitConverter.ToUInt64(this._uniqueHash, 0);
            }
            private set { _ = value; }
        }
        public ulong UniqueHash2 
        { 
            get 
            {
                //it's all zeroes: not set
                if (this._uniqueHash.All(x=> x == 0)){
                    XxHash128 xxHash = new();

                    xxHash.Append(Encoding.UTF8.GetBytes(this.Subject));
                    xxHash.Append(Encoding.UTF8.GetBytes(this.Body));
                    xxHash.Append(BitConverter.GetBytes(this.DateSent.Ticks));
                    if (this.Sender is not null)
                        xxHash.Append(Encoding.UTF8.GetBytes(this.Sender.Address));
                    xxHash.Append(Encoding.UTF8.GetBytes(
                            string.Join(";", this.Recipients.OrderBy(x=>x.Address).Select(x=>x.Address))));
                    xxHash.Append(Encoding.UTF8.GetBytes(
                            string.Join(";", this.RecipientsCc.OrderBy(x=>x.Address).Select(x=>x.Address))));
                    xxHash.Append(BitConverter.GetBytes(this.OwnerMailBoxId??0));

                    this._uniqueHash = xxHash.GetCurrentHash();
                }
                return BitConverter.ToUInt64(this._uniqueHash, 8);
            }
            private set { _ = value; }
        }

//ignore warning here because SearchVector will be will be defined by the DB itself
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public Mail(){}

        public Mail(MimeMessage msg, MailKit.UniqueId uid)
        {
            this.Id = 0;
            this.ImapReplyFromId = msg.InReplyTo;
            this.ImapMailId = msg.MessageId ?? Guid.NewGuid().ToString();
            this.ImapMailUID = uid;
            this.Subject = msg.Subject;
            this.Body = msg.HtmlBody ?? string.Join("", msg.BodyParts.Select(bp => bp is TextPart part ? part.Text : ""));
            this.MimeMessage = msg;
            this.DateSent = DateTime.SpecifyKind(msg.Date.ToUniversalTime().DateTime, DateTimeKind.Unspecified);

            MailboxAddress? from = msg.From.Count > 0 ? (MailboxAddress?) msg.From[0] : null;
            this.Sender = new EmailAddress(){FullName=from?.Name, Address=from?.Address ?? "UNKNOWN"};

            this.Recipients = [];
            foreach (MailboxAddress recipient in msg.To.Cast<MailboxAddress>())
                this.Recipients.Add(new EmailAddress(){Address = recipient.Address, FullName = recipient.Name});
            foreach (MailboxAddress recipient in msg.Cc.Cast<MailboxAddress>())
                this.RecipientsCc.Add(new EmailAddress(){Address = recipient.Address, FullName = recipient.Name});
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        [GeneratedRegex(@"\s+\n")]
        private static partial Regex RmRedundantNewLinesRegex();
        [GeneratedRegex(@"\s+[ ]")]
        private static partial Regex RmRedundantSpacesRegex();
    }
}