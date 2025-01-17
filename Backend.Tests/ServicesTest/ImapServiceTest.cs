using Backend.Models.DTO;
using Backend.Services;
using FluentAssertions;
using Moq;

namespace Backend.Tests.ServicesTest;
public class MailboxImapCheckTests
{
    private readonly MailboxImapCheck _mailboxImapCheck;

    public MailboxImapCheckTests()
    {
        this._mailboxImapCheck = new MailboxImapCheck();
    }

    [Fact]
    public async Task CheckConnection_ShouldReturnNullValue_WhenMailboxIsNull()
    {
        // Act
        var result = await this._mailboxImapCheck.CheckConnection(null);

        // Assert
        result.Should().Be(ImapCheckResult.NullValue);
    }

    [Fact]
    public async Task CheckConnection_ShouldReturnNullValue_WhenMailboxPropertiesAreNull()
    {
        // Arrange
        var mailbox = new UpdateMailBox
        {
            ImapDomain = null,
            ImapPort = null,
            Username = null,
            Password = null
        };

        // Act
        var result = await this._mailboxImapCheck.CheckConnection(mailbox);

        // Assert
        result.Should().Be(ImapCheckResult.NullValue);
    }

    [Fact]
    public async Task CheckConnection_ShouldReturnInvalidValue_WhenInvalidArgumentsProvided()
    {
        // Arrange
        var mailbox = new UpdateMailBox
        {
            ImapDomain = "",
            ImapPort = -1,
            Username = "",
            Password = ""
        };

        // Act
        var result = await this._mailboxImapCheck.CheckConnection(mailbox);

        // Assert
        result.Should().Be(ImapCheckResult.InvalidValue);
    }

    [Fact]
    public async Task CheckConnection_ShouldReturnConnectionToServerError_WhenConnectionFails()
    {
        // Arrange
        var mailbox = new UpdateMailBox
        {
            ImapDomain = "imap.does.not.exist.com",
            ImapPort = 993,
            Username = "user",
            Password = "password"
        };

        // Act
        var result = await this._mailboxImapCheck.CheckConnection(mailbox);

        // Assert
        result.Should().Be(ImapCheckResult.ConnectionToServerError);
    }

    [Fact]
    public async Task CheckConnection_ShouldReturnAuthenticationError_WhenAuthenticationFails()
    {
        // Arrange
        var mailbox = new UpdateMailBox
        {
            ImapDomain = "localhost",
            ImapPort = 3143,
            Username = "non_existant_user@localhost",
            Password = "password"
        };

        // Act
        var result = await this._mailboxImapCheck.CheckConnection(mailbox);

        // Assert
        result.Should().Be(ImapCheckResult.AuthenticationError);
    }

    [Fact]
    public async Task CheckConnection_ShouldReturnSuccess_WhenConnectionAndAuthenticationSucceeds()
    {
        // Arrange
        var mailbox = new UpdateMailBox
        {
            ImapDomain = "localhost",
            ImapPort = 3143,
            Username = "test@localhost",
            Password = "password"
        };

        // Act
        var result = await this._mailboxImapCheck.CheckConnection(mailbox);

        // Assert
        result.Should().Be(ImapCheckResult.Success);
    }
}
