using System.Data;
using Backend.Models;
using Backend.Models.DTO;
using MailKit;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public interface IMailService
    {
        public Task SaveMail(Mail mail);
        public Task<Mail?> GetMail(long id);
        public Task<Mail?> GetMail(UniqueId id);
        public Task DeleteMailAsync(Mail mail);
    }

    public class MailService : IMailService
    {
        private readonly ApplicationDBContext _context;

        public MailService(ApplicationDBContext context)
        {
            this._context = context;
        }

        public async Task<Mail?> GetMail(long id)
        {
            return await this._context.Mail.FindAsync(id);
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

        public async Task SaveMail(Mail mail)
        {
            //Link the parent email if this is a reply
            mail.RepliedFrom = mail.OwnerMailBox?.Mails.SingleOrDefault(x => mail.ImapReplyFromId == x.ImapMailId);
            await this._context.AddAsync(mail);
            await this._context.SaveChangesAsync();
        }
    }
}