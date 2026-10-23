using FitZone.Domain.Entities.Models;
using FitZone.Infrastructure.Data;
using FitZone.Infrastructure.Repositorys;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;


namespace FitZone.Testing.RepositoryTests
{
    public  class UserMembershipPlanRepositorytest
    {
        private FitZoneDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<FitZoneDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new FitZoneDbContext(options);
            return context;
        }

        [Fact]
        public async Task GetActiveByUserIdAsync_WhenMembershipDoesNotExist_ReturnsNull()
        {
            // Arrange
            await using var context = CreateContext();
            var repository = new UserMembershipPlanRepository(context);

            var userId = Guid.NewGuid();

            // Act
            var result = await repository.GetActiveByUserIdAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetActiveByUserIdAsync_WhenActiveMembershipExists_ReturnsMembership()
        {
            // Arrange
            await using var context = CreateContext();
            var repository = new UserMembershipPlanRepository(context);

            var userId = Guid.NewGuid();

            var plan = new MembershipPlan
            {
                Id = Guid.NewGuid(),
                Name = "Pro",
                Price = 19.00m,
                VisitLimit = 7,
                DurationInDays = 14
            };

            var membership = new UserMembershipPlan
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                MembershipPlanId = plan.Id,
                StartDate = DateTime.UtcNow.AddDays(-2),
                EndDate = DateTime.UtcNow.AddDays(12),
                MembershipPlan = plan
            };

            context.MembershipPlans.Add(plan);
            context.UserMembershipPlans.Add(membership);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetActiveByUserIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be(userId);
            result.MembershipPlan.Should().NotBeNull();
            result.MembershipPlan.Name.Should().Be("Pro");
        }

        [Fact]
        public async Task GetActiveByUserIdAsync_WhenMembershipHasExpired_ReturnsNull()
        {
            // Arrange
            await using var context = CreateContext();
            var repository = new UserMembershipPlanRepository(context);

            var userId = Guid.NewGuid();

            var membership = new UserMembershipPlan
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StartDate = DateTime.UtcNow.AddDays(-20),
                EndDate = DateTime.UtcNow.AddDays(-1)
            };

            context.UserMembershipPlans.Add(membership);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetActiveByUserIdAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_WhenMembershipIsProvided_AddsMembershipToContext()
        {
            // Arrange
            await using var context = CreateContext();
            var repository = new UserMembershipPlanRepository(context);

            var membership = new UserMembershipPlan
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30)
            };

            // Act
            await repository.AddAsync(membership);

            // Assert
            context.Entry(membership).State.Should().Be(EntityState.Added);
        }
    }

}

