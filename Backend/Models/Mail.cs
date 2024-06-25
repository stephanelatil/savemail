using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class Mail
    {
        public long Id { get; set; }
        public EmailAddress? Sender {get; set; } 
        public List<EmailAddress> Recipients { get; set; } = [];
        public List<EmailAddress> RecipientsCc { get; set; } = [];
        public List<EmailAddress> RecipientsBcc { get; set; } = [];
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty; 
        public List<Attachment> Attachments { get; set; } = [];
        public DateTimeOffset DateReceived { get; set; } = DateTimeOffset.Now;
        [Required]
        [JsonIgnore]
        public MailBox? OwnerMailBox { get; set; } = null;
        public Folder? Folder { get; set; } = null;
    }
}