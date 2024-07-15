using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly UserManager<AppUser> _userManager;

        public MailController(ApplicationDBContext context,
                                    UserManager<AppUser> userManager)
        {
            this._context = context;
            this._userManager = userManager;
        }

        // GET: api/Mail/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Mail>> GetMail(long id)
        {
            Mail? mail = await this._context.Mail.Where(m => m.Id == id)
                                                .Include(m => m.OwnerMailBox)
                                                .SingleOrDefaultAsync();
            if (mail is null)
                return this.NotFound();

            var self = await this._userManager.GetUserAsync(this.User);
            if (self is null || self.Id is null || self.Id != mail.OwnerMailBox?.OwnerId)
                return this.Forbid();
            return mail;
        }

        // DELETE: api/Mail/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteMail(long id)
        {
            Mail? mail = await this._context.Mail.Where(m => m.Id == id)
                                                .Include(m => m.OwnerMailBox)
                                                .SingleOrDefaultAsync();
            if (mail == null)
            {
                return this.NotFound();
            }
            var self = await this._userManager.GetUserAsync(this.User);
            if (self is null || self.Id != mail.OwnerMailBox?.OwnerId)
                return this.Forbid();

            this._context.Mail.Remove(mail);
            await this._context.SaveChangesAsync();

            return this.NoContent();
        }
    }
}
