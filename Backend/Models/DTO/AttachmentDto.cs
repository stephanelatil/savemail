namespace Backend.Models.DTO
{
    public class AttachmentDto
    {
        public long Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; } = 0;
        public string DownloadUrl { get; set; } = string.Empty;
        public AttachmentDto(){}
        public AttachmentDto(Attachment attachment)
        {
            this.Id = attachment.Id;
            this.FileName = attachment.FileName;
            this.FileSize = attachment.FileSize;
            this.DownloadUrl = $"/api/Attachment/{attachment.Id}";
        }
    }
}