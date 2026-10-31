using AutoMapper;
using FitZone.Application.DTOs.BookingDTOs;
using FitZone.Application.DTOs.PaginationDTOs;
using FitZone.Application.ServiceInterfaces;
using FitZone.Domain.CustomExceptions;
using FitZone.Domain.Entities.Models;
using FitZone.Domain.Enums;
using FitZone.Domain.Interfaces;
using FitZone.Domain.RepositoryInterfaces;
using Microsoft.Extensions.Logging;

namespace FitZone.Application.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IWorkOutSessionRepository _workOutSessionRepository;
        private readonly IUserMembershipRepository _userMembershipPlanRepository;
        private readonly ILogger<BookingService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;

        public BookingService(
            IBookingRepository repo,
            IWorkOutSessionRepository workout,
            IUserMembershipRepository userMembershipPlanRepository,
            ILogger<BookingService> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IUserRepository userRepository)
        {
            _bookingRepository = repo;
            _workOutSessionRepository = workout;
            _userMembershipPlanRepository = userMembershipPlanRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        public async Task CancelAsync(Guid userId, Guid bookingId, CancellationToken ct = default)
        {
            if (userId == Guid.Empty || bookingId == Guid.Empty)
                throw new InvalidIdException();


            var user = await _userRepository.GetByIdAsync(userId, ct) ?? throw new UserNotFoundException();

            var booking = await _bookingRepository.GetByIdAsync(bookingId, ct)?? throw new BookingNotFoundException();

            if (booking.UserId != userId)
                throw new ForbiddenException();

            if (booking.Status == BookingStatus.Cancelled)
                throw new BookingAlreadyCancelledException();

            if (booking.WorkoutSession.StartsAt <= DateTime.UtcNow)
                throw new SessionHasAlreadyStartedException();

            booking.Status = BookingStatus.Cancelled;
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Booking {BookingId} cancelled by user {UserId}.", bookingId, userId);
        }

        public async Task<BookingDTO> CreateAsync(Guid userId, CreateBookingRequestDTO request, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (userId == Guid.Empty || request.WorkoutSessionId == Guid.Empty)
                throw new InvalidIdException();

            _ = await _userRepository.GetByIdAsync(userId, ct) ?? throw new UserNotFoundException();

            var session = await _workOutSessionRepository.GetByIdAsync(request.WorkoutSessionId, ct)
                ?? throw new WorkoutSessionNotFoundException();

            if (session.StartsAt <= DateTime.UtcNow)
                throw new SessionHasAlreadyStartedException();

            var membership = await _userMembershipPlanRepository.GetActiveByUserIdAsync(userId, ct)
                ?? throw new NoActiveMembershipException();

            if (membership.RemainingVisits is <= 0)
                throw new NoVisitsLeftException();

            if (await _bookingRepository.UserHasActiveBookingAsync(userId, session.Id, ct))
                throw new AlreadyBookedException();

            var activeCount = await _workOutSessionRepository.CountActiveBookingsAsync(session.Id, ct);
            if (activeCount >= session.Capacity)
                throw new SessionFullException();

            if (await _bookingRepository.UserHasOverlapAsync(userId, session.StartsAt, session.EndsAt, ct))
                throw new BookingTimeConflictException();

            var booking = new Booking
            {
                UserId = userId,
                WorkoutSessionId = session.Id,
                BookedAt = DateTime.UtcNow,
                Status = BookingStatus.Active
            };

            await _bookingRepository.AddAsync(booking, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Booking {BookingId} created for user {UserId} on session {SessionId}.",
                booking.Id, userId, session.Id);

            var saved = await _bookingRepository.GetByIdAsync(booking.Id, ct)
                ?? throw new BookingNotFoundException();

            return _mapper.Map<BookingDTO>(saved);
        }

        public async Task<PagedResponseDTO<BookingDTO>> GetAllAsync(BookingFilterDTO filter, PaginationQueryDTO paging, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(filter);
            ArgumentNullException.ThrowIfNull(paging);

            if (paging.Page < 1)
                throw new BadRequestException("Page must be 1 or greater.");

            if (filter.WorkoutSessionId.HasValue && filter.WorkoutSessionId.Value == Guid.Empty)
                throw new InvalidIdException();

            if (paging.PageSize < 1 || paging.PageSize > 50)
                throw new PageSizeException();

            if (filter.Status.HasValue && !Enum.IsDefined(filter.Status.Value))
                throw new StatusNotValidException();

            if (filter.TrainerName is { Length: > 100 })
                throw new BadRequestException("TrainerName must be at most 100 characters.");

           var (items, total)= await _bookingRepository.GetFilteredAsync(filter.WorkoutSessionId, filter.Status, filter.TrainerName, filter.Date, paging.Page, paging.PageSize, ct);

            var pageResponse=new PagedResponseDTO<BookingDTO>()
            {
                TotalCount=total,
                Values= _mapper.Map<ICollection<BookingDTO>>(items)
            };
            return pageResponse;

        }

        public async Task<PagedResponseDTO<BookingDTO>> GetMyAsync(Guid userId, PaginationQueryDTO paging, CancellationToken ct = default)
        {
            if (userId == Guid.Empty)
                throw new InvalidIdException();

            ArgumentNullException.ThrowIfNull(paging);

            if (paging.Page < 1 || paging.PageSize < 1 || paging.PageSize > 50)
                throw new PageSizeException();

            var (items, total) = await _bookingRepository.GetByUserAsync(
                userId, paging.Page, paging.PageSize, ct);

            return new PagedResponseDTO<BookingDTO>
            {
                TotalCount = total,
                Values = _mapper.Map<ICollection<BookingDTO>>(items)
            };
        }
    }
}
