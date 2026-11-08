using FitZone.Web.Models;
using FitZone.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitZone.Web.Controllers;

/// <summary>
/// "My profile": ჩემი მონაცემები, მოქმედი აბონემენტი, მომავალი ჯავშნები და აბონემენტების, ჯავშნებისა და ვიზიტების ისტორია
/// ერთ გვერდზე (მხოლოდ წასაკითხი). გაუქმება / check-in რჩება თავის ადგილას (My bookings / My membership).
/// </summary>
[Authorize]
public class ProfileController : BaseController
{
    private const int PageSize = 10;

    private readonly ApiClient _api;

    public ProfileController(ApiClient api) => _api = api;

    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default)
    {
        var vm = new ProfileVm { IsUser = User.IsInRole("User") };

        var profile = await _api.GetAsync<ProfileDto>("api/profile", ct);
        if (profile.Success) vm.Profile = profile.Data;
        else Failure(profile.ErrorText);

        // აბონემენტი, ჯავშნები და ვიზიტები მხოლოდ User-ის როლს აქვს
        if (vm.IsUser)
        {
            var current = await _api.GetAsync<UserMembershipDto>("api/memberships/my", ct);
            if (current.Success) vm.Membership = current.Data;
            else if (current.StatusCode != 404) Failure(current.ErrorText);   // 404 = მოქმედი აბონემენტი არ აქვს

            var history = await _api.GetAsync<List<UserMembershipDto>>("api/memberships/history", ct);
            if (history.Success) vm.MembershipHistory = history.Data ?? new();
            else Failure(history.ErrorText);

            var upcoming = await _api.GetAsync<List<BookingDto>>("api/bookings/upcoming", ct);
            if (upcoming.Success) vm.Upcoming = upcoming.Data ?? new();
            else Failure(upcoming.ErrorText);

            var bookings = await _api.GetAsync<PagedResult<BookingDto>>(
                ApiClient.Url("api/bookings/my", ("page", page), ("pageSize", PageSize)), ct);
            if (bookings.Success)
            {
                vm.Bookings = bookings.Data?.Values ?? new();
                vm.Pager = new PagerModel { Page = page, PageSize = PageSize, TotalCount = bookings.Data?.TotalCount ?? 0 };
            }
            else Failure(bookings.ErrorText);

            var visits = await _api.GetAsync<List<VisitDto>>("api/visit/my", ct);
            if (visits.Success) vm.Visits = visits.Data ?? new();
            else Failure(visits.ErrorText);
        }

        return View(vm);
    }
}
