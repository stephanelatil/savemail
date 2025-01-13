using Backend.Models;
using Backend.Models.DTO;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.EntityFrameworkCore;

namespace Backend.Tests.ServicesTest;

public class MailBoxServiceTest
{
    private static Mock<ApplicationDBContext> CreateMockContext(params MailBox[] mailboxes){
        DbContextOptions<ApplicationDBContext> opt = new DbContextOptionsBuilder<ApplicationDBContext>()
                                                            .UseInMemoryDatabase("TestDb").Options;

        Mock<ApplicationDBContext> context = new(opt);
        context.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()).Result)
                    .Returns(1);

        context.SetupGet(c => c.MailBox).ReturnsDbSet(mailboxes);
        return context;
    }

    [Fact]
    public async Task ServiceTest_CreateMailBox()
    {
        // Given
        var context = CreateMockContext();

        Mock<ITokenEncryptionService> tokenServiceMock = new();
        tokenServiceMock.Setup(c => c.Encrypt(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                        .Returns((string pass, int id, string owner) => pass);
        tokenServiceMock.Setup(c => c.Decrypt(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                        .Returns((string pass, int id, string owner) => pass);
        var inMemorySettings = new Dictionary<string, string?> {
            {"AppSecret", "Value"},
            {"AttachmentsPath", "/tmp"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        MailBoxService service = new MailBoxService(context.Object, tokenServiceMock.Object, configuration);
        AppUser owner = new (){Id = Guid.Empty.ToString()};
        UpdateMailBox mb = new(){
            ImapDomain = "imap.mail.com",
            ImapPort = 143,
            Username = "example@mail.com",
            Password = "password1234"
        };

        // When
        var record = await Record.ExceptionAsync(
                                async () => 
                                await service.CreateMailBoxAsync(mb, owner));

        // Then
        //no exception thrown
        Assert.Null(record);
        //Assert saved to Db
        context.Verify(c=> c.MailBox.AddAsync(It.IsAny<MailBox>(), It.IsAny<CancellationToken>()));
        //twice because ID is needed to encrypt password
        context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        tokenServiceMock.Verify(t => t.Encrypt(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once());
        //ensure password is encrypted
        tokenServiceMock.Verify(t => t.Decrypt(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never());
    }

    [Fact]
    public async Task ServiceTest_UpdateMailBox()
    {
        AppUser owner = new (){Id = Guid.Empty.ToString()};
        var baseMb = new MailBox(){
            Id = 123,
            ImapDomain = "localhost",
            ImapPort = 3143,
            Username = "test2@localhost",
            Password = "password2",
            Provider = ImapProvider.Simple
        };
        // Given
        var context = CreateMockContext(baseMb);
        Mock<ITokenEncryptionService> tokenServiceMock = new();
        tokenServiceMock.Setup(c => c.Encrypt(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                        .Returns((string pass, int id, string owner) => pass);
        tokenServiceMock.Setup(c => c.Decrypt(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                        .Returns((string pass, int id, string owner) => pass);
        var inMemorySettings = new Dictionary<string, string?> {
            {"AppSecret", "Value"},
            {"AttachmentsPath", "/tmp"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        MailBoxService service = new(context.Object, tokenServiceMock.Object, configuration);

        UpdateMailBox mb = new(){
            Id = baseMb.Id,
            ImapPort = 999,
            ImapDomain = "newimap.email.com",
            Username = "email2@mail.com",
            Password = "NewP@ssword123"
        };

        // When
        var record = await Record.ExceptionAsync(
                                async () => 
                                await service.UpdateMailBoxAsync(baseMb.Id, mb));

        // Then
        //no exception thrown
        Assert.Null(record);
        //Assert saved to Db
        context.VerifyGet(c => c.MailBox, Times.AtLeastOnce());
        context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        context.Verify(c => c.TrackEntry(It.IsAny<MailBox>()), Times.Once());
    }

    [Fact]
    public async Task ServiceTest_DeleteMailBox()
    {
        AppUser owner = new (){Id = Guid.Empty.ToString()};
        var baseMb = new MailBox(){
            Id = 123,
            ImapDomain = "localhost",
            ImapPort = 3143,
            Username = "test2@localhost",
            Password = "password2",
            Provider = ImapProvider.Simple
        };
        // Given
        var context = CreateMockContext(baseMb);
        Mock<ITokenEncryptionService> tokenServiceMock = new();
        tokenServiceMock.Setup(c => c.Encrypt(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                        .Returns((string pass, int id, string owner) => pass);
        tokenServiceMock.Setup(c => c.Decrypt(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                        .Returns((string pass, int id, string owner) => pass);
        var inMemorySettings = new Dictionary<string, string?> {
            {"AppSecret", "Value"},
            {"AttachmentsPath", "/tmp"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        MailBoxService service = new(context.Object, tokenServiceMock.Object, configuration);
        // When
        var record = await Record.ExceptionAsync(
                                async () => 
                                await service.DeleteMailBoxAsync(baseMb));

        // Then
        //no exception thrown
        Assert.Null(record);
        //Assert saved to Db
        context.Verify(c => c.MailBox.Remove(It.Is<MailBox>(x => x.Id == baseMb.Id)), Times.AtLeastOnce());
        context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }
}