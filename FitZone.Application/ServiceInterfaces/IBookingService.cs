using FitZone.Application.DTOs.BookingDTOs;
using FitZone.Application.DTOs.PaginationDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.ServiceInterfaces
{
    public interface IBookingService
    {
       
            Task<BookingDTO> CreateAsync(Guid userId, CreateBookingRequestDTO request, CancellationToken ct = default);

            Task CancelAsync(Guid userId, Guid bookingId, CancellationToken ct = default);

            Task<PagedResponseDTO<BookingDTO>> GetMyAsync(Guid userId, PaginationQueryDTO paging, CancellationToken ct = default);

            Task<PagedResponseDTO<BookingDTO>> GetAllAsync(BookingFilterDTO filter, PaginationQueryDTO paging, CancellationToken ct = default);
        
    }
}
