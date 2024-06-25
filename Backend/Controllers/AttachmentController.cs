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
    public class AttachmentController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public AttachmentController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/Attachment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Attachment>>> GetAttachments()
        {
            return await _context.Attachment.ToListAsync();
        }

        // GET: api/Attachment/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Attachment>> GetAttachment(long id)
        {
            var attachment = await _context.Attachment.FindAsync(id);

            if (attachment == null)
            {
                return NotFound();
            }

            return attachment;
        }

        // PUT: api/Attachment/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAttachment(long id, Attachment attachment)
        {
            if (id != attachment.Id)
            {
                return BadRequest();
            }

            _context.Entry(attachment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AttachmentExists(id))
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

        // POST: api/Attachment
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Attachment>> PostAttachment(Attachment attachment)
        {
            _context.Attachment.Add(attachment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAttachment", new { id = attachment.Id }, attachment);
        }

        // DELETE: api/Attachment/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttachment(long id)
        {
            var attachment = await _context.Attachment.FindAsync(id);
            if (attachment == null)
            {
                return NotFound();
            }

            _context.Attachment.Remove(attachment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AttachmentExists(long id)
        {
            return _context.Attachment.Any(e => e.Id == id);
        }
    }
}
