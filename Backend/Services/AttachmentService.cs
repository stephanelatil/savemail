using Backend.Models;
using MimeKit;

namespace Backend.Services;

public interface IAttachmentService{
    public Task SaveAttachments(List<Mail> mails, string userId);
}

public class AttachmentService : IAttachmentService 
{
    private readonly ApplicationDBContext _context;
    private readonly ILogger _logger;
    private readonly string _attachmentPath;

    public AttachmentService(ApplicationDBContext context,
                             IConfiguration configuration,
                             ILogger<AttachmentService> logger)
    {
        this._context = context;
        this._logger = logger;
        this._attachmentPath = configuration.GetValue<string>("") ?? "../Attachments";
        Directory.CreateDirectory(this._attachmentPath);
    }

    public async Task SaveAttachments(List<Mail> mails, string userId){
        foreach (var m in mails)
            if (m.MimeMessage is not null && m.MimeMessage.Attachments.Any())
                await this.AddAttachments(m.MimeMessage.Attachments, m.Id, userId);
        await this._context.SaveChangesAsync();
    }

    public async Task AddAttachments(IEnumerable<MimeEntity> attachments, long mailId, string userId)
    {
        foreach (var attachment in attachments)
        {
            try{
                string? filepath = null;
                while (filepath is null || Path.Exists(filepath))
                    filepath = Path.Join(this._attachmentPath,Path.GetRandomFileName());
                var fileName = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;

                using var stream = File.Create(filepath);
                if (attachment is MessagePart part)
                    await part.Message.WriteToAsync(stream);
                else
                    await ((MimePart)attachment).Content.DecodeToAsync(stream);
                await stream.FlushAsync();

                var attachmentObj = new Attachment(){
                    FileName = fileName,
                    FilePath = filepath,
                    FileSize = stream.Length,
                    MailId = mailId,
                    OwnerId = userId
                };
                this._context.Attachment.Add(attachmentObj);
            }
            catch (Exception e)
            {
                this._logger.LogWarning("Unable to save attachment {} due to error: {}",
                                        attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name,
                                        e);
            }
        }
    }
}