using Backend.Models;
using Backend.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;

namespace Backend.Tests.ServicesTest;
public class MailServiceTests
{
    private readonly Mock<ApplicationDBContext> _mockContext;
    private readonly Mock<IAttachmentService> _mockAttachmentService;
    private readonly Mock<ILogger<MailService>> _mockLogger;
    private readonly MailService _mailService;

    public MailServiceTests()
    {
        this._mockContext = CreateMockContext();
        this._mockAttachmentService = new Mock<IAttachmentService>();
        this._mockLogger = new Mock<ILogger<MailService>>();

        this._mailService = new MailService(this._mockContext.Object, this._mockAttachmentService.Object, this._mockLogger.Object);
    }

    private static Mock<ApplicationDBContext> CreateMockContext(params Mail[] mails){
        Mock<ApplicationDBContext> context = new();
        context.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()).Result)
                    .Returns(1);
                    
        context.SetupGet(c => c.Mail).ReturnsDbSet(mails);
        context.SetupGet(c => c.EmailAddress).ReturnsDbSet([]);
        context.SetupGet(c => c.Attachment).ReturnsDbSet([]);
        return context;
    }

    [Fact]
    public async Task GetMail_ById_ReturnsCorrectMail()
    {
        // Arrange
        var mailId = 1L;
        var expectedMail = new Mail { Id = mailId, Subject = "Test Subject" };

        this._mockContext.Setup(c => c.Mail).ReturnsDbSet([expectedMail]);

        // Act
        var result = await this._mailService.GetMail(mailId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedMail);
    }

    [Fact]
    public async Task GetMail_ByUniqueId_ReturnsCorrectMail()
    {
        // Arrange
        var uniqueId = new MailKit.UniqueId(1234);
        var expectedMail = new Mail { Id = 1, ImapMailUID = uniqueId };

        this._mockContext.Setup(c => c.Mail).ReturnsDbSet([expectedMail]);

        // Act
        var result = await this._mailService.GetMail(uniqueId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedMail);
    }

    [Fact]
    public async Task DeleteMailAsync_RemovesMailFromDatabase()
    {
        // Arrange
        var mailToDelete = new Mail { Id = 1, Subject = "To Be Deleted" };

        this._mockContext.Setup(c => c.Mail).ReturnsDbSet([mailToDelete]);

        // Act
        await this._mailService.DeleteMailAsync(mailToDelete);

        // Assert
        this._mockContext.Verify(c => c.Mail.Remove(mailToDelete), Times.Once);
        this._mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // [Fact]
    // public async Task SaveMail_AddsNewMailsToDatabase()
    // {
    //     // Arrange
    //     var ownerUserId = "user123";
    //     var mails = new List<Mail>
    //     {
    //         new Mail { Id = 1 },
    //         new Mail { Id = 2 }
    //     };

    //     this._mockAttachmentService.Setup(a => a.SaveAttachments(It.IsAny<List<Mail>>(), ownerUserId)).Returns(Task.CompletedTask);

    //     // Act
    //     await this._mailService.SaveMail(mails, ownerUserId);

    //     // Assert
    //     this._mockContext.Verify(c => c.Mail.AddRangeAsync(It.IsAny<IEnumerable<Mail>>(), It.IsAny<CancellationToken>()), Times.Once);
    //     this._mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    //     this._mockAttachmentService.Verify(a => a.SaveAttachments(It.IsAny<List<Mail>>(), ownerUserId), Times.Once);
    // }

    // [Fact]
    // public async Task SaveMail_HandlesEmailAddressesCorrectly()
    // {
    //     // Arrange
    //     var mail = new Mail
    //     {
    //         Sender = new EmailAddress { Address = "sender@test.com" },
    //         Recipients = new List<EmailAddress> { new EmailAddress { Address = "recipient@test.com" } },
    //         RecipientsCc = new List<EmailAddress> { new EmailAddress { Address = "cc@test.com" } }
    //     };

    //     // Act
    //     await this._mailService.SaveMail(new List<Mail> { mail }, "user123");

    //     // Assert
    //     this._mockContext.Verify(c => c.EmailAddress.Add(It.IsAny<EmailAddress>()), Times.Exactly(3));
    // }
    
    //TODO Integration tests with InMemory DB
}
