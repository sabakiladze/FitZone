using FitZone.Web.Filters;
using FitZone.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ApiExceptionFilter>();
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddHttpContextAccessor();

// დროის ზონა და ვალუტა (appsettings.json -> Display)
Fmt.Configure(builder.Configuration["Display:TimeZone"], builder.Configuration["Display:Currency"]);

// ტიპიზებული HttpClient: ყველა გვერდი API-ს ამ ერთი კლასით ესაუბრება.
var apiClientBuilder = builder.Services.AddHttpClient<ApiClient>(client =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException("Api:BaseUrl is missing in appsettings.json.");
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// მხოლოდ Development-ში: localhost-ის dev სერტიფიკატს ენდობა, რომ API-ზე გამოძახება არ ჩავარდეს.
if (builder.Environment.IsDevelopment())
{
    apiClientBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
}

// UI-ს საკუთარი cookie ავტორიზაცია აქვს; JWT cookie-ში ინახება და API-ს Bearer-ად ეგზავნება.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "FitZone.Web";
        options.Cookie.HttpOnly = true;
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.SlidingExpiration = false;
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // refresh token-ის სიცოცხლე; access token-ს ApiClient თავისით ანახლებს
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ათწილადები ("50.5") ყოველთვის წერტილით იკითხება, ბრაუზერის ენის მიუხედავად.
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("en-US")
    .AddSupportedCultures("en-US")
    .AddSupportedUICultures("en-US"));

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
