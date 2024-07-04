using System.Data;
using Backend.Models;
using Backend.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public interface IFolderService
    {
        public Task<Folder?> GetFolderByIdAsync(int id);
        public Task<Folder> CreateFolderAsync(Folder folder, AppUser owner);
        public Task DeleteFolderAsync(Folder folder);
    }

    public class FolderService : IFolderService
    {
        private readonly ApplicationDBContext _context;

        public FolderService(ApplicationDBContext context)
        {
            this._context = context;
        }

        public async Task<Folder?> GetFolderByIdAsync(int id)
        {
            return await this._context.Folder.FindAsync(id);
        }

        private async Task<Folder> CreateFolderAsync(Folder folder, MailBox mailbox, bool save)
        {
            ArgumentNullException.ThrowIfNull(folder);
            ArgumentNullException.ThrowIfNull(folder.Path);
            ArgumentNullException.ThrowIfNull(mailbox);
            Folder? existing = mailbox.Folders.Where(f => f.Path == folder.Path).FirstOrDefault();
            if (existing is not null)
                return existing;
            
            Folder? parent = null;
            if (folder.Path.Contains('/'))
            {
                string parentPath = folder.Path[0..folder.Path.LastIndexOf('/')];
                parent = await this.CreateFolderAsync(new Folder(){
                    Path = parentPath,
                    MailBox = mailbox
                }, mailbox, false); //do not save yet wait to add all elements in the tree
            }

            folder.Parent = parent;
            var newFolder = this._context.Folder.Add(folder);
            newFolder.State = EntityState.Added;

            if (save)
                if (await this._context.SaveChangesAsync() == 0) 
                    throw new DbUpdateException();
            return newFolder.Entity;
        }

        /// <summary>
        /// Recursively creates the folder tree and saves to DB once all folders have been created
        /// </summary>
        /// <param name="folder">The folder with the path to add</param>
        /// <param name="mailbox">Othe mailbox where the folder resides</param>
        /// <returns>The added folder with </returns>
        /// <exception cref="DbUpdateException">If saving to the database fails</exception>
        /// <exception cref="DbUpdateConcurrencyException">If saving to the database fails due to a concurrent save</exception>
        public async Task<Folder> CreateFolderAsync(Folder folder, MailBox mailbox)
        {
            return await this.CreateFolderAsync(folder, mailbox, true);
        }

        public async Task DeleteFolderAsync(Folder folder)
        {
            ArgumentNullException.ThrowIfNull(folder);

            this._context.Folder.Remove(folder);
            if (await this._context.SaveChangesAsync() == 0)
                throw new DbUpdateException();
        }
    }
}