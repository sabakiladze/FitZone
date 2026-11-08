using FitZone.Domain.Entities.Models;
using FitZone.Domain.Enums;
using FitZone.Domain.Interfaces;
using FitZone.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly FitZoneDbContext _fitZoneDbContext;
        public UserRepository(FitZoneDbContext fitZoneDbContext)
        {
            _fitZoneDbContext = fitZoneDbContext;
        }
        public async Task<User> AddAsync(User user, CancellationToken ct = default)
        {
             await _fitZoneDbContext.Users.AddAsync(user, ct);
            return user;
            
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        {
            return await _fitZoneDbContext.Users.AnyAsync(u => u.Email == email, ct);
            
        }

        public async Task<bool> ExistsByUserNameAsync(string userName, CancellationToken ct = default)
        {
            return await _fitZoneDbContext.Users.AnyAsync(u => u.UserName == userName, ct);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            return await _fitZoneDbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email, ct);
        }

        public async Task<User?> GetUserByHashedToken(string hash, CancellationToken ct = default)
        {
            return await _fitZoneDbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.RefreshTokenHash == hash, ct);
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _fitZoneDbContext.Users
                .Include(x=>x.Role)
                .FirstOrDefaultAsync(u => u.Id == id, ct);
        }

        public async Task<(ICollection<User> Items, int TotalCount)> GetPagedAsync(string? search, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _fitZoneDbContext.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .Where(u => u.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u => u.UserName.Contains(search)
                    || u.Email.Contains(search)
                    || u.FirstName.Contains(search)
                    || u.LastName.Contains(search));
            }

            var total = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<bool> HasActiveFutureBookingsAsync(Guid userId, CancellationToken ct = default)
        {
            return await _fitZoneDbContext.Bookings.AnyAsync(x=>x.UserId== userId && x.Status!=BookingStatus.Cancelled && x.Status!=BookingStatus.NoShow && x.WorkoutSession.StartsAt>DateTime.UtcNow, ct);
        }
    }
}
