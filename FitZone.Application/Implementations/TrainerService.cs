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
        private readonly ILogger _logger;
        public TrainerService(ITrainerRepository repo,
           IMapper mapper,
           IUnitOfWork unit,
           ILogger logger)
        {
            _repo=repo;
            _mapper=mapper;
            _unit=unit;
            _logger = logger;
        }

        public Task AddAsync(CreateTrainerRequestDTO trainer, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(trainer);
        }

        public async Task<ICollection<TrainerDTO>> GetAllAsync(CancellationToken ct = default)
        {
           var result= await _repo.GetAllAsync(ct)?? throw new TrainersDoNotFoundException();
            return _mapper.Map<ICollection<TrainerDTO>>(result);
        }

        public async Task<TrainerDTO?> GetByIdAsync(Guid userId, CancellationToken ct = default)
        {
            var result=await _repo.GetByIdAsync(userId, ct) ?? throw new TrainersDoNotFoundException();
            return _mapper.Map<TrainerDTO>(result);
        }

        public async Task<ICollection<TrainerDTO>> GetByNameAsync(string name, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name)) 
                    {
                throw new TrainersDoNotFoundException();
            }
            var result = await _repo.GetByNameAsync(name, ct)?? throw new TrainersDoNotFoundException();
            return _mapper.Map<ICollection<TrainerDTO>>(result);
        }

        public async Task<ICollection<TrainerDTO>> GetBySpecializationAsync(string specialization, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(specialization) || string.IsNullOrWhiteSpace(specialization))
            {
                throw new TrainersDoNotFoundException();
            }
            var result = await _repo.GetBySpecializationAsync(specialization, ct) ?? throw new TrainersDoNotFoundException();
            return _mapper.Map<ICollection<TrainerDTO>>(result);
        }

        public Task RemoveAsync(Guid id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(UpdateTrainerRequestDTO trainer, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
