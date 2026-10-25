using FitZone.Application.DTOs.TrainerDTOs;
using FitZone.Domain.Entities.Models;

namespace FitZone.Application.ServiceInterfaces
{
    public interface ITrainerService
    {
        Task<TrainerDTO> AddAsync(CreateTrainerRequestDTO trainer, CancellationToken ct = default);

        Task<ICollection<TrainerDTO>> GetAllAsync(CancellationToken ct = default);

        Task<TrainerDTO?> GetByIdAsync(Guid userId, CancellationToken ct = default);

        Task<ICollection<TrainerDTO>> SearchAsync(string? name, string? specialization, CancellationToken ct = default);
        Task RemoveAsync(Guid id, CancellationToken ct = default);

        Task UpdateAsync(Guid id, UpdateTrainerRequestDTO request, CancellationToken ct = default);
    }
}