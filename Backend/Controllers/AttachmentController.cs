using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Microsoft.AspNetCore.Identity;
using System.Net.Mime;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers;

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

    // GET: api/Attachment/
    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<PhysicalFileResult>> GetAttachment(long id, CancellationToken cancellationToken = default)
    {
        var self = await this._userManager.GetUserAsync(this.User);
        if (self is null || self.Id is null)
            return this.Forbid();
        var attachment = await this._context.Attachment.Where(x=>x.Id == id).FirstOrDefaultAsync(cancellationToken);
        if (attachment is null)
            return this.NotFound();

        if (self.Id != attachment?.Owner?.Id)
            return this.Forbid();

        return this.PhysicalFile(attachment.FilePath, "multipart/bytes",attachment.FileName);
    }
}
