using FitZone.Domain.Entities.Models;
using FitZone.Infrastructure.Data;
using FitZone.Infrastructure.Repositorys;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Testing.RepositoryTests
{
    public class MembershipPlanRepositoryTest
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
        public async Task GetAllMembershipAsync_When_List_Is_Not_null()
        {

            // arrange
            var context = CreateContext();
            var repo = new MembershipPlanRepository(context);

            var plan = new MembershipPlan
            {
                Id = Guid.NewGuid(),
                Name = "pro",
                Price = 19.00m,
                VisitLimit = 7,
                DurationInDays = 14
            };



            await context.MembershipPlans.AddAsync(plan);
            await context.SaveChangesAsync();


            //Act
            var result = await repo.GetbyIdMembershipPlan(plan.Id);

            //assert

            result.Should().Be(plan);
            result.Name.Should().Be("pro");
            result.Price.Should().Be(19.00m);
            result.DurationInDays.Should().Be(14);

        }
        [Fact]
        public async Task GetAllMembershipAsync_When_List_Is__null()
        {

            // arrange
            var context = CreateContext();
            var repo = new MembershipPlanRepository(context);

            var plan = new MembershipPlan
            {
                Id = Guid.NewGuid(),
                Name = "pro",
                Price = 19.00m,
                VisitLimit = 7,
                DurationInDays = 14
            };

            await context.SaveChangesAsync();


            //Act
            var result = await repo.GetbyIdMembershipPlan(plan.Id);

            //assert

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdMembmershipPlan_When_Visits_is_null()
        {
            //arage
            var context = CreateContext();
            var repo = new MembershipPlanRepository(context);

            var plan = new MembershipPlan
            {
                Id = Guid.NewGuid(),
                Name = "pro",
                Price = 19.00m,
                VisitLimit = null,
                DurationInDays = 14
            };
            await context.MembershipPlans.AddAsync(plan);

            await context.SaveChangesAsync();


            //Act
            var result = await repo.GetbyIdMembershipPlan(plan.Id);

            //assert

            result.Should().Be(plan);
            result.VisitLimit.Should().Be(null);

        }
    }

}
