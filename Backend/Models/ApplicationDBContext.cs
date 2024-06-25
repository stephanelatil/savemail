using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Models
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Mail>().HasOne(m => m.Sender).WithMany(em => em.MailsSent);
            modelBuilder.Entity<Mail>().HasMany(m => m.Recipients).WithMany(em => em.MailsReceived);
            modelBuilder.Entity<Mail>().HasMany(m => m.RecipientsCc).WithMany(em => em.MailsCCed);
            modelBuilder.Entity<Mail>().HasMany(m => m.RecipientsBcc).WithMany(em => em.MailsBCCed);

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

        public DbSet<Attachment> Attachment { get; set; }
        public DbSet<EmailAddress> EmailAddress { get; set; }
        public DbSet<Folder> Folder { get; set; }
        public DbSet<Mail> Mail { get; set; }
        public DbSet<MailBox> MailBoxes { get; set; }
        public DbSet<Secret> Secret { get; set; }
        public DbSet<UserData> UserData { get; set; }
    }
}