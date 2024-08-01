namespace Backend.Models.DTO;

public class EmailAddressDto
{
    public string Address { get; set; } = "UNKNOWN";
    public string FullName { get; set; } = string.Empty;

    public EmailAddressDto(){}
    public EmailAddressDto(EmailAddress emailAddress)
    {
        this.Address = emailAddress.Address;
        this.FullName = emailAddress.FullName ?? string.Empty;
    }
}
