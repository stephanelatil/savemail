using Backend.Models;
using Backend.Utils;
using System.Collections.Concurrent;

namespace Backend.Services
{
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
        public Task RunTasks(AsyncQueue<IntOrObject<IImapFetchTaskService>> _taskRunners,CancellationToken cancellationToken);
    }

    public class TaskManager : ITaskManager
    {
        private readonly ILogger<TaskManager> _logger;
        private readonly AsyncQueue<IntOrObject<IImapFetchTaskService>> _mailboxesToFetch = new();

        public TaskManager(ILogger<TaskManager> logger)
        {
            this._logger = logger;
        }

        public void EnqueueTask(int mailboxId)
        {
            this._logger.LogWarning($"Task Enqueued {mailboxId}");
            this._mailboxesToFetch.Enqueue(new IntOrObject<IImapFetchTaskService>(mailboxId));
        }
        public async Task RunTasks(AsyncQueue<IntOrObject<IImapFetchTaskService>> availableRunners,
                                    CancellationToken cancellationToken)
        {
            int? nextId = null;
            ConcurrentDictionary<Task<IntOrObject<IImapFetchTaskService>>, bool> _runningTasks = new();
            {
                var startupTask = this._mailboxesToFetch.DequeueAsync(cancellationToken);
                _runningTasks.TryAdd(startupTask, true);

                this._logger.LogInformation($"Added task with id {startupTask.Id}");
            }
            //for the lifetime of the app (or until cancelled)
            while (!cancellationToken.IsCancellationRequested){
                //wait to get element from queue or for a runner to finish
                
                var t = await Task.WhenAny(_runningTasks.Keys);
                this._logger.LogInformation($"Got running task done: {t.Id}");
                _runningTasks.TryRemove(t,out _);
                IntOrObject<IImapFetchTaskService> result = await t;                

                if (result.HasInt){
                    int id = result.IntValue;
                    this._logger.LogWarning($"Task t Finished : got {id}");
                    //got mailbox id so request a runner.
                    nextId = id;
                    _runningTasks.TryAdd(availableRunners.DequeueAsync(cancellationToken), true);
                }
                // Got new runner because it has finished his task or has been gotten from a queue
                else if (result.HasObject)
                {
                    var runner = result.ObjectValue;
                    this._logger.LogWarning("Task t Finished : got Runner");
                    // nextId is null which means that a runner has finished
                    if(!nextId.HasValue)
                        availableRunners.Enqueue(new IntOrObject<IImapFetchTaskService>(runner));
                    // nextId is not null which means we got runner to execute the task
                    else
                    {
                        //start runner and await new mailbox id from queue
                        _runningTasks.TryAdd(runner.ExecuteTask(nextId.Value, cancellationToken), true);
                        nextId = null;
                        _runningTasks.TryAdd(this._mailboxesToFetch.DequeueAsync(cancellationToken), true);
                    }
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

        private void RunTaskManager(object? state)
        {
            if (state is not CancellationTokenSource source)
            {
                this._logger.LogWarning("Cancellation source error");
                throw new ArgumentException("state must be a CancellationTokenSource", nameof(state));
            }
            Task.Run(
                async () => await this.ExecuteAsync(source.Token),
                source.Token
            ).Wait(source.Token);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this._logger.LogWarning("Running background process");
            using IServiceScope scope = this._serviceScopeFactory.CreateScope();
            var taskManager = scope.ServiceProvider.GetRequiredService<ITaskManager>();
            AsyncQueue<IntOrObject<IImapFetchTaskService>> taskRunners = new();
            for (int i = 0; i < 5; ++i)
                taskRunners.Enqueue(
                    new IntOrObject<IImapFetchTaskService>(
                        scope.ServiceProvider.GetRequiredService<IImapFetchTaskService>()));
            this._logger.LogWarning("Running Tasks");
            await taskManager.RunTasks(taskRunners,stoppingToken);
        }

        private void DoWork(object? state)
        {
            this._logger.LogWarning("Scheduling daily tasks");
            if (state is not CancellationTokenSource source)
                throw new ArgumentException("State cannot be null. Must be TaskData", nameof(state));
            using IServiceScope scope = this._serviceScopeFactory.CreateScope();
            ITaskManager taskManager = scope.ServiceProvider.GetRequiredService<ITaskManager>();
            ApplicationDBContext context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            // taskManager.AddDailyTasks(context);
            foreach(var id in context.MailBox.Select(x => x.Id))
            {
                taskManager.EnqueueTask(id);
                this._logger.LogWarning($"Scheduling task {id}");
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

    public interface IImapFetchTaskService
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
        private readonly IImapFolderFetchService _imapFolderFetchService;
        private readonly IImapMailFetchService _imapMailFetchService;
        private readonly ILogger<ImapFetchTaskService> _logger;

        public ImapFetchTaskService(ILogger<ImapFetchTaskService> logger,
                                    ApplicationDBContext context,
                                    IFolderService folderService,
                                    IMailService mailService,
                                    IImapFolderFetchService imapFolderFetchService,
                                    IImapMailFetchService imapMailFetchService)
        {
            this._logger = logger;
            this._context = context;
            this._folderService = folderService;
            this._mailService = mailService;
            this._imapFolderFetchService = imapFolderFetchService;
            this._imapMailFetchService = imapMailFetchService;
        }

        public async Task<IntOrObject<IImapFetchTaskService>> ExecuteTask(int mailboxId, CancellationToken cancellationToken = default)
        {
            this._logger.LogWarning($"Running task {mailboxId}");
            await Task.Delay(3000, cancellationToken);
            this._logger.LogWarning("Task Run successfully");
            return new IntOrObject<IImapFetchTaskService>(this);
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