using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public MailController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/Mail
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Mail>>> GetMails()
        {
            return await _context.Mail.ToListAsync();
        }

        // GET: api/Mail/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Mail>> GetMail(long id)
        {
            var mail = await _context.Mail.FindAsync(id);

            if (mail == null)
            {
                return NotFound();
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
                return BadRequest();
            }

            _context.Entry(mail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MailExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Mail
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Mail>> PostMail(Mail mail)
        {
            _context.Mail.Add(mail);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMail", new { id = mail.Id }, mail);
        }

        // DELETE: api/Mail/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMail(long id)
        {
            var mail = await _context.Mail.FindAsync(id);
            if (mail == null)
            {
                return NotFound();
            }

            _context.Mail.Remove(mail);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MailExists(long id)
        {
            return _context.Mail.Any(e => e.Id == id);
        }
    }
}
