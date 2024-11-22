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
            //not in change tracker try to find it in the database
            EmailAddress? addr = await this._context.EmailAddress.FirstOrDefaultAsync(e => e.Address == address.Address);
            //not in Db => track new 
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
                this._context.Mail.Update(parent);
                reply.RepliedFrom = parent;
            }
        }

        /// <summary>
        /// Checks all given mails and returns the mails that aren't present in the database
        /// Those present in the DBs have their Id field set
        /// </summary>
        /// <param name="mails">A list of mils you'd like to add</param>
        /// <returns>The Mails that don't yet exist in the DB</returns>
        private List<Mail> GetMailsToAdd(List<Mail> mails){
            var uniqueHashes = mails.Select(m => new { m.UniqueHash, m.UniqueHash2 }).ToList();

            var existingMailsDict = this._context.Mail
                .Where(m => uniqueHashes.Any(u => u.UniqueHash == m.UniqueHash && u.UniqueHash2 == m.UniqueHash2))
                .Select(m => new { m.Id, m.UniqueHash, m.UniqueHash2 })
                .ToList().ToDictionary(
                m => (m.UniqueHash, m.UniqueHash2),
                m => m.Id);

            foreach (var mail in mails)
            {
                if (existingMailsDict.TryGetValue((mail.UniqueHash, mail.UniqueHash2), out var id))
                {
                    mail.Id = id;
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
                                     ?? await this._context.Mail.Include(m => m.Reply).AsSplitQuery()
                                                        .Where(m => m.OwnerMailBoxId == mail.OwnerMailBoxId)
                                                        .SingleOrDefaultAsync(x => mail.ImapReplyFromId == x.ImapMailId, cancellationToken);
                    if (mail.RepliedFrom is not null)
                        await this.InsertReply(mail, mail.RepliedFrom);
                }
                await this.HandleEmailAddresses(mail);
            }
            var newMails = this.GetMailsToAdd(mails);
            await this._context.Mail.AddRangeAsync(newMails, cancellationToken);
            await this._context.SaveChangesAsync(cancellationToken); 
            //All Ids are set and mail saved
            //Add attachments to new emails
            await this._attachmentService.SaveAttachments(newMails, ownerUserId);
        }
    }
}