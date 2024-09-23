using System.Text.RegularExpressions;

namespace Backend.Models.DTO;

public partial class MailDto
{
    private const int MAX_PARTIAL_FETCH_LEN=300;

    public long Id { get; set; }
    public long? RepliedFromId { get; set; } = null;
    public MailDto? RepliedFrom { get; set; } = null;
    public MailDto? Reply { get; set; } = null;
    public long? ReplyId { get; set; } = null;
    public EmailAddressDto Sender { get; set; } = new EmailAddressDto();
    public ICollection<EmailAddressDto> Recipients { get; set; } = [];
    public ICollection<EmailAddressDto> RecipientsCc { get; set; } = [];
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public ICollection<AttachmentDto> Attachments { get; set; } = [];
    public DateTime DateSent { get; set; } = DateTime.Now;

    public MailDto(){}

    /// <summary>
    /// The data transfer object of the Mail object. 
    /// </summary>
    /// <param name="mail">The original Mail object</param>
    /// <param name="fetch_partial">If true only returns part of the body text. Default is false</param>
    public MailDto(Mail mail, bool fetch_partial=false)
    {
        this.Id = mail.Id;
        this.RepliedFromId = mail.RepliedFromId;
        this.RepliedFrom = mail.RepliedFrom is null ? null : new MailDto(mail.RepliedFrom, fetch_partial);
        this.ReplyId = mail.ReplyId;
        this.Reply = mail.Reply is null ? null : new MailDto(mail.Reply, fetch_partial);
        this.Sender = mail.Sender is null ? new EmailAddressDto() : new EmailAddressDto(mail.Sender);
        this.Recipients = mail.Recipients.Select(x => new EmailAddressDto(x)).ToList();
        this.RecipientsCc = mail.RecipientsCc.Select(x => new EmailAddressDto(x)).ToList();
        this.Subject = mail.Subject;
        this.Body = mail.Body;
        if (fetch_partial)
        {
            HtmlAgilityPack.HtmlDocument doc = new();
            doc.LoadHtml(this.Body);
            this.Body = RmConcurrentHTMLWhiteSpaceRegex().Replace(doc.DocumentNode.InnerText, " ");
            if (this.Body.Length > MAX_PARTIAL_FETCH_LEN)
                this.Body = this.Body.Remove(MAX_PARTIAL_FETCH_LEN-3)+"...";
        }
        this.Attachments = mail.Attachments.Select(a => new AttachmentDto(a)).ToList();
        this.DateSent = mail.DateSent;
    }

    [GeneratedRegex(@"([&]nbsp[;])(\s+[ ])")]
    private static partial Regex RmConcurrentHTMLWhiteSpaceRegex();
}