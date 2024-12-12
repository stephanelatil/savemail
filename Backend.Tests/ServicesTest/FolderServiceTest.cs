using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Models;
using Backend.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MailKit;
using Xunit;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq.EntityFrameworkCore;

namespace Backend.Tests.Services;
/// <summary>
/// Unit tests for the FolderService class.
/// </summary>
public class FolderServiceTests
{
    private readonly Mock<ApplicationDBContext> _mockContext;
    private readonly Mock<ILogger<FolderService>> _mockLogger;
    private readonly FolderService _folderService;

    public FolderServiceTests()
    {
        this._mockContext = CreateMockContext();
        this._mockLogger = new Mock<ILogger<FolderService>>();
        this._folderService = new FolderService(this._mockContext.Object, this._mockLogger.Object);
    }

    private static Mock<ApplicationDBContext> CreateMockContext(params Folder[] folders){
        Mock<ApplicationDBContext> context = new();
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
            Folders = new List<Folder>() 
        };
        var newFolder = new Folder 
        { 
            Path = "Test/NewFolder", 
            MailBoxId = mailbox.Id 
        };

        // Mock MailBox DbSet with Include simulation
        var mailboxList = new List<MailBox> { mailbox }.AsQueryable();

        // Simulate Include behavior
        var mockIncludableQueryable = new Mock<IQueryable<MailBox>>();

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
        result.Path.Should().Be("Test/NewFolder");
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
}
