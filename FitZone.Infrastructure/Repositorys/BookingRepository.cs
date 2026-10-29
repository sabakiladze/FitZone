using FitZone.Domain.Entities.Models;
using FitZone.Domain.Enums;
using FitZone.Domain.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Infrastructure.Repositorys
{
    public class BookingRepository : IBookingRepository
    {
        public Task AddAsync(Booking booking, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<(ICollection<Booking> Items, int TotalCount)> GetByUserAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<(ICollection<Booking> Items, int TotalCount)> GetFilteredAsync(Guid? sessionId, BookingStatus? status, int page, int pageSize, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UserHasActiveBookingAsync(Guid userId, Guid sessionId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UserHasOverlapAsync(Guid userId, DateTime startsAt, DateTime endsAt, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
