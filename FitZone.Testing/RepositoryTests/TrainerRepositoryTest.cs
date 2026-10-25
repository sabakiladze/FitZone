using AutoMapper;
using FitZone.Domain.Entities.Models;
using FitZone.Infrastructure.Data;
using FitZone.Infrastructure.Repositorys;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace FitZone.Testing.RepositoryTests
{
    public class TrainerRepositoryTests
    {
        private static FitZoneDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<FitZoneDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new FitZoneDbContext(options);
        }

        private static TrainerRepository CreateRepo(FitZoneDbContext context)
        {
            var config = new MapperConfiguration(
                cfg => cfg.CreateMap<Trainer, Trainer>(),
                NullLoggerFactory.Instance);
            var mapper = config.CreateMapper(); return new TrainerRepository(context, config.CreateMapper());
        }

        private static Trainer NewTrainer(string name, string specialization, int years = 3) => new()
        {
            Id = Guid.NewGuid(),
            FullName = name,
            Specialization = specialization,
            ExperienceYears = years
        };


        [Fact]
        public async Task AddAsync_Should_Persist_Trainer_After_SaveChanges()
        {
            using var context = CreateContext();
            var repo = CreateRepo(context);
            var trainer = NewTrainer("Nino Kapanadze", "Yoga");

            await repo.AddAsync(trainer);
            await context.SaveChangesAsync();

            var saved = await context.Trainers.AsNoTracking().FirstOrDefaultAsync(t => t.Id == trainer.Id);
            saved.Should().NotBeNull();
            saved!.FullName.Should().Be("Nino Kapanadze");
        }

        [Fact]
        public async Task AddAsync_Should_Not_Persist_Until_SaveChanges_Is_Called()
        {
            using var context = CreateContext();
            var repo = CreateRepo(context);

            await repo.AddAsync(NewTrainer("Nino", "Yoga"));

            (await context.Trainers.AsNoTracking().CountAsync()).Should().Be(0);
        }


        [Fact]
        public async Task GetAllAsync_Should_Return_All_Trainers()
        {
            using var context = CreateContext();
            context.Trainers.AddRange(NewTrainer("A", "Yoga"), NewTrainer("B", "Cardio"));
            await context.SaveChangesAsync();
            var repo = CreateRepo(context);

            var result = await repo.GetAllAsync();

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_When_No_Trainers()
        {
            using var context = CreateContext();
            var repo = CreateRepo(context);

            var result = await repo.GetAllAsync();

            result.Should().BeEmpty();
        }


        [Fact]
        public async Task GetByIdAsync_Should_Return_Trainer_When_Exists()
        {
            using var context = CreateContext();
            var trainer = NewTrainer("Giorgi", "Strength");
            context.Trainers.Add(trainer);
            await context.SaveChangesAsync();
            var repo = CreateRepo(context);

            var result = await repo.GetByIdAsync(trainer.Id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(trainer.Id);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Not_Found()
        {
            using var context = CreateContext();
            var repo = CreateRepo(context);

            var result = await repo.GetByIdAsync(Guid.NewGuid());

            result.Should().BeNull();
        }

       


        [Fact]
        public async Task SearchAsync_Should_Return_All_When_No_Filters()
        {
            using var context = CreateContext();
            context.Trainers.AddRange(NewTrainer("A", "Yoga"), NewTrainer("B", "Cardio"), NewTrainer("C", "Strength"));
            await context.SaveChangesAsync();
            var repo = CreateRepo(context);

            var result = await repo.SearchAsync(null, null);

            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task SearchAsync_Should_Filter_By_Name()
        {
            using var context = CreateContext();
            context.Trainers.AddRange(NewTrainer("Nino Kapanadze", "Yoga"), NewTrainer("Giorgi Beridze", "Strength"));
            await context.SaveChangesAsync();
            var repo = CreateRepo(context);

            var result = await repo.SearchAsync("Nino", null);

            result.Should().ContainSingle();
            result.Single().FullName.Should().Contain("Nino");
        }

        [Fact]
        public async Task SearchAsync_Should_Filter_By_Specialization()
        {
            using var context = CreateContext();
            context.Trainers.AddRange(NewTrainer("A", "Yoga"), NewTrainer("B", "Yoga"), NewTrainer("C", "Cardio"));
            await context.SaveChangesAsync();
            var repo = CreateRepo(context);

            var result = await repo.SearchAsync(null, "Yoga");

            result.Should().HaveCount(2);
            result.Should().OnlyContain(t => t.Specialization == "Yoga");
        }

        [Fact]
        public async Task SearchAsync_Should_Apply_Both_Filters_Together()
        {
            using var context = CreateContext();
            context.Trainers.AddRange(NewTrainer("Nino A", "Yoga"), NewTrainer("Nino B", "Cardio"), NewTrainer("Giorgi", "Yoga"));
            await context.SaveChangesAsync();
            var repo = CreateRepo(context);

            var result = await repo.SearchAsync("Nino", "Yoga");

            result.Should().ContainSingle();
            result.Single().FullName.Should().Be("Nino A");
        }

        [Fact]
        public async Task SearchAsync_Should_Return_Empty_When_Nothing_Matches()
        {
            using var context = CreateContext();
            context.Trainers.Add(NewTrainer("Nino", "Yoga"));
            await context.SaveChangesAsync();
            var repo = CreateRepo(context);

            var result = await repo.SearchAsync("არარსებული", null);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchAsync_Should_Order_By_FullName()
        {
            using var context = CreateContext();
            context.Trainers.AddRange(NewTrainer("Charlie", "X"), NewTrainer("Alice", "X"), NewTrainer("Bob", "X"));
            await context.SaveChangesAsync();
            var repo = CreateRepo(context);

            var result = await repo.SearchAsync(null, null);

            result.Select(t => t.FullName).Should().ContainInOrder("Alice", "Bob", "Charlie");
        }


        [Fact]
        public async Task Remove_Should_Delete_Trainer_After_SaveChanges()
        {
            using var context = CreateContext();
            var trainer = NewTrainer("To Delete", "Yoga");
            context.Trainers.Add(trainer);
            await context.SaveChangesAsync();
            var repo = CreateRepo(context);

            var loaded = await repo.GetByIdAsync(trainer.Id);
            repo.Remove(loaded!);
            await context.SaveChangesAsync();

            (await context.Trainers.AsNoTracking().AnyAsync(t => t.Id == trainer.Id)).Should().BeFalse();
        }


        [Fact]
        public async Task Update_Should_Copy_Values_To_Existing_Trainer_After_SaveChanges()
        {
            using var context = CreateContext();
            var trainer = NewTrainer("Old Name", "Yoga", 2);
            context.Trainers.Add(trainer);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            var repo = CreateRepo(context);

            await repo.Update(new Trainer
            {
                Id = trainer.Id,
                FullName = "New Name",
                Specialization = "Cardio",
                ExperienceYears = 9
            });
            await context.SaveChangesAsync();

            var fromDb = await context.Trainers.AsNoTracking().FirstAsync(t => t.Id == trainer.Id);
            fromDb.FullName.Should().Be("New Name");
            fromDb.Specialization.Should().Be("Cardio");
            fromDb.ExperienceYears.Should().Be(9);
        }

        [Fact]
        public async Task Update_Should_Do_Nothing_When_Trainer_Does_Not_Exist()
        {
            using var context = CreateContext();
            var repo = CreateRepo(context);

            await repo.Update(NewTrainer("Ghost", "Yoga"));
            await context.SaveChangesAsync();

            (await context.Trainers.AsNoTracking().CountAsync()).Should().Be(0);
        }
    }
}