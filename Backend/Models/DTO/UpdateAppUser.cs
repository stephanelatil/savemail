namespace Backend.Models.DTO
{
    public class UpdateAppUser
    {
        public bool? TwoFactorEnabled { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}