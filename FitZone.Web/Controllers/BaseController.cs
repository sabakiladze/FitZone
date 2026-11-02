using FitZone.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitZone.Web.Controllers;

public abstract class BaseController : Controller
{
    protected const string SuccessKey = "Success";
    protected const string ErrorKey = "Error";

    protected void Success(string message) => TempData[SuccessKey] = message;
    protected void Failure(string message) => TempData[ErrorKey] = message;

    /// <summary>API-ს შეცდომები ფორმის ზემოთ გამოსაჩენად (ModelState-ის მოდელის დონის შეცდომები).</summary>
    protected void AddApiErrors(ApiResult result)
    {
        foreach (var e in result.Errors)
            ModelState.AddModelError(string.Empty, e);
    }
}
