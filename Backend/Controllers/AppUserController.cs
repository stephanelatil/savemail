using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Models.DTO;
using Backend.Models.Serializers;
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
            _context = context;
            _userManager = userManager;
            _userService = userService;
        }

        // GET: api/AppUser
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/AppUser/me
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<AppUser>> GetMe()
        {
            AppUser? self = await this._userManager.GetUserAsync(this.User);
            if (self == null)
                return Forbid();
            else
                return self;
        }

        // GET: api/AppUser/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AppUser>> GetAppUser(string id)
        {
            var appUser = await _userService.GetUserByIdAsync(id);

            if (appUser == null)
            {
                return NotFound();
            }

            return appUser;
        }

        [HttpPatch("{id}")]
        [Authorize]
        public async Task<IActionResult> PatchUser(string id, [FromBody] UpdateAppUser updateUserDto)
        {
            if (updateUserDto == null)
            {
                return BadRequest();
            }

            if (await _userService.UpdateUserAsync(id, updateUserDto))
                return NoContent();

            return Problem();
        }

        // DELETE: api/AppUser/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppUser(string id)
        {
            var appUser = await _userService.GetUserByIdAsync(id);
            if (appUser == null)
            {
                return NotFound();
            }

            if (await _userManager.GetUserAsync(User) != appUser)
                Forbid();

            string? result = await _userService.DeleteUserAsync(id);
            if (result is null)
                return NoContent();

            return this.Problem(result);
        }
    }
}
