namespace Backend.Models
{
    public class Folder
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Mail> Mails { get; set; } = [];
        public MailBox? MailBox { get; set; } = null;
    }
}