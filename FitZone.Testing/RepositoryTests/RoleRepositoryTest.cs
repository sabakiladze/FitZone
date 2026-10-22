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
    public class RoleRepositoryTest
    {
        private FitZoneDbContext GetFitZoneDbContext()
        {
            var options = new DbContextOptionsBuilder<FitZoneDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new FitZoneDbContext(options);
            return context;
        }

        [Fact]
        public async Task GetByNameAsync_When_Name_mAtches()
        {

            // arrange
            var context = GetFitZoneDbContext();
            var repository= new RoleRepository(context);

            var role = new Role
            {
                Id = Guid.NewGuid(),
                RoleName = "Admin"
            };

            await context.Roles.AddAsync(role);
            await context.SaveChangesAsync();

            //act
            var result = await repository.GetByNameAsync("Admin");
            

            // asert
            result.Should().NotBeNull();
            result.RoleName.Should().Be("Admin");
        }

        [Fact]
        public async Task GetByNameAsync_Should_Return_Role_When_Name_Matches_Different_Case()
        {
            // arrange
            using var context = GetFitZoneDbContext();
            var repository = new RoleRepository(context);

            var role = new Role
            {
                Id = Guid.NewGuid(),
                RoleName = "Admin"
            };

            await context.Roles.AddAsync(role);
            await context.SaveChangesAsync();

            // Act - ვეძებთ პატარა ასოებით
            var result = await repository.GetByNameAsync("admin");

            // Assert
            result.Should().NotBeNull();
            result.RoleName.Should().Be("Admin");
        }

        [Fact]
        public async Task GetByName_Should_Return_null_when_role_does_nit_exists()
        {
            // arange
            using var context = GetFitZoneDbContext();
            var repository = new RoleRepository(context);

            //act
            var result=await repository.GetByNameAsync("fneudu");

            // assert

            result.Should().BeNull();
        }

    }
}
