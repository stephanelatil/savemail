using Backend.Models;
using System.Collections.Concurrent;

namespace Backend.Services
{
    public interface ITaskManager
    {
        // Task AddTaskToQueue(int mailboxId);
        Task RunDailyTasks(CancellationToken cancellationToken);
    }

    public class TaskManager : ITaskManager
    {
        private readonly ApplicationDBContext _context;
        private readonly SemaphoreSlim _semaphore;
        //ensure each mailbox has only one running task
        private readonly ConcurrentDictionary<int, bool> _runningTasks = new();
        //ensures that a task is only used by one parallel for thread
        private readonly ConcurrentDictionary<IImapFetchTaskService, bool> _taskServices;

        public TaskManager(ApplicationDBContext context,
                            IImapFetchTaskService task1,
                            IImapFetchTaskService task2,
                            IImapFetchTaskService task3,
                            IImapFetchTaskService task4,
                            IImapFetchTaskService task5)
        {
            this._context = context;
            this._semaphore = new SemaphoreSlim(5, 5);
            this._taskServices = new ConcurrentDictionary<IImapFetchTaskService, bool>();
            this._taskServices.TryAdd(task1, true);
            this._taskServices.TryAdd(task2, true);
            this._taskServices.TryAdd(task3, true);
            this._taskServices.TryAdd(task4, true);
            this._taskServices.TryAdd(task5, true);
        }

        public async Task RunDailyTasks(CancellationToken cancellationToken = default)
        {
            List<int> mailboxIds = this._context.MailBox.Select(mb => mb.Id).ToList();
            
            await Parallel.ForEachAsync(mailboxIds, async (mailboxId, ct) =>
            {
                await this._semaphore.WaitAsync(ct);
                IImapFetchTaskService? task = null;
                try
                {
                    //take a task in the dict and mark it as in use
                    do{
                        task = this._taskServices.Where(t => t.Value).First().Key;
                        //ensure another thread is not marking it as used at the same time
                    }while(this._taskServices.TryUpdate(task, false, true));

                    if (this._runningTasks.TryAdd(mailboxId, true))
                    {
                        try
                        {
                            await task.ExecuteTask(mailboxId, ct);
                        }
                        finally
                        {
                            this._runningTasks.TryRemove(mailboxId, out _);
                        }
                    }
                }
                finally
                {
                    if (task is not null)
                        this._taskServices.TryUpdate(task, true, false);
                        //should not fail, only this thread has access to this task
                    this._semaphore.Release();
                }
            });
        }
    }

    public class DailyScheduleService : IHostedService
    {
        private readonly ITaskManager _taskManager;
        private readonly CancellationTokenSource cancellationTokenSource = new();
        private Timer? _timer = null;

        public DailyScheduleService(ITaskManager taskManager)
        {
            this._taskManager = taskManager;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this._timer = new Timer(this.DoWork, 
                                    this.cancellationTokenSource,
                                    TimeSpan.Zero,
                                    TimeSpan.FromDays(1));

            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            if (state is not CancellationTokenSource source)
                throw new ArgumentException("State cannot be null. Must be TaskData", nameof(state));

            Task.Run(async () =>
            {
                try
                {
                    await this._taskManager.RunDailyTasks(source.Token);
                }
                catch {}
            
            }, source.Token).Wait();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this._timer?.Change(Timeout.Infinite, 0);
            this.cancellationTokenSource.Cancel();

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            this._timer?.Dispose();
            this._timer = null;
        }
    }

    public interface IImapFetchTaskService
    {
        public Guid TaskId { get; }
        Task ExecuteTask(int mailboxId, CancellationToken cancellationToken = default);
    }

    public class ImapFetchTaskService : IImapFetchTaskService
    {
        public Guid TaskId => this._guid;
        private readonly Guid _guid = new();
        private readonly ApplicationDBContext _context;
        private readonly IFolderService _folderService;
        private readonly IMailService _mailService;
        private readonly IImapFolderFetchService _imapFolderFetchService;
        private readonly IImapMailFetchService _imapMailFetchService;

        public ImapFetchTaskService(ApplicationDBContext context,
                                    IFolderService folderService,
                                    IMailService mailService,
                                    IImapFolderFetchService imapFolderFetchService,
                                    IImapMailFetchService imapMailFetchService)
        {
            this._context = context;
            this._folderService = folderService;
            this._mailService = mailService;
            this._imapFolderFetchService = imapFolderFetchService;
            this._imapMailFetchService = imapMailFetchService;
        }

        public Task ExecuteTask(int mailboxId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object? obj)
        {
            if (obj is not IImapFetchTaskService task2)
                return false;
            return task2.TaskId == this.TaskId;
        }

        public override int GetHashCode() => this.TaskId.GetHashCode();
    }
}