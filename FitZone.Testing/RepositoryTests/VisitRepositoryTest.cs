using FitZone.Domain.Entities.Models;
using FitZone.Infrastructure.Data;
using FitZone.Infrastructure.Repositorys;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitZone.Testing.RepositoryTests
{
    public class VisitRepositoryTests
    {
        private static FitZoneDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<FitZoneDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new FitZoneDbContext(options);
        }

        private static Visit NewVisit(Guid userId, Guid membershipId, DateTime? at = null) => new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserMembershipPlanId = membershipId,
            CheckedInAt = at ?? DateTime.UtcNow
        };

        [Fact]
        public async Task AddAsync_Should_Persist_Visit_After_SaveChanges()
        {
            using var context = CreateContext();
            var repo = new VisitRepository(context);
            var visit = NewVisit(Guid.NewGuid(), Guid.NewGuid());

            await repo.AddAsync(visit);
            await context.SaveChangesAsync();

            var saved = await context.Visits.FindAsync(visit.Id);
            saved.Should().NotBeNull();
            saved!.UserId.Should().Be(visit.UserId);
        }

        [Fact]
        public async Task AddAsync_Should_Not_Persist_Until_SaveChanges_Is_Called()
        {
            using var context = CreateContext();
            var repo = new VisitRepository(context);

            await repo.AddAsync(NewVisit(Guid.NewGuid(), Guid.NewGuid()));

            context.ChangeTracker.Entries<Visit>().Should().ContainSingle();
            (await context.Visits.AsNoTracking().CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task GetByUserIdAsync_Should_Return_Only_Visits_Of_That_User()
        {
            using var context = CreateContext();
            var repo = new VisitRepository(context);
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            var plan = new MembershipPlan
            {
                Id = Guid.NewGuid(),
                Name = "Monthly",
                Price = 50m,
                DurationInDays = 30,
                VisitLimit = 12
            };
            var membership = new UserMembershipPlan(userId, plan.Id, DateTime.UtcNow, 30, 12);
            var otherMembership = new UserMembershipPlan(otherUserId, plan.Id, DateTime.UtcNow, 30, 12);

            context.AddRange(plan, membership, otherMembership);
            context.Visits.AddRange(
                NewVisit(userId, membership.Id),
                NewVisit(userId, membership.Id),
                NewVisit(otherUserId, otherMembership.Id));
            await context.SaveChangesAsync();

            var result = await repo.GetByUserIdAsync(userId);

            result.Should().HaveCount(2);
            result.Should().OnlyContain(v => v.UserId == userId);
        }

        [Fact]
        public async Task GetByUserIdAsync_Should_Return_Newest_First()
        {
            using var context = CreateContext();
            var repo = new VisitRepository(context);
            var userId = Guid.NewGuid();
            var membershipId = Guid.NewGuid();

            context.Visits.AddRange(
                NewVisit(userId, membershipId, DateTime.UtcNow.AddDays(-3)),
                NewVisit(userId, membershipId, DateTime.UtcNow),
                NewVisit(userId, membershipId, DateTime.UtcNow.AddDays(-1)));
            await context.SaveChangesAsync();

            var result = await repo.GetByUserIdAsync(userId);

            result.Should().BeInDescendingOrder(v => v.CheckedInAt);
        }

        [Fact]
        public async Task GetByUserIdAsync_Should_Return_Empty_When_User_Has_No_Visits()
        {
            using var context = CreateContext();
            var repo = new VisitRepository(context);

            var result = await repo.GetByUserIdAsync(Guid.NewGuid());

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByUserIdAsync_Should_Include_Membership_And_Plan()
        {
            using var context = CreateContext();
            var repo = new VisitRepository(context);
            var userId = Guid.NewGuid();

            var plan = new MembershipPlan
            {
                Id = Guid.NewGuid(),
                Name = "Monthly",
                Price = 50m,
                DurationInDays = 30,
                VisitLimit = 12
            };
            var membership = new UserMembershipPlan(userId, plan.Id, DateTime.UtcNow, 30, 12);
            context.AddRange(plan, membership);
            context.Visits.Add(NewVisit(userId, membership.Id));
            await context.SaveChangesAsync();

            var result = await repo.GetByUserIdAsync(userId);

            result.Single().UserMembershipPlan.Should().NotBeNull();
            result.Single().UserMembershipPlan.MembershipPlan.Name.Should().Be("Monthly");
        }
    }
}