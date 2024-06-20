using System.Collections.Generic;

namespace Backend.Models
{
    public class Mail
    {
        public long Id { get; set; }
        public EmailAddress? Sender {get; set; } 
        public List<EmailAddress> Recipients { get; set; } = [];
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty; //replace with index?

        // public email file?
        public List<Attachment> Attachments { get; set; } = [];
    }
}