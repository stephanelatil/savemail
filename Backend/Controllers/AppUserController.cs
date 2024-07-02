using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Backend.Services;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppUserController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserService _userService;

        public AppUserController(ApplicationDBContext context,
                                 UserManager<AppUser> userManager,
                                 IUserService userService)
        {
            this._context = context;
            this._userManager = userManager;
            this._userService = userService;
        }

        // GET: api/AppUser
        [HttpGet]
        public IQueryable<AppUserDto> GetUsers()
        {
            return from u in this._context.Users
                        select new AppUserDto(u);
        }
        
        // GET: api/AppUser/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AppUserDto>> GetUser(string id)
        {
            var user = await this._context.Users.Select(u =>
                new AppUserDto(u)).FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return this.NotFound();
            }

            return this.Ok(user);
        }

        // GET: api/AppUser/me
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<AppUserDto>> GetMe()
        {
            AppUser? self = await this._userManager.GetUserAsync(this.User);
            if (self == null)
                return this.Forbid();
            else
                return new AppUserDto(self);
        }

        // // GET: api/AppUser/5
        // [HttpGet("{id}")]
        // public async Task<ActionResult<AppUser>> GetAppUser(string id)
        // {
        //     var appUser = await _userService.GetUserByIdAsync(id);

        //     if (appUser == null)
        //     {
        //         return NotFound();
        //     }

        //     return appUser;
        // }

        [HttpPatch("{id}")]
        [Authorize]
        public async Task<IActionResult> PatchUser(string id, [FromBody] UpdateAppUser updateUserDto)
        {
            if (updateUserDto == null)
            {
                return this.BadRequest();
            }

            if (await this._userService.UpdateUserAsync(id, updateUserDto))
                return this.NoContent();

            return this.Problem();
        }

        // DELETE: api/AppUser/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppUser(string id)
        {
            var appUser = await this._userService.GetUserByIdAsync(id);
            if (appUser == null)
            {
                return this.NotFound();
            }

            if (await this._userManager.GetUserAsync(this.User) != appUser)
                this.Forbid();

            string? result = await this._userService.DeleteUserAsync(id);
            if (result is null)
                return this.NoContent();

            return this.Problem(result);
        }
    }
}
