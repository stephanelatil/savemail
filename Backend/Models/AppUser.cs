using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace Backend.Models
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        [JsonIgnore]
        public List<MailBox> MailBoxes { get; set; } = [];
    }
}