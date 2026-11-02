using FitZone.Web.Models;
using FitZone.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitZone.Web.Controllers;

[Authorize]
public class BookingsController : BaseController
{
    private const int PageSize = 10;

    private readonly ApiClient _api;

    public BookingsController(ApiClient api) => _api = api;

    // ---------- User ----------

    [Authorize(Roles = "User")]
    public async Task<IActionResult> My(int page = 1, CancellationToken ct = default)
    {
        var result = await _api.GetAsync<PagedResult<BookingDto>>(
            ApiClient.Url("api/bookings/my", ("page", page), ("pageSize", PageSize)), ct);
        if (!result.Success) Failure(result.ErrorText);

        return View(new BookingsListVm
        {
            Items = result.Data?.Values ?? new(),
            Pager = new PagerModel { Page = page, PageSize = PageSize, TotalCount = result.Data?.TotalCount ?? 0 }
        });
    }

    [HttpPost, Authorize(Roles = "User")]
    public async Task<IActionResult> Cancel(Guid id, string? returnTo, CancellationToken ct)
    {
        var result = await _api.DeleteAsync($"api/bookings/{id}", ct);

        if (result.Success) Success("Booking cancelled.");
        else Failure(result.ErrorText);

        // სესიის გვერდიდან გაუქმებისას იმავე გვერდზე ვბრუნდებით
        if (!string.IsNullOrEmpty(returnTo) && Url.IsLocalUrl(returnTo))
            return LocalRedirect(returnTo);

        return RedirectToAction(nameof(My));
    }

    // ---------- Admin ----------

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> All(Guid? sessionId, string? trainerName, DateOnly? date,
        BookingStatus? status, int page = 1, CancellationToken ct = default)
    {
        var result = await _api.GetAsync<PagedResult<BookingDto>>(
            ApiClient.Url("api/bookings",
                ("workoutSessionId", sessionId), ("trainerName", trainerName), ("date", date),
                ("status", status), ("page", page), ("pageSize", PageSize)), ct);
        if (!result.Success) Failure(result.ErrorText);

        return View(new BookingsListVm
        {
            Items = result.Data?.Values ?? new(),
            Pager = new PagerModel { Page = page, PageSize = PageSize, TotalCount = result.Data?.TotalCount ?? 0 },
            SessionId = sessionId,
            TrainerName = trainerName,
            Date = date,
            Status = status
        });
    }
}
