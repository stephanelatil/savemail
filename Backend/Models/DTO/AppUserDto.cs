namespace Backend.Models.DTO
{
    public class AppUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set;} = string.Empty;
        public bool EmailConfirmed { get; set; } = false;
        public bool TwoFactorEnabled { get; set; } = false;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public AppUserDto(AppUser u)
        {
            this.Id = u.Id;
            this.Email = u.Email ?? "";
            this.EmailConfirmed = u.EmailConfirmed;
            this.TwoFactorEnabled = u.TwoFactorEnabled;
            this.FirstName = u.FirstName;
            this.LastName = u.LastName;
        }
    }
    public class UpdateAppUser
    {
        public string? Id { get; set; }
        public bool? TwoFactorEnabled { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}