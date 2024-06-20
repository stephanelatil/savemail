using System.ComponentModel.DataAnnotations;
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
        private List<Mail> Mails { get; set; } = [];
    }
}