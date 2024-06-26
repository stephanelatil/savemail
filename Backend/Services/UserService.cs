using System.Data;
using Backend.Models;
using Backend.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public interface IUserService
    {
        public Task<AppUser?> GetUserByIdAsync(string id);
        public Task<bool> UpdateUserAsync(string id, UpdateAppUser? user);
        public Task<string?> DeleteUserAsync(string id);
    }

    public class UserService : IUserService
    {
        private readonly ApplicationDBContext _context;

        public UserService(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<AppUser?> GetUserByIdAsync(string id)
        {
            return await _context.Users.FindAsync(id);
        }

        /// <summary>
        /// It will update the associated user object. It still needs to be saved
        /// </summary>
        /// <param name="id">The user id to update</param>
        /// <param name="updateUserDto">The values to update. The values that should not be changed should be null</param>
        /// <returns>The user if the user object was updated, null otherwise</returns>
        public async Task<bool> UpdateUserAsync(string id, UpdateAppUser? updateUserDto)
        {
            if (updateUserDto is null)
                return false;
            AppUser? user = await GetUserByIdAsync(id);
            if (user is null)
                return false;
            
            user.TwoFactorEnabled = updateUserDto.TwoFactorEnabled ?? user.TwoFactorEnabled;
            user.FirstName = updateUserDto.FirstName ?? user.FirstName;
            user.LastName = updateUserDto.LastName ?? user.LastName;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<string?> DeleteUserAsync(string id)
        {
            var user = await GetUserByIdAsync(id);
            if (user is null)
                return "User not found";

            _context.Users.Remove(user);
            try
            {
                if (await _context.SaveChangesAsync() > 0)
                    return null;
            }
            catch (DbUpdateException e)
            {
                return e.Message;
            }
            catch (DBConcurrencyException e)
            {
                return e.Message;
            }
            return "User not deleted. Ensure it still exists in the database";
        }
    }
}