using Backend.Services;
using Backend.Models;
using Backend.Models.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.EntityFrameworkCore;
using FluentAssertions;
using System.Security.Claims;

namespace Backend.Tests.ServicesTest;
public class UserServiceTests
{
    private readonly Mock<ApplicationDBContext> _mockContext;
    private readonly Mock<UserManager<AppUser>> _mockUserManager;
    private readonly Mock<ITokenEncryptionService> _mockTokenEncryptionService;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        this._mockContext = CreateMockContext();
        this._mockUserManager = new Mock<UserManager<AppUser>>(
            Mock.Of<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);
        this._mockTokenEncryptionService = new Mock<ITokenEncryptionService>();

        var inMemorySettings = new Dictionary<string, string?> {
            {"AppSecret", "Value"},
            {"AttachmentsPath", "/tmp"}
        };

        var _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        this._userService = new UserService(this._mockContext.Object, this._mockUserManager.Object, _configuration);
    }

    private static Mock<ApplicationDBContext> CreateMockContext(params AppUser[] users){
        Mock<ApplicationDBContext> context = new();
        context.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()).Result)
                    .Returns(1);

        context.SetupGet(c => c.Users).ReturnsDbSet(users);
        return context;
    }

    [Fact]
    public async Task GetUserByIdAsync_ReturnsUser_WhenUserExists()
    {
        // Arrange
        var userId = "123";
        var expectedUser = new AppUser { Id = userId };
        this._mockContext.Setup(c => c.Users).ReturnsDbSet([expectedUser]);

        // Act
        var result = await this._userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task GetUserByIdAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = "123";
        this._mockContext.Setup(c => c.Users).ReturnsDbSet(Array.Empty<AppUser>());

        // Act
        var result = await this._userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateUserAsync_UpdatesUser_WhenValidDtoIsProvided()
    {
        // Arrange
        var userId = "123";
        var existingUser = new AppUser { Id = userId, FirstName = "Old" };
        var updateDto = new UpdateAppUser { Id = userId, FirstName = "New" };

        this._mockContext.Setup(c => c.Users).ReturnsDbSet(new[] { existingUser });

        // Act
        await this._userService.UpdateUserAsync(updateDto);

        // Assert
        this._mockContext.Verify(c => c.Users.Update(It.Is<AppUser>(u => u.FirstName == "New")), Times.Once);
        this._mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_ThrowsKeyNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var updateDto = new UpdateAppUser { Id = "123" };
        this._mockContext.Setup(c => c.Users).ReturnsDbSet(Array.Empty<AppUser>());

        // Act
        Func<Task> act = async () => await this._userService.UpdateUserAsync(updateDto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteUserAsync_DeletesUser_WhenUserExists()
    {
        // Arrange
        var userId = "123";
        var user = new AppUser { Id = userId };
        this._mockContext.Setup(c => c.Users).ReturnsDbSet(new[] { user });

        // Act
        await this._userService.DeleteUserAsync(user);

        // Assert
        this._mockContext.Verify(c => c.Users.Remove(user), Times.Once);
        this._mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetUserByEmailAsync_ReturnsUser_WhenUserExists()
    {
        // Arrange
        var email = "test@example.com";
        var user = new AppUser { Email = email };
        this._mockContext.Setup(c => c.Users).ReturnsDbSet(new[] { user });

        // Act
        var result = await this._userService.GetUserByEmailAsync(email);

        // Assert
        result.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetUserByClaimAsync_ReturnsUser_WhenClaimMatches()
    {
        // Arrange
        var userId = "123";
        var user = new AppUser { Id = userId };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));

        this._mockUserManager.Setup(m => m.GetUserAsync(claimsPrincipal)).ReturnsAsync(user);
        this._mockContext.Setup(c => c.Users).ReturnsDbSet(new[] { user });

        // Act
        var result = await this._userService.GetUserByClaimAsync(claimsPrincipal);

        // Assert
        result.Should().BeEquivalentTo(user);
    }
}
