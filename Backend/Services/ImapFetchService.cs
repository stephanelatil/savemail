using Backend.Models;
using MailKit;
using MailKit.Net.Imap;

namespace Backend.Services
{
    public interface IImapFolderFetchService
    {
        public Task<List<Folder>> GetNewFolders(MailBox mailbox, CancellationToken cancellationToken = default);
    }

    public class ImapFolderFetchService : IDisposable, IImapFolderFetchService
    {
        private ImapClient imapClient;
        private bool _disposed = false;
        private bool _connected = false;


        public ImapFolderFetchService(MailBox mailbox)
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
            this.Disconnect(); //Ensure not already connected
            
            await this.imapClient.ConnectAsync(mailbox.ImapDomain,
                                    mailbox.ImapPort,
                                    mailbox.SecureSocketOptions,
                                    cancellationToken);
            await this.imapClient.AuthenticateAsync(mailbox.GetSaslMechanism(),
                                    cancellationToken);

            foreach (var imapFolder in await this.imapClient.GetFoldersAsync(this.imapClient.PersonalNamespaces[0],
                                                                            false,
                                                                            cancellationToken))
            {
                if (!mailbox.Folders.Any(f => f.Path == imapFolder.FullName))
                    folders.Add(new Folder(imapFolder));
            }

            this.Disconnect();
            return folders;
        }

        private void Disconnect(){
            if (this._connected)
            {
                this._connected = false;
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
    }

    public interface IImapMailFetchService : IDisposable
    {
        public Task Prepare(MailBox mailbox, Folder folder, CancellationToken cancellationToken = default);
        public Task<List<Mail>> GetNextMails(int maxFetchPerLoop=20, CancellationToken cancellationToken = default);
    }

    public class ImapMailFetchService : IImapMailFetchService
    {
        private readonly ImapClient imapClient = new();
        private Folder? _folder;
        private IMailFolder? _imapFolder;
        private Queue<UniqueId>? _uids = null;        
        private bool _disposed = false;
        private bool _prepared = false;

        public ImapMailFetchService() {}

        private async Task<Queue<UniqueId>> GetUidsToFetchAsync(UniqueId? lastUid, DateTimeOffset lastDate,
                                                                CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(this._imapFolder);
            UniqueId? start = null;
            //Gets the latest mails starting at the 
            if (lastUid.HasValue && this._imapFolder.UidValidity == lastUid.Value.Validity)
                start = lastUid.Value;
            else if (lastDate == DateTimeOffset.MinValue)
            {
                MailKit.Search.SearchResults result = await this._imapFolder.SearchAsync(
                            MailKit.Search.SearchOptions.Min,
                            MailKit.Search.SearchQuery.DeliveredAfter(lastDate.DateTime),
                            cancellationToken);
                start = result.Min;
            }
            if (!start.HasValue)
                start = UniqueId.MinValue;

            UniqueIdRange range = new(start.Value, UniqueId.MaxValue);
            return new Queue<UniqueId>(
                            await this._imapFolder.SearchAsync(
                                    range, MailKit.Search.SearchQuery.NotDraft, cancellationToken));
        }

        public async Task Prepare(MailBox mailbox, Folder folder, CancellationToken cancellationToken = default)
        {
            if (this._prepared)
                throw new Exception("ImapClient is already connected");
            ObjectDisposedException.ThrowIf(this._disposed, this);
            this.Disconnect(); //Ensure not already connected
            
            await this.imapClient.ConnectAsync(mailbox.ImapDomain,
                                    mailbox.ImapPort,
                                    mailbox.SecureSocketOptions,
                                    cancellationToken);
            await this.imapClient.AuthenticateAsync(mailbox.GetSaslMechanism(),
                                    cancellationToken);

            this._imapFolder = await this.imapClient.GetFolderAsync(folder.Path, cancellationToken);
            await this._imapFolder.OpenAsync(FolderAccess.ReadOnly, cancellationToken);
            // MAIN LOOP HERE
            // Make sure to only get new emails
            // TODO return N emails gotten from the mailbox
            this._uids = await this.GetUidsToFetchAsync(folder.LastPulledUid,
                                                        folder.LastPulledInternalDate,
                                                        cancellationToken);
            this._prepared = true;
        }

        public async Task<List<Mail>> GetNextMails(int maxFetchPerLoop=20, CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(maxFetchPerLoop, 1);
            
            ArgumentNullException.ThrowIfNull(this._imapFolder, nameof(this._imapFolder));
            ArgumentNullException.ThrowIfNull(this._folder, nameof(this._folder));
            if (!this._prepared)
                throw new ArgumentException(nameof(this._prepared), "Must prepare the service before getting emails");
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

            return mails;
        }

        private void Disconnect(){
            this._prepared = false;
            if (this._prepared)
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
    }
}