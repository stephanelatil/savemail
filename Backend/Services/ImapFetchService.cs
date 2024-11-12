using System.Net.Sockets;
using Backend.Models;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Services
{
    public interface IImapFolderFetchService : IDisposable
    {
        public Task<List<Folder>?> GetNewFolders(MailBox mailbox, CancellationToken cancellationToken = default);
    }

    public class ImapFolderFetchService : IImapFolderFetchService
    {
        private ImapClient imapClient;
        private readonly ILogger<ImapFolderFetchService> _logger;
        private readonly TokenEncryptionService _tokenEncryptionService;
        private readonly IOAuthService _oAuthService;
        private bool _disposed = false;


        public ImapFolderFetchService(ILogger<ImapFolderFetchService> logger,
                                      IOAuthService oAuthService,
                                      TokenEncryptionService tokenEncryptionService)
        {
            this.imapClient = new();
            this._logger = logger;
            this._oAuthService = oAuthService;
            this._tokenEncryptionService = tokenEncryptionService;
        }

        /// <summary>
        /// Returns a list of all folders on the imap server that don't yet exist in the local mailbox
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>A list of Folder instances. They should still be added to the database with the FolderService!</returns>
        public async Task<List<Folder>?> GetNewFolders(MailBox mailbox, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(this._disposed, this);
            List<Folder> folders = [];
            try
            {
                await this.imapClient.ConnectAsync(mailbox.ImapDomain,
                                        mailbox.ImapPort,
                                        MailKit.Security.SecureSocketOptions.Auto,
                                        cancellationToken);
                await mailbox.ImapAuthenticateAsync(this.imapClient, this._tokenEncryptionService, this._oAuthService, cancellationToken);

                foreach (var imapFolder in await this.imapClient.GetFoldersAsync(this.imapClient.PersonalNamespaces[0],
                                                                                false,
                                                                                cancellationToken))
                {
                    if (mailbox.Provider == ImapProvider.Google && imapFolder.FullName == "[Gmail]/All Mail")
                        continue; //Ignore this folder which is just inbox+user created folders. Makes duplicate emails
                    if (imapFolder.Name == "Trash") //ignore trash folder: should not be synced. TODO: edited in settings
                        continue;
                    if (!mailbox.Folders.Any(f => f.Path == imapFolder.FullName))
                        folders.Add(new Folder(imapFolder));
                }
            }
            catch (SocketException){
                this._logger.LogWarning("Unable to connect to imap server '{}' on port {}", mailbox.ImapDomain, mailbox.ImapPort);
            }
            catch(MailKit.Security.AuthenticationException e){
                if (mailbox.OAuthCredentials is not null)
                    await this._oAuthService.SetNeedReauth(mailbox.OAuthCredentials);
                mailbox.NeedsReauth = true;
                this._logger.LogWarning(e, "Unable to connect to connect and authenticate for mailbox {}", mailbox.Id);
                return null;
            }
            catch(SecurityTokenExpiredException){}
            catch(Exception e)
            {
                this._logger.LogWarning(e, "Unable to connect to imap service for mailbox {}", mailbox.Id);
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
        public bool IsConnected { get; }
        public bool IsAuthenticated { get; }
    }

    public class ImapMailFetchService : IImapMailFetchService
    {
        private ImapClient imapClient = new();
        private readonly IOAuthService _oAuthService;
        private readonly TokenEncryptionService _tokenEncryptionService;
        private Folder? _folder;
        private IMailFolder? _imapFolder;
        private Queue<MailKit.UniqueId>? _uids = null;        
        private bool _disposed = false;
        public bool Prepared { get; private set; } = false;
        public bool IsConnected => this.imapClient.IsConnected;
        public bool IsAuthenticated => this.imapClient.IsAuthenticated;

        private readonly ILogger<ImapMailFetchService> _logger;

        public ImapMailFetchService(ILogger<ImapMailFetchService> logger,
                                    IOAuthService oAuthService,
                                    TokenEncryptionService tokenEncryptionService) 
        {
            this._logger = logger;
            this._oAuthService = oAuthService;
            this._tokenEncryptionService = tokenEncryptionService;
        }

        private async Task<Queue<MailKit.UniqueId>> GetUidsToFetchAsync(MailKit.UniqueId? lastUid, DateTime lastDate,
                                                                CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(this._imapFolder);
            MailKit.UniqueId? start = null;
            //Gets the latest mails starting at the 
            if (lastUid.HasValue && this._imapFolder.UidValidity == lastUid.Value.Validity)
                start = lastUid.Value;
            else if (lastDate != DateTime.MinValue)
            {
                try{
                MailKit.Search.SearchResults result = await this._imapFolder.SearchAsync(
                            MailKit.Search.SearchOptions.All,
                            MailKit.Search.SearchQuery.DeliveredAfter(lastDate),
                            cancellationToken);
                    return new Queue<MailKit.UniqueId>(result.UniqueIds);
                }
                catch {}//Doesn't support ESEARCH
            }
            if (!start.HasValue)
                start = MailKit.UniqueId.MinValue;

            UniqueIdRange range = new(start.Value, MailKit.UniqueId.MaxValue);
            return new Queue<MailKit.UniqueId>(
                            await this._imapFolder.SearchAsync(
                                    range, MailKit.Search.SearchQuery.NotDraft, cancellationToken));
        }

        public async Task Prepare(MailBox mailbox, CancellationToken cancellationToken = default)
        {
            this.Disconnect();
            this.imapClient = new();
            
            try{
                await this.imapClient.ConnectAsync(mailbox.ImapDomain,
                                    mailbox.ImapPort,
                                    MailKit.Security.SecureSocketOptions.Auto,
                                    cancellationToken);
                await mailbox.ImapAuthenticateAsync(this.imapClient, this._tokenEncryptionService,
                                                    this._oAuthService, cancellationToken);
            }
            catch(SecurityTokenExpiredException){}
            catch (SocketException){
                this._logger.LogWarning("Unable to connect to imap server '{}' on port {}", mailbox.ImapDomain, mailbox.ImapPort);
            }
            catch(MailKit.Security.AuthenticationException e){
                if (mailbox.OAuthCredentials is not null)
                    await this._oAuthService.SetNeedReauth(mailbox.OAuthCredentials);
                mailbox.NeedsReauth = true;
                this._logger.LogWarning(e, "Unable to connect to connect and authenticate for mailbox {}", mailbox.Id);
            }
            catch (Exception e){
                this._logger.LogWarning(e, "Unable to connect to connect and authenticate for mailbox {}", mailbox.Id);
            }
            this.Prepared = true;
        }

        public async Task<List<Mail>> GetNextMails(int maxFetchPerLoop=20, CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(maxFetchPerLoop, 1);
            
            ArgumentNullException.ThrowIfNull(this._imapFolder, nameof(this._imapFolder));
            ArgumentNullException.ThrowIfNull(this._folder, nameof(this._folder));
            if (!this.Prepared)
                throw new ArgumentException("Must prepare the service before getting emails", nameof(this.Prepared));
            if (this._uids is null || this._uids.Count == 0) //queue should not be empty
                throw new InvalidOperationException("Now more UIDs available");

            List<Mail> mails = new(maxFetchPerLoop);
            for (int i = 0; i < maxFetchPerLoop; ++i)
            {
                if (!this._uids.TryDequeue(out MailKit.UniqueId uid))
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
                this.imapClient.Dispose();
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