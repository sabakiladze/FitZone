using FitZone.Web.Models;
using FitZone.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitZone.Web.Controllers;

// განრიგის ნახვა: თუ API საჯაროს ტოვებს, ანონიმურიც ხედავს; თუ 401 დააბრუნა, გვერდი "შედი სისტემაში" გამოჩნდება.
public class SessionsController : BaseController
{
    private const int PageSize = 12;

    private readonly ApiClient _api;

    public SessionsController(ApiClient api) => _api = api;

    public async Task<IActionResult> Index(Guid? trainerId, DateOnly? date, int page = 1, CancellationToken ct = default)
    {
        var trainers = await _api.GetAsync<List<TrainerDto>>("api/trainers", ct);

        var result = await _api.GetAsync<PagedResult<WorkoutSessionDto>>(
            ApiClient.Url("api/training-sessions",
                ("trainerId", trainerId), ("date", date), ("page", page), ("pageSize", PageSize)), ct);

        var vm = new SessionsIndexVm
        {
            Items = result.Data?.Values ?? new(),
            Pager = new PagerModel { Page = page, PageSize = PageSize, TotalCount = result.Data?.TotalCount ?? 0 },
            Trainers = trainers.Data ?? new(),
            TrainerId = trainerId,
            Date = date
        };

        if (!result.Success)
        {
            if (result.StatusCode == 401) vm.NeedsLogin = true;
            else Failure(result.ErrorText);
        }

        return View(vm);
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var session = await _api.GetAsync<WorkoutSessionDto>($"api/training-sessions/{id}", ct);

        if (session.StatusCode == 401)
            return View(new SessionDetailsVm { NeedsLogin = true });

        if (!session.Success || session.Data is null)
        {
            Failure(session.ErrorText);
            return RedirectToAction(nameof(Index));
        }

        var vm = new SessionDetailsVm { Session = session.Data };

        if (User.IsInRole("Admin"))
        {
            var bookings = await _api.GetAsync<PagedResult<BookingDto>>(
                ApiClient.Url("api/bookings", ("workoutSessionId", id), ("pageSize", 50)), ct);
            vm.Bookings = bookings.Data?.Values ?? new();
        }
        else if (User.IsInRole("User"))
        {
            // აქვს თუ არა ამ სესიაზე აქტიური ჯავშანი (ღილაკი Book / Cancel booking)
            var mine = await _api.GetAsync<PagedResult<BookingDto>>(
                ApiClient.Url("api/bookings/my", ("pageSize", 50)), ct);
            vm.MyBooking = mine.Data?.Values.FirstOrDefault(b =>
                b.WorkoutSessionId == id && b.Status is BookingStatus.Active or BookingStatus.Pending);
        }

        return View(vm);
    }

    // ---------- User: დაჯავშნა ----------

    [HttpPost, Authorize(Roles = "User")]
    public async Task<IActionResult> Book(Guid id, CancellationToken ct)
    {
        var result = await _api.PostAsync<BookingDto>("api/bookings", new { WorkoutSessionId = id }, ct);

        if (result.Success)
        {
            Success("Session booked.");
            return RedirectToAction("My", "Bookings");
        }

        Failure(result.ErrorText);
        return RedirectToAction(nameof(Details), new { id });
    }

    // ---------- Admin ----------

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Guid? trainerId, CancellationToken ct)
    {
        var start = Fmt.ToLocal(DateTime.UtcNow).Date.AddDays(1).AddHours(10);
        var form = new SessionForm { StartsAt = start, EndsAt = start.AddHours(1) };
        if (trainerId is not null) form.TrainerId = trainerId.Value;
        return View("Form", await WithTrainers(form, ct));
    }

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(SessionForm form, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View("Form", await WithTrainers(form, ct));

        var result = await _api.PostAsync("api/training-sessions", new
        {
            form.TrainerId,
            StartsAt = Fmt.ToUtc(form.StartsAt),
            EndsAt = Fmt.ToUtc(form.EndsAt),
            form.Capacity
        }, ct);

        if (!result.Success)
        {
            AddApiErrors(result);
            return View("Form", await WithTrainers(form, ct));
        }

        Success("Session created.");
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var result = await _api.GetAsync<WorkoutSessionDto>($"api/training-sessions/{id}", ct);
        if (!result.Success || result.Data is null)
        {
            Failure(result.ErrorText);
            return RedirectToAction(nameof(Index));
        }

        var s = result.Data;
        return View("Form", new SessionForm
        {
            Id = s.Id,
            TrainerId = s.TrainerId,
            TrainerName = s.TrainerName,
            StartsAt = Fmt.ToLocal(s.StartsAt),
            EndsAt = Fmt.ToLocal(s.EndsAt),
            Capacity = s.Capacity
        });
    }

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(Guid id, SessionForm form, CancellationToken ct)
    {
        form.Id = id;
        if (!ModelState.IsValid) return View("Form", form);

        // API-ს Update DTO-ში TrainerId არ არის: სესიის trainer-ი არ იცვლება.
        var result = await _api.PutAsync($"api/training-sessions/{id}", new
        {
            StartsAt = Fmt.ToUtc(form.StartsAt),
            EndsAt = Fmt.ToUtc(form.EndsAt),
            form.Capacity
        }, ct);

        if (!result.Success)
        {
            AddApiErrors(result);
            return View("Form", form);
        }

        Success("Session updated.");
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _api.DeleteAsync($"api/training-sessions/{id}", ct);

        if (result.Success)
        {
            Success("Session deleted.");
            return RedirectToAction(nameof(Index));
        }

        // 409: სესიას ჯავშნები აქვს (გაუქმებულიც კი)
        Failure(result.ErrorText);
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<SessionForm> WithTrainers(SessionForm form, CancellationToken ct)
    {
        var trainers = await _api.GetAsync<List<TrainerDto>>("api/trainers", ct);
        form.Trainers = trainers.Data ?? new();
        return form;
    }
}
