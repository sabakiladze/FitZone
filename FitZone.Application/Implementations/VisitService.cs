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
        public async Task<CheckInResponseDTO> AddVisitAsync(Guid userId, CancellationToken ct)
        {

            var membership = await _userMembershipRepo.GetActiveByUserIdAsync(userId, ct) // rom shevamowmo remaining visits
                ?? throw new NoActiveMembershipException();

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

}
