using Microsoft.EntityFrameworkCore;

namespace Backend.Models
{
    [Index(nameof(Address), IsUnique = true)]
    public class MailBox
    {
        public long Id { get; set; }
        public EmailAddress? Address { get; set; }
        public string ImapDomain { get; set; } = string.Empty;
        public short ImapPort { get; set; }
        private UserData User { get; set; }
        //need additional data/tokens etc?
    }
}