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

    public class ImapMailFetchService : IDisposable, IAsyncEnumerable<List<Mail>>
    {
        private ImapClient imapClient;
        private MailBox _mailbox;
        private Folder _folder;
        private bool _disposed = false;
        private bool _connected = false;
        public int MaxFetchPerLoop { get; } = 20;

        public ImapMailFetchService(MailBox mailbox, Folder folder, int maxFetchPerLoop=20)
        {
            this._mailbox = mailbox;
            this._folder =  folder;
            this.imapClient = new();
            this.MaxFetchPerLoop = maxFetchPerLoop;
        }

        public ImapMailFetchService(Folder folder, int maxFetchPerLoop=20)
        {
            this._folder =  folder;
            ArgumentNullException.ThrowIfNull(folder.MailBox);
            this._mailbox = folder.MailBox;
            this.imapClient = new();
            this.MaxFetchPerLoop = maxFetchPerLoop;
        }

        private async Task<IList<UniqueId>> GetUidsToFetchAsync(IMailFolder folder, UniqueId? lastUid, DateTimeOffset lastDate,
                                                                CancellationToken cancellationToken = default)
        {
            UniqueId? start = null;
            //Gets the latest mails starting at the 
            if (lastUid.HasValue && folder.UidValidity == lastUid.Value.Validity)
                start = lastUid.Value;
            else if (lastDate == DateTimeOffset.MinValue)
            {
                var result = await folder.SearchAsync(
                    MailKit.Search.SearchOptions.Min,
                    MailKit.Search.SearchQuery.DeliveredAfter(lastDate.DateTime),
                    cancellationToken
                );
                start = result.Min;
            }
            if (!start.HasValue)
                start = UniqueId.MinValue;

            UniqueIdRange range = new(start.Value, UniqueId.MaxValue);
            return await folder.SearchAsync(range, MailKit.Search.SearchQuery.NotDraft, cancellationToken);
        }

        public async IAsyncEnumerator<List<Mail>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            if (this._connected)
                throw new Exception("ImapClient is already connected");
            ObjectDisposedException.ThrowIf(this._disposed, this);
            this.Disconnect(); //Ensure not already connected
            
            await this.imapClient.ConnectAsync(this._mailbox.ImapDomain,
                                    this._mailbox.ImapPort,
                                    this._mailbox.SecureSocketOptions,
                                    cancellationToken);
            this._connected = true;
            await this.imapClient.AuthenticateAsync(this._mailbox.GetSaslMechanism(),
                                    cancellationToken);

            IMailFolder imapFolder = await this.imapClient.GetFolderAsync(this._folder.Path, cancellationToken);
            await imapFolder.OpenAsync(MailKit.FolderAccess.ReadOnly, cancellationToken);
            // MAIN LOOP HERE
            // Make sure to only get new emails
            // TODO return N emails gotten from the mailbox
            IList<UniqueId> uids = await this.GetUidsToFetchAsync(imapFolder,
                                                                  this._folder.LastPulledUid,
                                                                  this._folder.LastPulledInternalDate,
                                                                  cancellationToken);
            List<Mail> newMails = [];
            foreach (UniqueId mailUid in uids)
            {
                while (newMails.Count < this.MaxFetchPerLoop)
                {
                    if (cancellationToken.IsCancellationRequested || !this._connected)
                        break;
                    
                    Mail mail = new(await imapFolder.GetMessageAsync(mailUid, cancellationToken),
                                        mailUid, this._folder);
                    newMails.Add(mail);
                }
                if (cancellationToken.IsCancellationRequested || !this._connected)
                    break;
                yield return newMails;
                newMails.Clear();
            }

            await imapFolder.CloseAsync(false, CancellationToken.None);
            this.Disconnect();
            yield break;
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
}