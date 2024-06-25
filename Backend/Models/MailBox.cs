using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models
{
    [Index(nameof(Address), IsUnique = true)]
    public class MailBox
    {
        public long Id { get; set; }
        public string Address { get; set; } = string.Empty;
        public string ImapDomain { get; set; } = string.Empty;
        public short ImapPort { get; set; }
        [Required]
        private AppUser? User { get; set; } = null;

        [JsonIgnore]
        public List<Secret> Secrets { get; set; } = [];

        [JsonIgnore]
        public List<Mail> Mails { get;set; } = [];
        public List<Folder> Folders { get; set;} = [];
    }
}