namespace Backend.Models
{
    public class UserData
    {
        public long Id { get; set;}
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public List<MailBox> MailBoxes { get; set; } = [];
    }
}