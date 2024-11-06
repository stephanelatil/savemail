using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Backend.Models.DTO;

namespace Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MailBoxController : ControllerBase
{
    private readonly ApplicationDBContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMailBoxService _mailBoxService;
    private readonly ITaskManager _taskManager;
    private IMailBoxImapCheck _mailBoxImapCheckService;
    private readonly ILogger<MailBoxController> _logger;

    public MailBoxController(ApplicationDBContext context,
                                UserManager<AppUser> userManager,
                                IMailBoxService mailboxService,
                                ITaskManager taskManager,
                                IMailBoxImapCheck mailBoxImapCheck,
                                ILogger<MailBoxController> logger)
    {
        this._context = context;
        this._userManager = userManager;
        this._mailBoxService = mailboxService;
        this._taskManager = taskManager;
        this._mailBoxImapCheckService = mailBoxImapCheck;
        this._logger = logger;
    }

    // GET: api/MailBox/
    [HttpGet()]
    [Authorize]
    public async Task<ActionResult<MailBoxDto[]>> GetMailBoxes()
    {
        AppUser? self = await this._userManager.GetUserAsync(this.User);
        if (self is null)
            return this.Forbid();
        
        return await this._context.MailBox.Where(mb => mb.OwnerId == self.Id)
                                            .Include(mb => mb.Folders)
                                            .Select(mb => new MailBoxDto(mb))
                                            .ToArrayAsync();
    }

    // GET: api/MailBox/5
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<MailBoxDto>> GetMailBox(int id)
    {
        MailBox? mailbox = await this._mailBoxService.GetMailboxByIdAsync(id, true);
        if (mailbox == null)
        {
            return this.NotFound();
        }

        AppUser? self = await this._userManager.GetUserAsync(this.User);
        if (self is null || mailbox.OwnerId != self.Id)
            return this.Forbid();
        return new MailBoxDto(mailbox);
    }

    // Patch: api/MailBox/5
    [HttpPatch("{id}")]
    [Authorize]
    public async Task<IActionResult> PatchMailBox(int id, UpdateMailBox mailbox, CancellationToken cancellationToken=default)
    {
        AppUser? self = await this._userManager.GetUserAsync(this.User);
        if (self is null)
            return this.Forbid();
        if (id != mailbox.Id)
        {
            return this.BadRequest();
        }
        if ((await this._context.MailBox.SingleOrDefaultAsync(mb => mb.Id == id, cancellationToken))?.OwnerId != self.Id)
            return this.Forbid();

        var result = await this._mailBoxImapCheckService.CheckConnection(mailbox, cancellationToken);
        switch (result)
        {
            case ImapCheckResult.NullValue:
            case ImapCheckResult.InvalidValue:
                return this.BadRequest("Invalid or null value supplied");
            case ImapCheckResult.ConnectionToServerError:
                return this.BadRequest("Unable to connect to server");
            case ImapCheckResult.AuthenticationError:
                return this.BadRequest("Invalid credentials");
            case ImapCheckResult.Success:
            default:
            //all OK
                break;
        }

        try
        {
            await this._mailBoxService.UpdateMailBoxAsync(id, mailbox);
        }
        catch(ArgumentNullException)
        {return this.BadRequest("Json body object cannot be null");}
        catch(KeyNotFoundException)
        {return this.NotFound();}
        catch(DbUpdateException)
        {return this.Problem("Database saving issue: Please try again.");}

        return this.NoContent();
    }

    // POST: api/MailBox
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<MailBoxDto?>> PostMailBox(UpdateMailBox updateMailBox, CancellationToken cancellationToken = default)
    {
        AppUser? self = await this._userManager.GetUserAsync(this.User);
        if (self is null)
            return this.Forbid();

        if (updateMailBox.ImapDomain is null)
            return this.BadRequest("imapDomain Cannot be null");
        if (!updateMailBox.ImapPort.HasValue)
            return this.BadRequest("imapPort Cannot be null");

        //Attempt to connect and auth before adding
        var result = await this._mailBoxImapCheckService.CheckConnection(updateMailBox, cancellationToken);
        switch (result)
        {
            case ImapCheckResult.NullValue:
            case ImapCheckResult.InvalidValue:
                return this.BadRequest("Invalid or null value supplied");
            case ImapCheckResult.ConnectionToServerError:
                return this.BadRequest("Unable to connect to server");
            case ImapCheckResult.AuthenticationError:
                return this.BadRequest("Invalid credentials");
            case ImapCheckResult.Success:
            default:
            //all OK
                break;
        }

        MailBox? mailbox = await this._mailBoxService.CreateMailBoxAsync(updateMailBox, self);
        return this.CreatedAtAction("GetMailBox", new { id = mailbox.Id }, new MailBoxDto(mailbox));
    }

    // POST: api/MailBox/{id}/sync
    [HttpPost("{id}/sync")]
    [Authorize]
    public async Task<ActionResult> RequestImapSyncMailbox(int id)
    {
        MailBox? mailBox = await this._context.MailBox.SingleOrDefaultAsync(mb => mb.Id == id);
        if (mailBox == null)
        {
            return this.NotFound();
        }

        AppUser? self = await this._userManager.GetUserAsync(this.User);
        if (self is null || mailBox.OwnerId != self.Id)
            return this.Forbid();

        if (mailBox.NeedsReauth)
            return this.BadRequest("Credentials are invalid or have expired. Please re-login");
        
        this._taskManager.EnqueueTask(id);
        return this.Ok();
    }

    // Get: api/MailBox/5/Folders
    [HttpGet("{id}/Folders")]
    [Authorize]
    public async Task<ActionResult<List<FolderDto>>> ListFoldersMailBox(int id)
    {
        MailBox? mailbox = await this._context.MailBox.Where(mb => mb.Id == id)
                                                        .Include(mb =>mb.Folders)
                                                        .SingleOrDefaultAsync();
        if (mailbox == null)
        {
            return this.NotFound();
        }

        AppUser? self = await this._userManager.GetUserAsync(this.User);
        if (self is null || mailbox.OwnerId != self.Id)
            return this.Forbid();
        
        List<FolderDto> folders = [];

        foreach (var f in mailbox.Folders.Where(f => f.Parent is null))
        {
            if (f.Parent != null)
                continue;
            if (f.Path == "[Gmail]")
                // If gmail Folder: Remove prefix and add children of the folder
                folders.AddRange(f.Children.Select(f => {f.Path = f.Path[8..]; return new FolderDto(f);}));
            else
                folders.Add(new FolderDto(f));
        }
        
        return this.Ok(folders.OrderBy(f => f.Id).ToList());
    }

    // DELETE: api/MailBox/5
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteMailBox(int id)
    {
        MailBox? mailbox = await this._context.MailBox.SingleOrDefaultAsync(mb => mb.Id == id);
        if (mailbox == null)
        {
            return this.NotFound();
        }

        AppUser? self = await this._userManager.GetUserAsync(this.User);
        if (self is null || mailbox.OwnerId != self.Id)
            return this.Forbid();

        try
        {
            await this._mailBoxService.DeleteMailBoxAsync(mailbox);
        }
        catch(ArgumentNullException)
        {return this.NotFound();}
        catch(DbUpdateException)
        {return this.Problem("Database saving issue: Please try again.");}

        return this.NoContent();
    }
}
