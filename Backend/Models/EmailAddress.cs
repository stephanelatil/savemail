using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models
{
    [Index(nameof(Address), IsUnique = true)]
    public class EmailAddress
    {
        [Key]
        [MaxLength(256)]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [ReadOnly(true)]
        public string Address { get; set; } = string.Empty;
        [MaxLength(128)]
        public string? FullName { get; set; } = null;
        [JsonIgnore]
        public ICollection<Mail> MailsSent { get; set; } = [];
        [JsonIgnore]
        public ICollection<Mail> MailsReceived { get; set; } = [];
        [JsonIgnore]
        public ICollection<Mail> MailsCCed { get; set; } = [];
    }
}