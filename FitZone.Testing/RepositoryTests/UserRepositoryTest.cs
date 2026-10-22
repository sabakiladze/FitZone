using FitZone.Domain.Entities.Models;
using FitZone.Domain.Interfaces;
using FitZone.Infrastructure.Data;
using FitZone.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Testing.RepositoryTests
{
    public class UserRepositoryTest
    {


        // დამხმარე მეთოდი, რომელიც ქმნის იზოლირებულ In-Memory ბაზას ყოველი ტესტისთვის
        // და აბრუნებს ამ ბაზას
        private FitZoneDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<FitZoneDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new FitZoneDbContext(options);
            return context;
        }


        [Fact]
        public async Task GetByIdAsync_Should_Return_User_When_User_Exists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();

            var userId = Guid.NewGuid();
            context.Users.Add(new User
            {
                Id = userId,
                UserName = "testuser",
                FirstName = "Test",
                LastName = "User",
                Email = "test@fitzone.com",
                PasswordHash = "hashed_password",
                RoleId = Guid.NewGuid()
            });
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);

            // Act
            var result = await repository.GetByIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(userId);
            result.Email.Should().Be("test@fitzone.com");
        }

        [Fact]
        public async Task AddAsync_Should_Add_User_Successfully()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new UserRepository(context);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@fitzone.com",
                FirstName= "Test",
                LastName="test",
                UserName = "testuser",
                PasswordHash = "hash123"
            };

            // Act
            var result = await repository.AddAsync(user);
            await context.SaveChangesAsync(); // ვინახავთ ცვლილებებს ინ-მემორი

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be("test@fitzone.com");

            var userInDb = await context.Users.FindAsync(user.Id);
            userInDb.Should().NotBeNull();
        }

        [Fact]
        public async Task ExistsByEmailAsync_Should_Return_True_When_Email_Exists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new UserRepository(context);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "exist@fitzone.com",
                UserName = "user1",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "hash"
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // Act
            var exists = await repository.ExistsByEmailAsync("exist@fitzone.com");

            // Assert
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByEmailAsync_Should_Return_False_When_Email_Does_Not_Exist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new UserRepository(context);

            // Act
            var exists = await repository.ExistsByEmailAsync("notfound@fitzone.com");

            // Assert
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsByUserNameAsync_Should_Return_True_When_UserName_Exists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new UserRepository(context);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "u1@fitzone.com",
                UserName = "fit_master",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "hash"
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // Act
            var exists = await repository.ExistsByUserNameAsync("fit_master");

            // Assert
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task GetByEmailAsync_Should_Return_User_When_Email_Matches()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new UserRepository(context);

            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Email = "findme@fitzone.com",
                UserName = "finder",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "hash"
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByEmailAsync("findme@fitzone.com");

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(userId);
            result.Email.Should().Be("findme@fitzone.com");
        }

        [Fact]
        public async Task GetUserByHashedToken_Should_Return_User_When_Token_Matches()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var repository = new UserRepository(context);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "token@fitzone.com",
                UserName = "tokenuser",
                PasswordHash = "hash",
                FirstName = "Test",
                LastName = "User",
                RefreshTokenHash = "super_secret_hash_123"
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetUserByHashedToken("super_secret_hash_123");

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be("token@fitzone.com");
        }

    }
}
