using System.Collections.Concurrent;
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
        //add concurrency fo only track/save synchronously
        ConcurrentBag<Attachment> attachments = [];
        var tasks = mails.Select(m => m.MimeMessage is not null && m.MimeMessage.Attachments.Any()
                                         ? this.ConcurrentDLAttachments(attachments, m.MimeMessage, m.Id, userId)
                                         : Task.CompletedTask)
                         .ToArray() ?? [];
        await Task.WhenAll(tasks);

        await this._context.AddRangeAsync(attachments);
        await this._context.SaveChangesAsync();
    }

    private async Task ConcurrentDLAttachments(ConcurrentBag<Attachment> newAttachments,
                                                MimeMessage message,
                                                long mailId,
                                                string userId)
    {
        //ensures dir exists (will not do anything if it does)
        Directory.CreateDirectory(this._attachmentPath);

        foreach (var attachment in message.Attachments)
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

                newAttachments.Add(new Attachment(){
                                        FileName = fileName,
                                        FilePath = filepath,
                                        FileSize = stream.Length,
                                        MailId = mailId,
                                        OwnerId = userId
                                    });
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