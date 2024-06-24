using System.ComponentModel.DataAnnotations;

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
        public long MailBoxId { get; set; }
        [Required]
        public MailBox MailBox { get; set; }
        public long FolderId { get; set; }
        public Folder? Folder { get; set; } = null;
    }
}