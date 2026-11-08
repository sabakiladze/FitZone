using FitZone.Application.Implementations;
using FitZone.Domain.CustomExceptions;
using FitZone.Domain.Entities.Models;
using FitZone.Domain.Interfaces;
using FitZone.Domain.RepositoryInterfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace FitZone.Testing.ServiceTests;

public class ProfileServiceTests
{
    private readonly Mock<IUserRepository> _users = new();

    private ProfileService CreateSut() => new(_users.Object);

    private static User CreateUser() => new()
    {
        UserName = "nika",
        FirstName = "Nika",
        LastName = "Beridze",
        Email = "nika@test.com",
        Role = new Role { RoleName = "User" }
    };

    [Fact]
    public async Task Get_EmptyId_Throws()
    {
        var act = () => CreateSut().GetAsync(Guid.Empty);

        await act.Should().ThrowAsync<EnterProperIdException>();
        _users.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_UnknownUser_Throws()
    {
        var id = Guid.NewGuid();
        _users.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var act = () => CreateSut().GetAsync(id);

        await act.Should().ThrowAsync<UserNotFoundException>();
    }

    [Fact]
    public async Task Get_DeletedUser_Throws()
    {
        var user = CreateUser();
        user.MarkAsDeleted();
        _users.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var act = () => CreateSut().GetAsync(user.Id);

        await act.Should().ThrowAsync<UserNotFoundException>();
    }

    [Fact]
    public async Task Get_ExistingUser_ReturnsProfileWithoutSecrets()
    {
        var user = CreateUser();
        _users.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await CreateSut().GetAsync(user.Id);

        result.Id.Should().Be(user.Id);
        result.UserName.Should().Be("nika");
        result.FirstName.Should().Be("Nika");
        result.LastName.Should().Be("Beridze");
        result.Email.Should().Be("nika@test.com");
        result.Role.Should().Be("User");
        result.CreatedAt.Should().Be(user.CreatedAt);
    }
}
