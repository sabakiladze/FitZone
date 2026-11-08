using AutoMapper;
using FitZone.Application.DTOs.VisitDTOs;
using FitZone.Application.ServiceInterfaces;
using FitZone.Domain.CustomExceptions;
using FitZone.Domain.Entities.Models;
using FitZone.Domain.Interfaces;
using FitZone.Domain.RepositoryInterfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.Implementations
{
    public class VisitService : IVisitService
    {
        private readonly IVisitRepository _visitRepo;
        private readonly IUnitOfWork _unit;
        private readonly ILogger<VisitService> _logger;
        private readonly IMapper _mapper;
        private readonly IUserMembershipRepository _userMembershipRepo;

        private const int CheckInCooldownHours = 12;   // one visit per 12 hours

        public VisitService(IVisitRepository visitRepo,
            IUnitOfWork unit,
            ILogger<VisitService> logger,
            IMapper mapper,
            IUserMembershipRepository userMembershipRepo)
        {
            _visitRepo = visitRepo;
            _unit = unit;
            _logger = logger;
            _mapper = mapper;
            _userMembershipRepo = userMembershipRepo;
        }
        public async Task<CheckInResponseDTO> CheckInAsync(Guid userId, CancellationToken ct=default)
        {

            if (userId == Guid.Empty) throw new EnterProperIdException();
            var membership = await _userMembershipRepo.GetActiveByUserIdAsync(userId, ct) // rom shevamowmo remaining visits
                ?? throw new NoActiveMembershipException();

            if (await _visitRepo.HasVisitSinceAsync(userId, DateTime.UtcNow.AddHours(-CheckInCooldownHours), ct))
                throw new CheckInTooSoonException();

            membership.MakeVisit();  // დარჩენილი ვიზიტების შემოწმება და დაკლება კლასში 

            var visit = new Visit
            {
                UserId = userId,
                UserMembershipPlanId = membership.Id,
                CheckedInAt = DateTime.UtcNow,
            };

            await _visitRepo.AddAsync(visit, ct);
            await _unit.SaveChangesAsync(ct);

            _logger.LogInformation("User {UserId} checked in with membership {MembershipId}.", userId, membership.Id);


            return new CheckInResponseDTO
            {
                VisitId = visit.Id,
                CheckedInAt = visit.CheckedInAt,
                RemainingVisits = membership.RemainingVisits

            };
        }
        public async Task<VisitDTO> UpdateAsync(Guid id, UpdateVisitRequestDTO request, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (id == Guid.Empty) throw new EnterProperIdException();

            var visit = await _visitRepo.GetByIdAsync(id, ct) ?? throw new VisitNotFoundException();

            var checkedInAt = request.CheckedInAt.ToUniversalTime();
            var membership = visit.UserMembershipPlan;
            if (checkedInAt < membership.StartDate || checkedInAt > membership.EndDate)
                throw new BadRequestException("CheckedInAt must be within the membership period.");

            visit.CheckedInAt = checkedInAt;
            await _unit.SaveChangesAsync(ct);

            _logger.LogInformation("Visit {VisitId} time changed to {CheckedInAt}.", id, checkedInAt);
            return _mapper.Map<VisitDTO>(visit);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            if (id == Guid.Empty) throw new EnterProperIdException();

            var visit = await _visitRepo.GetByIdAsync(id, ct) ?? throw new VisitNotFoundException();

            // limited plan that is still active: the deleted visit is given back to the user
            var membership = visit.UserMembershipPlan;
            var limit = membership.MembershipPlan?.VisitLimit;
            if (limit.HasValue && membership.EndDate > DateTime.UtcNow)
                membership.RefundVisit(limit.Value);

            _visitRepo.Remove(visit);
            await _unit.SaveChangesAsync(ct);

            _logger.LogInformation("Visit {VisitId} deleted.", id);
        }

        public async Task<ICollection<VisitDTO>> GetMyVisitsAsync(Guid userId, CancellationToken ct = default)
        {
            if (userId == Guid.Empty) throw new EnterProperIdException();
            var visits = await _visitRepo.GetByUserIdAsync(userId, ct);
            return _mapper.Map<ICollection<VisitDTO>>(visits);
        }

    }
}
