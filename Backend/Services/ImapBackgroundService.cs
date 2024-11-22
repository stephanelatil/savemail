using Backend.Models;
using Backend.Utils;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Backend.Services
{
    /// <summary>
    /// Represents a union type that can hold either an integer or an object value.
    /// Used for task queue management where we need to handle both mailbox IDs and task runners.
    /// </summary>
    public class IntOrObject<T>
    {
        private readonly int? _intValue;
        private readonly T? _objectValue;
        
        public bool HasInt => this._intValue.HasValue;
        public bool HasObject => this._objectValue is not null;
        public T ObjectValue => this._objectValue ?? throw new ArgumentNullException(nameof(this._objectValue));
        public int IntValue => this._intValue ?? throw new ArgumentNullException(nameof(this._intValue));

        public IntOrObject(int val) => this._intValue = val;
        public IntOrObject(T val) => this._objectValue = val;
    }

    public interface ITaskManager
    {
        void EnqueueTask(int mailboxId);
        public Task RunTasks(CancellationToken cancellationToken);
    }

    /// <summary>
    /// Manages the execution of IMAP fetch tasks, coordinating between mailboxes that need processing
    /// and available task runners.
    /// </summary>
    public class TaskManager : ITaskManager
    {
        private readonly ILogger<TaskManager> _logger;

        private readonly AsyncQueue<int> _mailboxesToFetch = new();
        private readonly Queue<IImapFetchTaskService> _runners;

        public TaskManager(ILogger<TaskManager> logger,
                           IImapFetchTaskService runner1,
                           IImapFetchTaskService runner2,
                           IImapFetchTaskService runner3)
        {
            this._logger = logger;
            this._runners = new Queue<IImapFetchTaskService>([runner1, runner2, runner3]);
        }

        public void EnqueueTask(int mailboxId)
        {
            this._mailboxesToFetch.Enqueue(mailboxId);
        }

        public async Task RunTasks(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                int mailboxId = await this._mailboxesToFetch.DequeueAsync(cancellationToken:cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    return;
                var runner = this._runners.Dequeue();
                this._runners.Enqueue(runner); //pace it back at the end of the list
                ThreadPool.QueueUserWorkItem(
                            (mailboxId) => runner.ExecuteTask((int?)mailboxId, cancellationToken),
                            mailboxId, false);
            }
        }
    }

    public class DailyScheduleService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<DailyScheduleService> _logger;
        private readonly CancellationTokenSource cancellationTokenSource = new();
        private Timer? _timer = null;
        private Timer? _longRunner = null;

        public DailyScheduleService(IServiceScopeFactory serviceScopeFactory, ILogger<DailyScheduleService> logger)
        {
            // this._taskManager = taskManager;
            this._serviceScopeFactory = serviceScopeFactory;
            this._logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            this._timer = new Timer(this.DoWork, 
                                    this.cancellationTokenSource,
                                    TimeSpan.FromSeconds(2),
                                    TimeSpan.FromDays(1));
            this._longRunner = new Timer(this.RunTaskManager,
                                    this.cancellationTokenSource,
                                    3000,
                                    Timeout.Infinite);
            return Task.CompletedTask;
        }

        private void RunTaskManager(object? state)
        {
            if (state is not CancellationTokenSource source)
            {
                throw new ArgumentException("state must be a CancellationTokenSource", nameof(state));
            }
            try{
                Task.Run(
                    async () => await this.ExecuteAsync(source.Token),
                    source.Token
                ).Wait(source.Token);
            }
            catch(OperationCanceledException){} //no problem :)
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using IServiceScope scope = this._serviceScopeFactory.CreateScope();
            var taskManager = scope.ServiceProvider.GetRequiredService<ITaskManager>();
            
            await taskManager.RunTasks(stoppingToken);
        }

        private void DoWork(object? state)
        {
            if (state is not CancellationTokenSource source)
                throw new ArgumentException("State cannot be null. Must be TaskData", nameof(state));
            using IServiceScope scope = this._serviceScopeFactory.CreateScope();
            ITaskManager taskManager = scope.ServiceProvider.GetRequiredService<ITaskManager>();
            ApplicationDBContext context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            // taskManager.AddDailyTasks(context);
            foreach(var id in context.MailBox.Select(x => x.Id))
            {
                taskManager.EnqueueTask(id);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            this._timer?.Change(Timeout.Infinite, 0);
            this._longRunner?.Change(Timeout.Infinite, 0);
            this.cancellationTokenSource.Cancel();

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
            this._timer?.Dispose();
            this._timer = null;
            this._longRunner?.Dispose();
            this._longRunner = null;
            base.Dispose();
        }
    }

    public interface IImapFetchTaskService : IDisposable
    {
        public Guid TaskId { get; }
        Task ExecuteTask(int? mailboxId, CancellationToken cancellationToken = default);
    }

    public class ImapFetchTaskService : IImapFetchTaskService
    {
        public Guid TaskId => this._guid;
        private readonly Guid _guid = new();
        private readonly ApplicationDBContext _context;
        private readonly IFolderService _folderService;
        private readonly IMailService _mailService;
        private readonly IAttachmentService _attachmentService;
        private readonly IImapFolderFetchService _imapFolderFetchService;
        private readonly IImapMailFetchService _imapMailFetchService;
        private readonly ILogger<ImapFetchTaskService> _logger;
        /// <summary>
        /// Use sem to make sure only one sync is running at once
        /// </summary>
        private readonly Semaphore _busy;

        public ImapFetchTaskService(ILogger<ImapFetchTaskService> logger,
                                    ApplicationDBContext context,
                                    IFolderService folderService,
                                    IMailService mailService,
                                    IAttachmentService attachmentService,
                                    IImapFolderFetchService imapFolderFetchService,
                                    IImapMailFetchService imapMailFetchService)
        {
            this._logger = logger;
            this._context = context;
            this._folderService = folderService;
            this._mailService = mailService;
            this._attachmentService = attachmentService;
            this._imapFolderFetchService = imapFolderFetchService;
            this._imapMailFetchService = imapMailFetchService;
            this._busy = new Semaphore(1,1);
        }

        public async Task ExecuteTask(int? mailboxId, CancellationToken cancellationToken = default)
        {
            if (mailboxId is null)
                return;
            do{
                //wait for other task to finish or cancel to be requested
                if (cancellationToken.IsCancellationRequested)
                    return;
            }while(!this._busy.WaitOne(500));

            try
            {
                this._context.ChangeTracker.Clear();
                MailBox? mailbox = await this._context.MailBox.Where(x=> x.Id == mailboxId)
                                                                .Include(mb =>mb.Folders)
                                                                .Include(mb =>mb.OAuthCredentials)
                                                                .AsSplitQuery()
                                                                .FirstOrDefaultAsync(cancellationToken);
                if (mailbox == null)
                    //mailbox invalid or deleted. ignore
                    return;

                //get new undiscovered folders
                List<Folder>? folders = await this._imapFolderFetchService.GetNewFolders(mailbox, cancellationToken);
                if (folders is not null && folders.Count > 0){
                    this._logger.LogDebug("Folders To Add: {}", string.Join(", ", folders.Select(f => f.Name)));
                    foreach(var folder in folders)
                        await this._folderService.CreateFolderAsync(folder, mailbox, cancellationToken);

                    //run fixup to map folders to mailbox correctly
                    this._context.ChangeTracker.DetectChanges();
                    mailbox = await this._context.MailBox.Where(x=> x.Id == mailboxId)
                                                            .Include(mb =>mb.Folders)
                                                            .Include(mb =>mb.OAuthCredentials)
                                                            .AsSplitQuery()
                                                            .FirstOrDefaultAsync(cancellationToken);
                    if (mailbox is null)
                        return;
                    this._logger.LogDebug("Ran Fixup: got total {} folders to handle", mailbox.Folders.Count);
                }
                //Prepare Imapclient_context to fetch new mails
                await this._imapMailFetchService.Prepare(mailbox, cancellationToken);
                if (!(this._imapMailFetchService.IsConnected && this._imapMailFetchService.IsAuthenticated))
                {
                    //unable to connect
                    this._imapMailFetchService.Disconnect();
                    if (mailbox.NeedsReauth)
                    { // Credentials issue. Save NeedsReauth value
                        this._context.MailBox.Update(mailbox);
                        await this._context.SaveChangesAsync(cancellationToken);
                    }
                    return;
                }
                
                foreach(Folder folder in mailbox.Folders)//this._context.Folder.Where(f=>f.MailBoxId == mailboxId))
                {
                    try{
                        await this.PopulateFolder(mailbox, folder, cancellationToken);
                    }
                    catch(Exception e)
                    {
                        this._logger.LogError(e, "Unable to fetch or save emails in folder: {}", folder.Path);
                    }
                }
                this._imapMailFetchService.Disconnect();
            }
            finally{
                this._busy.Release();
            }
        }

        private async Task PopulateFolder(MailBox mailbox, Folder folder, CancellationToken cancellationToken)
        {
            if (folder.Name == "[Gmail]")
                return; //this folder is a Gmail specific folder that contains all folders except "Inbox"
                        // it is a virtual folder and does not really exist or contain anything. Opening it leads to an error. Ignoring it is the way to go!
            await this._imapMailFetchService.SelectFolder(folder, cancellationToken);
            
            List<Mail> newMails = [];
            while (true)
            {
                try
                {
                    newMails.AddRange(await this._imapMailFetchService.GetNextMails(25, cancellationToken));
                }
                catch(InvalidOperationException)
                {
                    this._logger.LogDebug("End of uids. Saving all new emails");
                    break;
                }
                catch (Exception e)
                {
                    this._logger.LogError(e, "Error when fetching emails");
                    break;
                }
                if (newMails.Count >= 250)
                    await this.SaveNewMails(newMails, mailbox, folder, cancellationToken);
            }
            await this.SaveNewMails(newMails, mailbox, folder, cancellationToken);
        }

        private async Task SaveNewMails(List<Mail> newMails, MailBox mailBox, Folder folder, CancellationToken cancellationToken)
        {
            this._logger.LogDebug($"Saving {newMails.Count} emails");
            Mail? last = this.GetLastMail(newMails);
            if (last is null)
                return;
            foreach (var mail in newMails)
            {
                //ensure parents are set correctly
                mail.OwnerMailBox = mailBox;
                mail.OwnerMailBoxId = mailBox.Id;
            }
            //saves new mails and existing mails get their Id field set to the correct value. All Ids are set
            await this._mailService.SaveMail(newMails, mailBox.OwnerId, cancellationToken);
            this._context.TrackEntry(folder);
            this._context.TrackEntry(mailBox);
            folder.LastPulledInternalDate = last.DateSent;
            folder.LastPulledUid = last.ImapMailUID;

            //Add mails to folder ref
            newMails.ForEach(m => folder.Mails.Add(m));
            await this._context.SaveChangesAsync(cancellationToken);
            newMails.Clear();
        }

        private Mail? GetLastMail(List<Mail> mails)
        {
            if (mails.Count == 0)
                return null; //empty list no last element

            Mail last = mails.Last();
            foreach(Mail mail in mails)
                if (mail.ImapMailUID > last.ImapMailUID)
                    last = mail;
            return last;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not IImapFetchTaskService task2)
                return false;
            return task2.TaskId == this.TaskId;
        }

        public override int GetHashCode() => this.TaskId.GetHashCode();

        public void Dispose()
        {
            this._imapFolderFetchService.Dispose();
            this._imapMailFetchService.Dispose();
        }
    }
}