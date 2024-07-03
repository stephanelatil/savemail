using System.Data;
using Backend.Models;
using Backend.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public interface IMailBoxService
    {
        public Task<MailBox?> GetMailboxByIdAsync(int id);
        public Task UpdateMailBoxAsync(int id, UpdateMailBox? user);
        public Task<MailBox> CreateMailBoxAsync(UpdateMailBox mailbox, AppUser owner);
        public Task DeleteMailBoxAsync(MailBox mailbox);
    }

    public class MailBoxService : IMailBoxService
    {
        private readonly ApplicationDBContext _context;

        public MailBoxService(ApplicationDBContext context)
        {
            this._context = context;
        }

        public async Task<MailBox?> GetMailboxByIdAsync(int id)
        {
            return await this._context.MailBox.FindAsync(id);
        }

        public async Task UpdateMailBoxAsync(int id, UpdateMailBox? updateMailBox)
        {
            ArgumentNullException.ThrowIfNull(updateMailBox);

            MailBox? mailBox = await this.GetMailboxByIdAsync(id);
            if (mailBox is null)
                throw new KeyNotFoundException();
            mailBox = this._context.MailBox.Entry(mailBox).Entity;
            
            mailBox.Username = updateMailBox.Username ?? mailBox.Username;
            mailBox.Password = updateMailBox.Password ?? mailBox.Password;
            mailBox.ImapDomain = updateMailBox.ImapDomain ?? mailBox.ImapDomain;
            mailBox.ImapPort = updateMailBox.ImapPort ?? mailBox.ImapPort;
            mailBox.Provider = updateMailBox.Provider ?? mailBox.Provider;

            if (await this._context.SaveChangesAsync() == 0)
                throw new DbUpdateException();
        }


        public async Task<MailBox> CreateMailBoxAsync(UpdateMailBox mailbox, AppUser owner)
        {
            ArgumentNullException.ThrowIfNull(mailbox);
            ArgumentNullException.ThrowIfNull(mailbox.Password);
            ArgumentNullException.ThrowIfNull(mailbox.ImapPort);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(mailbox.ImapDomain);

            MailBox newmb = new(){
                Username = mailbox.Username ?? string.Empty,
                Password = mailbox.Password,
                ImapDomain = mailbox.ImapDomain,
                ImapPort = mailbox.ImapPort.Value,
                Provider = mailbox.Provider ?? ImapProvider.Simple,
                Owner = owner
            };

            var entry =  await this._context.MailBox.AddAsync(newmb);
            if (await this._context.SaveChangesAsync() == 0) 
                throw new DbUpdateException();
            return entry.Entity;
        }

        public async Task DeleteMailBoxAsync(MailBox mailbox)
        {
            ArgumentNullException.ThrowIfNull(mailbox);

            this._context.MailBox.Remove(mailbox);
            if (await this._context.SaveChangesAsync() == 0)
                throw new DbUpdateException();
        }
    }
}