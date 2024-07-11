using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Backend.Models.DTO;

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

        public MailBoxController(ApplicationDBContext context,
                                 UserManager<AppUser> userManager,
                                 IMailBoxService mailboxService,
                                 ITaskManager taskManager)
        {
            this._context = context;
            this._userManager = userManager;
            this._mailBoxService = mailboxService;
            this._taskManager = taskManager;
        }

        // GET: api/MailBox/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<MailBoxDto>> GetMailBox(int id)
        {
            MailBox? mailbox = await this._context.MailBox.FindAsync(id);
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
        public async Task<ActionResult<MailBoxDto?>> PostMailBox(UpdateMailBox updateMailBox)
        {
            AppUser? self = await this._userManager.GetUserAsync(this.User);
            if (self is null)
                return this.Forbid();
            MailBox? mailbox = await this._mailBoxService.CreateMailBoxAsync(updateMailBox, self);
            return this.CreatedAtAction("GetMailBox", new { id = mailbox.Id }, new MailBoxDto(mailbox));
        }

        // POST: api/MailBox{id}/sync
        [HttpGet("{id}/sync")]
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
            MailBox? mailbox = await this._context.MailBox.FindAsync(id);
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
