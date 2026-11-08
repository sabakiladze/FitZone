using FitZone.Web.Models;
using FitZone.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitZone.Web.Controllers;

/// <summary>Admin: ყველა მომხმარებლის სია და თითოეულის აბონემენტი, ჯავშნები და ვიზიტები (ვიზიტის შესწორება / წაშლა).</summary>
[Authorize(Roles = "Admin")]
public class UsersController : BaseController
{
    private const int ListPageSize = 12;
    private const int BookingsPageSize = 10;

    private readonly ApiClient _api;

    public UsersController(ApiClient api) => _api = api;

    public async Task<IActionResult> Index(string? search, int page = 1, CancellationToken ct = default)
    {
        var result = await _api.GetAsync<PagedResult<UserListItemDto>>(
            ApiClient.Url("api/users", ("search", search), ("page", page), ("pageSize", ListPageSize)), ct);
        if (!result.Success) Failure(result.ErrorText);

        return View(new UsersIndexVm
        {
            Items = result.Data?.Values ?? new(),
            Pager = new PagerModel { Page = page, PageSize = ListPageSize, TotalCount = result.Data?.TotalCount ?? 0 },
            Search = search
        });
    }

    public async Task<IActionResult> Details(Guid id, int page = 1, CancellationToken ct = default)
    {
        var profile = await _api.GetAsync<ProfileDto>($"api/users/{id}", ct);
        if (!profile.Success || profile.Data is null)
        {
            Failure(profile.ErrorText);
            return RedirectToAction(nameof(Index));
        }

        var vm = new UserDetailsVm { Profile = profile.Data };

        // ჯავშნები, ვიზიტები და აბონემენტი მხოლოდ User-ის როლს აქვს
        if (vm.IsUser)
        {
            var membership = await _api.GetAsync<UserMembershipDto>($"api/users/{id}/membership", ct);
            if (membership.Success) vm.Membership = membership.Data;
            else if (membership.StatusCode != 404) Failure(membership.ErrorText);   // 404 = აქტიური აბონემენტი არ აქვს

            var bookings = await _api.GetAsync<PagedResult<BookingDto>>(
                ApiClient.Url($"api/users/{id}/bookings", ("page", page), ("pageSize", BookingsPageSize)), ct);
            if (bookings.Success)
            {
                vm.Bookings = bookings.Data?.Values ?? new();
                vm.Pager = new PagerModel { Page = page, PageSize = BookingsPageSize, TotalCount = bookings.Data?.TotalCount ?? 0 };
            }
            else Failure(bookings.ErrorText);

            var visits = await _api.GetAsync<List<VisitDto>>($"api/users/{id}/visits", ct);
            if (visits.Success) vm.Visits = visits.Data ?? new();
            else Failure(visits.ErrorText);
        }

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _api.DeleteAsync($"api/users/{id}", ct);

        if (result.Success)
        {
            Success("User deleted.");
            return RedirectToAction(nameof(Index));
        }

        Failure(result.ErrorText);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> EditVisit(Guid userId, Guid visitId, DateTime checkedInAt, CancellationToken ct)
    {
        // ფორმიდან მოსული დრო ადგილობრივია -> API-ს UTC უნდა
        var result = await _api.PutAsync($"api/visit/{visitId}", new { CheckedInAt = Fmt.ToUtc(checkedInAt) }, ct);

        if (result.Success) Success("Visit time updated.");
        else Failure(result.ErrorText);

        return RedirectToAction(nameof(Details), new { id = userId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteVisit(Guid userId, Guid visitId, CancellationToken ct)
    {
        var result = await _api.DeleteAsync($"api/visit/{visitId}", ct);

        if (result.Success) Success("Visit deleted.");
        else Failure(result.ErrorText);

        return RedirectToAction(nameof(Details), new { id = userId });
    }
}
