using FitZone.Domain.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
        Task<bool> ExistsByUserNameAsync(string userName, CancellationToken ct = default);
        Task<User> AddAsync(User user, CancellationToken ct = default);

        //CancellationToken ct = default ნიშნავს: ეს პარამეტრი არასავალდებულოა, და თუ არ გადასცემ, გამოიყენება "ცარიელი" ტოკენი, რომლის გაუქმებაც არასდროს მოხდება.
    }
}
