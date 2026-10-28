using FitZone.Domain.Entities.Models;
using FitZone.Domain.RepositoryInterfaces;
using FitZone.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FitZone.Infrastructure.Repositorys
{
    public class WorkOutSessioRepository : IWorkOutSessionRepository
    {
        private readonly FitZoneDbContext _fitZonedb;

        public WorkOutSessioRepository(FitZoneDbContext fitzonedb)
        {
            _fitZonedb = fitzonedb;
        }

        public async Task AddAsync(WorkoutSession session, CancellationToken ct = default)
        {
            await _fitZonedb.AddAsync(session, ct);
        }

        public async Task<int> CountActiveBookingsAsync(Guid sessionId, CancellationToken ct = default)
        {
            return await _fitZonedb.Bookings.CountAsync(x =>
            (x.WorkoutSessionId == sessionId &&
            x.Status != Domain.Enums.BookingStatus.Cancelled &&
            x.Status != Domain.Enums.BookingStatus.NoShow), ct);
        }

        public async Task<WorkoutSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _fitZonedb.WorkoutSessions.FirstOrDefaultAsync(x=>x.Id==id, ct);
        }

        public async Task<(ICollection<WorkoutSession>, int count)> GetFilteredAsync(Guid? trainerId, DateOnly? date, int page, int pageSize, CancellationToken ct = default)
        {

            var query = _fitZonedb.WorkoutSessions.AsNoTracking().AsQueryable();

            if (trainerId.HasValue)
                query = query.Where(s => s.TrainerId == trainerId.Value);

            if (date.HasValue)
            {
                var from = date.Value.ToDateTime(TimeOnly.MinValue);//  MinValue დროის უმცირესი საზომი, ანუ წამებსაც დაამატებს რა. ანუ წამებსაც ჩათლის
                var to = from.AddDays(1);// anu 00:00 დან 00:00 მდე ვამოწმებთ რომ მაგალითად იმ 
                query = query.Where(s => s.StartsAt >= from && s.StartsAt < to);

            }

            var sessions=await _fitZonedb.WorkoutSessions
                .AsNoTracking()
                .Include(x=>x.Trainer)
                .Include(x=>x.Bookings)
                .OrderBy(x=>x.StartsAt)
                .Skip((page-1)*pageSize).Take(pageSize)
                .ToListAsync(ct);


            return (sessions, page);
        }

        public void Remove(WorkoutSession session)
        {
            _fitZonedb?.Remove(session);
        }
    }
}
