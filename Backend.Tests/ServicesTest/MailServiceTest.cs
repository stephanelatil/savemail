using Backend.Models;
using Backend.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;

namespace Backend.Tests.ServicesTest;
public class MailServiceTests :PostgresTestcontainerBase
{
    private readonly Mock<ApplicationDBContext> _mockContext;
    private readonly Mock<IAttachmentService> _mockAttachmentService;
    private readonly Mock<ILogger<MailService>> _mockLogger;
    private readonly MailService _mailService;
    private readonly Mock<DbSet<Mail>> mockMailSet = new();
    private readonly Mock<DbSet<Attachment>> mockAttachmentSet = new();
    private readonly Mock<DbSet<EmailAddress>> mockEmailSet = new();

    public MailServiceTests() : base()
    {
        this._mockContext = CreateMockContext();
        this._mockAttachmentService = new Mock<IAttachmentService>();
        this._mockLogger = new Mock<ILogger<MailService>>();

        this._mailService = new MailService(this._mockContext.Object, this._mockAttachmentService.Object, this._mockLogger.Object);
    }

    private Mock<ApplicationDBContext> CreateMockContext(params Mail[] mails){
        DbContextOptions<ApplicationDBContext> opt = new DbContextOptionsBuilder<ApplicationDBContext>()
                                                            .UseInMemoryDatabase("TestDb").Options;

        Mock<ApplicationDBContext> context = new(opt);
        context.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()).Result)
                    .Returns(1);

        Mock<DbSet<Mail>> mockMailSet = new(){ };
                    
        context.SetupGet(c => c.Mail).ReturnsDbSet(mails, this.mockMailSet);
        context.SetupGet(c => c.EmailAddress).ReturnsDbSet([], this.mockEmailSet);
        context.SetupGet(c => c.Attachment).ReturnsDbSet([], this.mockAttachmentSet);
        return context;
    }

    // private static Mock<ApplicationDBContext> CreateMockContext(params Mail[] mails){
    //     Mock<ApplicationDBContext> context = new();
    //     context.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()).Result)
    //                 .Returns(1);

    //     context.SetupGet(c => c.Mail).ReturnsDbSet(mails);
    //     context.SetupGet(c => c.EmailAddress).ReturnsDbSet([]);
    //     context.SetupGet(c => c.Attachment).ReturnsDbSet([]);
    //     return context;
    // }

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

    [Fact]
    public async Task SaveMail_AddsNewMailsToDatabase()
    {
        // Arrange
        var context = await this.CreateContextAsync();
        AppUser owner = new ();
        context.Users.Add(owner);
        MailBox mb = new(){ Owner = owner};
        context.MailBox.Add(mb);
        await context.SaveChangesAsync();
        var mails = new List<Mail>
        {
            new () { Body = "123", OwnerMailBox=mb },
            new () { Body = "456", OwnerMailBox=mb }
        };
        this._mockAttachmentService.Setup(a => a.SaveAttachments(It.IsAny<List<Mail>>(), owner.Id)).Returns(Task.CompletedTask);

        // Act
        await new MailService(context, this._mockAttachmentService.Object, new Mock<ILogger<MailService>>().Object)
                .SaveMail(mails, owner.Id);

        // Assert
        this._mockAttachmentService.Verify(a => a.SaveAttachments(It.IsAny<List<Mail>>(), owner.Id), Times.Once);
        foreach (var body in context.Mail.Select(m => m.Body))
            mails.Select(m=>m.Body).Should().Contain(body);
    }

    [Fact]
    public async Task SaveMail_HandlesEmailAddressesCorrectly()
    {
        // Arrange
        var context = await this.CreateContextAsync();
        AppUser owner = new ();
        context.Users.Add(owner);
        MailBox mb = new(){ Owner = owner};
        context.MailBox.Add(mb);
        await context.SaveChangesAsync();
        var mail = new Mail
        {
            OwnerMailBox = mb,
            Sender = new EmailAddress { Address = "sender@test.com" },
            Recipients = [new EmailAddress { Address = "recipient@test.com" }],
            RecipientsCc = [new EmailAddress { Address = "cc@test.com" }]
        };

        // Act
        await new MailService(context, this._mockAttachmentService.Object, new Mock<ILogger<MailService>>().Object)
                .SaveMail([mail], "user123");

        // Assert
        context.EmailAddress.Count().Should().Be(3);
    }
    /// <summary>
    /// Tests GetOrCreateEmailAddresses when the address exists in the database
    /// </summary>
    [Fact]
    public async Task GetOrCreateEmailAddresses_ExistingInDatabase_ReturnsExisting()
    {
        // Arrange
        var existingAddress = new EmailAddress { Address = "test@example.com" };
        var context = await this.CreateContextAsync();
        context.EmailAddress.Add(existingAddress);
        var service = new MailService(context, this._mockAttachmentService.Object, this._mockLogger.Object);

        // Act
        var result = await service.GetOrCreateEmailAddresses(
            new EmailAddress { Address = "test@example.com" }
        );

        // Assert
        result.Should().Be(existingAddress);
    }

    /// <summary>
    /// Tests GetOrCreateEmailAddresses when the address doesn't exist
    /// </summary>
    [Fact]
    public async Task GetOrCreateEmailAddresses_NewAddress_CreatesNew()
    {
        // Arrange
        var newAddress = new EmailAddress { Address = "new@example.com" };
        var context = await this.CreateContextAsync();
        this.mockEmailSet.Setup(s => s.Add(It.IsAny<EmailAddress>()))
            .Returns((EmailAddress a) => context.EmailAddress.Add(a));
        this._mockContext.Setup(c => c.ChangeTracker).Returns(context.ChangeTracker);

        // Act
        var result = await this._mailService.GetOrCreateEmailAddresses(newAddress);

        // Assert
        this._mockContext.Verify(
            c => c.EmailAddress.Add(It.Is<EmailAddress>(a => a.Address == "new@example.com")), 
            Times.Once
        );
    }

    /// <summary>
    /// Tests InsertReply when there's no existing reply chain
    /// </summary>
    [Fact]
    public async Task InsertReply_NoExistingChain_SetsAsDirectReply()
    {
        // Arrange
        var parent = new Mail { Id = 1 };
        var reply = new Mail { Id = 2 };

        // Act
        await this._mailService.InsertReply(reply, parent);

        // Assert
        parent.HasReply.Should().BeTrue();
        parent.Reply.Should().Be(reply);
        reply.RepliedFrom.Should().Be(parent);
    }

    /// <summary>
    /// Tests InsertReply when there's an existing reply chain
    /// </summary>
    [Fact]
    public async Task InsertReply_ExistingChain_AppendsToEnd()
    {
        var context = await this.CreateContextAsync();
        // Arrange
        var originalMail = new Mail { Id = 1 };
        var existingReply = new Mail { Id = 2 };
        var newReply = new Mail { Id = 3 };

        originalMail.HasReply = true;
        originalMail.Reply = existingReply;

        context.Mail.Add(existingReply);
        context.Mail.Add(originalMail);

        MailService service = new (context, this._mockAttachmentService.Object, this._mockLogger.Object);

        // Act
        await service.InsertReply(newReply, originalMail);

        // Assert
        existingReply.HasReply.Should().BeTrue();
        existingReply.Reply.Should().Be(newReply);
        newReply.RepliedFrom.Should().Be(existingReply);
    }

    /// <summary>
    /// Tests GetMailsToAdd with no existing mails
    /// </summary>
    [Fact]
    public async Task GetMailsToAdd_NoExistingMails_ReturnsAllMails()
    {
        // Arrange
        var mails = new List<Mail>
        {
            new() { Body = "body1" },
            new() { Body = "body2" }
        };

        // Act
        var result = await this._mailService.GetMailsToAdd(mails);

        // Assert
        result.Should().BeEquivalentTo(mails);
        result.All(m => m.Id == 0).Should().BeTrue();
    }

    /// <summary>
    /// Tests GetMailsToAdd with some existing mails
    /// </summary>
    [Fact]
    public async Task GetMailsToAdd_SomeExistingMails_ReturnsOnlyNewMails()
    {
        // Arrange
        var existingMail = new Mail 
        { 
            Id = 1, 
            DateSent = new DateTime(2025,1,2,3,4,5,DateTimeKind.Utc),
            Subject = "Test",
            Body = "Body"
        };

        var mails = new List<Mail>
        {
            new() { 
                DateSent = existingMail.DateSent,
                Subject = existingMail.Subject,
                Body = existingMail.Body
            },
            new() { Body = "test" }
        };

        this._mockContext.Setup(c => c.Mail).ReturnsDbSet([existingMail]);

        // Act
        var result = await this._mailService.GetMailsToAdd(mails);

        // Assert
        result.Should().HaveCount(1);
        result.Single().Body.Should().Be("test");
    }

    private static Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<EmailAddress> MockEntityEntry(EmailAddress address)
    {
        var mockEntry = new Mock<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<EmailAddress>>();
        mockEntry.Setup(e => e.Entity).Returns(address);
        return mockEntry.Object;
    }

    /// <summary>
    /// Tests the full email address handling flow with a real database
    /// </summary>
    [Fact]
    public async Task HandleEmailAddresses_WithRealDatabase_ManagesAddressesCorrectly()
    {
        // Arrange
        await using var context = await this.CreateContextAsync();
        AppUser owner = new ();
        context.Users.Add(owner);
        MailBox mb = new(){ Owner = owner};
        context.MailBox.Add(mb);
        await context.SaveChangesAsync();
        var service = new MailService(context, this._mockAttachmentService.Object, this._mockLogger.Object);

        var mail = new Mail
        {
            OwnerMailBox = mb,
            Sender = new EmailAddress { Address = "sender@test.com" },
            Recipients = [ new() { Address = "recipient1@test.com" },
                            new() { Address = "recipient2@test.com" }],
            RecipientsCc = [new() { Address = "cc@test.com" } ]
        };

        // Act
        await service.HandleEmailAddresses(mail);
        await context.SaveChangesAsync();

        // Assert
        var addresses = await context.EmailAddress.ToListAsync();
        addresses.Should().HaveCount(4);
        addresses.Select(a => a.Address).Should().Contain([
            "sender@test.com",
            "recipient1@test.com",
            "recipient2@test.com",
            "cc@test.com"
        ]);
    }

    /// <summary>
    /// Tests the reply chain handling with a real database
    /// </summary>
    [Fact]
    public async Task InsertReply_WithRealDatabase_HandlesReplyChainCorrectly()
    {
        // Arrange
        await using var context = await this.CreateContextAsync();
        AppUser owner = new ();
        context.Users.Add(owner);
        MailBox mb = new(){ Owner = owner};
        context.MailBox.Add(mb);
        await context.SaveChangesAsync();
        var service = new MailService(context, this._mockAttachmentService.Object, this._mockLogger.Object);

        var originalMail = new Mail { Subject = "Original", OwnerMailBox = mb };
        var firstReply = new Mail { Subject = "First Reply", OwnerMailBox = mb };
        var secondReply = new Mail { Subject = "Second Reply", OwnerMailBox = mb };

        context.Mail.Add(originalMail);
        await context.SaveChangesAsync();

        // Act
        await service.InsertReply(firstReply, originalMail);
        await service.InsertReply(secondReply, originalMail);
        await context.SaveChangesAsync();

        // Assert
        var loadedOriginal = await context.Mail
            .Include(m => m.Reply)
            .FirstAsync(m => m.Subject == "Original");

        loadedOriginal.HasReply.Should().BeTrue();
        loadedOriginal.Reply.Should().NotBeNull();
        loadedOriginal.Reply.Subject.Should().Be("First Reply");
        loadedOriginal.Reply.Reply.Should().NotBeNull();
        loadedOriginal.Reply.Reply.Subject.Should().Be("Second Reply");
    }

    /// <summary>
    /// Tests duplicate mail detection with a real database
    /// </summary>
    [Fact]
    public async Task GetMailsToAdd_WithRealDatabase_DetectsDuplicatesCorrectly()
    {
        // Arrange
        await using var context = await this.CreateContextAsync();
        AppUser owner = new ();
        context.Users.Add(owner);
        MailBox mb = new(){ Owner = owner};
        context.MailBox.Add(mb);
        await context.SaveChangesAsync();
        var service = new MailService(context, this._mockAttachmentService.Object, this._mockLogger.Object);

        var existingMail = new Mail
        {
            OwnerMailBox = mb,
            DateSent = new DateTime(2025,1,1,1,1,1,DateTimeKind.Utc),
            Subject = "Test",
            Body = "Content"
        };

        context.Mail.Add(existingMail);
        await context.SaveChangesAsync();

        var mails = new List<Mail>
        {
            new()
            {
                OwnerMailBox = mb,
                DateSent = existingMail.DateSent,
                Subject = existingMail.Subject,
                Body = existingMail.Body
            },
            new()
            {
                OwnerMailBox = mb,
                DateSent = new DateTime(2025,1,2,3,4,5,DateTimeKind.Utc),
                Subject = "New",
                Body = "New Content"
            }
        };

        // Act
        var result = await service.GetMailsToAdd(mails);

        // Assert
        result.Should().HaveCount(1);
        result.Single().UniqueHash.Should().Be(mails[1].UniqueHash);
    }
}
