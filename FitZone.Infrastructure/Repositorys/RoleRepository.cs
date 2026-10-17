using FitZone.Domain.Entities.Models;
using FitZone.Domain.Interfaces;
using FitZone.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace FitZone.Infrastructure.Repositorys
{
    public class RoleRepository : IRoleRepository
    {
        private readonly FitZoneDbContext _fitZoneDbContext;
        public RoleRepository(FitZoneDbContext fitzone)
        {
            _fitZoneDbContext = fitzone;
        }
        public async Task<Role?> GetByNameAsync(string roleName, CancellationToken ct = default)
        {
            return await _fitZoneDbContext.Roles.FirstOrDefaultAsync(x => x.RoleName.ToLower() == roleName.ToLower(), ct);
        }
    }
}
