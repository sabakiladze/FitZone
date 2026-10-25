using FitZone.Domain.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.RepositoryInterfaces
{
    public interface ITrainerRepository
    {
        Task<Trainer?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<ICollection<Trainer>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Trainer>> SearchAsync(string? name, string? specialization, CancellationToken ct = default);
        Task AddAsync(Trainer trainer, CancellationToken ct = default);
        Task Update(Trainer trainer);
        void Remove(Trainer trainer);

    }
}
