using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Backend.Models.DTO;
using MailKit.Net.Imap;
using MailKit.Security;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailBoxController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMailBoxService _mailBoxService;
        private readonly ITaskManager _taskManager;
        private readonly ILogger<MailBoxController> _logger;

        public MailBoxController(ApplicationDBContext context,
                                 UserManager<AppUser> userManager,
                                 IMailBoxService mailboxService,
                                 ITaskManager taskManager,
                                 ILogger<MailBoxController> logger)
        {
            this._context = context;
            this._userManager = userManager;
            this._mailBoxService = mailboxService;
            this._taskManager = taskManager;
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
            return new MailBoxDto(mailbox);
        }

        // Patch: api/MailBox/5
        [HttpPatch("{id}")]
        [Authorize]
        public async Task<IActionResult> PutMailBox(int id, UpdateMailBox mailbox)
        {
            AppUser? self = await this._userManager.GetUserAsync(this.User);
            if (self is null)
                return this.Forbid();
            if (id != mailbox.Id)
            {
                return this.BadRequest();
            }
            if ((await this._context.MailBox.FindAsync(id))?.OwnerId != self.Id)
                return this.Forbid();

            try
            {
                await this._mailBoxService.UpdateMailBoxAsync(id, mailbox);
            }
            catch(ArgumentNullException)
            {return this.BadRequest(new {message="Json body object cannot be null"});}
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
            using var client = new ImapClient();
            try
            {
                await client.ConnectAsync(updateMailBox.ImapDomain,
                                        updateMailBox.ImapPort.Value,
                                        updateMailBox.SecureSocketOptions,
                                        cancellationToken);
                await MailBox.ImapAuthenticateAsync(client, updateMailBox, cancellationToken);
            }
            catch(ArgumentException){ return this.BadRequest(); }
            catch(IOException){ return this.Problem($"Unable to access domain {updateMailBox.ImapDomain}:{updateMailBox.ImapPort}"); }
            catch(ImapProtocolException){ return this.Problem($"Unable to access domain {updateMailBox.ImapDomain}:{updateMailBox.ImapPort}"); }
            catch(SaslException){ return this.BadRequest($"Unable to create SaslMechanism with these credentials"); }
            catch(AuthenticationException){ 
                List<ImapProvider> validProviders = [];
                HashSet<string> authMethods = client.AuthenticationMechanisms;
                foreach (ImapProvider provider in Enum.GetValues(typeof(ImapProvider)))
                    if (provider.IsValidProvider(authMethods))
                        validProviders.Add(provider);
                return this.BadRequest("Unable to Authenticate to imap server. "+
                    "Check credentials or use one of the providers supported by the server: "+
                    string.Join(',', validProviders.Select(p => (int)p)));
            }

            MailBox? mailbox = await this._mailBoxService.CreateMailBoxAsync(updateMailBox, self);
            return this.CreatedAtAction("GetMailBox", new { id = mailbox.Id }, new MailBoxDto(mailbox));
        }

        // POST: api/MailBox{id}/sync
        [HttpPost("{id}/sync")]
        [Authorize]
        public async Task<ActionResult> RequestImapSyncMailbox(int id)
        {
            MailBox? mailBox = await this._context.MailBox.FindAsync(id);
            if (mailBox == null)
            {
                return this.NotFound();
            }

            AppUser? self = await this._userManager.GetUserAsync(this.User);
            if (self is null || mailBox.OwnerId != self.Id)
                return this.Forbid();
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
            
            return this.Ok(mailbox.Folders
                                    .Where(f => f.Parent is null)
                                    .Select(f => new FolderDto(f))
                                    .ToList());
        }

        // DELETE: api/MailBox/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteMailBox(int id)
        {
            MailBox? mailbox = await this._context.MailBox.FindAsync(id);
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
}
