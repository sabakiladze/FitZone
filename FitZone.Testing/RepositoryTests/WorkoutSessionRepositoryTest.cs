using FitZone.Domain.CustomExceptions;
using FitZone.Domain.Entities.Models;
using FitZone.Domain.Enums;
using FitZone.Infrastructure.Data;
using FitZone.Infrastructure.Repositorys;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FitZone.Testing.RepositoryTests
{
    public class WorkoutSessionRepositoryTests
    {
        private static readonly DateTime Day = new(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc);

        private static FitZoneDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<FitZoneDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new FitZoneDbContext(options);
        }

        // "10:30" -> 2026-11-01 10:30
        private static DateTime At(string hhmm, int addDays = 0)
            => Day.AddDays(addDays).Add(TimeSpan.Parse(hhmm));

        private static Trainer NewTrainer(string name = "Test Trainer") => new()
        {
            Id = Guid.NewGuid(),
            FullName = name,
            Specialization = "Yoga",
            ExperienceYears = 3
        };

        private static WorkoutSession NewSession(
            Guid trainerId, string from, string to, int capacity = 10, int addDays = 0) => new()
            {
                TrainerId = trainerId,
                StartsAt = At(from, addDays),
                EndsAt = At(to, addDays),
                Capacity = capacity
            };
        [Fact]
        public async Task TrainerHasOverlapAsync_TrainerWithoutSessions_Should_ReturnFalse()
        {
            using var context = CreateContext();
            var trainer = NewTrainer();
            context.Trainers.Add(trainer);
            await context.SaveChangesAsync();
            var repo = new WorkOutSessioRepository(context);

            var result = await repo.TrainerHasOverlapAsync(trainer.Id, At("10:00"), At("11:00"));

            result.Should().BeFalse();
        }

        [Fact]
        public async Task TrainerHasOverlapAsync_SessionOfAnotherTrainer_Should_NotConflict()
        {
            using var context = CreateContext();
            var trainer1 = NewTrainer("T1");
            var trainer2 = NewTrainer("T2");
            context.Trainers.AddRange(trainer1, trainer2);
            context.WorkoutSessions.Add(NewSession(trainer2.Id, "10:00", "11:00"));
            await context.SaveChangesAsync();
            var repo = new WorkOutSessioRepository(context);

            var result = await repo.TrainerHasOverlapAsync(trainer1.Id, At("10:00"), At("11:00"));

            result.Should().BeFalse();
        }

        [Fact]
        public async Task TrainerHasOverlapAsync_UnknownTrainer_Should_Throw()
        {
            using var context = CreateContext();
            var repo = new WorkOutSessioRepository(context);

            Func<Task> act = () => repo.TrainerHasOverlapAsync(Guid.NewGuid(), At("10:00"), At("11:00"));

            await act.Should().ThrowAsync<TrainersDoNotFoundException>();
        }


       
        [Fact]
        public async Task TrainerHasOverlapAsync_OnUpdate_SessionOfAnotherTrainer_Should_NotConflict()
        {
            using var context = CreateContext();
            var trainer1 = NewTrainer("T1");
            var trainer2 = NewTrainer("T2");
            var sessionA = NewSession(trainer1.Id, "10:00", "11:00");
            context.Trainers.AddRange(trainer1, trainer2);
            context.WorkoutSessions.AddRange(sessionA, NewSession(trainer2.Id, "10:30", "11:30"));
            await context.SaveChangesAsync();
            var repo = new WorkOutSessioRepository(context);

            var result = await repo.TrainerHasOverlapAsync(
                trainer1.Id, At("10:15"), At("11:15"), excludeSessionId: sessionA.Id);

            result.Should().BeFalse();
        }


        [Fact]
        public async Task CountActiveBookingsAsync_Should_Skip_Cancelled_And_NoShow()
        {
            using var context = CreateContext();
            var trainer = NewTrainer();
            var session = NewSession(trainer.Id, "10:00", "11:00");
            context.Trainers.Add(trainer);
            context.WorkoutSessions.Add(session);

            foreach (var status in new[]
            {
                BookingStatus.Active, BookingStatus.Pending, BookingStatus.Attended,
                BookingStatus.Cancelled, BookingStatus.NoShow
            })
            {
                context.Bookings.Add(new Booking
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    WorkoutSessionId = session.Id,
                    BookedAt = DateTime.UtcNow,
                    Status = status
                });
            }
            await context.SaveChangesAsync();
            var repo = new WorkOutSessioRepository(context);

            var count = await repo.CountActiveBookingsAsync(session.Id);

            count.Should().Be(3);  
        }


        [Fact]
        public async Task GetFilteredAsync_Should_Paginate_And_Return_Full_TotalCount()
        {
            using var context = CreateContext();
            var trainer = NewTrainer();
            context.Trainers.Add(trainer);
            for (var h = 1; h <= 7; h++)
                context.WorkoutSessions.Add(NewSession(trainer.Id, $"{h:00}:00", $"{h:00}:30"));
            await context.SaveChangesAsync();
            var repo = new WorkOutSessioRepository(context);

            var page1 = await repo.GetFilteredAsync(null, null, page: 1, pageSize: 5);
            var page2 = await repo.GetFilteredAsync(null, null, page: 2, pageSize: 5);
            var page3 = await repo.GetFilteredAsync(null, null, page: 3, pageSize: 5);

            page1.Items.Should().HaveCount(5);
            page2.Items.Should().HaveCount(2);
            page3.Items.Should().BeEmpty();
            page1.TotalCount.Should().Be(7);
            page2.TotalCount.Should().Be(7);
            page3.TotalCount.Should().Be(7);
        }

        [Fact]
        public async Task GetFilteredAsync_Should_Order_By_StartsAt()
        {
            using var context = CreateContext();
            var trainer = NewTrainer();
            context.Trainers.Add(trainer);
            context.WorkoutSessions.AddRange(
                NewSession(trainer.Id, "15:00", "16:00"),
                NewSession(trainer.Id, "09:00", "10:00"),
                NewSession(trainer.Id, "12:00", "13:00"));
            await context.SaveChangesAsync();
            var repo = new WorkOutSessioRepository(context);

            var (items, _) = await repo.GetFilteredAsync(null, null, 1, 10);

            items.Select(s => s.StartsAt).Should().BeInAscendingOrder();
        }

        [Fact]
        public async Task GetFilteredAsync_Should_Filter_By_Trainer()
        {
            using var context = CreateContext();
            var trainer1 = NewTrainer("T1");
            var trainer2 = NewTrainer("T2");
            context.Trainers.AddRange(trainer1, trainer2);
            context.WorkoutSessions.AddRange(
                NewSession(trainer1.Id, "09:00", "10:00"),
                NewSession(trainer1.Id, "11:00", "12:00"),
                NewSession(trainer2.Id, "09:00", "10:00"));
            await context.SaveChangesAsync();
            var repo = new WorkOutSessioRepository(context);

            var (items, total) = await repo.GetFilteredAsync(trainer1.Id, null, 1, 10);

            total.Should().Be(2);
            items.Should().OnlyContain(s => s.TrainerId == trainer1.Id);
        }

        [Fact]
        public async Task GetFilteredAsync_Should_Filter_By_Date()
        {
            using var context = CreateContext();
            var trainer = NewTrainer();
            context.Trainers.Add(trainer);
            context.WorkoutSessions.AddRange(
                NewSession(trainer.Id, "09:00", "10:00", addDays: 0),
                NewSession(trainer.Id, "23:00", "23:59", addDays: 0),
                NewSession(trainer.Id, "09:00", "10:00", addDays: 1));
            await context.SaveChangesAsync();
            var repo = new WorkOutSessioRepository(context);

            var (items, total) = await repo.GetFilteredAsync(
                null, DateOnly.FromDateTime(Day), 1, 10);

            total.Should().Be(2);
            items.Should().OnlyContain(s => s.StartsAt.Date == Day.Date);
        }
    }

}