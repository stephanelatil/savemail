using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Identity;

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

        // GET: api/Mail
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Mail>>> GetMails()
        {
            return await this._context.Mail.ToListAsync();
        }

        // GET: api/Mail/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Mail>> GetMail(long id)
        {
            var mail = await this._context.Mail.FindAsync(id);

            if (mail == null)
            {
                return this.NotFound();
            }

            return mail;
        }

        // PUT: api/Mail/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMail(long id, Mail mail)
        {
            if (id != mail.Id)
            {
                return this.BadRequest();
            }

            this._context.Entry(mail).State = EntityState.Modified;

            try
            {
                await this._context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!this.MailExists(id))
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

        // POST: api/Mail
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Mail>> PostMail(Mail mail)
        {
            this._context.Mail.Add(mail);
            await this._context.SaveChangesAsync();

            return this.CreatedAtAction("GetMail", new { id = mail.Id }, mail);
        }

        // DELETE: api/Mail/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMail(long id)
        {
            var mail = await this._context.Mail.FindAsync(id);
            if (mail == null)
            {
                return this.NotFound();
            }

            this._context.Mail.Remove(mail);
            await this._context.SaveChangesAsync();

            return this.NoContent();
        }

        private bool MailExists(long id)
        {
            return this._context.Mail.Any(e => e.Id == id);
        }
    }
}
