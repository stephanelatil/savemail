using Backend.Models;
using Backend.Models.DTO;
using Backend.Services;
using MailKit.Security;
using Moq;
using Moq.EntityFrameworkCore;

namespace Backend.Tests.ServicesTest;

public class MailBoxServiceTest
{
    private static Mock<ApplicationDBContext> CreateMockContext(params MailBox[] mailboxes){
        Mock<ApplicationDBContext> context = new();
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
        MailBoxService service = new MailBoxService(context.Object);
        AppUser owner = new (){Id = Guid.Empty.ToString()};
        UpdateMailBox mb = new(){
            ImapDomain = "imap.mail.com",
            ImapPort = 143,
            Username = "example@mail.com",
            Password = "password1234",
            Provider = ImapProvider.Simple
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
        context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task ServiceTest_UpdateMailBox()
    {
        AppUser owner = new (){Id = Guid.Empty.ToString()};
        var baseMb = new MailBox(){
            Id = 123,
            ImapDomain = "imap.mail.com",
            ImapPort = 993,
            Username = "example@mail.com",
            Password = "password1234",
            Provider = ImapProvider.Simple
        };
        // Given
        var context = CreateMockContext(baseMb);
        MailBoxService service = new(context.Object);

        UpdateMailBox mb = new(){
            Id = baseMb.Id,
            ImapPort = 999,
            ImapDomain = "newimap.email.com",
            Username = "email2@mail.com",
            Password = "NewP@ssword123",
            Provider = ImapProvider.Simple
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
            ImapDomain = "imap.mail.com",
            ImapPort = 993,
            Username = "example@mail.com",
            Password = "password1234",
            Provider = ImapProvider.Simple
        };
        // Given
        var context = CreateMockContext(baseMb);
        MailBoxService service = new(context.Object);
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