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
    public class EmailAddressController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public EmailAddressController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/EmailAddress
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmailAddress>>> GetEmailAddresses()
        {
            return await _context.EmailAddresses.ToListAsync();
        }

        // GET: api/EmailAddress/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EmailAddress>> GetEmailAddress(long id)
        {
            var emailAddress = await _context.EmailAddresses.FindAsync(id);

            if (emailAddress == null)
            {
                return NotFound();
            }

            return emailAddress;
        }

        // PUT: api/EmailAddress/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmailAddress(long id, EmailAddress emailAddress)
        {
            if (id != emailAddress.Id)
            {
                return BadRequest();
            }

            _context.Entry(emailAddress).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmailAddressExists(id))
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

        // POST: api/EmailAddress
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<EmailAddress>> PostEmailAddress(EmailAddress emailAddress)
        {
            _context.EmailAddresses.Add(emailAddress);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEmailAddress", new { id = emailAddress.Id }, emailAddress);
        }

        // DELETE: api/EmailAddress/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmailAddress(long id)
        {
            var emailAddress = await _context.EmailAddresses.FindAsync(id);
            if (emailAddress == null)
            {
                return NotFound();
            }

            _context.EmailAddresses.Remove(emailAddress);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmailAddressExists(long id)
        {
            return _context.EmailAddresses.Any(e => e.Id == id);
        }
    }
}
