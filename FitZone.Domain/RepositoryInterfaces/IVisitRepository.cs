using FitZone.Domain.Entities.Models;

namespace FitZone.Domain.RepositoryInterfaces
{
    public interface IVisitRepository
    {
        Task<ICollection<Visit>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

        Task AddAsync(Visit visit, CancellationToken ct = default);
    }
}
