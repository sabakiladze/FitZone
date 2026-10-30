using AutoMapper;
using FitZone.Application.DTOs.MembershipDTOs;
using FitZone.Application.ServiceInterfaces;
using FitZone.Domain.CustomExceptions;
using FitZone.Domain.Entities.Models;
using FitZone.Domain.Interfaces;
using FitZone.Domain.RepositoryInterfaces;
using Microsoft.Extensions.Logging;

namespace FitZone.Application.Implementations
{
    public class MembershipService : IMembershipService
    {
        private readonly IUserMembershipRepository _userMembershipRepository;
        private readonly IMembershipPlanRepository _membershipPlanRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<MembershipService> _logger;

        public MembershipService(
            IUserMembershipRepository userMembershipRepository,
            IMembershipPlanRepository membershipPlanRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<MembershipService> logger)
        {
            _userMembershipRepository = userMembershipRepository;
            _membershipPlanRepository = membershipPlanRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<UserMembershipDTO> ActivateAsync(
            Guid userId, ActivateMembershipRequestDTO request, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (userId == Guid.Empty) throw new EnterProperIdException();
            if (request.PlanId == Guid.Empty) throw new EnterProperIdException();


            var plan = await _membershipPlanRepository.GetbyIdMembershipPlan(request.PlanId, ct)
                ?? throw new MembershipPlanDoNotExistsException();
            if (request.Amount != plan.Price)
                throw new NotEnoyghMoneyException();


            var existing = await _userMembershipRepository.GetActiveByUserIdAsync(userId, ct);
            if (existing is not null)
                throw new YouAlreadyHavePlanException();

            var startDate = DateTime.UtcNow;
            var membership = new UserMembershipPlan(userId, plan.Id, DateTime.UtcNow, plan.DurationInDays, plan.VisitLimit);

            await _userMembershipRepository.AddAsync(membership, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("User {UserId} activated plan {PlanId}.", userId, plan.Id);

            var dto = _mapper.Map<UserMembershipDTO>(membership);
            dto.PlanName = plan.Name;
            return dto;
        }

        public async Task<UserMembershipDTO?> GetMyActiveAsync(Guid userId, CancellationToken ct = default)
        {

            if (userId == Guid.Empty) throw new EnterProperIdException();
            // usermembership უსერ ის მოქმედ აბონიმენტს აბრუნებს
            var membership = await _userMembershipRepository.GetActiveByUserIdAsync(userId, ct);
            return membership is null ? null : _mapper.Map<UserMembershipDTO>(membership);
        }

        public async Task<ICollection<MembershipPlanDTO>> GetPlansAsync(CancellationToken ct = default)
        {
            var plans = await _membershipPlanRepository.GetAllMembersipPlanAsync(ct);
            return _mapper.Map<ICollection<MembershipPlanDTO>>(plans);
        }
    }
}