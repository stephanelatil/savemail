using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Microsoft.AspNetCore.Identity;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttachmentController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AttachmentController(ApplicationDBContext context,
                                    UserManager<AppUser> userManager)
        {
            this._context = context;
            this._userManager = userManager;
        }

        // GET: api/Attachment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Attachment>>> GetAttachments()
        {
            return await this._context.Attachment.ToListAsync();
        }

        // GET: api/Attachment/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Attachment>> GetAttachment(long id)
        {
            var attachment = await this._context.Attachment.FindAsync(id);

            if (attachment == null)
            {
                return this.NotFound();
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
                return this.BadRequest();
            }

            this._context.TrackEntry(attachment);

            try
            {
                await this._context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!this.AttachmentExists(id))
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

        // POST: api/Attachment
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Attachment>> PostAttachment(Attachment attachment)
        {
            this._context.Attachment.Add(attachment);
            await this._context.SaveChangesAsync();

            return this.CreatedAtAction("GetAttachment", new { id = attachment.Id }, attachment);
        }

        // DELETE: api/Attachment/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttachment(long id)
        {
            var attachment = await this._context.Attachment.FindAsync(id);
            if (attachment == null)
            {
                return this.NotFound();
            }

            this._context.Attachment.Remove(attachment);
            await this._context.SaveChangesAsync();

            return this.NoContent();
        }

        private bool AttachmentExists(long id)
        {
            return this._context.Attachment.Any(e => e.Id == id);
        }
    }
}
