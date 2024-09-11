using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MailKit;
using Backend.Utils;

namespace Backend.Models
{
    public class ApplicationDBContext : IdentityDbContext<AppUser>
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        { }
        public ApplicationDBContext()
            : base(new DbContextOptionsBuilder().UseInMemoryDatabase("TestDb").Options)
        { }

        public virtual void TrackEntry(Mail mail) => this.Mail.Entry(mail);
        public virtual void TrackEntry(MailBox mailbox) => this.MailBox.Entry(mailbox);
        public virtual void TrackEntry(AppUser user) => this.Users.Entry(user);
        public virtual void TrackEntry(Folder folder) => this.Folder.Entry(folder);
        public virtual void TrackEntry(Attachment attachment) => this.Attachment.Entry(attachment);
        public virtual void TrackEntry(EmailAddress eAddress) => this.EmailAddress.Entry(eAddress);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasPostgresExtension("pg_trgm").HasPostgresExtension("btree_gin");

            modelBuilder.Entity<Mail>().HasOne(m => m.Sender).WithMany(em => em.MailsSent);
            modelBuilder.Entity<Mail>().HasMany(m => m.Recipients).WithMany(em => em.MailsReceived);
            modelBuilder.Entity<Mail>().HasMany(m => m.RecipientsCc).WithMany(em => em.MailsCCed);
            modelBuilder.Entity<Mail>().HasOne(m => m.RepliedFrom).WithMany(m => m.Replies);

            // Create composite GIN index on Mails
            modelBuilder.Entity<Mail>()
                .HasIndex(m => new { m.Subject, m.Body })
                .HasMethod("GIN")
                .HasOperators("gin_trgm_ops")
                .IsCreatedConcurrently();

            // Ensure hash is calculated and stored
            modelBuilder
                .Entity<Mail>()
                .Property(e => e.UniqueHash)
                .UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);

            modelBuilder.Entity<OAuthCredentials>().HasOne(c => c.OwnerMailbox)
                                                   .WithOne(mb => mb.OAuthCredentials)
                                                   .HasForeignKey<OAuthCredentials>(c => c.OwnerMailboxId)
                                                   .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<MailBox>().HasOne(mb => mb.OAuthCredentials)
                                          .WithOne(c => c.OwnerMailbox)
                                          .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MailBox>().Navigation(mb => mb.OAuthCredentials).AutoInclude();
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);
            configurationBuilder.Properties<UniqueId>()
                                .HaveConversion<UniqueIdConverter>();
        }

        public virtual DbSet<Attachment> Attachment { get; set; }
        public virtual DbSet<EmailAddress> EmailAddress { get; set; }
        public virtual DbSet<Folder> Folder { get; set; }
        public virtual DbSet<Mail> Mail { get; set; }
        public virtual DbSet<MailBox> MailBox { get; set; }
        public virtual DbSet<OAuthCredentials> OAuthCredentials{ get; set; }
    }
}