using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
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

        [JsonIgnore]
        public List<Mail> MailsSent { get; set; } = [];
        [JsonIgnore]
        public List<Mail> MailsReceived { get; set; } = [];
        [JsonIgnore]
        public List<Mail> MailsCCed { get; set; } = [];
        [JsonIgnore]
        public List<Mail> MailsBCCed { get; set; } = [];
    }
}