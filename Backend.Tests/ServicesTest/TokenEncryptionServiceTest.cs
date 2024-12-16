using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Backend.Services;

namespace Backend.Tests.ServicesTest;

public class TokenEncryptionServiceTests
{
    private readonly Mock<ILogger<TokenEncryptionService>> _loggerMock;
    private readonly IConfiguration _configuration;
    private readonly TokenEncryptionService _service;
    private const string TestAppSecret = "TEST_SECRET_KEY";

    public TokenEncryptionServiceTests()
    {
        this._loggerMock = new Mock<ILogger<TokenEncryptionService>>();
        var inMemorySettings = new Dictionary<string, string?> {
            {"AppSecret", "A_SECRET_STRING"}
        };

        this._configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        this._service = new TokenEncryptionService(this._loggerMock.Object, this._configuration);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public void Encrypt_WithEmptyOrNullToken_ReturnsOriginalToken(string token)
    {
        // Arrange
        const int id = 1;
        const string ownerId = "owner123";

        // Act
        var result = this._service.Encrypt(token, id, ownerId);

        // Assert
        result.Should().Be(token);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public void Decrypt_WithEmptyOrNullToken_ReturnsOriginalToken(string token)
    {
        // Arrange
        const int id = 1;
        const string ownerId = "owner123";

        // Act
        var result = this._service.Decrypt(token, id, ownerId);

        // Assert
        result.Should().Be(token);
    }

    [Theory]
    [InlineData("simplePassword123")]
    [InlineData("Complex!P@ssw0rd")]
    [InlineData("user-token-12345")]
    [InlineData("Bearer abc123.def456.ghi789")]
    public void EncryptDecrypt_WithVariousTokenTypes_ReturnsOriginalValue(string originalToken)
    {
        // Arrange
        const int id = 1;
        const string ownerId = "owner123456789012345"; // 16+ chars for IV

        // Act
        var encrypted = this._service.Encrypt(originalToken, id, ownerId);
        var decrypted = this._service.Decrypt(encrypted, id, ownerId);

        // Assert
        encrypted.Should().NotBe(originalToken);
        encrypted.Should().NotBeNullOrEmpty();
        decrypted.Should().Be(originalToken);
    }

    [Fact]
    public void Encrypt_WithShortOwnerId_ReturnsOriginalToken()
    {
        // Arrange
        const string originalToken = "myPassword123";
        const int id = 1;
        const string shortOwnerId = "short"; // Less than 16 chars

        // Act
        var result = this._service.Encrypt(originalToken, id, shortOwnerId);

        // Assert
        result.Should().Be(originalToken);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Encrypt_WithInvalidOwnerId_ReturnsOriginalToken(string ownerId)
    {
        // Arrange
        const string originalToken = "myPassword123";
        const int id = 1;

        // Act
        var result = this._service.Encrypt(originalToken, id, ownerId);

        // Assert
        result.Should().Be(originalToken);
    }

    [Fact]
    public void Decrypt_WithInvalidBase64_ReturnsOriginalToken()
    {
        // Arrange
        const string invalidToken = "not-a-valid-base64!@#$";
        const int id = 1;
        const string ownerId = "owner123456789012345";

        // Act
        var result = this._service.Decrypt(invalidToken, id, ownerId);

        // Assert
        result.Should().Be(invalidToken);
    }

    [Fact]
    public void Decrypt_WithWrongKey_ReturnsOriginalToken()
    {
        // Arrange
        const string originalToken = "myPassword123";
        const string ownerId = "owner123456789012345";
        
        // Encrypt with ID 1
        var encrypted = this._service.Encrypt(originalToken, 1, ownerId);
        
        // Try to decrypt with different ID (different key)
        var result = this._service.Decrypt(encrypted, 2, ownerId);

        // Assert
        result.Should().Be(encrypted);
    }

    [Fact]
    public void Decrypt_WithWrongOwnerId_ReturnsOriginalToken()
    {
        // Arrange
        const string originalToken = "myPassword123";
        const int id = 1;
        
        // Encrypt with one owner ID
        var encrypted = this._service.Encrypt(originalToken, id, "owner123456789012345");
        
        // Try to decrypt with different owner ID (different IV)
        var result = this._service.Decrypt(encrypted, id, "different12345678901");

        // Assert
        result.Should().Be(encrypted);
    }

    [Fact]
    public void Constructor_WithNullConfiguration_UsesDefaultKey()
    {

        // Act
        var service = new TokenEncryptionService(this._loggerMock.Object, this._configuration);
        const string testToken = "testPassword123";
        const string ownerId = "owner123456789012345";
        
        var encrypted = service.Encrypt(testToken, 1, ownerId);
        var decrypted = service.Decrypt(encrypted, 1, ownerId);

        // Assert
        encrypted.Should().NotBe(testToken);
        decrypted.Should().Be(testToken);
    }

    [Theory]
    [InlineData("password123", -1)]
    [InlineData("password123", 0)]
    [InlineData("password123", int.MinValue)]
    public void EncryptDecrypt_WithNonPositiveId_StillWorks(string originalToken, int id)
    {
        // Arrange
        const string ownerId = "owner123456789012345";

        // Act
        var encrypted = this._service.Encrypt(originalToken, id, ownerId);
        var decrypted = this._service.Decrypt(encrypted, id, ownerId);

        // Assert
        encrypted.Should().NotBe(originalToken);
        decrypted.Should().Be(originalToken);
    }

    [Fact]
    public void EncryptDecrypt_WithUnicodePasswords_WorksCorrectly()
    {
        // Arrange
        const string originalToken = "пароль123!@#";  // Russian characters
        const int id = 1;
        const string ownerId = "owner123456789012345";

        // Act
        var encrypted = this._service.Encrypt(originalToken, id, ownerId);
        var decrypted = this._service.Decrypt(encrypted, id, ownerId);

        // Assert
        decrypted.Should().Be(originalToken);
    }
}