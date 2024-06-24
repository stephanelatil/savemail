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
    public class FolderController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public FolderController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/Folder
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Folder>>> GetFolder()
        {
            return await _context.Folder.ToListAsync();
        }

        // GET: api/Folder/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Folder>> GetFolder(long id)
        {
            var folder = await _context.Folder.FindAsync(id);

            if (folder == null)
            {
                return NotFound();
            }

            return folder;
        }

        // PUT: api/Folder/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFolder(long id, Folder folder)
        {
            if (id != folder.Id)
            {
                return BadRequest();
            }

            _context.Entry(folder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FolderExists(id))
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

        // POST: api/Folder
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Folder>> PostFolder(Folder folder)
        {
            _context.Folder.Add(folder);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFolder", new { id = folder.Id }, folder);
        }

        // DELETE: api/Folder/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFolder(long id)
        {
            var folder = await _context.Folder.FindAsync(id);
            if (folder == null)
            {
                return NotFound();
            }

            _context.Folder.Remove(folder);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FolderExists(long id)
        {
            return _context.Folder.Any(e => e.Id == id);
        }
    }
}
