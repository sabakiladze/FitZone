using AutoMapper;
using FitZone.Application.Implementations;
using FitZone.Domain.CustomExceptions;
using FitZone.Domain.Entities.Models;
using FitZone.Domain.Interfaces;
using FitZone.Domain.RepositoryInterfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace FitZone.Testing.ServiceTests;

public class VisitServiceTests
{
    private readonly Mock<IVisitRepository> _visitRepo = new();
    private readonly Mock<IUnitOfWork> _unit = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<IUserMembershipRepository> _membershipRepo = new();

    private VisitService CreateSut() => new(
        _visitRepo.Object,
        _unit.Object,
        NullLogger<VisitService>.Instance,
        _mapper.Object,
        _membershipRepo.Object);

    // თუ კონსტრუქტორი სხვანაირია, მხოლოდ აქ შეცვალე.
    private static UserMembershipPlan CreateMembership(Guid userId, int? visitLimit)
        => new(userId, Guid.NewGuid(), DateTime.UtcNow, 30, visitLimit);

    private void SetupActiveMembership(Guid userId, UserMembershipPlan? membership)
        => _membershipRepo
            .Setup(r => r.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);

    [Fact]
    public async Task CheckIn_EmptyUserId_ThrowsAndTouchesNothing()
    {
        var sut = CreateSut();

        var act = () => sut.CheckInAsync(Guid.Empty);

        await act.Should().ThrowAsync<EnterProperIdException>();
        _membershipRepo.VerifyNoOtherCalls();
        _visitRepo.VerifyNoOtherCalls();
        _unit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CheckIn_NoActiveMembership_ThrowsAndSavesNothing()
    {
        var userId = Guid.NewGuid();
        SetupActiveMembership(userId, null);
        var sut = CreateSut();

        var act = () => sut.CheckInAsync(userId);

        await act.Should().ThrowAsync<NoActiveMembershipException>();
        _visitRepo.Verify(r => r.AddAsync(It.IsAny<Visit>(), It.IsAny<CancellationToken>()), Times.Never);
        _unit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckIn_LimitedPlan_DecrementsVisitsAndSavesVisit()
    {
        var userId = Guid.NewGuid();
        var membership = CreateMembership(userId, visitLimit: 5);
        SetupActiveMembership(userId, membership);

        Visit? savedVisit = null;
        _visitRepo
            .Setup(r => r.AddAsync(It.IsAny<Visit>(), It.IsAny<CancellationToken>()))
            .Callback<Visit, CancellationToken>((v, _) => savedVisit = v);

        var sut = CreateSut();

        var result = await sut.CheckInAsync(userId);

        result.RemainingVisits.Should().Be(4);
        membership.RemainingVisits.Should().Be(4);

        savedVisit.Should().NotBeNull();
        savedVisit!.UserId.Should().Be(userId);
        savedVisit.UserMembershipPlanId.Should().Be(membership.Id);
        savedVisit.CheckedInAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        result.VisitId.Should().Be(savedVisit.Id);

        _visitRepo.Verify(r => r.AddAsync(It.IsAny<Visit>(), It.IsAny<CancellationToken>()), Times.Once);
        _unit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckIn_UnlimitedPlan_RemainingStaysNullAndVisitIsSaved()
    {
        var userId = Guid.NewGuid();
        var membership = CreateMembership(userId, visitLimit: null);
        SetupActiveMembership(userId, membership);
        var sut = CreateSut();

        var result = await sut.CheckInAsync(userId);

        result.RemainingVisits.Should().BeNull();
        _visitRepo.Verify(r => r.AddAsync(It.IsAny<Visit>(), It.IsAny<CancellationToken>()), Times.Once);
        _unit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckIn_NoVisitsLeft_ThrowsAndSavesNothing()
    {
        var userId = Guid.NewGuid();
        var membership = CreateMembership(userId, visitLimit: 1);
        membership.MakeVisit();                       
        SetupActiveMembership(userId, membership);
        var sut = CreateSut();

        var act = () => sut.CheckInAsync(userId);

        await act.Should().ThrowAsync<NoVisitsLeftException>();
        _visitRepo.Verify(r => r.AddAsync(It.IsAny<Visit>(), It.IsAny<CancellationToken>()), Times.Never);
        _unit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}