using FitZone.Application.DTOs.PaginationDTOs;
using FitZone.Application.Implementations;
using FitZone.Domain.CustomExceptions;
using FitZone.Domain.Entities.Models;
using FitZone.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace FitZone.Testing.ServiceTests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _users = new();

    private readonly Mock<IUnitOfWork> _unit = new();

    private UserService CreateSut() => new(_users.Object, _unit.Object, NullLogger<UserService>.Instance);

    private static User CreateUser(string role) => new()
    {
        UserName = "nika",
        FirstName = "Nika",
        LastName = "Beridze",
        Email = "nika@test.com",
        Role = new Role { RoleName = role }
    };

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 51)]
    public async Task GetAll_InvalidPaging_Throws(int page, int pageSize)
    {
        var act = () => CreateSut().GetAllAsync(null, new PaginationQueryDTO { Page = page, PageSize = pageSize });

        await act.Should().ThrowAsync<PageSizeException>();
        _users.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetAll_TrimsSearch_AndMapsUsers()
    {
        var user = new User
        {
            UserName = "nika",
            FirstName = "Nika",
            LastName = "Beridze",
            Email = "nika@test.com",
            Role = new Role { RoleName = "User" }
        };
        _users.Setup(r => r.GetPagedAsync("nik", 2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<User> { user }, 11));

        var result = await CreateSut().GetAllAsync("  nik  ", new PaginationQueryDTO { Page = 2, PageSize = 5 });

        result.TotalCount.Should().Be(11);
        result.Values.Should().ContainSingle();
        var item = result.Values.First();
        item.UserName.Should().Be("nika");
        item.Role.Should().Be("User");
        item.Email.Should().Be("nika@test.com");
    }

    [Fact]
    public async Task GetAll_BlankSearch_IsPassedAsNull()
    {
        _users.Setup(r => r.GetPagedAsync(null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<User>(), 0));

        var result = await CreateSut().GetAllAsync("   ", new PaginationQueryDTO());

        result.Values.Should().BeEmpty();
        _users.Verify(r => r.GetPagedAsync(null, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_EmptyId_Throws()
    {
        var act = () => CreateSut().DeleteAsync(Guid.Empty);

        await act.Should().ThrowAsync<EnterProperIdException>();
    }

    [Fact]
    public async Task Delete_UnknownUser_Throws()
    {
        var id = Guid.NewGuid();
        _users.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var act = () => CreateSut().DeleteAsync(id);

        await act.Should().ThrowAsync<UserNotFoundException>();
        _unit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_Admin_IsForbidden()
    {
        var admin = CreateUser("Admin");
        _users.Setup(r => r.GetByIdAsync(admin.Id, It.IsAny<CancellationToken>())).ReturnsAsync(admin);

        var act = () => CreateSut().DeleteAsync(admin.Id);

        await act.Should().ThrowAsync<ForbiddenException>();
        admin.DeletedAt.Should().BeNull();
    }

    [Fact]
    public async Task Delete_UserWithUpcomingBookings_Throws()
    {
        var user = CreateUser("User");
        _users.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _users.Setup(r => r.HasActiveFutureBookingsAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => CreateSut().DeleteAsync(user.Id);

        await act.Should().ThrowAsync<UserHasActiveBookingsException>();
        user.DeletedAt.Should().BeNull();
        _unit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_NormalUser_IsMarkedDeletedAndSaved()
    {
        var user = CreateUser("User");
        _users.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _users.Setup(r => r.HasActiveFutureBookingsAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await CreateSut().DeleteAsync(user.Id);

        user.DeletedAt.Should().NotBeNull();
        user.IsActive.Should().BeFalse();
        _unit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
