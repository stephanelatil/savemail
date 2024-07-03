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

        public MailBoxController(ApplicationDBContext context,
                                 UserManager<AppUser> userManager,
                                 IMailBoxService mailboxService)
        {
            this._context = context;
            this._userManager = userManager;
            this._mailBoxService = mailboxService;
        }

        // GET: api/MailBox/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MailBoxDto>> GetMailBox(long id)
        {
            var mailBox = await this._context.MailBox.FindAsync(id);

            if (mailBox == null)
            {
                return this.NotFound();
            }

            return new MailBoxDto(mailBox);
        }

        // Patch: api/MailBox/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> PutMailBox(int id, UpdateMailBox mailBox)
        {
            if (id != mailBox.Id)
            {
                return this.BadRequest();
            }
            try
            {
                await this._mailBoxService.UpdateMailBoxAsync(id, mailBox);
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
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<MailBoxDto?>> PostMailBox(UpdateMailBox updateMailBox)
        {
            AppUser? self = await this._userManager.GetUserAsync(this.User);
            if (self is null)
                return this.Forbid();
            MailBox? mailbox = await this._mailBoxService.CreateMailBoxAsync(updateMailBox, self);
            return this.CreatedAtAction("GetMailBox", new { id = mailbox.Id }, new MailBoxDto(mailbox));
        }

        // DELETE: api/MailBox/5
        [HttpGet("{id}/Folders")]
        public async Task<ActionResult<List<FolderDto>>> ListFoldersMailBox(int id)
        {
            MailBox? mailbox = await this._mailBoxService.GetMailboxByIdAsync(id);
            if (mailbox is null)
                return this.NotFound();
            return this.Ok(mailbox.Folders
                                    .Where(f => f.Parent is null)
                                    .Select(f => new FolderDto(f))
                                    .ToList());
        }

        // DELETE: api/MailBox/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMailBox(long id)
        {
            var mailBox = await this._context.MailBox.FindAsync(id);
            if (mailBox == null)
            {
                return this.NotFound();
            }

            try
            {
                await this._mailBoxService.DeleteMailBoxAsync(mailBox);
            }
            catch(ArgumentNullException)
            {return this.NotFound();}
            catch(DbUpdateException)
            {return this.Problem("Database saving issue: Please try again.");}

            return this.NoContent();
        }
    }
}
