using System.Data;
using Backend.Models;
using MailKit;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public interface IMailService
    {
        public Task SaveMail(List<Mail> mails, string ownerUserId, CancellationToken cancellationToken = default);
        public Task<Mail?> GetMail(long id);
        public Task<Mail?> GetMail(UniqueId id);
        public Task DeleteMailAsync(Mail mail);
    }

    public class MailService : IMailService
    {
        private readonly ApplicationDBContext _context;
        private readonly IAttachmentService _attachmentService;
        private readonly ILogger<MailService> _logger;

        public MailService(ApplicationDBContext context,
                           IAttachmentService attachmentService,
                           ILogger<MailService> logger)
        {
            this._context = context;
            this._attachmentService = attachmentService;
            this._logger = logger;
        }

        public async Task<Mail?> GetMail(long id)
        {
            return await this._context.Mail.Where(x=> x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Mail?> GetMail(UniqueId id)
        {
            return await this._context.Mail.SingleOrDefaultAsync(x => id == x.ImapMailUID);
        }

        public async Task DeleteMailAsync(Mail mail)
        {
            this._context.Mail.Remove(mail);
            if (await this._context.SaveChangesAsync() == 0)
                throw new DbUpdateException("Unable to delete. Please try again");
        }
        
        private async Task<EmailAddress> GetOrCreateEmailAddresses(EmailAddress address)
        {
            //Check in change tracker or in the database
            EmailAddress? addr = this._context.ChangeTracker.Entries<EmailAddress>()
                                                        .FirstOrDefault(e => e.Entity.Address == address.Address)?.Entity
                                ?? await this._context.EmailAddress.Where(e => e.Address == address.Address)
                                                        .SingleOrDefaultAsync();
            //not in Db or change-tracker => track new 
            return addr ?? this._context.EmailAddress.Add(address).Entity;
        }

        private async Task HandleEmailAddresses(Mail mail)
        {
            mail.Sender = await this.GetOrCreateEmailAddresses(mail.Sender ?? new EmailAddress(){Address="UNKNOWN"});
            List<EmailAddress> tmp = [];
            foreach (var addr in mail.Recipients)
                tmp.Add(await this.GetOrCreateEmailAddresses(addr));
            mail.Recipients = tmp;
            tmp = [];
            foreach (var addr in mail.RecipientsCc)
                tmp.Add(await this.GetOrCreateEmailAddresses(addr));
            mail.RecipientsCc = tmp;
        }

        private async Task InsertReply(Mail reply, Mail? parent)
        {
            if (parent is null)
                return;
            if (parent.HasReply && parent.Reply is not null)
                //keep going down the reply list (in chronological order) until the bottom
                await this.InsertReply(reply, 
                        await this._context.Mail.Where(m => m.Id == parent.Reply.Id)
                                                .Include(m => m.Reply)
                                                .AsSplitQuery()
                                                .SingleOrDefaultAsync());
            else
            {
                //bottom of reply list hit :
                parent.HasReply = true;
                parent.Reply = reply;
                this._context.TrackEntry(parent);
                reply.RepliedFrom = parent;
            }
        }

        /// <summary>
        /// Checks all given mails and returns the mails that aren't present in the database
        /// Those present in the DBs have their Id field set
        /// </summary>
        /// <param name="mails">A list of mails you'd like to add</param>
        /// <returns>The Mails that don't yet exist in the DB</returns>
        private async Task<List<Mail>> GetMailsToAdd(List<Mail> mails){
            var uniqueHashes1 = mails.Select(m => m.UniqueHash);

            var existingMailsDict = await this._context.Mail
                .Where(m => uniqueHashes1.Any(u => u == m.UniqueHash))
                .Select(m => new { m.Id, m.UniqueHash, m.UniqueHash2})
                .ToListAsync();

            foreach (var mail in mails)
            {
                foreach (var existingMail in existingMailsDict
                                .Where(m => mail.UniqueHash == m.UniqueHash && m.UniqueHash2 == mail.UniqueHash2))
                {
                    var collisionCheck = await this._context.Mail.Where(m => m.Id == existingMail.Id)
                                                .Select(m => new {m.DateSent, m.Subject, m.Body})
                                                .FirstOrDefaultAsync();
                    //better duplicate detection in case of collision (very unlikely)
                    if (collisionCheck is not null
                            && mail.DateSent == collisionCheck.DateSent
                            && mail.Subject == collisionCheck.Subject
                            && mail.Body == collisionCheck.Body)
                        mail.Id = existingMail.Id;
                }
            }

            // Return mails that are not in the database (Id is still 0)
            return mails.Where(m => m.Id == 0).ToList();
        }

        public async Task SaveMail(List<Mail> mails, string ownerUserId, CancellationToken cancellationToken = default)
        {
            this._logger.LogDebug("Adding {} emails", mails.Count);
            foreach(var mail in mails)
            {
                if (mail.ImapReplyFromId is not null){
                    mail.RepliedFrom = mails.Where(m => m.OwnerMailBoxId == mail.OwnerMailBoxId)
                                                        .SingleOrDefault(x => mail.ImapReplyFromId == x.ImapMailId)
                                     ?? this._context.ChangeTracker.Entries<Mail>()
                                                        .Where(m => m.Entity.OwnerMailBoxId == mail.OwnerMailBoxId &&
                                                                mail.ImapReplyFromId == m.Entity.ImapMailId)
                                                        .SingleOrDefault()?.Entity
                                     ?? await this._context.Mail.Include(m => m.Reply).AsSplitQuery()
                                                        .Where(m => m.OwnerMailBoxId == mail.OwnerMailBoxId)
                                                        .SingleOrDefaultAsync(x => mail.ImapReplyFromId == x.ImapMailId, cancellationToken);
                    if (mail.RepliedFrom is not null)
                        await this.InsertReply(mail, mail.RepliedFrom);
                }
                await this.HandleEmailAddresses(mail);
            }
            var newMails = await this.GetMailsToAdd(mails);
            await this._context.Mail.AddRangeAsync(newMails, cancellationToken);
            await this._context.SaveChangesAsync(cancellationToken); 
            //All Ids are set and mail saved
            //Add attachments to *new* emails only
            await this._attachmentService.SaveAttachments(newMails, ownerUserId);
        }
    }
}