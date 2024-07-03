using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models
{
    [Index(nameof(Key), IsUnique=true)]
    public class ConfigKV
    {
        [Key]
        [ReadOnly(true)]
        public string Key { get; set; } = string.Empty;
        public string? Value { get; set; } = null;
    }

}