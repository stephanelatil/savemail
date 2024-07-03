using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTO
{
    public class MailBoxDto
    {
        [ReadOnly(true)]
        public int Id { get; set; }
        public string ImapDomain { get; set; } = string.Empty;
        public short ImapPort { get; set; }
        public string Username {get ; set;} = string.Empty;
        [DataType(DataType.Password)]
        public string Password { private get; set; } = string.Empty;
        private ImapProvider Provider { get; set; } = ImapProvider.Simple;
        [ReadOnly(true)]
        public List<FolderDto> Folders { get; set;} = [];

        public MailBoxDto(){}

        public MailBoxDto(MailBox mailBox)
        {
            this.Id = mailBox.Id;
            this.ImapDomain = mailBox.ImapDomain;
            this.ImapPort = mailBox.ImapPort;
            this.Username = mailBox.Username;
            this.Password = mailBox.Password;
            this.Provider = mailBox.Provider;
            this.Folders = mailBox.Folders
                                    .Where(f => f.Parent is null)
                                    .Select(f => new FolderDto(f))
                                    .ToList();
        }
    }

    public class UpdateMailBox
    {
        public int Id { get; set; } = -1;
        public string? ImapDomain { get; set; } = null;
        public short? ImapPort { get; set; } = null;
        public string? Username {get ; set;} = null;
        [DataType(DataType.Password)]
        public string? Password { get; set; } = null;
        public ImapProvider? Provider { get; set; } = null;
    }
}