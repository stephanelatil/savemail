using System.Data;
using System.Security.Claims;
using Backend.Models;
using Backend.Models.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public interface IUserService
    {
        public Task<AppUser?> GetUserByIdAsync(string id, bool includeMailboxes=false);
        public Task<AppUser?> GetUserByEmailAsync(string email, bool includeMailboxes=false);
        public Task<AppUser?> GetUserByClaimAsync(ClaimsPrincipal principal, bool includeMailboxes=false);
        public Task UpdateUserAsync(UpdateAppUser? user);
        public Task DeleteUserAsync(AppUser user);
    }

    public class UserService : IUserService
    {
        private readonly ApplicationDBContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly string _attachmentPath;

        public UserService(ApplicationDBContext context,
                            UserManager<AppUser> userManager,
                            IConfiguration configuration)
        {
            this._context = context;
            this._userManager = userManager;
            this._attachmentPath = configuration.GetValue<string>("AttachmentsPath") ?? "../Attachments";
        }

        public async Task<AppUser?> GetUserByIdAsync(string id, bool includeMailboxes=false)
        {
            var query = this._context.Users.Where(x=> x.Id == id);
            if (includeMailboxes)
                query.Include(u => u.MailBoxes).AsSplitQuery();
            return await query.FirstOrDefaultAsync();
        }

        public async Task<AppUser?> GetUserByClaimAsync(ClaimsPrincipal principal, bool includeMailboxes = false)
        {
            var user = await this._userManager.GetUserAsync(principal);
            if (user is null)
                return null;
            if (includeMailboxes)
                user = await this._context.Users.Where(u => u.Id == user.Id)
                                                .Include(x => x.MailBoxes)
                                                .AsSplitQuery()
                                                .FirstOrDefaultAsync();
            return user;
        }

        public async Task UpdateUserAsync(UpdateAppUser? updateUserDto)
        {
            ArgumentNullException.ThrowIfNull(updateUserDto);
            ArgumentNullException.ThrowIfNull(updateUserDto.Id);
            AppUser? user = await this.GetUserByIdAsync(updateUserDto.Id) ?? throw new KeyNotFoundException();

            user.FirstName = updateUserDto.FirstName ?? user.FirstName;
            user.LastName = updateUserDto.LastName ?? user.LastName;
            this._context.Users.Update(user);

            if (await this._context.SaveChangesAsync() == 0)
                throw new DbUpdateException();
        }

        public async Task DeleteUserAsync(AppUser user)
        {
            Directory.Delete(Path.Join(this._attachmentPath, user.Id));
            this._context.Users.Remove(user);
            if (await this._context.SaveChangesAsync() == 0)
                throw new DbUpdateException();
        }

        public async Task<AppUser?> GetUserByEmailAsync(string email, bool includeMailboxes = false)
        {
            var users = this._context.Users;
            if (includeMailboxes)
                return await users.Include(u => u.MailBoxes)
                                  .AsSplitQuery()
                                  .FirstOrDefaultAsync(u => u.Email == email);
            return await users.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}