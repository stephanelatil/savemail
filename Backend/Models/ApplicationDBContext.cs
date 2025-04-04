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
            modelBuilder.HasPostgresExtension("btree_gin");

            modelBuilder.Entity<Mail>().HasOne(m => m.Sender).WithMany(em => em.MailsSent);
            modelBuilder.Entity<Mail>().HasMany(m => m.Recipients).WithMany(em => em.MailsReceived);
            modelBuilder.Entity<Mail>().HasMany(m => m.RecipientsCc).WithMany(em => em.MailsCCed);
            modelBuilder.Entity<Mail>().HasOne(m => m.RepliedFrom).WithOne(m => m.Reply)
                                       .OnDelete(DeleteBehavior.Cascade).HasForeignKey<Mail>(m => m.RepliedFromId);
            modelBuilder.Entity<Mail>().HasOne(m => m.Reply).WithOne(m => m.RepliedFrom)
                                       .OnDelete(DeleteBehavior.Cascade).HasForeignKey<Mail>(m => m.ReplyId);
            modelBuilder.Entity<Mail>().Navigation(m => m.Sender).AutoInclude();
            modelBuilder.Entity<Mail>().Navigation(m => m.Recipients).AutoInclude();
            modelBuilder.Entity<Mail>().Navigation(m => m.RecipientsCc).AutoInclude();
            modelBuilder.Entity<Mail>().HasMany(m => m.Attachments).WithOne(a => a.Mail)
                                        .HasForeignKey(a => a.MailId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<MailBox>().HasMany(mb => mb.Folders).WithOne(f => f.MailBox)
                                        .HasForeignKey(f =>f.MailBoxId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<MailBox>().HasMany(mb => mb.Mails).WithOne(m => m.OwnerMailBox)
                                        .HasForeignKey(m => m.OwnerMailBoxId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Attachment>().HasOne(a => a.Owner).WithMany() //there is no navigation property in AppUser
                                        .HasForeignKey(a => a.OwnerId).OnDelete(DeleteBehavior.Restrict); // Prevent deletion of User
            modelBuilder.Entity<Folder>().HasMany(f => f.Mails).WithMany();

            // Create composite GIN index on Mails
            modelBuilder.Entity<Mail>()
                .HasGeneratedTsVectorColumn(
                    m => m.SearchVector,
                    "english",
                    m => new {m.Subject, m.BodyText}
                ).HasIndex(m => m.SearchVector)
                .HasMethod("GIN");
                // .Property(m => m.SearchVector)
                // .HasColumnType("tsvector")
                // .HasComputedColumnSql("to_tsvector('english', coalesce(\"Subject\", '') || ' ' || coalesce(\"BodyText\", ''))", stored: true);

            // Create GIN index on the computed tsvector column
            modelBuilder.Entity<Mail>()
                .HasIndex(m => m.SearchVector)
                .HasMethod("GIN");

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
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);
            configurationBuilder.Properties<UniqueId>()
                                .HaveConversion<UniqueIdConverter>();
            configurationBuilder.Properties<DateTime>()
                                .HaveConversion<DateTimeConverter>();
        }

        public virtual DbSet<Attachment> Attachment { get; set; }
        public virtual DbSet<EmailAddress> EmailAddress { get; set; }
        public virtual DbSet<Folder> Folder { get; set; }
        public virtual DbSet<Mail> Mail { get; set; }
        public virtual DbSet<MailBox> MailBox { get; set; }
        public virtual DbSet<OAuthCredentials> OAuthCredentials{ get; set; }
    }
}