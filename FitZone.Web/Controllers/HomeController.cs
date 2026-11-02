using FitZone.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace FitZone.Web.Controllers;

public class HomeController : BaseController
{
    // Home API-ს არ ეძახის, ამიტომ API გამორთულიც რომ იყოს, მთავარი გვერდი იხსნება.
    public IActionResult Index() => View();

    public IActionResult Error(string? message) =>
        View(new ErrorVm { Message = message ?? "Something went wrong." });
}
