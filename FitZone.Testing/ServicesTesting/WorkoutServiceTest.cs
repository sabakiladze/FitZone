using AutoMapper;
using FitZone.Application.DTOs.PaginationDTOs;
using FitZone.Application.DTOs.WorkOutSessionDTOs;
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

public class WorkoutSessionServiceTests
{
    private readonly Mock<IWorkOutSessionRepository> _repo = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<IUnitOfWork> _unit = new();
    private readonly Mock<ITrainerRepository> _trainerRepo = new();

    private WorkoutSessionService CreateSut() => new(
        _repo.Object, _mapper.Object, NullLogger<WorkoutSessionService>.Instance,
        _unit.Object, _trainerRepo.Object);

    private static CreateWorkoutSessionRequestDTO CreateRequest(
        int startOffsetHours = 2, int durationHours = 1, int capacity = 10, Guid? trainerId = null)
    {
        var start = DateTime.UtcNow.AddHours(startOffsetHours);
        return new CreateWorkoutSessionRequestDTO
        {
            TrainerId = trainerId ?? Guid.NewGuid(),
            StartsAt = start,
            EndsAt = start.AddHours(durationHours),
            Capacity = capacity
        };
    }

    private static UpdateWorkoutSessionRequestDTO UpdateRequest(
        int startOffsetHours = 2, int durationHours = 1, int capacity = 10)
    {
        var start = DateTime.UtcNow.AddHours(startOffsetHours);
        return new UpdateWorkoutSessionRequestDTO
        {
            StartsAt = start,
            EndsAt = start.AddHours(durationHours),
            Capacity = capacity
        };
    }

    private static WorkoutSession CreateSession(Guid? trainerId = null) => new()
    {
        Id = Guid.NewGuid(),
        TrainerId = trainerId ?? Guid.NewGuid(),
        StartsAt = DateTime.UtcNow.AddDays(1),
        EndsAt = DateTime.UtcNow.AddDays(1).AddHours(1),
        Capacity = 5,
        Bookings = new List<Booking>()
    };

    private static (ICollection<WorkoutSession> Items, int TotalCount) ToTuple(
        List<WorkoutSession> items, int total) => (items, total);

    private void VerifyNothingSaved()
    {
        _repo.Verify(r => r.AddAsync(It.IsAny<WorkoutSession>(), It.IsAny<CancellationToken>()), Times.Never);
        _unit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Add_EmptyTrainerId_Throws()
    {
        var act = () => CreateSut().AddAsync(CreateRequest(trainerId: Guid.Empty));

        await act.Should().ThrowAsync<BadRequestException>();
        VerifyNothingSaved();
    }

    [Theory]
    [InlineData(-1, 1, 10)]   // დაწყება წარსულშია
    [InlineData(2, 0, 10)]    // EndsAt == StartsAt
    [InlineData(2, -1, 10)]   // EndsAt < StartsAt
    [InlineData(2, 1, 0)]     // capacity 0
    [InlineData(2, 1, -5)]    // capacity უარყოფითი
    public async Task Add_InvalidInput_ThrowsBadRequest(int startOffset, int duration, int capacity)
    {
        var act = () => CreateSut().AddAsync(CreateRequest(startOffset, duration, capacity));

        await act.Should().ThrowAsync<BadRequestException>();
        VerifyNothingSaved();
    }

    [Fact]
    public async Task Add_TrainerNotFound_Throws()
    {
        var request = CreateRequest();
        _trainerRepo.Setup(t => t.GetByIdAsync(request.TrainerId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Trainer?)null);

        var act = () => CreateSut().AddAsync(request);

        await act.Should().ThrowAsync<TrainersDoNotFoundException>();
        VerifyNothingSaved();
    }

    [Fact]
    public async Task Add_TrainerHasOverlap_ThrowsConflictAndSavesNothing()
    {
        var request = CreateRequest();
        var trainer = new Trainer { Id = request.TrainerId };
        _trainerRepo.Setup(t => t.GetByIdAsync(request.TrainerId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(trainer);
        _mapper.Setup(m => m.Map<WorkoutSession>(request)).Returns(CreateSession(trainer.Id));
        _repo.Setup(r => r.TrainerHasOverlapAsync(
                trainer.Id, request.StartsAt, request.EndsAt, null, It.IsAny<CancellationToken>()))
             .ReturnsAsync(true);

        var act = () => CreateSut().AddAsync(request);

        await act.Should().ThrowAsync<SessionTimeConflictException>();
        VerifyNothingSaved();
    }

    [Fact]
    public async Task Add_Valid_SavesOnceAndReturnsMappedDto()
    {
        var request = CreateRequest();
        var trainer = new Trainer { Id = request.TrainerId };
        var session = CreateSession(trainer.Id);
        var dto = new WorkoutSessionDTO();

        _trainerRepo.Setup(t => t.GetByIdAsync(request.TrainerId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(trainer);
        _mapper.Setup(m => m.Map<WorkoutSession>(request)).Returns(session);
        _repo.Setup(r => r.TrainerHasOverlapAsync(
                trainer.Id, request.StartsAt, request.EndsAt, null, It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);
        _repo.Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _mapper.Setup(m => m.Map<WorkoutSessionDTO>(session)).Returns(dto);

        var result = await CreateSut().AddAsync(request);

        result.Should().BeSameAs(dto);
        _repo.Verify(r => r.AddAsync(session, It.IsAny<CancellationToken>()), Times.Once);
        _unit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 51)]
    public async Task Get_InvalidPaging_Throws(int page, int pageSize)
    {
        var act = () => CreateSut().GetAsync(
            new WorkoutSessionFilterDTO(), new PaginationQueryDTO { Page = page, PageSize = pageSize });

        await act.Should().ThrowAsync<BadRequestException>();
        _repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_EmptyTrainerIdFilter_Throws()
    {
        var act = () => CreateSut().GetAsync(
            new WorkoutSessionFilterDTO { TrainerId = Guid.Empty },
            new PaginationQueryDTO { Page = 1, PageSize = 10 });

        await act.Should().ThrowAsync<BadRequestException>();
        _repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_Valid_ReturnsMappedItemsAndTotalFromRepository()
    {
        var items = new List<WorkoutSession> { CreateSession(), CreateSession() };
        var dtos = new List<WorkoutSessionDTO> { new(), new() };

        _repo.Setup(r => r.GetFilteredAsync(null, null, 1, 50, It.IsAny<CancellationToken>()))
             .ReturnsAsync(ToTuple(items, 123));
        _mapper.Setup(m => m.Map<List<WorkoutSessionDTO>>(items)).Returns(dtos);

        var result = await CreateSut().GetAsync(
            new WorkoutSessionFilterDTO(), new PaginationQueryDTO { Page = 1, PageSize = 50 });

        result.Values.Should().BeSameAs(dtos);
        result.TotalCount.Should().Be(123);     // საერთო რაოდენობა რეპოზიტორიდან, არა items.Count
    }

    [Fact]
    public async Task GetById_EmptyId_Throws()
    {
        var act = () => CreateSut().GetByIdAsync(Guid.Empty);
        await act.Should().ThrowAsync<EnterProperIdException>();
    }

    [Fact]
    public async Task GetById_NotFound_Throws()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((WorkoutSession?)null);

        var act = () => CreateSut().GetByIdAsync(id);

        await act.Should().ThrowAsync<WorkoutSessionNotFoundException>();
    }

    [Fact]
    public async Task Remove_NotFound_Throws()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((WorkoutSession?)null);

        var act = () => CreateSut().RemoveAsync(id);

        await act.Should().ThrowAsync<WorkoutSessionNotFoundException>();
        _unit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Remove_SessionHasBookings_ThrowsAndDoesNotRemove()
    {
        var session = CreateSession();
        session.Bookings.Add(new Booking());
        _repo.Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        var act = () => CreateSut().RemoveAsync(session.Id);

        await act.Should().ThrowAsync<SessionHasBookingsException>();
        _repo.Verify(r => r.Remove(It.IsAny<WorkoutSession>()), Times.Never);
        _unit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Remove_NoBookings_RemovesAndSaves()
    {
        var session = CreateSession();
        _repo.Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        await CreateSut().RemoveAsync(session.Id);

        _repo.Verify(r => r.Remove(session), Times.Once);
        _unit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_EmptyId_Throws()
    {
        var act = () => CreateSut().UpdateAsync(Guid.Empty, UpdateRequest());
        await act.Should().ThrowAsync<EnterProperIdException>();
    }

    [Theory]
    [InlineData(-1, 1, 10)]
    [InlineData(2, 0, 10)]
    [InlineData(2, -1, 10)]
    [InlineData(2, 1, 0)]
    public async Task Update_InvalidInput_ThrowsBadRequest(int startOffset, int duration, int capacity)
    {
        var act = () => CreateSut().UpdateAsync(
            Guid.NewGuid(), UpdateRequest(startOffset, duration, capacity));

        await act.Should().ThrowAsync<BadRequestException>();
        _unit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_NotFound_Throws()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((WorkoutSession?)null);

        var act = () => CreateSut().UpdateAsync(id, UpdateRequest());

        await act.Should().ThrowAsync<WorkoutSessionNotFoundException>();
    }

    [Fact]
    public async Task Update_CapacityBelowActiveBookings_ThrowsAndDoesNotChangeSession()
    {
        var session = CreateSession();
        var originalCapacity = session.Capacity;
        _repo.Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _repo.Setup(r => r.CountActiveBookingsAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(4);

        var act = () => CreateSut().UpdateAsync(session.Id, UpdateRequest(capacity: 3));

        await act.Should().ThrowAsync<BadRequestException>();
        session.Capacity.Should().Be(originalCapacity);
        _unit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_Overlap_ThrowsConflictAndExcludesOwnSession()
    {
        var session = CreateSession();
        var request = UpdateRequest();
        _repo.Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _repo.Setup(r => r.CountActiveBookingsAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(0);
        _repo.Setup(r => r.TrainerHasOverlapAsync(
                session.TrainerId, request.StartsAt, request.EndsAt, session.Id, It.IsAny<CancellationToken>()))
             .ReturnsAsync(true);

        var act = () => CreateSut().UpdateAsync(session.Id, request);

        await act.Should().ThrowAsync<SessionTimeConflictException>();
        _unit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_Valid_ChangesEntityAndSavesOnce()
    {
        var session = CreateSession();
        var request = UpdateRequest(startOffsetHours: 5, durationHours: 2, capacity: 12);
        var dto = new WorkoutSessionDTO();

        _repo.Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _repo.Setup(r => r.CountActiveBookingsAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(2);
        _repo.Setup(r => r.TrainerHasOverlapAsync(
                session.TrainerId, request.StartsAt, request.EndsAt, session.Id, It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);
        _mapper.Setup(m => m.Map<WorkoutSessionDTO>(session)).Returns(dto);

        var result = await CreateSut().UpdateAsync(session.Id, request);

        session.StartsAt.Should().Be(request.StartsAt);
        session.EndsAt.Should().Be(request.EndsAt);
        session.Capacity.Should().Be(12);
        result.Should().BeSameAs(dto);
        _unit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}