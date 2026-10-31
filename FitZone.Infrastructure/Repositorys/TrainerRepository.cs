using AutoMapper;
using FitZone.Domain.Entities.Models;
using FitZone.Domain.RepositoryInterfaces;
using FitZone.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Infrastructure.Repositorys
{
    public class TrainerRepository : ITrainerRepository
    {
        private readonly FitZoneDbContext _context;
        private readonly IMapper _mapper;

        public TrainerRepository(FitZoneDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task AddAsync(Trainer trainer, CancellationToken ct = default)
        {
            await _context.Trainers.AddAsync(trainer);
        }

        public async Task<ICollection<Trainer>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Trainers.AsNoTracking().Where(x=>x.IsActive).ToListAsync();
        }

        public async Task<Trainer?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Trainers.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IReadOnlyList<Trainer>> SearchAsync(string? name, string? specialization, CancellationToken ct = default)
        {
            var query = _context.Trainers.AsNoTracking().Where(x=>x.IsActive);

            if (name is not null)
                query = query.Where(t => t.FullName.Contains(name));

            if (specialization is not null)
                query = query.Where(t => t.Specialization.Contains(specialization));

            return await query.OrderBy(t => t.FullName).ToListAsync(ct);
        }

        public  void  Remove(Trainer trainer)
        {
             _context.Trainers.Remove(trainer);
        }

        public async Task Update(Trainer trainer)
        {
            var existingTrainer = await _context.Trainers
                .FirstOrDefaultAsync(x => x.Id == trainer.Id);

            if (existingTrainer is null)
                return;

            _mapper.Map(trainer, existingTrainer);
        }

        public async Task<bool> HasUpcomingSessionsAsync(Guid trainerId, CancellationToken ct = default)
        {
            return await _context.WorkoutSessions
        .AnyAsync(s => s.TrainerId == trainerId && s.StartsAt > DateTime.UtcNow, ct);

            // Tu true daabruna anu aqvs momavalshi
        }
    }
}
