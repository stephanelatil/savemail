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
    public class FolderController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly UserManager<AppUser> _userManager;

        public FolderController(ApplicationDBContext context,
                                UserManager<AppUser> userManager)
        {
            this._context = context;
            this._userManager = userManager;
        }

        // GET: api/Folder
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Folder>>> GetFolder()
        {
            return await this._context.Folder.ToListAsync();
        }

        // GET: api/Folder/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Folder>> GetFolder(long id)
        {
            var folder = await this._context.Folder.FindAsync(id);

            if (folder == null)
            {
                return this.NotFound();
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
                return this.BadRequest();
            }

            this._context.Entry(folder).State = EntityState.Modified;

            try
            {
                await this._context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!this.FolderExists(id))
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

        // POST: api/Folder
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Folder>> PostFolder(Folder folder)
        {
            this._context.Folder.Add(folder);
            await this._context.SaveChangesAsync();

            return this.CreatedAtAction("GetFolder", new { id = folder.Id }, folder);
        }

        // DELETE: api/Folder/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFolder(long id)
        {
            var folder = await this._context.Folder.FindAsync(id);
            if (folder == null)
            {
                return this.NotFound();
            }

            this._context.Folder.Remove(folder);
            await this._context.SaveChangesAsync();

            return this.NoContent();
        }

        private bool FolderExists(long id)
        {
            return this._context.Folder.Any(e => e.Id == id);
        }
    }
}
