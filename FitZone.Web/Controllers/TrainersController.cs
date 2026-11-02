using FitZone.Web.Models;
using FitZone.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitZone.Web.Controllers;

// სია და დეტალები ღიაა ყველასთვის (API-ს GET trainers საჯაროა); შექმნა/შეცვლა/წაშლა მხოლოდ Admin-ისთვის.
public class TrainersController : BaseController
{
    private readonly ApiClient _api;

    public TrainersController(ApiClient api) => _api = api;

    public async Task<IActionResult> Index(string? name, string? specialization, CancellationToken ct)
    {
        var hasFilter = !string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(specialization);
        var url = hasFilter
            ? ApiClient.Url("api/trainers/search", ("name", name), ("specialization", specialization))
            : "api/trainers";

        var result = await _api.GetAsync<List<TrainerDto>>(url, ct);
        if (!result.Success) Failure(result.ErrorText);

        return View(new TrainersIndexVm
        {
            Items = result.Data ?? new(),
            Name = name,
            Specialization = specialization
        });
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var trainer = await _api.GetAsync<TrainerDto>($"api/trainers/{id}", ct);
        if (!trainer.Success || trainer.Data is null)
        {
            Failure(trainer.ErrorText);
            return RedirectToAction(nameof(Index));
        }

        var vm = new TrainerDetailsVm { Trainer = trainer.Data };

        // მომავალი სესიები: API-ს განრიგი შეიძლება მხოლოდ შესულებისთვის იყოს; 401 ანონიმურზე = "შედი სისტემაში".
        var sessions = await _api.GetAsync<PagedResult<WorkoutSessionDto>>(
            ApiClient.Url("api/training-sessions", ("trainerId", id), ("pageSize", 50)), ct);

        if (sessions.Success)
            vm.UpcomingSessions = (sessions.Data?.Values ?? new())
                .Where(s => !Fmt.IsPast(s.StartsAt))
                .OrderBy(s => s.StartsAt)
                .ToList();
        else if (sessions.StatusCode == 401)
            vm.SessionsNeedLogin = true;
        else
            vm.SessionsError = sessions.ErrorText;

        return View(vm);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View("Form", new TrainerForm());

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(TrainerForm form, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View("Form", form);

        var result = await _api.PostAsync("api/trainers", Body(form), ct);
        if (!result.Success)
        {
            AddApiErrors(result);
            return View("Form", form);
        }

        Success("Trainer created.");
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var result = await _api.GetAsync<TrainerDto>($"api/trainers/{id}", ct);
        if (!result.Success || result.Data is null)
        {
            Failure(result.ErrorText);
            return RedirectToAction(nameof(Index));
        }

        var t = result.Data;
        return View("Form", new TrainerForm
        {
            Id = t.Id,
            FullName = t.FullName,
            Specialization = t.Specialization,
            ExperienceYears = t.ExperienceYears
        });
    }

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(Guid id, TrainerForm form, CancellationToken ct)
    {
        form.Id = id;
        if (!ModelState.IsValid) return View("Form", form);

        var result = await _api.PutAsync($"api/trainers/{id}", Body(form), ct);
        if (!result.Success)
        {
            AddApiErrors(result);
            return View("Form", form);
        }

        Success("Trainer updated.");
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _api.DeleteAsync($"api/trainers/{id}", ct);

        if (result.Success)
        {
            Success("Trainer removed.");
            return RedirectToAction(nameof(Index));
        }

        // მაგ. 409: trainer-ს მომავალი სესიები აქვს
        Failure(result.ErrorText);
        return RedirectToAction(nameof(Details), new { id });
    }

    private static object Body(TrainerForm f) => new { f.FullName, f.Specialization, f.ExperienceYears };
}
