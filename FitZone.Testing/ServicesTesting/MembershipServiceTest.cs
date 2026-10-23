using AutoMapper;
using FitZone.Application.DTOs.MembershipDTOs;
using FitZone.Application.Implementations;
using FitZone.Domain.CustomExceptions;
using FitZone.Domain.Entities.Models;
using FitZone.Domain.Interfaces;
using FitZone.Domain.RepositoryInterfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FitZone.Testing.ServiceTests
{
    public class MembershipServiceTests
    {
        private readonly Mock<IUserMembershipRepository> _userMembershipRepo = new();
        private readonly Mock<IMembershipPlanRepository> _planRepo = new();
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly MembershipService _service;

        public MembershipServiceTests()
        {
            _mapper.Setup(m => m.Map<UserMembershipDTO>(It.IsAny<UserMembershipPlan>()))
                   .Returns((UserMembershipPlan m) => new UserMembershipDTO
                   {
                       StartDate = m.StartDate,
                       EndDate = m.EndDate,
                       RemainingVisits = m.RemainingVisits
                   });

            _service = new MembershipService(
                _userMembershipRepo.Object,
                _planRepo.Object,
                _uow.Object,
                _mapper.Object,
                NullLogger<MembershipService>.Instance);
        }

        private static MembershipPlan CreatePlan(int? visitLimit) => new()
        {
            Id = Guid.NewGuid(),
            Name = "Monthly",
            Price = 50m,
            DurationInDays = 30,
            VisitLimit = visitLimit
        };

        [Fact]
        public async Task ActivateAsync_Should_Throw_When_Plan_Does_Not_Exist()
        {
            _planRepo.Setup(r => r.GetbyIdMembershipPlan(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((MembershipPlan?)null);

            var act = () => _service.ActivateAsync(
                Guid.NewGuid(), new ActivateMembershipRequestDTO { PlanId = Guid.NewGuid() });

            await act.Should().ThrowAsync<MembershipPlanDoNotExistsException>();
            _userMembershipRepo.Verify(r => r.AddAsync(It.IsAny<UserMembershipPlan>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ActivateAsync_Should_Throw_When_User_Already_Has_Active_Plan()
        {
            var plan = CreatePlan(12);
            _planRepo.Setup(r => r.GetbyIdMembershipPlan(plan.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(plan);
            _userMembershipRepo.Setup(r => r.GetActiveByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new UserMembershipPlan());

            var act = () => _service.ActivateAsync(
                Guid.NewGuid(), new ActivateMembershipRequestDTO { PlanId = plan.Id });

            await act.Should().ThrowAsync<YouAlreadyHavePlanException>();
            _unitOfWork_SaveWasNeverCalled();
        }

        [Fact]
        public async Task ActivateAsync_Should_Create_Membership_With_Correct_Dates_And_Visits()
        {
            var plan = CreatePlan(12);
            var userId = Guid.NewGuid();
            UserMembershipPlan? saved = null;

            _planRepo.Setup(r => r.GetbyIdMembershipPlan(plan.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(plan);
            _userMembershipRepo.Setup(r => r.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                               .ReturnsAsync((UserMembershipPlan?)null);
            _userMembershipRepo.Setup(r => r.AddAsync(It.IsAny<UserMembershipPlan>(), It.IsAny<CancellationToken>()))
                               .Callback<UserMembershipPlan, CancellationToken>((m, _) => saved = m)
                               .Returns(Task.CompletedTask);

            var result = await _service.ActivateAsync(userId, new ActivateMembershipRequestDTO { PlanId = plan.Id });

            saved.Should().NotBeNull();
            saved!.UserId.Should().Be(userId);
            saved.RemainingVisits.Should().Be(12);
            (saved.EndDate - saved.StartDate).Days.Should().Be(30);
            result.PlanName.Should().Be("Monthly");
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ActivateAsync_Should_Set_RemainingVisits_Null_For_Unlimited_Plan()
        {
            var plan = CreatePlan(null);
            UserMembershipPlan? saved = null;

            _planRepo.Setup(r => r.GetbyIdMembershipPlan(plan.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(plan);
            _userMembershipRepo.Setup(r => r.GetActiveByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync((UserMembershipPlan?)null);
            _userMembershipRepo.Setup(r => r.AddAsync(It.IsAny<UserMembershipPlan>(), It.IsAny<CancellationToken>()))
                               .Callback<UserMembershipPlan, CancellationToken>((m, _) => saved = m)
                               .Returns(Task.CompletedTask);

            await _service.ActivateAsync(Guid.NewGuid(), new ActivateMembershipRequestDTO { PlanId = plan.Id });

            saved!.RemainingVisits.Should().BeNull();
        }

        [Fact]
        public async Task GetMyActiveAsync_Should_Return_Null_When_User_Has_No_Active_Plan()
        {
            _userMembershipRepo.Setup(r => r.GetActiveByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync((UserMembershipPlan?)null);

            var result = await _service.GetMyActiveAsync(Guid.NewGuid());

            result.Should().BeNull();
        }

        private void _unitOfWork_SaveWasNeverCalled() =>
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}