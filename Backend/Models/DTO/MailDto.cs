using Backend.Models.DTO;

namespace Backend.Models
{
    public class MailDto
    {
        public long Id { get; set; }
        public long? ReplyTo { get; set; } = null;
        public ICollection<MailDto> Replies { get; set; } = [];
        public EmailAddressDto Sender { get; set; } = new EmailAddressDto();
        public ICollection<EmailAddressDto> Recipients { get; set; } = [];
        public ICollection<EmailAddressDto> RecipientsCc { get; set; } = [];
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public ICollection<AttachmentDto> Attachments { get; set; } = [];
        public DateTimeOffset DateReceived { get; set; } = DateTimeOffset.Now;

        public MailDto(){}

        public MailDto(Mail mail)
        {
            this.Id = mail.Id;
            this.ReplyTo = mail.RepliedFrom?.Id;
            this.Replies = mail.Replies.Select(m => new MailDto(m)).ToList();
            this.Sender = mail.Sender is null ? new EmailAddressDto() : new EmailAddressDto(mail.Sender);
            this.Recipients = mail.Recipients.Select(x => new EmailAddressDto(x)).ToList();
            this.RecipientsCc = mail.RecipientsCc.Select(x => new EmailAddressDto(x)).ToList();
            this.Subject = mail.Subject;
            this.Body = mail.Body;
            this.Attachments = mail.Attachments.Select(a => new AttachmentDto(a)).ToList();
            this.DateReceived = mail.DateReceived;
        }
    }
}