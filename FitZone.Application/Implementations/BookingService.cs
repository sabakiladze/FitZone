using FitZone.Application.DTOs;
using FitZone.Application.DTOs.BookingDTOs;
using FitZone.Application.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.Implementations
{
    public class BookingService : IBookingService
    {
        public Task CancelAsync(Guid userId, Guid bookingId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<BookingDTO> CreateAsync(Guid userId, CreateBookingRequestDTO request, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (request.WorkoutSessionId == Guid.Empty) throw new EntryPointNotFoundException();

        }

        public Task<PagedResponseDTO<BookingDTO>> GetAllAsync(BookingFilterDTO filter, PaginationQueryDTO paging, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResponseDTO<BookingDTO>> GetMyAsync(Guid userId, PaginationQueryDTO paging, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
