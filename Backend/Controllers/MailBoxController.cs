using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailBoxController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly UserManager<AppUser> _userManager;

        public MailBoxController(ApplicationDBContext context,
                                 UserManager<AppUser> userManager)
        {
            this._context = context;
            this._userManager = userManager;
        }

        // GET: api/MailBox
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MailBox>>> GetMailBoxes()
        {
            return await this._context.MailBox.ToListAsync();
        }

        // GET: api/MailBox/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MailBox>> GetMailBox(long id)
        {
            var mailBox = await this._context.MailBox.FindAsync(id);

            if (mailBox == null)
            {
                return this.NotFound();
            }

            return mailBox;
        }

        // PUT: api/MailBox/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMailBox(long id, MailBox mailBox)
        {
            if (id != mailBox.Id)
            {
                return this.BadRequest();
            }

            this._context.Entry(mailBox).State = EntityState.Modified;

            try
            {
                await this._context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!this.MailBoxExists(id))
                {
                    return this.NotFound();
                }
                else
                {
                    throw;
                }
            }

            return this.NoContent();
        }

        // POST: api/MailBox
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<MailBox>> PostMailBox(MailBox mailBox)
        {
            mailBox.Owner = await this._userManager.GetUserAsync(this.User);
            this._context.MailBox.Add(mailBox);
            await this._context.SaveChangesAsync();

            return this.CreatedAtAction("GetMailBox", new { id = mailBox.Id }, mailBox);
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

            this._context.MailBox.Remove(mailBox);
            await this._context.SaveChangesAsync();

            return this.NoContent();
        }

        private bool MailBoxExists(long id)
        {
            return this._context.MailBox.Any(e => e.Id == id);
        }
    }
}
