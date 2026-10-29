using FitZone.Domain.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.RepositoryInterfaces
{
    public interface IWorkOutSessionRepository
    {

        // იმის მიხედვით რას გადავცემ Guid? trainerId, DateOnly? null აბლე რადგნააა მაშინ შემიძლია რომ სხვადასხვა კომბინაციებით გაოვიტანო. იქნება ეს ყველა, მხოლოდ დრო, მხოლოდ ტრნერი თუ ორივე.
        Task<(ICollection<WorkoutSession> Items, int TotalCount)> GetFilteredAsync(Guid? trainerId, DateOnly? date, int page, int pageSize,
            CancellationToken ct = default);// აბრუნებს რამდენი ობიექტია ჯამში რადგან ღილაკების რაოდენობა განსაზღვროს და იმ გვერდის დიქსირებული ზომის ობიექტებს.  სერვისში გადავქცევ dto- ოდ select ის საშუალებით.

        Task<WorkoutSession?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<bool> TrainerHasOverlapAsync(Guid trainerId, DateTime startsAt, DateTime endsAt, Guid? excludeSessionId = null, CancellationToken ct = default);
        Task<int> CountActiveBookingsAsync(Guid sessionId, CancellationToken ct = default);  //workoutsession ის აქტიური bookin
        Task AddAsync(WorkoutSession session, CancellationToken ct = default);
        void Remove(WorkoutSession session);


    }
}
