using AutoMapper;
using Castle.Core.Logging;
using FitZone.Application.DTOs;
using FitZone.Application.DTOs.WorkOutSessionDTOs;
using FitZone.Application.ServiceInterfaces;
using FitZone.Domain.CustomExceptions;
using FitZone.Domain.Entities.Models;
using FitZone.Domain.Interfaces;
using FitZone.Domain.RepositoryInterfaces;
using Microsoft.Extensions.Logging;


namespace FitZone.Application.Implementations
{
    public class WorkoutSessionService : IWorkOutSessionService
    {
        private readonly IWorkOutSessionRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<WorkoutSession> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITrainerRepository _trainerRepository;

        public WorkoutSessionService(IWorkOutSessionRepository repository,
            IMapper mapper,
            ILogger<WorkoutSession> logger,
            IUnitOfWork unitOfWork,
            ITrainerRepository trainerRepo)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _trainerRepository = trainerRepo;
        }
        public async Task<WorkoutSessionDTO> AddAsync(CreateWorkoutSessionRequestDTO request, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.TrainerId == Guid.Empty)
                throw new BadRequestException("TrainerId is required.");

            if (request.StartsAt == default || request.EndsAt == default)
                throw new BadRequestException("StartsAt and EndsAt are required.");

            if (request.EndsAt <= request.StartsAt)
                throw new BadRequestException("EndsAt must be later than StartsAt.");

            if (request.StartsAt <= DateTime.UtcNow)
                throw new BadRequestException("Session must start in the future.");

            var trainer = await _trainerRepository.GetByIdAsync(request.TrainerId, ct)?? throw new TrainersDoNotFoundException();


            var session = _mapper.Map<WorkoutSession>(request);
            await _repository.AddAsync(session, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("WorkoutSession {SessionId} created.", session.Id);

            return _mapper.Map<WorkoutSessionDTO>(request);
            // დასასრულებელი
        }

        public async Task<PagedResponseDTO<WorkoutSessionDTO>> GetAsync(WorkoutSessionFilterDTO filter, PaginationQueryDTO paging, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(filter);
            ArgumentNullException.ThrowIfNull(paging);

            if (paging.Page < 1)
                throw new BadRequestException("Page must be 1 or greater.");

            if (paging.PageSize < 1 || paging.PageSize > 50)
                throw new BadRequestException("PageSize must be between 1 and 50.");

            if (filter.TrainerId.HasValue && filter.TrainerId.Value == Guid.Empty)
                throw new BadRequestException("TrainerId is not valid.");

            var (items, total) = await _repository.GetFilteredAsync(filter.TrainerId, filter.Date, paging.Page, paging.PageSize, ct);
            items = (ICollection<WorkoutSession>)_mapper.Map<WorkoutSessionDTO>(items);


        }

        public async Task<WorkoutSessionDTO> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var workout = await _repository.GetByIdAsync(id, ct) ?? throw new WorkoutSessionNotFounfException();
            return _mapper.Map<WorkoutSessionDTO>(workout);

        }

        public async Task RemoveAsync(Guid id, CancellationToken ct = default)
        {
            var workout = await _repository.GetByIdAsync(id, ct) ?? throw new WorkoutSessionNotFounfException();
             _repository.Remove(workout);

            _logger.LogInformation($"Removed workoutsession by {workout.Id} Id", id);

            await _unitOfWork.SaveChangesAsync(ct);

        }

        public async Task UpdateAsync(Guid id, UpdateWorkoutSessionRequestDTO request, CancellationToken ct = default)
        {
            var session = await _repository.GetByIdAsync(id, ct) ?? throw new WorkoutSessionNotFounfException();

            session.TrainerId = request.TrainerId;
            session.StartsAt = request.StartsAt;
            session.EndsAt = request.EndsAt;
            session.Capacity= request.Capacity;

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
