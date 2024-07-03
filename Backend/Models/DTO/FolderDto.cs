namespace Backend.Models.DTO
{
    public class FolderDto
    {
        public int Id { get; set; }
        public string Name { get; private set;} = string.Empty;
        public string Path { get; set; } = string.Empty;
        public List<Folder> Children { get; set; } = [];

        public FolderDto(){}
        
        public FolderDto(Folder folder)
        {
            this.Id = folder.Id;
            this.Name = folder.Name;
            this.Path = folder.Path;
            this.Children = folder.Children;
        }
    }
}