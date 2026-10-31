using AutoMapper;
using FitZone.Application.DTOs.BookingDTOs;
using FitZone.Application.DTOs.PaginationDTOs;
using FitZone.Application.Implementations;
using FitZone.Domain.CustomExceptions;
using FitZone.Domain.Entities.Models;
using FitZone.Domain.Enums;
using FitZone.Domain.Interfaces;
using FitZone.Domain.RepositoryInterfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace FitZone.Testing.ServiceTests;

public class BookingServiceTests
{
    private readonly Mock<IBookingRepository> _bookingRepo = new();
    private readonly Mock<IWorkOutSessionRepository> _sessionRepo = new();
    private readonly Mock<IUserMembershipRepository> _membershipRepo = new();
    private readonly Mock<IUnitOfWork> _unit = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<IUserRepository> _userRepo = new();

    private Booking? _addedBooking;

    private BookingService CreateSut() => new(
        _bookingRepo.Object, _sessionRepo.Object, _membershipRepo.Object,
        NullLogger<BookingService>.Instance, _unit.Object, _mapper.Object, _userRepo.Object);

    private static UserMembershipPlan CreateMembership(Guid userId, int? visitLimit)
        => new(userId, Guid.NewGuid(), DateTime.UtcNow, 30, visitLimit);

    private static (ICollection<Booking> Items, int TotalCount) ToTuple(List<Booking> items, int total)
        => (items, total);
    private (CreateBookingRequestDTO Request, WorkoutSession Session) SetupHappyPath(
        Guid userId, int? visitLimit = 5, int capacity = 10)
    {
        var session = new WorkoutSession
        {
            Id = Guid.NewGuid(),
            StartsAt = DateTime.UtcNow.AddDays(1),
            EndsAt = DateTime.UtcNow.AddDays(1).AddHours(1),
            Capacity = capacity,
            Bookings = new List<Booking>()
        };
        var request = new CreateBookingRequestDTO { WorkoutSessionId = session.Id };

        _userRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new User());
        _sessionRepo.Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _membershipRepo.Setup(r => r.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(CreateMembership(userId, visitLimit));
        _bookingRepo.Setup(r => r.UserHasActiveBookingAsync(userId, session.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);
        _sessionRepo.Setup(r => r.CountActiveBookingsAsync(session.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(0);
        _bookingRepo.Setup(r => r.UserHasOverlapAsync(
                userId, session.StartsAt, session.EndsAt, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);
        _bookingRepo.Setup(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                    .Callback<Booking, CancellationToken>((b, _) => _addedBooking = b);
        _bookingRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(() => _addedBooking);

        return (request, session);
    }

    private void VerifyNothingSaved()
    {
        _bookingRepo.Verify(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Never);
        _unit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_NullRequest_Throws()
    {
        var act = () => CreateSut().CreateAsync(Guid.NewGuid(), null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Create_EmptyUserId_Throws()
    {
        var act = () => CreateSut().CreateAsync(
            Guid.Empty, new CreateBookingRequestDTO { WorkoutSessionId = Guid.NewGuid() });

        await act.Should().ThrowAsync<InvalidIdException>();
        VerifyNothingSaved();
    }

    [Fact]
    public async Task Create_EmptySessionId_Throws()
    {
        var act = () => CreateSut().CreateAsync(
            Guid.NewGuid(), new CreateBookingRequestDTO { WorkoutSessionId = Guid.Empty });

        await act.Should().ThrowAsync<InvalidIdException>();
        VerifyNothingSaved();
    }

    [Fact]
    public async Task Create_UserNotFound_Throws()
    {
        var userId = Guid.NewGuid();
        var (request, _) = SetupHappyPath(userId);
        _userRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var act = () => CreateSut().CreateAsync(userId, request);

        await act.Should().ThrowAsync<UserNotFoundException>();
        VerifyNothingSaved();
    }

    [Fact]
    public async Task Create_SessionNotFound_Throws()
    {
        var userId = Guid.NewGuid();
        var (request, session) = SetupHappyPath(userId);
        _sessionRepo.Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((WorkoutSession?)null);

        var act = () => CreateSut().CreateAsync(userId, request);

        await act.Should().ThrowAsync<WorkoutSessionNotFoundException>();
        VerifyNothingSaved();
    }

    [Fact]
    public async Task Create_SessionAlreadyStarted_Throws()
    {
        var userId = Guid.NewGuid();
        var (request, session) = SetupHappyPath(userId);
        session.StartsAt = DateTime.UtcNow.AddMinutes(-1);

        var act = () => CreateSut().CreateAsync(userId, request);

        await act.Should().ThrowAsync<SessionHasAlreadyStartedException>();
        VerifyNothingSaved();
    }

    [Fact]
    public async Task Create_NoActiveMembership_Throws()
    {
        var userId = Guid.NewGuid();
        var (request, _) = SetupHappyPath(userId);
        _membershipRepo.Setup(r => r.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync((UserMembershipPlan?)null);

        var act = () => CreateSut().CreateAsync(userId, request);

        await act.Should().ThrowAsync<NoActiveMembershipException>();
        VerifyNothingSaved();
    }

    [Fact]
    public async Task Create_NoVisitsLeft_Throws()
    {
        var userId = Guid.NewGuid();
        var (request, _) = SetupHappyPath(userId);
        var exhausted = CreateMembership(userId, visitLimit: 1);
        exhausted.MakeVisit();                              // Remaining == 0
        _membershipRepo.Setup(r => r.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(exhausted);

        var act = () => CreateSut().CreateAsync(userId, request);

        await act.Should().ThrowAsync<NoVisitsLeftException>();
        VerifyNothingSaved();
    }

    [Fact]
    public async Task Create_UnlimitedPlan_IsNotBlockedByVisitCheck()
    {
        var userId = Guid.NewGuid();
        var (request, _) = SetupHappyPath(userId, visitLimit: null);   // null = უსაზღვრო

        var act = () => CreateSut().CreateAsync(userId, request);

        await act.Should().NotThrowAsync();
        _bookingRepo.Verify(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_AlreadyBooked_Throws()
    {
        var userId = Guid.NewGuid();
        var (request, session) = SetupHappyPath(userId);
        _bookingRepo.Setup(r => r.UserHasActiveBookingAsync(userId, session.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

        var act = () => CreateSut().CreateAsync(userId, request);

        await act.Should().ThrowAsync<AlreadyBookedException>();
        VerifyNothingSaved();
    }

    [Theory]
    [InlineData(10, 10)]
    [InlineData(11, 10)]
    public async Task Create_SessionFull_Throws(int activeCount, int capacity)
    {
        var userId = Guid.NewGuid();
        var (request, session) = SetupHappyPath(userId, capacity: capacity);
        _sessionRepo.Setup(r => r.CountActiveBookingsAsync(session.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(activeCount);

        var act = () => CreateSut().CreateAsync(userId, request);

        await act.Should().ThrowAsync<SessionFullException>();
        VerifyNothingSaved();
    }

    [Fact]
    public async Task Create_LastFreeSpot_Succeeds()
    {
        var userId = Guid.NewGuid();
        var (request, session) = SetupHappyPath(userId, capacity: 10);
        _sessionRepo.Setup(r => r.CountActiveBookingsAsync(session.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(9);                       // 9 < 10: ერთი ადგილი დარჩა

        var act = () => CreateSut().CreateAsync(userId, request);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Create_TimeConflict_Throws()
    {
        var userId = Guid.NewGuid();
        var (request, session) = SetupHappyPath(userId);
        _bookingRepo.Setup(r => r.UserHasOverlapAsync(
                userId, session.StartsAt, session.EndsAt, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

        var act = () => CreateSut().CreateAsync(userId, request);

        await act.Should().ThrowAsync<BookingTimeConflictException>();
        VerifyNothingSaved();
    }

    [Fact]
    public async Task Create_Valid_SavesActiveBookingAndReturnsMappedDto()
    {
        var userId = Guid.NewGuid();
        var (request, session) = SetupHappyPath(userId);
        var dto = new BookingDTO();
        _mapper.Setup(m => m.Map<BookingDTO>(It.IsAny<Booking>())).Returns(dto);

        var result = await CreateSut().CreateAsync(userId, request);

        result.Should().BeSameAs(dto);
        _addedBooking.Should().NotBeNull();
        _addedBooking!.UserId.Should().Be(userId);
        _addedBooking.WorkoutSessionId.Should().Be(session.Id);
        _addedBooking.Status.Should().Be(BookingStatus.Active);
        _addedBooking.BookedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _unit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Booking CreateBooking(Guid userId, BookingStatus status = BookingStatus.Active,
        DateTime? sessionStart = null) => new()
        {
            UserId = userId,
            Status = status,
            WorkoutSession = new WorkoutSession { StartsAt = sessionStart ?? DateTime.UtcNow.AddDays(1) }
        };

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task Cancel_EmptyId_Throws(bool emptyUser, bool emptyBooking)
    {
        var act = () => CreateSut().CancelAsync(
            emptyUser ? Guid.Empty : Guid.NewGuid(), emptyBooking ? Guid.Empty : Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidIdException>();
        _unit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Cancel_UserNotFound_Throws()
    {
        var userId = Guid.NewGuid();
        _userRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var act = () => CreateSut().CancelAsync(userId, Guid.NewGuid());

        await act.Should().ThrowAsync<UserNotFoundException>();
    }

    [Fact]
    public async Task Cancel_BookingNotFound_Throws()
    {
        var userId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        _userRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new User());
        _bookingRepo.Setup(r => r.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Booking?)null);

        var act = () => CreateSut().CancelAsync(userId, bookingId);

        await act.Should().ThrowAsync<BookingNotFoundException>();
    }

    [Fact]
    public async Task Cancel_ForeignBooking_ThrowsForbiddenAndKeepsStatus()
    {
        var userId = Guid.NewGuid();
        var booking = CreateBooking(userId: Guid.NewGuid());        // სხვისია
        _userRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new User());
        _bookingRepo.Setup(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        var act = () => CreateSut().CancelAsync(userId, booking.Id);

        await act.Should().ThrowAsync<ForbiddenException>();
        booking.Status.Should().Be(BookingStatus.Active);
        _unit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Cancel_AlreadyCancelled_Throws()
    {
        var userId = Guid.NewGuid();
        var booking = CreateBooking(userId, BookingStatus.Cancelled);
        _userRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new User());
        _bookingRepo.Setup(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        var act = () => CreateSut().CancelAsync(userId, booking.Id);

        await act.Should().ThrowAsync<BookingAlreadyCancelledException>();
        _unit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Cancel_SessionAlreadyStarted_ThrowsAndKeepsStatus()
    {
        var userId = Guid.NewGuid();
        var booking = CreateBooking(userId, sessionStart: DateTime.UtcNow.AddMinutes(-5));
        _userRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new User());
        _bookingRepo.Setup(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        var act = () => CreateSut().CancelAsync(userId, booking.Id);

        await act.Should().ThrowAsync<SessionHasAlreadyStartedException>();
        booking.Status.Should().Be(BookingStatus.Active);
        _unit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Cancel_Valid_SetsCancelledAndSavesOnce()
    {
        var userId = Guid.NewGuid();
        var booking = CreateBooking(userId);
        _userRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new User());
        _bookingRepo.Setup(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        await CreateSut().CancelAsync(userId, booking.Id);

        booking.Status.Should().Be(BookingStatus.Cancelled);
        _unit.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAll_PageBelowOne_ThrowsBadRequest()
    {
        var act = () => CreateSut().GetAllAsync(
            new BookingFilterDTO(), new PaginationQueryDTO { Page = 0, PageSize = 10 });

        await act.Should().ThrowAsync<BadRequestException>();
        _bookingRepo.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public async Task GetAll_PageSizeOutOfRange_ThrowsPageSizeException(int pageSize)
    {
        var act = () => CreateSut().GetAllAsync(
            new BookingFilterDTO(), new PaginationQueryDTO { Page = 1, PageSize = pageSize });

        await act.Should().ThrowAsync<PageSizeException>();
        _bookingRepo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetAll_EmptySessionIdFilter_Throws()
    {
        var act = () => CreateSut().GetAllAsync(
            new BookingFilterDTO { WorkoutSessionId = Guid.Empty },
            new PaginationQueryDTO { Page = 1, PageSize = 10 });

        await act.Should().ThrowAsync<InvalidIdException>();
        _bookingRepo.VerifyNoOtherCalls();
    }
}