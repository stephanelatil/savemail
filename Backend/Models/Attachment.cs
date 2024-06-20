namespace Backend.Models
{
    public class Attachment
    {
        public long Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        // public FileObject? { get; set; }
    }
}