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
    public class MailBoxController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public MailBoxController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/MailBox
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MailBox>>> GetMailBoxes()
        {
            return await _context.MailBoxes.ToListAsync();
        }

        // GET: api/MailBox/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MailBox>> GetMailBox(long id)
        {
            var mailBox = await _context.MailBoxes.FindAsync(id);

            if (mailBox == null)
            {
                return NotFound();
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
                return BadRequest();
            }

            _context.Entry(mailBox).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MailBoxExists(id))
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

        // POST: api/MailBox
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<MailBox>> PostMailBox(MailBox mailBox)
        {
            _context.MailBoxes.Add(mailBox);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMailBox", new { id = mailBox.Id }, mailBox);
        }

        // DELETE: api/MailBox/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMailBox(long id)
        {
            var mailBox = await _context.MailBoxes.FindAsync(id);
            if (mailBox == null)
            {
                return NotFound();
            }

            _context.MailBoxes.Remove(mailBox);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MailBoxExists(long id)
        {
            return _context.MailBoxes.Any(e => e.Id == id);
        }
    }
}
