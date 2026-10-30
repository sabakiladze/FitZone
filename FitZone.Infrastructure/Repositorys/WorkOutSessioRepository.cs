using FitZone.Domain.CustomExceptions;
using FitZone.Domain.Entities.Models;
using FitZone.Domain.RepositoryInterfaces;
using FitZone.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;


namespace FitZone.Infrastructure.Repositorys
{
    public class WorkOutSessioRepository : IWorkOutSessionRepository
    {
        private readonly FitZoneDbContext _fitZonedb;

        public WorkOutSessioRepository(FitZoneDbContext fitzonedb)
        {
            _fitZonedb = fitzonedb;
        }

        public async Task AddAsync(WorkoutSession session, CancellationToken ct = default)
        {
            await _fitZonedb.WorkoutSessions.AddAsync(session, ct);
        }

        public async Task<int> CountActiveBookingsAsync(Guid sessionId, CancellationToken ct = default)
        {
            return await _fitZonedb.Bookings.CountAsync(x =>
            (x.WorkoutSessionId == sessionId &&
            x.Status != Domain.Enums.BookingStatus.Cancelled &&
            x.Status != Domain.Enums.BookingStatus.NoShow), ct);

           // ეს ეძებს და ითვლის ისეთ book ობიექტებს რომლებიც ეკუთვნის მოცემული სესსის აიდის და ასევე მისი სტატუსი არაა გაუქმებული ანუ არის აქტიური
           // გამიყენება იმსთის რომ გავიგოთ რამდენი ადგილი დაგვრჩა.
        }

        public async Task<WorkoutSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _fitZonedb.WorkoutSessions.Include(x=>x.Trainer).Include(x=>x.Bookings).FirstOrDefaultAsync(x=>x.Id==id, ct);
        }

        public async Task<(ICollection<WorkoutSession> Items, int TotalCount)> GetFilteredAsync(Guid? trainerId, DateOnly? date, int page, int pageSize, CancellationToken ct = default)
        {

            var query = _fitZonedb.WorkoutSessions.AsNoTracking().AsQueryable();

            if (trainerId.HasValue)
                query = query.Where(s => s.TrainerId == trainerId.Value);

            if (date.HasValue)
            {
                var from = date.Value.ToDateTime(TimeOnly.MinValue);//  MinValue 00;00;00 შუაღამე წამების ჩატლით
                var to = from.AddDays(1);// anu 00:00 დან 00:00 მდე ვამოწმებთ. // შემდეგი დღის 00:00
                query = query.Where(s => s.StartsAt >= from && s.StartsAt < to);

            }
            var total = await query.CountAsync(ct);

            var  items=await query
                .AsNoTracking()
                .Include(x=>x.Trainer)
                .Include(x=>x.Bookings)
                .OrderBy(x=>x.StartsAt)
                .Skip((page-1)*pageSize).Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        public void Remove(WorkoutSession session)
        {
            _fitZonedb.Remove(session);
        }

        public async Task<bool> TrainerHasOverlapAsync(Guid trainerId, DateTime startsAt, DateTime endsAt, Guid? excludeSessionId = null, CancellationToken ct = default)
        {
            var trainer = await _fitZonedb.Trainers.Include(x=>x.WorkoutSessions).FirstOrDefaultAsync(x=>x.Id==trainerId, ct)??throw new TrainersDoNotFoundException();

            bool allowed = true; 
            if(excludeSessionId is  null)
            {

                // თუ ახალის დრო აკმაყოფილებს ამ პირობას რომ არ ეჯახება სხვების დროების, მაშინ კითხვაზე "ეჯახება კი სხვებს" დააბრუნოს არა. ანუ სხვებს არ უშლის ხელს.
                // აქ ამ პირობას თუ აკმაყოფილებს დაა ბრუნებს true,მაგრამ ! allowed დააბრუნებს false, ანუ არა, არ უშლის ხელს.
                allowed = trainer.WorkoutSessions.All(s => s.EndsAt <= startsAt || s.StartsAt >= endsAt);     

                return !allowed;
            }


            // თუ update გვინდა საჭიროა რომ ეს სესია დროს არ მივათვალოთ და ყველა სხვა სესიასთან შევამოწმო დროები ხომ არ ეჯახება
            var workout = _fitZonedb.WorkoutSessions
                .Where(x => x.TrainerId == trainerId && x.Id != excludeSessionId);

            // თუ რომელიმეს მაინც ეჯახება დააბრუნებს false. ამის გამო მერე მე უნდა დავაბრუნო !allowed 
            allowed =  workout.All(x=>(x.EndsAt <= startsAt || x.StartsAt >= endsAt));  


            return !allowed;

            // თუ დააბრუნა true, მაშინ ფარავს სხვს და არ მაქვს უფლება
            // თუ დააბრუნა false ანუ არა არ ფარავს და მაქვს უფლება

        }




    }
}
