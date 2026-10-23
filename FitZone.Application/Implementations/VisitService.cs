using AutoMapper;
using FitZone.Application.DTOs.VisitDTOs;
using FitZone.Application.ServiceInterfaces;
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

        public VisitService(IVisitRepository visitRepo,
            IUnitOfWork unit,
            ILogger<VisitService> logger,
            IMapper mapper)
        {
            _visitRepo = visitRepo;
            _unit = unit;
            _logger = logger;
            _mapper = mapper;
        }
        public async Task<CheckInResponseDTO> AddVisitAsync(Visit visit, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(visit);
            await _visitRepo.AddAsync(visit, ct);


        }

        public Task<ICollection<VisitDTO>> GetMyVisitHistory(Visit visit, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
