using Microsoft.EntityFrameworkCore;

namespace Backend.Models
{
    class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Create composite GIN index on Mails
            modelBuilder.Entity<Mail>()
                .HasIndex(p => new { p.Subject, p.Body })
                .HasMethod("GIN")
                .HasOperators("gin_trgm_ops")
                .IsCreatedConcurrently();
            // Create composite GIN index on Mails
            modelBuilder.Entity<EmailAddress>()
                .HasIndex(p => new { p.FullName, p.FullAddress })
                .HasMethod("GIN")
                .HasOperators("gin_trgm_ops")
                .IsCreatedConcurrently();
        }

        public DbSet<MailBox> MailBoxes { get; set; }
        public DbSet<Mail> Mails { get; set; }
        public DbSet<EmailAddress> EmailAddresses { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<UserData> UserDatas { get; set; }
    }
}