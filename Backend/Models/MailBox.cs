using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models
{
    [Index(nameof(Address), IsUnique = true)]
    public class MailBox
    {
        public long Id { get; set; }
        public string Address { get; set; }
        public string ImapDomain { get; set; } = string.Empty;
        public short ImapPort { get; set; }
        [Required]
        private UserData User { get; set; }
        //need additional data/tokens etc?
        public List<Mail> Mails { get;set; } = [];
        public List<Folder> Folders { get; set;} = [];
    }
}