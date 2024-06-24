using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models
{
    [Index(nameof(FullAddress), IsUnique = true)]
    public class EmailAddress
    {
        public long Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        [StringLength(256)]
        public string FullAddress { get; set; } = string.Empty;

        public List<Mail> MailsSent { get; set; } = [];
        public List<Mail> MailsReceived { get; set; } = [];
        public List<Mail> MailsCCed { get; set; } = [];
        public List<Mail> MailsBCCed { get; set; } = [];
    }
}