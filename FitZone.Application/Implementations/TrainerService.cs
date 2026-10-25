using AutoMapper;
using FitZone.Application.DTOs.TrainerDTOs;
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
using System.Xml.Linq;

namespace FitZone.Application.Implementations
{
    public class TrainerService : ITrainerService
    {
        private readonly ITrainerRepository _repo;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unit;
        private readonly ILogger<TrainerService> _logger;


        public TrainerService(ITrainerRepository repo, IMapper mapper,
            IUnitOfWork unit, ILogger<TrainerService> logger)
        {
            _repo = repo;
            _mapper = mapper;
            _unit = unit;
            _logger = logger;
        }


        

        public async Task RemoveAsync(Guid id, CancellationToken ct = default)
        {
            var trainer = await _repo.GetByIdAsync(id, ct)
               ?? throw new TrainersDoNotFoundException();

            _repo.Remove(trainer);
            await _unit.SaveChangesAsync(ct);

            _logger.LogInformation("Trainer {TrainerId} deleted.", trainer.Id);
        }



        public async Task<TrainerDTO> AddAsync(CreateTrainerRequestDTO request, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var trainer = _mapper.Map<Trainer>(request);
            await _repo.AddAsync(trainer, ct);
            await _unit.SaveChangesAsync(ct);

            _logger.LogInformation("Trainer {TrainerId} created.", trainer.Id);
            return _mapper.Map<TrainerDTO>(trainer);
        }

        public async Task<ICollection<TrainerDTO>> GetAllAsync(CancellationToken ct = default)
        {
            var trainers = await _repo.GetAllAsync(ct);
            return _mapper.Map<ICollection<TrainerDTO>>(trainers);
        }

        public async Task<TrainerDTO> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var trainer = await _repo.GetByIdAsync(id, ct)
                ?? throw new TrainersDoNotFoundException();
            return _mapper.Map<TrainerDTO>(trainer);
        }

        public async Task UpdateAsync(Guid id, UpdateTrainerRequestDTO request, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var trainer = await _repo.GetByIdAsync(id, ct)
                ?? throw new TrainersDoNotFoundException();

            _mapper.Map(request, trainer);
            await _unit.SaveChangesAsync(ct);

            _logger.LogInformation("Trainer {TrainerId} updated.", trainer.Id);
        }

        public async Task<ICollection<TrainerDTO>> SearchAsync(string? name, string? specialization, CancellationToken ct = default)
        {
            var trainers = await _repo.SearchAsync(
                string.IsNullOrWhiteSpace(name) ? null : name.Trim(),
                string.IsNullOrWhiteSpace(specialization) ? null : specialization.Trim(),
                ct);

            return _mapper.Map<ICollection<TrainerDTO>>(trainers);
        }
    }
}
