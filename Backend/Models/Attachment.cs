using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class Attachment
    {
        [Key]
        public long Id { get; set; }
        [JsonIgnore]
        public Mail? Mail { get; set; }
        public long? MailId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; } = 0;
        public string FilePath { get; set; } = string.Empty;
        public AppUser? Owner { get; set; } = null;
        public string? OwnerId { get; set; } = null;
        public Attachment(){}
    }
}