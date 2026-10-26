using FitZone.Domain.Entities.Models;
using FitZone.Domain.RepositoryInterfaces;
using FitZone.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Infrastructure.Repositorys
{
    public class WorkOutSessioRepository : IWorkOutSessionRepository
    {
        private readonly FitZoneDbContext _fitZonedb;

        public WorkOutSessioRepository(FitZoneDbContext fitzonedb)
        {
            _fitZonedb = fitzonedb;
        }
        public Task AddWorkOutSession(WorkoutSession workoutSession, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteWorkOutSession(Guid id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<WorkoutSession>> GetAllWorkoutSession(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<WorkoutSession> GetWorkoutSessionById(Guid id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<WorkoutSession> GetWorkoutSessionByUserId(Guid userId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateWorkOutSession(WorkoutSession session, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
