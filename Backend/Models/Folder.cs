using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MailKit;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models
{
    [Index(nameof(Path), nameof(MailBoxId), IsUnique = true)]
    public class Folder
    {
        [Key]
        public int Id { get; set; }
        public string Name => this.Path.Split('/', StringSplitOptions.RemoveEmptyEntries
                                                  |StringSplitOptions.TrimEntries)
                                            .Last();
        public string Path { get; set; } = string.Empty;
        public Folder? Parent { get; set; } = null;
        public ICollection<Folder> Children { get; set; } = [];
        [ReadOnly(true)]
        public int MailBoxId { get; set; }
        [ReadOnly(true)]
        [JsonIgnore]
        public MailBox? MailBox { get; set; } = null;
        [JsonIgnore]
        public ICollection<Mail> Mails { get; set; } = [];
        public UniqueId LastPulledUid { get; set; } = UniqueId.MinValue;
        public DateTime LastPulledInternalDate { get; set; } = DateTime.MinValue;

        public Folder(){}
        public Folder(IMailFolder folder){
            this.Path = folder.FullName;
        }
    }
}