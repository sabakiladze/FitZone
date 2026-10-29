using FitZone.Domain.Entities.Models;
using FitZone.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.RepositoryInterfaces
{
    public  interface IBookingRepository
    {
        Task AddAsync(Booking booking, CancellationToken ct = default);

        Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<(ICollection<Booking> Items, int TotalCount)> GetByUserAsync(
    Guid userId, int page, int pageSize, CancellationToken ct = default);

        Task<(ICollection<Booking> Items, int TotalCount)> GetFilteredAsync(
    Guid? sessionId, BookingStatus? status, int page, int pageSize, CancellationToken ct = default);

        Task<bool> UserHasActiveBookingAsync(Guid userId, Guid sessionId, CancellationToken ct = default);

        Task<bool> UserHasOverlapAsync(Guid userId, DateTime startsAt, DateTime endsAt, CancellationToken ct = default);

    }
}
