using System.Security.Claims;
using FitZone.Web.Models;
using FitZone.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitZone.Web.Controllers;

public class AccountController : BaseController
{
    private readonly ApiClient _api;

    public AccountController(ApiClient api) => _api = api;

    // ---------- Register ----------

    [HttpGet]
    public IActionResult Register() => View(new RegisterVm());

    [HttpPost]
    public async Task<IActionResult> Register(RegisterVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await _api.PostAsync("api/auth/register", vm, ct);
        if (!result.Success)
        {
            AddApiErrors(result);
            return View(vm);
        }

        // API რეგისტრაციისას კოდს არ აგზავნის: ცალკე endpoint-ია. ამიტომ აქვე ვიძახებთ.
        var send = await _api.PostAsync("api/auth/send-verification-code", new { vm.Email }, ct);
        if (send.Success)
            Success("Account created. We sent a 6-digit code to your email.");
        else
            Failure($"Account created, but the code could not be sent: {send.ErrorText} You can request a new one below.");

        return RedirectToAction(nameof(Verify), new { email = vm.Email });
    }

    // ---------- Verify ----------

    [HttpGet]
    public IActionResult Verify(string? email) => View(new VerifyVm { Email = email ?? "" });

    [HttpPost]
    public async Task<IActionResult> Verify(VerifyVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await _api.PostAsync("api/auth/verify-email", new { vm.Email, vm.Code }, ct);
        if (!result.Success)
        {
            AddApiErrors(result);
            return View(vm);
        }

        Success("Email verified. You can log in now.");
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    public async Task<IActionResult> ResendCode(string email, CancellationToken ct)
    {
        var result = await _api.PostAsync("api/auth/send-verification-code", new { Email = email }, ct);

        if (result.Success) Success("A new code was sent.");
        else Failure(result.ErrorText);

        return RedirectToAction(nameof(Verify), new { email });
    }

    // ---------- Login / Logout ----------

    [HttpGet]
    public IActionResult Login(string? returnUrl, bool expired = false)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        if (expired) Failure("Your session has expired. Please log in again.");
        return View(new LoginVm { ReturnUrl = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await _api.PostAsync<AuthResponse>("api/auth/login", new { vm.Email, vm.Password }, ct);
        if (!result.Success || result.Data is null)
        {
            AddApiErrors(result);

            // ვერიფიცირებული არ არის -> პირდაპირ კოდის გვერდზე გადავიყვანოთ
            if (result.StatusCode == 403 && result.ErrorText.Contains("Verif", StringComparison.OrdinalIgnoreCase))
            {
                Failure(result.ErrorText);
                return RedirectToAction(nameof(Verify), new { email = vm.Email });
            }

            return View(vm);
        }

        var auth = result.Data;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, auth.UserId.ToString()),
            new(ClaimTypes.Name, auth.UserName),
            new(ClaimTypes.Email, auth.Email),
            new(ClaimTypes.Role, auth.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        // cookie ცოცხალია refresh token-ის ვადამდე (7 დღე); access token-ს ApiClient თავისით ანახლებს.
        var properties = new AuthenticationProperties
        {
            IsPersistent = false,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        };

        properties.StoreTokens(new[]
        {
            new AuthenticationToken { Name = "access_token", Value = auth.AccessToken },
            new AuthenticationToken { Name = "refresh_token", Value = auth.RefreshToken }
        });

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            properties);

        if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return LocalRedirect(vm.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var refresh = await HttpContext.GetTokenAsync("refresh_token");
        if (!string.IsNullOrEmpty(refresh))
        {
            try { await _api.PostAsync("api/auth/logout", new { RefreshToken = refresh }, ct); }
            catch (UnauthorizedApiException) { /* სესია უკვე ვადაგასულია, cookie მაინც იშლება */ }
            catch (HttpRequestException) { /* API გამორთულია: ლოკალურად მაინც გავდივართ */ }
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => View();
}
