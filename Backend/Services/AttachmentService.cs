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
        this._attachmentPath = configuration.GetValue<string>("AttachmentsPath") ?? "../Attachments";
        Directory.CreateDirectory(this._attachmentPath);
    }

    public async Task SaveAttachments(List<Mail> mails, string userId){
        //add concurrency fo only track/save synchronously
        ConcurrentBag<Attachment> attachments = [];
        var tasks = mails.Select(m => m.MimeMessage is not null && m.MimeMessage.Attachments.Any()
                                         ? this.ConcurrentDLAttachments(attachments, m, userId)
                                         : Task.CompletedTask)
                         .ToArray() ?? [];
        await Task.WhenAll(tasks);

        mails.ForEach(m => this._context.TrackEntry(m));
        foreach (var att in attachments)
            this._context.Add(att);
        await this._context.SaveChangesAsync();
        
    }

    private async Task ConcurrentDLAttachments(ConcurrentBag<Attachment> newAttachments,
                                                Mail mail,
                                                string userId)
    {
        //ensures dir exists (will not do anything if it does)
        if (mail is null || mail.MimeMessage is null || mail.Id == 0)
            return;
        if (mail.Attachments.Count == 0)
            return;
        Directory.CreateDirectory(Path.Join(this._attachmentPath, userId, mail.OwnerMailBoxId.ToString()));

        foreach (var attachment in mail.MimeMessage.Attachments)
        {
            try{
                string? filepath = null;
                while (filepath is null || Path.Exists(filepath))
                    filepath = Path.Join(this._attachmentPath, userId, mail.OwnerMailBoxId.ToString(), Path.GetRandomFileName());
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
                                        Mail = mail,
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
    // TODO Handle duplicates here:
        //Can be at the model level (single attachment linked to multiple mails)
        //Or at the file level (multiple attachment Models pointing to the same file with path property)
}