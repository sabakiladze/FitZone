using FitZone.Domain.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.RepositoryInterfaces
{
    public interface IUserMembershipRepository
    {
        
        // user ის გეგმას ვნახავთ, რომელიც ამ მომენტში აქვს
        Task<UserMembershipPlan?> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default);

        // ეს გამოიყენება იმისთვის, რომ როდესააც user იყიდის გეგმას, შუამავალი  ცხრილი UserMembership 
        // ხომ უნდა შეიქმნას და ამას გადავცემ რომ დაამატოს ეს ობიექტი.
        Task AddAsync(UserMembershipPlan membership, CancellationToken ct = default);

        // user-ის ყველა აბონემენტი (მოქმედიც და ვადაგასულიც), ახალი პირველი
        Task<ICollection<UserMembershipPlan>> GetAllByUserIdAsync(Guid userId, CancellationToken ct = default);

    }
}
