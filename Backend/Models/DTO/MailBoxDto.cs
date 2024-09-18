using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTO;

public class MailBoxDto
{
    [ReadOnly(true)]
    public int Id { get; set; }
    public string ImapDomain { get; set; } = string.Empty;
    public short ImapPort { get; set; } = 993;
    public string Username {get ; set;} = string.Empty;
    public ImapProvider Provider { get; set; } = ImapProvider.Simple;
    [ReadOnly(true)]
    public List<FolderDto> Folders { get; set;} = [];

    public MailBoxDto(){}

    public MailBoxDto(MailBox mailBox)
    {
        this.Id = mailBox.Id;
        this.ImapDomain = mailBox.ImapDomain;
        this.ImapPort = mailBox.ImapPort;
        this.Provider = mailBox.Provider;
        this.Username = mailBox.Username;

        this.Folders = [];
        foreach (var f in mailBox.Folders.Where(f => f.Parent is null))
        {
            if (f.Parent != null)
                continue;
            if (f.Path == "[Gmail]")
                // If gmail Folder: Remove prefix and add children of the folder
                this.Folders.AddRange(f.Children.Select(f => {f.Path = f.Path[8..]; return new FolderDto(f);}));
            else
                this.Folders.Add(new FolderDto(f));
        }
        this.Folders = this.Folders.OrderBy(f => f.Id).ToList();
    }
}

public class UpdateMailBox
{
    public int Id { get; set; } = -1;
    public string? ImapDomain { get; set; } = null;
    public short? ImapPort { get; set; } = 993;
    public string? Username {get ; set;} = null;
    [DataType(DataType.Password)]
    public string? Password { get; set; } = null;
}
