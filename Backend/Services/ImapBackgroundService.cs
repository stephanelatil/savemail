using Backend.Models;
using Backend.Utils;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Backend.Tests")]

namespace Backend.Services;

public class IntOrObject<T>
{
    private int? _intValue;
    private T? _objectValue;
    public bool HasInt => this._intValue.HasValue;
    public bool HasObject => this._objectValue is not null;
    public T ObjectValue => this._objectValue ?? throw new ArgumentNullException();
    public int IntValue => this._intValue ?? throw new ArgumentNullException();

    public IntOrObject(int val)
    {
        this._intValue = val;
    }
    public IntOrObject(T val)
    {
        this._objectValue = val;
    }

}

public interface ITaskManager
{
    void EnqueueTask(int mailboxId);
    public Task RunTasks(int numberOfWorkers, CancellationToken cancellationToken);
}

public class TaskManager : ITaskManager
{
    private readonly ILogger<TaskManager> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly AsyncQueue<int> _mailboxesToFetch = new();

    public TaskManager(ILogger<TaskManager> logger, IServiceScopeFactory serviceScopeFactory)
    {
        this._logger = logger;
        this._serviceScopeFactory = serviceScopeFactory;
    }

    public void EnqueueTask(int mailboxId)
    {
        this._mailboxesToFetch.Enqueue(mailboxId);
    }
    
    public async Task RunTasks(int numberOfWorkers, CancellationToken cancellationToken)
    {
        // Create an array to hold the worker tasks
        Task[] workerTasks = new Task[numberOfWorkers];

        // Start each worker task 
        for (int i = 0; i < numberOfWorkers; i++)
        {
            workerTasks[i] = Task.Run(async () => await this.RunWorkerAsync(cancellationToken));
        }

        // Wait for all worker tasks to complete
        await Task.WhenAll(workerTasks);
    }

    internal async Task RunWorkerAsync(CancellationToken cancellationToken)
    {
        // Create a new scope for each worker to ensure separate instances of scoped services
        using IServiceScope scope = this._serviceScopeFactory.CreateScope();
        
        // Resolve scoped services within the worker scope
        IImapFetchTaskService taskService = scope.ServiceProvider.GetRequiredService<IImapFetchTaskService>();
        
        while (!cancellationToken.IsCancellationRequested)
        {
            int mailboxId = 0;
            try
            {
                mailboxId = await this._mailboxesToFetch.DequeueAsync(-1, cancellationToken);
                // Execute the task for the mailbox
                await taskService.ExecuteTask(mailboxId, cancellationToken);
            }
            catch (TimeoutException)
            { /*Ok just stop requested*/ }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Error processing mailbox {MailboxId}", mailboxId);
            }
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

    internal void RunTaskManager(object? state)
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

    //TODO find way to avoid multiple runners updating the same mailbox at the same time (or stop if mailbox deleted)
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using IServiceScope scope = this._serviceScopeFactory.CreateScope();
        ITaskManager taskManager = scope.ServiceProvider.GetRequiredService<ITaskManager>();
        
        // Specify the desired number of worker tasks
        int numberOfWorkers = 5;

        await taskManager.RunTasks(numberOfWorkers, stoppingToken);
    }

    internal void DoWork(object? state)
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
    Task<IntOrObject<IImapFetchTaskService>> ExecuteTask(int mailboxId, CancellationToken cancellationToken = default);
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
    }

    public async Task<IntOrObject<IImapFetchTaskService>> ExecuteTask(int mailboxId, CancellationToken cancellationToken = default)
    {
        this._context.ChangeTracker.Clear();
        MailBox? mailbox = await this._context.MailBox.Where(x=> x.Id == mailboxId)
                                                        .Include(mb =>mb.Folders)
                                                        .Include(mb =>mb.OAuthCredentials)
                                                        .AsSplitQuery()
                                                        .FirstOrDefaultAsync(cancellationToken);
        if (mailbox == null)
            //mailbox invalid or deleted. ignore
            return new IntOrObject<IImapFetchTaskService>(this);

        //get new undiscovered folders
        List<Folder>? folders = await this._imapFolderFetchService.GetNewFolders(mailbox, cancellationToken);
        if (folders is not null){
            this._logger.LogDebug("Folders To Add: {}", string.Join(", ", folders.Select(f => f.Name)));
            foreach(var folder in folders)
                await this._folderService.CreateFolderAsync(folder, mailbox, cancellationToken);
            
            if (cancellationToken.IsCancellationRequested)
                return new IntOrObject<IImapFetchTaskService>(this);

            //run fixup to map folders to mailbox correctly
            this._context.ChangeTracker.DetectChanges();
            mailbox = await this._context.MailBox.Where(x=> x.Id == mailboxId)
                                                    .Include(mb =>mb.Folders)
                                                    .Include(mb =>mb.OAuthCredentials)
                                                    .AsSplitQuery()
                                                    .FirstOrDefaultAsync(cancellationToken);
            if (mailbox is null)
                return new IntOrObject<IImapFetchTaskService>(this);
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
            return new IntOrObject<IImapFetchTaskService>(this);
        }
        
        foreach(Folder folder in mailbox.Folders)//this._context.Folder.Where(f=>f.MailBoxId == mailboxId))
        {
            try{
                await this.PopulateFolder(mailbox, folder, cancellationToken);
            }
            catch(Exception e)
            {
                this._logger.LogError(e, "Unable to fetch or save emails");
            }
        }
        this._imapMailFetchService.Disconnect();
        return new IntOrObject<IImapFetchTaskService>(this);
    }

    internal async Task PopulateFolder(MailBox mailbox, Folder folder, CancellationToken cancellationToken)
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

    internal async Task SaveNewMails(List<Mail> newMails, MailBox mailBox, Folder folder, CancellationToken cancellationToken)
    {
        this._logger.LogDebug($"Saving {newMails.Count} emails");
        foreach (var mail in newMails)
        {
            //ensure parents are set correctly
            mail.OwnerMailBox = mailBox;
            mail.OwnerMailBoxId = mailBox.Id;
        }
        Mail? last = this.GetLastMail(newMails);
        if (last is null)
            return;
        this._context.Folder.Update(folder);
        folder.LastPulledInternalDate = last.DateSent;
        folder.LastPulledUid = last.ImapMailUID;
        await this._mailService.SaveMail(newMails, mailBox.OwnerId, cancellationToken);
        newMails.ForEach(m => folder.Mails.Add(m));
        await this._context.SaveChangesAsync();
        newMails.Clear();
    }

    internal Mail? GetLastMail(List<Mail> mails)
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
