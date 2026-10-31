using FitZone.Application.DTOs.PaginationDTOs;
using FitZone.Application.DTOs.WorkOutSessionDTOs;
using FitZone.Domain.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.ServiceInterfaces
{
    public interface IWorkOutSessionService
    {
        Task<WorkoutSessionDTO> AddAsync(CreateWorkoutSessionRequestDTO request, CancellationToken ct = default);
        Task<WorkoutSessionDTO> UpdateAsync(Guid id, UpdateWorkoutSessionRequestDTO request, CancellationToken ct = default);

        Task RemoveAsync(Guid id, CancellationToken ct = default);
        Task<WorkoutSessionDTO> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<PagedResponseDTO<WorkoutSessionDTO>> GetAsync(WorkoutSessionFilterDTO filter, PaginationQueryDTO paging, CancellationToken ct = default);

    }
}
