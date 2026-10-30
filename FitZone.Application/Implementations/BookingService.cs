using AutoMapper;
using Castle.Core.Logging;
using FitZone.Application.DTOs;
using FitZone.Application.DTOs.BookingDTOs;
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
        public BookingService(IBookingRepository repo, IWorkOutSessionRepository workout, IUserMembershipRepository userMembershipPlanRepository, ILogger<BookingService> logger, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _bookingRepository = repo;
            _workOutSessionRepository= workout;
            _userMembershipPlanRepository = userMembershipPlanRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public Task CancelAsync(Guid userId, Guid bookingId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<BookingDTO> CreateAsync(Guid userId, CreateBookingRequestDTO request, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.WorkoutSessionId == Guid.Empty) throw new EntryPointNotFoundException();

            var user = await _bookingRepository.GetByIdAsync(userId, ct) ?? throw new UserNotFoundException();

            var session= await _workOutSessionRepository.GetByIdAsync(userId, ct) ?? throw new WorkoutSessionNotFoundException();

            if (session.StartsAt <= DateTime.UtcNow) throw new SessionHasAlreadyStartedException();

            var validPlan=await _userMembershipPlanRepository.GetActiveByUserIdAsync(userId) ?? throw new NoActiveMembershipException();

            if (validPlan.RemainingVisits is <= 0) throw new NoVisitsLeftException();

            var has = await _bookingRepository.UserHasActiveBookingAsync(userId, request.WorkoutSessionId, ct);
            if (has == true) throw new AlreadyBookedException();

            if(session.Capacity>=await _workOutSessionRepository.CountActiveBookingsAsync(request.WorkoutSessionId, ct)) throw new SessionFullException();

            if (await _bookingRepository.UserHasOverlapAsync(userId,session.StartsAt, session.EndsAt, ct)) throw new BookingTimeConflictException();

            var booking = new Booking
            {
                UserId = userId,
                WorkoutSessionId = session.Id,
                BookedAt = DateTime.UtcNow,
                Status = BookingStatus.Active          
            };

            await _bookingRepository.AddAsync(booking, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Booking {BookingId} created for user {UserId} on session {SessionId}.",booking.Id, userId, session.Id);


            var saved = await _bookingRepository.GetByIdAsync(booking.Id, ct)?? throw new BookingNotFoundException();
            return _mapper.Map<BookingDTO>(saved);
        }

        public Task<PagedResponseDTO<BookingDTO>> GetAllAsync(BookingFilterDTO filter, PaginationQueryDTO paging, CancellationToken ct = default)
        {
            //ArgumentNullException.ThrowIfNull(filter);
            //ArgumentNullException.ThrowIfNull(paging);

            //if (paging.Page < 1)
            //    throw new BadRequestException("Page must be 1 or greater.");

            //if (filter.WorkoutSessionId.HasValue && filter.WorkoutSessionId.Value == Guid.Empty)
            //    throw new BadRequestException("WorkoutSessionId is not valid.");

            //if (filter.Status.HasValue && !Enum.IsDefined(filter.Status.Value))
            //    throw new BadRequestException("Status is not valid.");

            //if (filter.TrainerName is { Length: > 100 })
            //    throw new BadRequestException("TrainerName must be at most 100 characters.");

            throw new NotImplementedException();


        }

        public Task<PagedResponseDTO<BookingDTO>> GetMyAsync(Guid userId, PaginationQueryDTO paging, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
