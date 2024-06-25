using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Secret
    {
        public long Id { get; set; }
        [Required]
        public MailBox? SecretOwner { get; set; } = null;
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}