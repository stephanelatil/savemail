using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class Folder
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public List<Mail> Mails { get; set; } = [];
        [JsonIgnore]
        public MailBox? MailBox { get; set; } = null;
    }
}