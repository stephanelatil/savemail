using System.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using MailKit;

namespace Backend.Services
{
    public interface IFolderService
    {
        public Task<Folder?> GetFolderByIdAsync(int id,
                                            CancellationToken cancellationToken=default);
        public Task<Folder> CreateFolderAsync(Folder folder, MailBox mailbox,
                                            CancellationToken cancellationToken=default);
        public Task UpdateLastPullDataAsync(Folder folder, UniqueId lastMailUid, DateTimeOffset lastMailDate,
                                            CancellationToken cancellationToken=default);
        public Task DeleteFolderAsync(Folder folder,
                                            CancellationToken cancellationToken=default);
    }

    public class FolderService : IFolderService
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<FolderService> _logger;

        public FolderService(ApplicationDBContext context,
                             ILogger<FolderService> logger)
        {
            this._context = context;
            this._logger = logger;
        }

        public async Task<Folder?> GetFolderByIdAsync(int id, CancellationToken cancellationToken=default)
        {
            return await this._context.Folder.Where(f => f.Id == id)
                                            .SingleOrDefaultAsync(cancellationToken:cancellationToken);
        }

        private async Task<Folder> CreateFolderAsync(Folder folder, MailBox mailbox, bool save,
                                            CancellationToken cancellationToken=default)
        {
            ArgumentNullException.ThrowIfNull(folder);
            ArgumentNullException.ThrowIfNull(folder.Path);
            ArgumentNullException.ThrowIfNull(mailbox);
            if (cancellationToken.IsCancellationRequested)
                return folder;
            Folder? existing = mailbox.Folders.Where(f => f.Path == folder.Path).FirstOrDefault();

            this._logger.LogDebug($"Looking for folder '{folder.Path}' im mailbox folders: {string.Join(",", mailbox.Folders.Select(f=> $"{f.Id}: {f.Name}"))}");
            if (existing is not null)
            {
                return existing;
            }
            
            Folder? parent = null;
            if (folder.Path.Contains('/'))
            {
                string parentPath = folder.Path[0..folder.Path.LastIndexOf('/')];
                parent = await this.CreateFolderAsync(new Folder(){
                    Path = parentPath,
                    MailBoxId = mailbox.Id
                }, mailbox, false, cancellationToken); //do not save yet wait to add all elements in the tree
            }

            folder.Parent = parent;
            folder.MailBoxId = mailbox.Id; //ensure mailbox parent is set
            var newFolder = this._context.Folder.Add(folder);
            newFolder.State = EntityState.Added;
            this._logger.LogDebug($"Created new folder {newFolder.Entity.Path}");

            if (cancellationToken.IsCancellationRequested)
                return folder;
            if (save)
            {
                if (await this._context.SaveChangesAsync(cancellationToken) == 0) 
                    throw new DbUpdateException();
                this._logger.LogDebug("Folders saved");
            }
            return newFolder.Entity;
        }

        /// <summary>
        /// Recursively creates the folder tree and saves to DB once all folders have been created
        /// </summary>
        /// <param name="folder">The folder with the path to add</param>
        /// <param name="mailbox">The mailbox where the folder resides</param>
        /// <returns>The added folder with </returns>
        /// <exception cref="DbUpdateException">If saving to the database fails</exception>
        /// <exception cref="DbUpdateConcurrencyException">If saving to the database fails due to a concurrent save</exception>
        public async Task<Folder> CreateFolderAsync(Folder folder, MailBox mailbox,
                                            CancellationToken cancellationToken=default)
        {
            ArgumentNullException.ThrowIfNull(mailbox);
            folder.MailBoxId = mailbox.Id;
            mailbox = await this._context.MailBox
                                                .Where(mb =>mb.Id == mailbox.Id)
                                                .Include(mb => mb.Folders)
                                                .SingleOrDefaultAsync(cancellationToken)
                                ?? throw new ArgumentNullException(nameof(mailbox));
            return await this.CreateFolderAsync(folder, mailbox, true, cancellationToken);
        }

        public async Task DeleteFolderAsync(Folder folder,
                                            CancellationToken cancellationToken=default)
        {
            ArgumentNullException.ThrowIfNull(folder);

            this._context.Folder.Remove(folder);
            if (await this._context.SaveChangesAsync(cancellationToken) == 0)
                throw new DbUpdateException();
        }

        public async Task UpdateLastPullDataAsync(Folder folder,
                                            UniqueId lastMailUid,
                                            DateTimeOffset lastMailDate,
                                            CancellationToken cancellationToken=default)
        {
            this._context.TrackEntry(folder);
            if (folder.LastPulledUid < lastMailUid)
                folder.LastPulledUid = lastMailUid;
            if (folder.LastPulledUid < lastMailUid)
                folder.LastPulledUid = lastMailUid;

            await this._context.SaveChangesAsync(cancellationToken);
        }
    }
}