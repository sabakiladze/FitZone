using FitZone.Domain.Entities.Models;

namespace FitZone.Domain.RepositoryInterfaces
{
    public interface IVisitRepository
    {
        Task<ICollection<Visit>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

        Task AddAsync(Visit visit, CancellationToken ct = default);

        Task<bool> HasVisitSinceAsync(Guid userId, DateTime since, CancellationToken ct = default);

        Task<Visit?> GetByIdAsync(Guid id, CancellationToken ct = default);

        void Remove(Visit visit);
    }
}
