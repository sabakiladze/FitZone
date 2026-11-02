using FitZone.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FitZone.Web.Filters;

/// <summary>
/// ორი ტიპის შეცდომა ერთ ადგილას:
/// 1) ვადაგასული ტოკენი -> გამოსვლა და Login გვერდი;
/// 2) API მიუწვდომელია -> გასაგები შეტყობინების გვერდი.
/// </summary>
public sealed class ApiExceptionFilter : IAsyncExceptionFilter
{
    public async Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is UnauthorizedApiException)
        {
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            context.Result = new RedirectToActionResult("Login", "Account", new { expired = true });
            context.ExceptionHandled = true;
            return;
        }

        if (context.Exception is HttpRequestException or TaskCanceledException)
        {
            context.Result = new RedirectToActionResult("Error", "Home", new
            {
                message = "The FitZone API is not reachable. Make sure FitZone.WebApi is running and Api:BaseUrl is correct."
            });
            context.ExceptionHandled = true;
        }
    }
}
