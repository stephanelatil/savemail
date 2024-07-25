using System.Data;
using Backend.Models;
using Backend.Models.DTO;
using MailKit;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public interface IMailService
    {
        public Task SaveMail(List<Mail> mail, CancellationToken cancellationToken = default);
        public Task<Mail?> GetMail(long id);
        public Task<Mail?> GetMail(UniqueId id);
        public Task DeleteMailAsync(Mail mail);
    }

    public class MailService : IMailService
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<MailService> _logger;

        public MailService(ApplicationDBContext context,
                           ILogger<MailService> logger)
        {
            this._context = context;
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
        
        private EmailAddress GetOrCreateEmailAddresses(EmailAddress address)
        {
            //try to get address from change tracker
            var addr = this._context.ChangeTracker.Entries<EmailAddress>()
                                                    .Where(e => e.Entity.Address == address.Address)
                                                    .Select(e => e.Entity)
                                                    .SingleOrDefault();
            //not in change tracker try to find it in the database
            addr ??= this._context.EmailAddress.SingleOrDefault(e => e.Address == address.Address);
            //not in Db => track new 
            return addr ?? this._context.EmailAddress.Add(address).Entity;
        }

        private void HandleEmailAddresses(Mail mail)
        {
            mail.Sender = this.GetOrCreateEmailAddresses(mail.Sender ?? new EmailAddress(){Address="UNKNOWN"});
            mail.Recipients = mail.Recipients.Select(this.GetOrCreateEmailAddresses).ToList();
            mail.RecipientsCc = mail.RecipientsCc.Select(this.GetOrCreateEmailAddresses).ToList();
        }

        public async Task SaveMail(List<Mail> mails, CancellationToken cancellationToken = default)
        {
            this._logger.LogDebug($"Adding {mails.Count} emails");
            foreach(var mail in mails)
            {
                if (mail.ImapReplyFromId is not null){
                    mail.RepliedFrom = mails.Where(m => m.OwnerMailBoxId == mail.OwnerMailBoxId)
                                                        .SingleOrDefault(x => mail.ImapReplyFromId == x.ImapMailId)
                                     ?? await this._context.Mail.Where(m => m.OwnerMailBoxId == mail.OwnerMailBoxId)
                                                        .SingleOrDefaultAsync(x => mail.ImapReplyFromId == x.ImapMailId, cancellationToken);
                }
                this.HandleEmailAddresses(mail);
            }
            await this._context.Mail.AddRangeAsync(mails, cancellationToken);
            await this._context.SaveChangesAsync(cancellationToken);
        }
    }
}