using Backend.Models;
using MailKit;
using MailKit.Net.Imap;

namespace Backend.Services
{
    public interface IImapFolderFetchService : IDisposable
    {
        public Task<List<Folder>> GetNewFolders(MailBox mailbox, CancellationToken cancellationToken = default);
    }

    public class ImapFolderFetchService : IImapFolderFetchService
    {
        private ImapClient imapClient;
        private bool _disposed = false;


        public ImapFolderFetchService()
        {
            this.imapClient = new();
        }

        /// <summary>
        /// Returns a list of all folders on the imap server that don't yet exist in the local mailbox
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>A list of Folder instances. They should still be added to the database with the FolderService!</returns>
        public async Task<List<Folder>> GetNewFolders(MailBox mailbox, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this._disposed, this);
            List<Folder> folders = [];
            try
            {
                await this.imapClient.ConnectAsync(mailbox.ImapDomain,
                                        mailbox.ImapPort,
                                        mailbox.SecureSocketOptions,
                                        cancellationToken);
                await mailbox.ImapAuthenticateAsync(this.imapClient, cancellationToken);

                foreach (var imapFolder in await this.imapClient.GetFoldersAsync(this.imapClient.PersonalNamespaces[0],
                                                                                false,
                                                                                cancellationToken))
                {
                    if (!mailbox.Folders.Any(f => f.Path == imapFolder.FullName))
                        folders.Add(new Folder(imapFolder));
                }
            }
            finally
            {
                await this.imapClient.DisconnectAsync(true, cancellationToken);
            }
            return folders;
        }

        protected virtual void Dispose(bool disposing)
        {
            try{
                this.imapClient.Disconnect(true);
            }catch{}
            if (disposing)
            {
                this.imapClient?.Dispose();
            }
        }

        public void Dispose()
        {
            this.Dispose(!this._disposed);
            this._disposed = true;
            GC.SuppressFinalize(this);
        }
    }

    public interface IImapMailFetchService : IDisposable
    {
        public bool Prepared { get; }
        public Task Prepare(MailBox mailbox, CancellationToken cancellationToken = default);
        public Task SelectFolder(Folder folder, CancellationToken cancellationToken = default);
        public Task<List<Mail>> GetNextMails(int maxFetchPerLoop=20, CancellationToken cancellationToken = default);
        public void Disconnect();
    }

    public class ImapMailFetchService : IImapMailFetchService
    {
        private ImapClient imapClient = new();
        private Folder? _folder;
        private IMailFolder? _imapFolder;
        private Queue<UniqueId>? _uids = null;        
        private bool _disposed = false;
        public bool Prepared { get; private set; } = false;

        private readonly ILogger<ImapMailFetchService> _logger;

        public ImapMailFetchService(ILogger<ImapMailFetchService> logger) 
        {
            this._logger = logger;
        }

        private async Task<Queue<UniqueId>> GetUidsToFetchAsync(UniqueId? lastUid, DateTimeOffset lastDate,
                                                                CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(this._imapFolder);
            UniqueId? start = null;
            //Gets the latest mails starting at the 
            if (lastUid.HasValue && this._imapFolder.UidValidity == lastUid.Value.Validity)
                start = lastUid.Value;
            else if (lastDate != DateTimeOffset.MinValue)
            {
                try{
                MailKit.Search.SearchResults result = await this._imapFolder.SearchAsync(
                            MailKit.Search.SearchOptions.All,
                            MailKit.Search.SearchQuery.DeliveredAfter(lastDate.DateTime),
                            cancellationToken);
                    return new Queue<UniqueId>(result.UniqueIds);
                }
                catch {}//Doesn't support ESEARCH
            }
            if (!start.HasValue)
                start = UniqueId.MinValue;

            UniqueIdRange range = new(start.Value, UniqueId.MaxValue);
            return new Queue<UniqueId>(
                            await this._imapFolder.SearchAsync(
                                    range, MailKit.Search.SearchQuery.NotDraft, cancellationToken));
        }

        public async Task Prepare(MailBox mailbox, CancellationToken cancellationToken = default)
        {
            this.Disconnect();
            this.imapClient = new();
            
            await this.imapClient.ConnectAsync(mailbox.ImapDomain,
                                    mailbox.ImapPort,
                                    mailbox.SecureSocketOptions,
                                    cancellationToken);
            await mailbox.ImapAuthenticateAsync(this.imapClient, cancellationToken);
            this.Prepared = true;
        }

        public async Task<List<Mail>> GetNextMails(int maxFetchPerLoop=20, CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(maxFetchPerLoop, 1);
            
            ArgumentNullException.ThrowIfNull(this._imapFolder, nameof(this._imapFolder));
            ArgumentNullException.ThrowIfNull(this._folder, nameof(this._folder));
            if (!this.Prepared)
                throw new ArgumentException(nameof(this.Prepared), "Must prepare the service before getting emails");
            if (this._uids is null || this._uids.Count == 0) //queue should not be empty
                throw new InvalidOperationException("Now more UIDs available");

            List<Mail> mails = new(maxFetchPerLoop);
            for (int i = 0; i < maxFetchPerLoop; ++i)
            {
                if (!this._uids.TryDequeue(out UniqueId uid))
                    break; //done if end of queue reached
                
                mails.Add(new Mail(await this._imapFolder.GetMessageAsync(uid),
                                    uid,
                                    this._folder));
            }

            this._logger.LogDebug($"Fetched {mails.Count} mails from folder: {this._folder.Path}");
            return mails;
        }

        public void Disconnect(){
            this.Prepared = false;
            if (this.Prepared)
            {
                this._imapFolder?.Close();
                this._imapFolder = null;
                this._uids = null;
                this._folder = null;
                this.imapClient.Disconnect(true);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            this.Disconnect();
            if (disposing)
            {
                this.imapClient?.Dispose();
            }
        }

        public void Dispose()
        {
            this.Dispose(!this._disposed);
            this._disposed = true;
            GC.SuppressFinalize(this);
        }

        public async Task SelectFolder(Folder folder, CancellationToken cancellationToken = default)
        {
            this._folder = folder;
            this._imapFolder = await this.imapClient.GetFolderAsync(folder.Path, cancellationToken);
            await this._imapFolder.OpenAsync(FolderAccess.ReadOnly, cancellationToken);
            this._uids = await this.GetUidsToFetchAsync(folder.LastPulledUid,
                                                        folder.LastPulledInternalDate,
                                                        cancellationToken);
            this._logger.LogDebug($"Got {this._uids.Count} mail uids to fetch");
        }
    }
}