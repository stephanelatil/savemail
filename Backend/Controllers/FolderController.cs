using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Microsoft.AspNetCore.Identity;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Backend.Models.DTO;

namespace Backend.Controllers;

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
    public async Task<ActionResult<FolderDto>> GetFolder(int id)
    {
        var folder = await this._context.Folder.Where(f => f.Id == id)
                                                .Include(f=>f.MailBox)
                                                .Include(f => f.Parent)
                                                .Include(f => f.Children)
                                                .FirstOrDefaultAsync();

        if (folder == null)
        {
            return this.NotFound();
        }
        
        var self = await this._userManager.GetUserAsync(this.User);
        if (self is null || self.Id is null || folder?.MailBox?.OwnerId != self.Id)
            return this.Forbid();

        return new FolderDto(folder);
    }

    // GET: api/Folder/5
    [HttpGet("{id}/Mails")]
    [Authorize]
    public async Task<ActionResult<PaginatedList<MailDto>>> GetMails(int id, PaginationQueryParameters paginationQueryParameters)
    {
        var folder = await this._context.Folder.Where(f => f.Id == id)
                                                .Include(f=>f.MailBox)
                                                .SingleOrDefaultAsync();

        if (folder == null)
        {
            return this.NotFound();
        }
        
        var self = await this._userManager.GetUserAsync(this.User);
        if (self is null || folder?.MailBox?.Owner != self)
            return this.Forbid();

        return this.Ok(await PaginationService.GetPagedResult(
                        this._context.Mail.Where(m => m.FolderId == folder.Id)
                                                    .OrderByDescending(m =>m.DateSent)
                                                    .ThenByDescending(m => m.ImapMailUID),
                        paginationQueryParameters,
                        (x)=>new MailDto(x, true),
                        this.Request.PathBase+this.Request.Path
                        ));
    }

    // DELETE: api/Folder/5
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteFolder(int id)
    {
        var folder = await this._context.Folder.Where(f => f.Id == id)
                                                .Include(f=>f.MailBox)
                                                .SingleOrDefaultAsync();
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
