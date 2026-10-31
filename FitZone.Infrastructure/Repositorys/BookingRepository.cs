using FitZone.Domain.Entities.Models;
using FitZone.Domain.Enums;
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
    public class BookingRepository : IBookingRepository
    {
        private readonly FitZoneDbContext _fitZone;
        public BookingRepository(FitZoneDbContext fitZone)
        {
            _fitZone = fitZone;
        }
        public async Task AddAsync(Booking booking, CancellationToken ct = default)
        {
            await _fitZone.Bookings.AddAsync(booking, ct);

        }

        public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _fitZone.Bookings.Include(x => x.WorkoutSession).ThenInclude(x => x.Trainer).FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<(ICollection<Booking> Items, int TotalCount)> GetByUserAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _fitZone.Bookings
                        .AsNoTracking()
                        .Where(x => x.UserId == userId);

            var total = await query.CountAsync(ct);

            var items = await query
                .Include(x => x.WorkoutSession).ThenInclude(s => s.Trainer)
                .OrderByDescending(x => x.WorkoutSession.StartsAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<(ICollection<Booking> Items, int TotalCount)> GetFilteredAsync(Guid? sessionId,
            BookingStatus? status, string? trainerName, DateOnly?
            date, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _fitZone.Bookings.AsNoTracking().AsQueryable();

            if (sessionId.HasValue)
                query = query.Where(x => x.WorkoutSessionId == sessionId.Value);

            if (status.HasValue)
                query = query.Where(x => x.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(trainerName))
                query = query.Where(x => x.WorkoutSession.Trainer!.FullName.Contains(trainerName));

            if (date.HasValue)
            {
                var from = date.Value.ToDateTime(TimeOnly.MinValue);
                var to = from.AddDays(1);
                query = query.Where(x => x.WorkoutSession.StartsAt >= from && x.WorkoutSession.StartsAt < to);
            }

            var total = await query.CountAsync(ct);

            var items = await query
                .Include(x => x.WorkoutSession).ThenInclude(s => s.Trainer)
                .OrderByDescending(x => x.BookedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }


        public  async Task<bool> UserHasActiveBookingAsync(Guid userId, Guid sessionId, CancellationToken ct = default)
        {//  უდნა შევამოწმო არის თუ არა რეგისტირრებული უსერი ამ სესიაზე


            //bookings ში უ არსებობს ისეთი ობიექტი სადას user and workoutsession id-ები ემთხვევა ამას და სტატუსი არაა გაუქმებული.
            // ანუ თუ არსებობს ასეთი ჯავშანი უსერის და სესიის ადით სადაც სტატუსი არაა გაუქმებული

            var registere = _fitZone.Bookings.Where(x => x.UserId == userId && x.WorkoutSessionId == sessionId && x.Status != BookingStatus.Cancelled && x.Status != BookingStatus.NoShow);
            if (registere.Any()) { return true; } // aqvs ukve
            return false; // ar aqvs
        }

        public async Task<bool> UserHasOverlapAsync(Guid userId, DateTime startsAt, DateTime endsAt, CancellationToken ct = default)
        {
            var user = await _fitZone.Users.AsNoTracking().Include(x => x.Bookings).ThenInclude(b => b.WorkoutSession).FirstOrDefaultAsync(x => x.Id == userId, ct);

            if (user is null) return false;

            return user.Bookings.Any(x =>
                x.Status != BookingStatus.Cancelled &&
                x.Status != BookingStatus.NoShow &&
                x.WorkoutSession.StartsAt < endsAt &&
                x.WorkoutSession.EndsAt > startsAt);
            // თუ არსეობოს ერთი ობიექტიც ასეთი მაშინ დააბრუნებს true, ანუ ეჯახებიან და არ მაქვს უფლება შევქმნა ახალი დაჯავშნა

        }


    }
}
