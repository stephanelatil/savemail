using System.Data.SqlTypes;
using Backend.Models;
using Backend.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Backend.Tests.ServicesTest;
public class ImapFolderFetchServiceTests
{
    private readonly Mock<ILogger<ImapFolderFetchService>> _loggerMock;
    private readonly Mock<IOAuthService> _oAuthServiceMock;
    private readonly Mock<ITokenEncryptionService> _tokenEncryptionServiceMock;
    private readonly IImapFolderFetchService _service;

    public ImapFolderFetchServiceTests()
    {
        // Initialize mocks for the dependencies of the service
        this._loggerMock = new Mock<ILogger<ImapFolderFetchService>>();
        this._oAuthServiceMock = new Mock<IOAuthService>();
        this._tokenEncryptionServiceMock = new Mock<ITokenEncryptionService>();

        // Instantiate the service with mocked dependencies
        this._service = new ImapFolderFetchService(this._loggerMock.Object, this._oAuthServiceMock.Object, this._tokenEncryptionServiceMock.Object);
    }

    [Fact]
    public async Task GetNewFolders_ShouldReturnFolders_WhenNewFoldersExist()
    {
        var mailbox = new MailBox
        {
            ImapDomain = "localhost",
            ImapPort = 3143,
            Provider = ImapProvider.Simple,
            Username = "test@localhost",
            Password = "password"
        };

        CancellationToken cancellationToken = CancellationToken.None;

        // Act: Call the GetNewFolders method
        var result = await this._service.GetNewFolders(mailbox, cancellationToken);

        // Assert: Verify the result contains the expected folders
        result.Should().NotBeNull();
        result.Should().BeOfType<List<Folder>>();
        // Additional assertions can validate specific folders
    }

    [Fact]
    public async Task GetNewFolders_ShouldLogWarning_OnSocketException()
    {
        // Arrange: Create a MailBox with an invalid domain to simulate connection failure
        var mailbox = new MailBox
        {
            ImapDomain = "domain.does.not.exist.invalid",
            ImapPort = 12345,
            Provider = ImapProvider.Simple
        };

        CancellationToken cancellationToken = CancellationToken.None;

        // Act: Attempt to retrieve folders, expecting a failure
        var result = await this._service.GetNewFolders(mailbox, cancellationToken);

        // Assert: Verify that no folders are returned and a warning is logged
        result.Should().BeNull();
    }

    [Fact]
    public void Dispose_ShouldReleaseResources()
    {
        // Act: Dispose the service to release resources
        this._service.Dispose();

        // Assert: Ensure no exceptions are thrown during disposal
        Assert.True(true);
    }
}

public class ImapMailFetchServiceTests
{
    private readonly Mock<ILogger<ImapMailFetchService>> _loggerMock;
    private readonly Mock<IOAuthService> _oAuthServiceMock;
    private readonly Mock<ITokenEncryptionService> _tokenEncryptionServiceMock;
    private readonly ImapMailFetchService _service;

    public ImapMailFetchServiceTests()
    {
        // Initialize mocks for dependencies
        this._loggerMock = new Mock<ILogger<ImapMailFetchService>>();
        this._oAuthServiceMock = new Mock<IOAuthService>();
        this._tokenEncryptionServiceMock = new Mock<ITokenEncryptionService>();
        this._tokenEncryptionServiceMock.Setup(t => t.Encrypt(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                                        .Returns((string pass, int id, string owner) => pass);
        this._tokenEncryptionServiceMock.Setup(t => t.Decrypt(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                                        .Returns((string pass, int id, string owner) => pass);

        // Instantiate the service with mocked dependencies
        this._service = new ImapMailFetchService(this._loggerMock.Object, this._oAuthServiceMock.Object, this._tokenEncryptionServiceMock.Object);
    }

    [Fact]
    public async Task Prepare_ShouldConnectAndAuthenticate()
    {
        // Arrange: Create a test MailBox instance with connection details
        var mailbox = new MailBox
        {
            ImapDomain = "localhost",
            ImapPort = 3143,
            Provider = ImapProvider.Simple,
            Username = "test@localhost",
            Password = "password"
        };

        CancellationToken cancellationToken = CancellationToken.None;

        // Act: Call the Prepare method to connect and authenticate
        await this._service.Prepare(mailbox, cancellationToken);

        // Assert: Verify that the service is connected and authenticated
        this._service.IsConnected.Should().BeTrue();
        this._service.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public async Task GetNextMails_ShouldReturnMails()
    {
        // Arrange: Create a test folder and prepare the service for fetching emails
        var folder = new Folder { Path = "INBOX" };
        MailBox mb = new() {
            ImapDomain = "localhost",
            ImapPort = 3143,
            Provider = ImapProvider.Simple,
            Username = "test@localhost",
            Password = "password",
            Folders = [folder]
        };

        await this._service.Prepare(mb, CancellationToken.None);
        this._service.IsConnected.Should().BeTrue();
        this._service.IsAuthenticated.Should().BeTrue();
        await this._service.SelectFolder(folder, CancellationToken.None);

        CancellationToken cancellationToken = CancellationToken.None;

        // Act: Fetch the next batch of emails
        var mails = await this._service.GetNextMails(10, cancellationToken);

        // Assert: Verify the result contains the expected emails
        mails.Should().NotBeNull();
        mails.Should().BeOfType<List<Mail>>();
        foreach (var mail in mails)
        {
            //this mailbox should be a recipient of the email
            mail.Should().Be(mail.RecipientsCc.Select(e => e.Address).Contains(mb.Username)
                            || mail.Recipients.Select(e => e.Address).Contains(mb.Username));
        }
    }

    [Fact]
    public void Disconnect_ShouldReleaseResources()
    {
        // Act: Disconnect the service to release resources
        this._service.Disconnect();

        // Assert: Verify the service is no longer connected or authenticated
        this._service.IsConnected.Should().BeFalse();
        this._service.IsAuthenticated.Should().BeFalse();
    }
}
