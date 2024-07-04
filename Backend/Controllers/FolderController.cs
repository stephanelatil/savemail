using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Microsoft.AspNetCore.Identity;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FolderController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IFolderService _folderService;

        public FolderController(ApplicationDBContext context,
                                UserManager<AppUser> userManager,
                                IFolderService folderService)
        {
            this._context = context;
            this._userManager = userManager;
            this._folderService = folderService;
        }

        // GET: api/Folder/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Folder>> GetFolder(long id)
        {
            var folder = await this._context.Folder.FindAsync(id);

            if (folder == null)
            {
                return this.NotFound();
            }
            
            var self = await this._userManager.GetUserAsync(this.User);
            if (self is null || folder?.MailBox?.Owner != self)
                return this.Forbid();

            return folder;
        }

        // DELETE: api/Folder/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteFolder(long id)
        {
            var folder = await this._context.Folder.FindAsync(id);
            if (folder == null)
            {
                return this.NotFound();
            }

            var self = await this._userManager.GetUserAsync(this.User);
            if (self is null || folder?.MailBox?.Owner != self)
                return this.Forbid();

            this._context.Folder.Remove(folder);
            await this._context.SaveChangesAsync();

            return this.NoContent();
        }
    }
}
