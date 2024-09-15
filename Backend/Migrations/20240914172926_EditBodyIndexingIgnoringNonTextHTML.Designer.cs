﻿// <auto-generated />
using System;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

#nullable disable

namespace Backend.Migrations
{
    [DbContext(typeof(ApplicationDBContext))]
    [Migration("20240914172926_EditBodyIndexingIgnoringNonTextHTML")]
    partial class EditBodyIndexingIgnoringNonTextHTML
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "btree_gin");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Backend.Models.AppUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Backend.Models.Attachment", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("FileSize")
                        .HasColumnType("bigint");

                    b.Property<long?>("MailId")
                        .HasColumnType("bigint");

                    b.Property<string>("OwnerId")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("MailId");

                    b.HasIndex("OwnerId");

                    b.ToTable("Attachment");
                });

            modelBuilder.Entity("Backend.Models.EmailAddress", b =>
                {
                    b.Property<string>("Address")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("FullName")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.HasKey("Address");

                    b.HasIndex("Address")
                        .IsUnique();

                    b.ToTable("EmailAddress");
                });

            modelBuilder.Entity("Backend.Models.Folder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<long>("LastPulledInternalDate")
                        .HasColumnType("bigint");

                    b.Property<ulong>("LastPulledUid")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("MailBoxId")
                        .HasColumnType("integer");

                    b.Property<int?>("ParentId")
                        .HasColumnType("integer");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("MailBoxId");

                    b.HasIndex("ParentId");

                    b.HasIndex("Path", "MailBoxId")
                        .IsUnique();

                    b.ToTable("Folder");
                });

            modelBuilder.Entity("Backend.Models.Mail", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Body")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("BodyText")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("DateSent")
                        .HasColumnType("bigint");

                    b.Property<int>("FolderId")
                        .HasColumnType("integer");

                    b.Property<string>("ImapMailId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<ulong>("ImapMailUID")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int?>("OwnerMailBoxId")
                        .IsRequired()
                        .HasColumnType("integer");

                    b.Property<long?>("RepliedFromId")
                        .HasColumnType("bigint");

                    b.Property<NpgsqlTsVector>("SearchVector")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("tsvector")
                        .HasComputedColumnSql("to_tsvector('english', coalesce(\"Subject\", '') || ' ' || coalesce(\"BodyText\", ''))", true);

                    b.Property<string>("SenderAddress")
                        .HasColumnType("character varying(256)");

                    b.Property<string>("Subject")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("UniqueHash")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("FolderId");

                    b.HasIndex("OwnerMailBoxId");

                    b.HasIndex("RepliedFromId");

                    b.HasIndex("SearchVector");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("SearchVector"), "GIN");

                    b.HasIndex("SenderAddress");

                    b.HasIndex("UniqueHash");

                    b.ToTable("Mail");
                });

            modelBuilder.Entity("Backend.Models.MailBox", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ImapDomain")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<short>("ImapPort")
                        .HasColumnType("smallint");

                    b.Property<string>("OwnerId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Provider")
                        .HasColumnType("integer");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("MailBox");
                });

            modelBuilder.Entity("Backend.Models.OAuthCredentials", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("AccessToken")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("AccessTokenValidity")
                        .HasColumnType("bigint");

                    b.Property<bool>("NeedReAuth")
                        .HasColumnType("boolean");

                    b.Property<int>("OwnerMailboxId")
                        .HasColumnType("integer");

                    b.Property<int>("Provider")
                        .HasColumnType("integer");

                    b.Property<string>("RefreshToken")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("OwnerMailboxId")
                        .IsUnique();

                    b.ToTable("OAuthCredentials");
                });

            modelBuilder.Entity("EmailAddressMail", b =>
                {
                    b.Property<long>("MailsReceivedId")
                        .HasColumnType("bigint");

                    b.Property<string>("RecipientsAddress")
                        .HasColumnType("character varying(256)");

                    b.HasKey("MailsReceivedId", "RecipientsAddress");

                    b.HasIndex("RecipientsAddress");

                    b.ToTable("EmailAddressMail");
                });

            modelBuilder.Entity("EmailAddressMail1", b =>
                {
                    b.Property<long>("MailsCCedId")
                        .HasColumnType("bigint");

                    b.Property<string>("RecipientsCcAddress")
                        .HasColumnType("character varying(256)");

                    b.HasKey("MailsCCedId", "RecipientsCcAddress");

                    b.HasIndex("RecipientsCcAddress");

                    b.ToTable("EmailAddressMail1");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("text");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .HasColumnType("text");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("Backend.Models.Attachment", b =>
                {
                    b.HasOne("Backend.Models.Mail", "Mail")
                        .WithMany("Attachments")
                        .HasForeignKey("MailId");

                    b.HasOne("Backend.Models.AppUser", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId");

                    b.Navigation("Mail");

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Backend.Models.Folder", b =>
                {
                    b.HasOne("Backend.Models.MailBox", "MailBox")
                        .WithMany("Folders")
                        .HasForeignKey("MailBoxId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend.Models.Folder", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("ParentId");

                    b.Navigation("MailBox");

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("Backend.Models.Mail", b =>
                {
                    b.HasOne("Backend.Models.Folder", "Folder")
                        .WithMany("Mails")
                        .HasForeignKey("FolderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend.Models.MailBox", "OwnerMailBox")
                        .WithMany("Mails")
                        .HasForeignKey("OwnerMailBoxId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend.Models.Mail", "RepliedFrom")
                        .WithMany("Replies")
                        .HasForeignKey("RepliedFromId");

                    b.HasOne("Backend.Models.EmailAddress", "Sender")
                        .WithMany("MailsSent")
                        .HasForeignKey("SenderAddress");

                    b.Navigation("Folder");

                    b.Navigation("OwnerMailBox");

                    b.Navigation("RepliedFrom");

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("Backend.Models.MailBox", b =>
                {
                    b.HasOne("Backend.Models.AppUser", "Owner")
                        .WithMany("MailBoxes")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Backend.Models.OAuthCredentials", b =>
                {
                    b.HasOne("Backend.Models.MailBox", "OwnerMailbox")
                        .WithOne("OAuthCredentials")
                        .HasForeignKey("Backend.Models.OAuthCredentials", "OwnerMailboxId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("OwnerMailbox");
                });

            modelBuilder.Entity("EmailAddressMail", b =>
                {
                    b.HasOne("Backend.Models.Mail", null)
                        .WithMany()
                        .HasForeignKey("MailsReceivedId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend.Models.EmailAddress", null)
                        .WithMany()
                        .HasForeignKey("RecipientsAddress")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("EmailAddressMail1", b =>
                {
                    b.HasOne("Backend.Models.Mail", null)
                        .WithMany()
                        .HasForeignKey("MailsCCedId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend.Models.EmailAddress", null)
                        .WithMany()
                        .HasForeignKey("RecipientsCcAddress")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Backend.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Backend.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Backend.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Backend.Models.AppUser", b =>
                {
                    b.Navigation("MailBoxes");
                });

            modelBuilder.Entity("Backend.Models.EmailAddress", b =>
                {
                    b.Navigation("MailsSent");
                });

            modelBuilder.Entity("Backend.Models.Folder", b =>
                {
                    b.Navigation("Children");

                    b.Navigation("Mails");
                });

            modelBuilder.Entity("Backend.Models.Mail", b =>
                {
                    b.Navigation("Attachments");

                    b.Navigation("Replies");
                });

            modelBuilder.Entity("Backend.Models.MailBox", b =>
                {
                    b.Navigation("Folders");

                    b.Navigation("Mails");

                    b.Navigation("OAuthCredentials");
                });
#pragma warning restore 612, 618
        }
    }
}