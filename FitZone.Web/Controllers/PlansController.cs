using FitZone.Web.Models;
using FitZone.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitZone.Web.Controllers;

// "Memberships" გვერდი: ყველა plan-ის ნახვა საჯაროა (API-ს GET plans საჯაროა).
// გააქტიურება: User; შექმნა/შეცვლა/წაშლა: Admin.
public class PlansController : BaseController
{
    private readonly ApiClient _api;

    public PlansController(ApiClient api) => _api = api;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var plans = await _api.GetAsync<List<MembershipPlanDto>>("api/memberships/plans", ct);
        if (!plans.Success) Failure(plans.ErrorText);

        return View(new PlansIndexVm
        {
            Plans = plans.Data ?? new(),
            MyMembership = await LoadMyMembership(ct)
        });
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var plans = await _api.GetAsync<List<MembershipPlanDto>>("api/memberships/plans", ct);
        var plan = plans.Data?.FirstOrDefault(p => p.Id == id);
        if (plan is null)
        {
            Failure(plans.Success ? "Plan not found." : plans.ErrorText);
            return RedirectToAction(nameof(Index));
        }

        return View(new PlanDetailsVm { Plan = plan, MyMembership = await LoadMyMembership(ct) });
    }

    [HttpPost, Authorize(Roles = "User")]
    public async Task<IActionResult> Activate(Guid planId, CancellationToken ct)
    {
        // API ითხოვს ზუსტ თანხას (Amount == plan.Price). ფასს ფორმიდან კი არა, API-დან ხელახლა ვიღებთ,
        // რომ ფორმის გაყალბებით სხვა თანხა ვერავინ გაგზავნოს.
        var plans = await _api.GetAsync<List<MembershipPlanDto>>("api/memberships/plans", ct);
        var plan = plans.Data?.FirstOrDefault(p => p.Id == planId);
        if (plan is null)
        {
            Failure("Plan not found.");
            return RedirectToAction(nameof(Index));
        }

        var result = await _api.PostAsync<UserMembershipDto>(
            "api/memberships/activate", new { PlanId = plan.Id, Amount = plan.Price }, ct);

        if (result.Success)
        {
            Success($"Plan \"{plan.Name}\" activated.");
            return RedirectToAction("Index", "Membership");
        }

        Failure(result.ErrorText);
        return RedirectToAction(nameof(Details), new { id = planId });
    }

    // ---------- Admin ----------

    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View("Form", new PlanForm());

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(PlanForm form, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View("Form", form);

        var result = await _api.PostAsync("api/memberships/plans", Body(form), ct);
        if (!result.Success)
        {
            AddApiErrors(result);
            return View("Form", form);
        }

        Success("Plan created.");
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var plans = await _api.GetAsync<List<MembershipPlanDto>>("api/memberships/plans", ct);
        var plan = plans.Data?.FirstOrDefault(p => p.Id == id);
        if (plan is null)
        {
            Failure(plans.Success ? "Plan not found." : plans.ErrorText);
            return RedirectToAction(nameof(Index));
        }

        return View("Form", new PlanForm
        {
            Id = plan.Id,
            Name = plan.Name,
            DurationInDays = plan.DurationInDays,
            VisitLimit = plan.VisitLimit,
            Price = plan.Price
        });
    }

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(Guid id, PlanForm form, CancellationToken ct)
    {
        form.Id = id;
        if (!ModelState.IsValid) return View("Form", form);

        var result = await _api.PutAsync($"api/memberships/plans/{id}", Body(form), ct);
        if (!result.Success)
        {
            AddApiErrors(result);
            return View("Form", form);
        }

        Success("Plan updated.");
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _api.DeleteAsync($"api/memberships/plans/{id}", ct);

        if (result.Success)
        {
            Success("Plan deleted.");
            return RedirectToAction(nameof(Index));
        }

        // 409: plan-ს მომხმარებლები იყენებენ
        Failure(result.ErrorText);
        return RedirectToAction(nameof(Details), new { id });
    }

    private static object Body(PlanForm f) => new { f.Name, f.DurationInDays, f.VisitLimit, f.Price };

    /// <summary>მხოლოდ შესული User-ისთვის. 404 = "აქტიური აბონემენტი არ აქვს": ეს ნორმალური მდგომარეობაა.</summary>
    private async Task<UserMembershipDto?> LoadMyMembership(CancellationToken ct)
    {
        if (!User.IsInRole("User")) return null;

        var mine = await _api.GetAsync<UserMembershipDto>("api/memberships/my", ct);
        if (mine.Success) return mine.Data;
        if (mine.StatusCode != 404) Failure(mine.ErrorText);
        return null;
    }
}
