using System.Data;
using Backend.Models;
using Backend.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public interface IUserService
    {
        public Task<AppUser?> GetUserByIdAsync(string id);
        public Task UpdateUserAsync(UpdateAppUser? user);
        public Task DeleteUserAsync(AppUser user);
    }

    public class UserService : IUserService
    {
        private readonly ApplicationDBContext _context;

        public UserService(ApplicationDBContext context)
        {
            this._context = context;
        }

        public async Task<AppUser?> GetUserByIdAsync(string id)
        {
            return await this._context.Users.FindAsync(id);
        }

        public async Task UpdateUserAsync(UpdateAppUser? updateUserDto)
        {
            ArgumentNullException.ThrowIfNull(updateUserDto);
            ArgumentNullException.ThrowIfNull(updateUserDto.Id);
            AppUser? user = await this.GetUserByIdAsync(updateUserDto.Id);
            if (user is null)
                throw new KeyNotFoundException();
            
            user = this._context.Users.Entry(user).Entity;
            
            user.TwoFactorEnabled = updateUserDto.TwoFactorEnabled ?? user.TwoFactorEnabled;
            user.FirstName = updateUserDto.FirstName ?? user.FirstName;
            user.LastName = updateUserDto.LastName ?? user.LastName;

            if (await this._context.SaveChangesAsync() == 0)
                throw new DbUpdateException();
        }

        public async Task DeleteUserAsync(AppUser user)
        {
            this._context.Users.Remove(user);
            if (await this._context.SaveChangesAsync() == 0)
                throw new DbUpdateException();
        }
    }
}