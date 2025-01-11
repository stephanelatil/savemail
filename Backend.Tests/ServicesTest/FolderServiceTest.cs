using Backend.Models;
using Backend.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MailKit;
using Moq.EntityFrameworkCore;

namespace Backend.Tests.ServicesTest;
/// <summary>
/// Unit tests for the FolderService class.
/// </summary>
public class FolderServiceTests : PostgresTestcontainerBase
{
    private readonly Mock<ApplicationDBContext> _mockContext;
    private readonly Mock<ILogger<FolderService>> _mockLogger;
    private readonly FolderService _folderService;

    public FolderServiceTests() : base()
    {
        this._mockContext = CreateMockContext();
        this._mockLogger = new Mock<ILogger<FolderService>>();
        this._folderService = new FolderService(this._mockContext.Object, this._mockLogger.Object);
    }

    private static Mock<ApplicationDBContext> CreateMockContext(params Folder[] folders){
        DbContextOptions<ApplicationDBContext> opt = new DbContextOptionsBuilder<ApplicationDBContext>()
                                                            .UseInMemoryDatabase("TestDb").Options;

        Mock<ApplicationDBContext> context = new(opt);
        context.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()).Result)
                    .Returns(1);

        context.SetupGet(c => c.Folder).ReturnsDbSet(folders);
        return context;
    }

    /// <summary>
    /// Tests creating a folder within a mailbox.
    /// </summary>
    [Fact]
    public async Task CreateFolderAsync_NewFolder_CreatesSuccessfully()
    {
        // Arrange
        var mailbox = new MailBox
        {
            Id = 1,
            Folders = []
        };
        var newFolder = new Folder
        {
            Path = "Test",
            MailBoxId = mailbox.Id
        };

        // Mock MailBox DbSet with Include simulation
        var mailboxList = new List<MailBox> { mailbox }.AsQueryable();

        var mockFolderSet = new Mock<DbSet<Folder>>();
        mockFolderSet.Setup(m => m.Add(It.IsAny<Folder>()));

        this._mockContext.SetupGet(c => c.MailBox).ReturnsDbSet(mailboxList);
        this._mockContext.SetupGet(c => c.Folder).ReturnsDbSet([], mockFolderSet);
        this._mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this._folderService.CreateFolderAsync(newFolder, mailbox);

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be("Test");
        result.MailBoxId.Should().Be(mailbox.Id);
    }

    /// <summary>
    /// Tests creating a folder tree from a sub-folder.
    /// </summary>
    [Fact]
    public async Task CreateFolderAsync_NewFolderTreeFromSingle_CreatesSuccessfully()
    {
        // Arrange
        var mailbox = new MailBox
        {
            Id = 1,
            Folders = []
        };
        var newFolder = new Folder
        {
            Path = "Test/NewFolder/SubFolder",
            MailBoxId = mailbox.Id
        };

        // Mock MailBox DbSet with Include simulation
        var mailboxList = new List<MailBox> { mailbox }.AsQueryable();

        var mockFolderSet = new Mock<DbSet<Folder>>();
        mockFolderSet.Setup(m => m.Add(It.IsAny<Folder>()));

        this._mockContext.SetupGet(c => c.MailBox).ReturnsDbSet(mailboxList);
        this._mockContext.SetupGet(c => c.Folder).ReturnsDbSet([], mockFolderSet);
        this._mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await this._folderService.CreateFolderAsync(newFolder, mailbox);

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be("Test/NewFolder/SubFolder");
        mockFolderSet.Verify(f => f.Add(It.IsAny<Folder>()), Times.Exactly(3));
        result.Parent.Should().NotBeNull();
        result.Parent.Path.Should().Be("Test/NewFolder");
        result?.Parent?.Parent.Should().NotBeNull();
        result.Parent.Parent.Name.Should().Be("Test");
        result.MailBoxId.Should().Be(mailbox.Id);
    }

    /// <summary>
    /// Tests retrieving a folder by its ID successfully.
    /// </summary>
    [Fact]
    public async Task GetFolderByIdAsync_ExistingFolder_ReturnsFolderSuccessfully()
    {
        // Arrange
        var expectedFolder = new Folder { Id = 1, Path = "Test/Folder" };
        this._mockContext.Setup(c => c.Folder).ReturnsDbSet([expectedFolder]);

        // Act
        var result = await this._folderService.GetFolderByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result?.Id.Should().Be(1);
        result?.Path.Should().Be("Test/Folder");
    }

    /// <summary>
    /// Tests deleting a folder.
    /// </summary>
    [Fact]
    public async Task DeleteFolderAsync_ExistingFolder_DeletesSuccessfully()
    {
        // Arrange
        var folderToDelete = new Folder { Id = 1, Path = "Test/Folder" };

        var mockFolderSet = new Mock<DbSet<Folder>>();
        this._mockContext.Setup(c => c.Folder).Returns(mockFolderSet.Object);
        this._mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await this._folderService.DeleteFolderAsync(folderToDelete);

        // Assert
        mockFolderSet.Verify(m => m.Remove(folderToDelete), Times.Once);
        this._mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests updating last pull data for a folder.
    /// </summary>
    [Fact]
    public async Task UpdateLastPullDataAsync_UpdatesLastPulledData()
    {
        // Arrange
        var folder = new Folder { Id = 123, LastPulledUid = UniqueId.MinValue };
        var newLastUid = new UniqueId(10);
        var newLastDate = DateTime.UtcNow;

        this._mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await this._folderService.UpdateLastPullDataAsync(folder, newLastUid, newLastDate);

        // Assert
        folder.LastPulledUid.Should().Be(newLastUid);
        this._mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateFolderAsync_Internal_WithExistingFolder_ReturnsExistingFolder()
    {
        // Arrange
        var mailbox = new MailBox
        {
            Id = 1,
            Folders = [new() { Id = 1, Path = "ExistingFolder", MailBoxId = 1 }]
        };

        var folderToCreate = new Folder { Path = "ExistingFolder", MailBoxId = 1 };

        // Act
        var result = await this._folderService.CreateFolderAsync(folderToCreate, mailbox, true);

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be("ExistingFolder");
        result.Id.Should().Be(1);
        this._mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateFolderAsync_Internal_WithCancellation_ReturnsFolderWithoutSaving()
    {
        // Arrange
        var mailbox = new MailBox { Id = 1, Folders = [] };
        var folderToCreate = new Folder { Path = "NewFolder", MailBoxId = 1 };
        var cancellationToken = new CancellationToken(true);

        // Act
        var result = await this._folderService.CreateFolderAsync(folderToCreate, mailbox, true, cancellationToken);

        // Assert
        result.Should().BeNull();
        mailbox.Folders.Should().BeEmpty();
        this._mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateFolderAsync_Internal_WithNestedPath_CreatesParentFolders()
    {
        // Arrange
        var mailbox = new MailBox { Id = 1, Folders = [] };
        var folderToCreate = new Folder { Path = "Parent/Child/SubChild", MailBoxId = 1 };

        // Act
        var result = await this._folderService.CreateFolderAsync(folderToCreate, mailbox, true);

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be("Parent/Child/SubChild");
        result.Parent.Should().NotBeNull();
        result.Parent.Path.Should().Be("Parent/Child");
        result.Parent.Parent.Should().NotBeNull();
        result.Parent.Parent.Path.Should().Be("Parent");
        mailbox.Folders.Should().HaveCount(3);
        this._mockContext.Verify(c => c.Folder.Add(It.IsAny<Folder>()), Times.Exactly(3));
    }

    [Fact]
    public async Task CreateFolderAsync_Internal_WithInvalidPath_ThrowsArgumentException()
    {
        // Arrange
        var mailbox = new MailBox { Id = 1, Folders = [] };
        var folderToCreateNullPath = new Folder { Path = null, MailBoxId = 1 };
        var folderToCreateEmptyPath = new Folder { Path = " ", MailBoxId = 1 };
        var folderToCreate = new Folder { Path = "ABC", MailBoxId = 1 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            this._folderService.CreateFolderAsync(folderToCreateEmptyPath, mailbox, true));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            this._folderService.CreateFolderAsync(folderToCreateNullPath, mailbox, true));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            this._folderService.CreateFolderAsync(folderToCreate, null, true));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            this._folderService.CreateFolderAsync(null, mailbox, true));
    }

    [Fact]
    public async Task CreateFolderAsync_Integration_CreatesCompleteHierarchy()
    {
        // Arrange
        await using var context = await this.CreateContextAsync();
        var folderService = new FolderService(context, this._mockLogger.Object);
        
        var user = new AppUser() { };
        var mailbox = new MailBox() { Owner = user };
        context.Users.Add(user);
        context.MailBox.Add(mailbox);
        await context.SaveChangesAsync();

        var folderToCreate = new Folder { Path = "A/B/C", MailBoxId = mailbox.Id };

        // Act
        var result = await folderService.CreateFolderAsync(folderToCreate, mailbox, true);

        // Assert
        result.Should().NotBeNull();
        var folders = await context.Folder.ToListAsync();
        folders.Should().HaveCount(3);
        folders.Select(f => f.Path).Should().Contain(new[] { "A", "A/B", "A/B/C" });
    }

    [Fact]
    public async Task CreateFolderAsync_Integration_HandlesExistingFolders()
    {
        // Arrange
        await using var context = await this.CreateContextAsync();
        var folderService = new FolderService(context, this._mockLogger.Object);
        
        var user = new AppUser() { };
        var mailbox = new MailBox() { Owner = user };
        context.Users.Add(user);
        context.MailBox.Add(mailbox);
        await context.SaveChangesAsync();
        
        var existingFolder = new Folder { Path = "A", MailBoxId = mailbox.Id };
        context.Folder.Add(existingFolder);
        await context.SaveChangesAsync();

        var folderToCreate = new Folder { Path = "A/B", MailBoxId = mailbox.Id };

        // Act
        var result = await folderService.CreateFolderAsync(folderToCreate, mailbox, true);

        // Assert
        result.Should().NotBeNull();
        var folders = await context.Folder.ToListAsync();
        folders.Should().HaveCount(2);
        folders.Select(f => f.Path).Should().Contain(new[] { "A", "A/B" });
    }
}
