using Microsoft.EntityFrameworkCore;

namespace Backend.Models
{
    class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions options)
            : base(options)
        { }

        public DbSet<MailBox> MailBoxes { get; set; }
        public DbSet<Mail> Mails { get; set; }
        public DbSet<EmailAddress> EmailAddresses { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<UserData> UserDatas { get; set; }
    }
}