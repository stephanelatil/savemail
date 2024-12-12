using System.Data;
using Backend.Models;
using Backend.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public interface IMailBoxService
    {
        public Task<MailBox?> GetMailboxByIdAsync(int id, bool includeFolders=false);
        public Task UpdateMailBoxAsync(int id, UpdateMailBox? user);
        public Task<MailBox> CreateMailBoxAsync(UpdateMailBox mailbox, AppUser owner);
        public Task DeleteMailBoxAsync(MailBox mailbox);
    }

    public class MailBoxService : IMailBoxService
    {
        private readonly ApplicationDBContext _context;
        private readonly ITokenEncryptionService _tokenEncryptionService;
        private readonly string _attachmentPath;

        public MailBoxService(ApplicationDBContext context,
                                ITokenEncryptionService tokenEncryptionService,
                                IConfiguration configuration)
        {
            this._tokenEncryptionService = tokenEncryptionService;
            this._context = context;
            this._attachmentPath = configuration.GetValue<string>("AttachmentsPath") ?? "./Attachments";
        }

        public async Task<MailBox?> GetMailboxByIdAsync(int id, bool includeFolders=false)
        {
            var query =  this._context.MailBox.Where(x => x.Id == id);
            if (includeFolders)
                query = query.Include(mb => mb.Folders)
                             .AsSplitQuery();
            return await query.FirstOrDefaultAsync();
        }

        public async Task UpdateMailBoxAsync(int id, UpdateMailBox? updateMailBox)
        {
            ArgumentNullException.ThrowIfNull(updateMailBox);

            MailBox? mailBox = await this.GetMailboxByIdAsync(id) ?? throw new KeyNotFoundException();
            this._context.TrackEntry(mailBox);
            
            mailBox.Username = updateMailBox.Username ?? mailBox.Username;
            mailBox.Password = updateMailBox.Password is null ? mailBox.Password
                                            : this._tokenEncryptionService.Encrypt(updateMailBox.Password, mailBox.Id, mailBox.OwnerId);
            mailBox.ImapDomain = updateMailBox.ImapDomain ?? mailBox.ImapDomain;
            mailBox.ImapPort = updateMailBox.ImapPort ?? mailBox.ImapPort;

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
                OwnerId = owner.Id
            };

            await this._context.MailBox.AddAsync(newmb);
            if (await this._context.SaveChangesAsync() == 0) 
                throw new DbUpdateException();
            newmb.Password = this._tokenEncryptionService.Encrypt(newmb.Password, newmb.Id, newmb.OwnerId);
            this._context.MailBox.Update(newmb);
            await this._context.SaveChangesAsync();
            return newmb;
        }

        public async Task DeleteMailBoxAsync(MailBox mailbox)
        {
            ArgumentNullException.ThrowIfNull(mailbox);

            //Also delete all attachments
            string attachmentsPath = Path.Join(this._attachmentPath, mailbox.OwnerId, mailbox.Id.ToString());
            if (Directory.Exists(attachmentsPath))
                Directory.Delete(attachmentsPath, true);

            this._context.MailBox.Remove(mailbox);
            if (await this._context.SaveChangesAsync() == 0)
                throw new DbUpdateException();
        }
    }
}