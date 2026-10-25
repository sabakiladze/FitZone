
using AutoMapper;
using FitZone.Application;
using FitZone.Domain.Entities.Models;
using FitZone.Infrastructure.Data;
using FitZone.Infrastructure.Repositorys;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FitZone.Tests.Infrastructure.Repositories
{
    public class TrainerRepositoryTests
    {
        private readonly IMapper _mapper;

        private IMapper 

        private FitZoneDbContext GetFitZoneDbContext()
        {
            var options = new DbContextOptionsBuilder<FitZoneDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new FitZoneDbContext(options);
        }

        private TrainerRepository GetRepository(FitZoneDbContext context)
        {
            return new TrainerRepository(context, _mapper);
        }

        private Trainer CreateTrainer(
            string fullName = "John Smith",
            string specialization = "Fitness")
        {
            return new Trainer
            {
                Id = Guid.NewGuid(),
                FullName = fullName,
                Specialization = specialization,
                ExperienceYears = 5
            };
        }

        [Fact]
        public async Task AddAsync_ShouldAddTrainerToContext()
        {
            using var context = GetFitZoneDbContext();
            var repository = GetRepository(context);
            var trainer = CreateTrainer();

            await repository.AddAsync(trainer);

            context.ChangeTracker.Entries<Trainer>()
                .Should().ContainSingle();

            context.Trainers.Local.Should().Contain(trainer);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllTrainers()
        {
            using var context = GetFitZoneDbContext();
            var repository = GetRepository(context);

            var trainers = new[]
            {
                CreateTrainer("John Smith"),
                CreateTrainer("Jane Doe")
            };

            await context.Trainers.AddRangeAsync(trainers);
            await context.SaveChangesAsync();

            var result = await repository.GetAllAsync();

            result.Should().HaveCount(2);
            result.Should().Contain(t => t.FullName == "John Smith");
            result.Should().Contain(t => t.FullName == "Jane Doe");
        }

        [Fact]
        public async Task GetAllAsync_WhenNoTrainersExist_ShouldReturnEmptyCollection()
        {
            using var context = GetFitZoneDbContext();
            var repository = GetRepository(context);

            var result = await repository.GetAllAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdAsync_WhenTrainerExists_ShouldReturnTrainer()
        {
            using var context = GetFitZoneDbContext();
            var repository = GetRepository(context);
            var trainer = CreateTrainer();

            context.Trainers.Add(trainer);
            await context.SaveChangesAsync();

            var result = await repository.GetByIdAsync(trainer.Id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(trainer.Id);
            result.FullName.Should().Be(trainer.FullName);
        }

        [Fact]
        public async Task GetByIdAsync_WhenTrainerDoesNotExist_ShouldReturnNull()
        {
            using var context = GetFitZoneDbContext();
            var repository = GetRepository(context);

            var result = await repository.GetByIdAsync(Guid.NewGuid());

            result.Should().BeNull();
        }

        [Fact]
        public async Task SearchAsync_ByName_ShouldReturnMatchingTrainers()
        {
            using var context = GetFitZoneDbContext();
            var repository = GetRepository(context);

            var trainers = new[]
            {
                CreateTrainer("John Smith", "Fitness"),
                CreateTrainer("John Brown", "Yoga"),
                CreateTrainer("Jane Doe", "Fitness")
            };

            await context.Trainers.AddRangeAsync(trainers);
            await context.SaveChangesAsync();

            var result = await repository.SearchAsync("John", null);

            result.Should().HaveCount(2);
            result.Should().OnlyContain(t => t.FullName.Contains("John"));
        }

        [Fact]
        public async Task SearchAsync_BySpecialization_ShouldReturnMatchingTrainers()
        {
            using var context = GetFitZoneDbContext();
            var repository = GetRepository(context);

            var trainers = new[]
            {
                CreateTrainer("John Smith", "Fitness"),
                CreateTrainer("Jane Doe", "Yoga"),
                CreateTrainer("Alex Brown", "Fitness")
            };

            await context.Trainers.AddRangeAsync(trainers);
            await context.SaveChangesAsync();

            var result = await repository.SearchAsync(null, "Fitness");

            result.Should().HaveCount(2);
            result.Should().OnlyContain(
                t => t.Specialization.Contains("Fitness"));
        }

        [Fact]
        public async Task SearchAsync_WithoutFilters_ShouldReturnTrainersOrderedByName()
        {
            using var context = GetFitZoneDbContext();
            var repository = GetRepository(context);

            var trainers = new[]
            {
                CreateTrainer("John Smith"),
                CreateTrainer("Alex Brown"),
                CreateTrainer("Jane Doe")
            };

            await context.Trainers.AddRangeAsync(trainers);
            await context.SaveChangesAsync();

            var result = await repository.SearchAsync(null, null);

            result.Select(t => t.FullName)
                .Should()
                .BeInAscendingOrder();
        }

        [Fact]
        public async Task Remove_ShouldMarkTrainerAsDeleted()
        {
            using var context = GetFitZoneDbContext();
            var repository = GetRepository(context);
            var trainer = CreateTrainer();

            context.Trainers.Add(trainer);
            await context.SaveChangesAsync();

            repository.Remove(trainer);

            context.Entry(trainer).State
                .Should().Be(EntityState.Deleted);
        }

        [Fact]
        public async Task Update_WhenTrainerExists_ShouldUpdateTrainer()
        {
            using var context = GetFitZoneDbContext();
            var repository = GetRepository(context);

            var existingTrainer = CreateTrainer("John Smith", "Fitness");

            context.Trainers.Add(existingTrainer);
            await context.SaveChangesAsync();

            var updatedTrainer = new Trainer
            {
                Id = existingTrainer.Id,
                FullName = "John Updated",
                Specialization = "Yoga",
                ExperienceYears = 10
            };

            await repository.Update(updatedTrainer);
            await context.SaveChangesAsync();

            var result = await context.Trainers
                .AsNoTracking()
                .FirstAsync(t => t.Id == existingTrainer.Id);

            result.FullName.Should().Be("John Updated");
            result.Specialization.Should().Be("Yoga");
            result.ExperienceYears.Should().Be(10);
        }

        [Fact]
        public async Task Update_WhenTrainerDoesNotExist_ShouldNotAddTrainer()
        {
            using var context = GetFitZoneDbContext();
            var repository = GetRepository(context);
            var trainer = CreateTrainer();

            await repository.Update(trainer);

            (await context.Trainers.CountAsync()).Should().Be(0);
        }
    }
}
