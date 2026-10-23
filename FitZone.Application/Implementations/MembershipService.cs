using AutoMapper;
using FitZone.Application.DTOs.MembershipDTOs;
using FitZone.Application.ServiceInterfaces;
using FitZone.Domain.CustomExceptions;
using FitZone.Domain.Entities.Models;
using FitZone.Domain.Interfaces;
using FitZone.Domain.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.Implementations
{
    public class MembershipService : IMembershipService
    {
        private readonly IUserMembershipRepository _userMembershipPlanRepository;
        private readonly IMembershipPlanRepository _membershipPlanRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        public MembershipService(IUserMembershipRepository userMembershipPlanRepository,
            IUnitOfWork unitOfWork,
            IMembershipPlanRepository membershipPlanRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _userMembershipPlanRepository = userMembershipPlanRepository;
            _membershipPlanRepository = membershipPlanRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userRepository = userRepository;
        }
        public async Task<UserMembershipDTO> ActivateAsync(Guid userId, ActivateMembershipRequestDTO request, CancellationToken ct = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, ct) ?? throw new UserNotFoundException();

            var plan = await _membershipPlanRepository.GetbyIdMembershipPlan(request.PlanId, ct) ?? throw new MembershipPlanDoNotExistsException();

            var current_plan = _userMembershipPlanRepository.GetActiveByUserIdAsync(userId, ct) ?? throw new YouAlreadyHavePlanException();

            var startDate = DateTime.UtcNow;
            //var membership = new UserMembershipPlan
            //{
            //    UserId = user.Id,
            //    MembershipPlanId = plan.Id,
            //    StartDate = startDate,
            //    EndDate = startDate.AddDays(plan.DurationInDays),
            //    RemainingVisits = plan.VisitLimit

            //};
            //await _userMembershipRepository.AddAsync(membership, ct);
            //await _unitOfWork.SaveChangesAsync(ct);

            //_logger.LogInformation("User {UserId} activated plan {PlanId}.", userId, plan.Id);

            //return new UserMembershipDTO
            //{
            //    Id = membership.Id,
            //    PlanName = plan.Name,
            //    StartDate = membership.StartDate,
            //    EndDate = membership.EndDate,
            //    RemainingVisits = membership.RemainingVisits,
            //    IsActive = true
            //};

            throw new Exception();
        }

        public async Task<UserMembershipDTO?> GetMyActiveAsync(Guid userId, CancellationToken ct = default)
        {
            var membership = await _userMembershipPlanRepository.GetActiveByUserIdAsync(userId, ct);
            if (membership is null) return null;
            return _mapper.Map<UserMembershipDTO>(membership);

        }

        public async Task<ICollection<MembershipPlanDTO>> GetPlansAsync(CancellationToken ct = default)
        {
            throw new Exception();
        }
    }
}
