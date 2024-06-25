using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class Secret
    {
        public long Id { get; set; }
        [Required]
        [JsonIgnore]
        public MailBox? SecretOwner { get; set; } = null;
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}