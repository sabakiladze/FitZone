using FitZone.Web.Models;
using FitZone.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitZone.Web.Controllers;

/// <summary>
/// "My membership": ჩემი აბონემენტი, შესვლის (check-in) ღილაკი, ვიზიტების ისტორია და გაუქმება: ყველა ერთ გვერდზე.
/// </summary>
[Authorize(Roles = "User")]
public class MembershipController : BaseController
{
    private readonly ApiClient _api;

    public MembershipController(ApiClient api) => _api = api;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var vm = new MyMembershipVm();

        var mine = await _api.GetAsync<UserMembershipDto>("api/memberships/my", ct);
        if (mine.Success) vm.Membership = mine.Data;
        else if (mine.StatusCode != 404) Failure(mine.ErrorText);   // 404 = აქტიური აბონემენტი არ აქვს

        if (vm.Membership is not null)
        {
            var plans = await _api.GetAsync<List<MembershipPlanDto>>("api/memberships/plans", ct);
            vm.Plan = plans.Data?.FirstOrDefault(p =>
                string.Equals(p.Name, vm.Membership.PlanName, StringComparison.OrdinalIgnoreCase));
        }

        var visits = await _api.GetAsync<List<VisitDto>>("api/visits/my", ct);
        if (visits.Success) vm.Visits = visits.Data ?? new();
        else Failure(visits.ErrorText);

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> CheckIn(CancellationToken ct)
    {
        var result = await _api.PostAsync<CheckInResponse>("api/visits/check-in", null, ct);

        if (result.Success)
        {
            var left = result.Data?.RemainingVisits;
            Success(left is null
                ? "Checked in. Your plan has unlimited visits."
                : $"Checked in. Visits left: {left}.");
        }
        else Failure(result.ErrorText);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(CancellationToken ct)
    {
        var result = await _api.DeleteAsync("api/memberships/my", ct);

        if (result.Success)
        {
            Success("Membership cancelled. You can choose a new plan.");
            return RedirectToAction("Index", "Plans");
        }

        Failure(result.ErrorText);
        return RedirectToAction(nameof(Index));
    }
}
