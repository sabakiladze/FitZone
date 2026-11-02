using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FitZone.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace FitZone.Web.Services;

/// <summary>
/// ერთადერთი ადგილი, სადაც UI API-ს ესაუბრება:
/// ამატებს Bearer ტოკენს cookie-დან და შეცდომებს (ProblemDetails / ValidationProblemDetails) ტექსტებად აქცევს.
/// </summary>
public sealed class ApiClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _context;

    public ApiClient(HttpClient http, IHttpContextAccessor context)
    {
        _http = http;
        _context = context;
    }

    public Task<ApiResult<T>> GetAsync<T>(string url, CancellationToken ct = default)
        => SendAsync<T>(HttpMethod.Get, url, null, ct);

    public Task<ApiResult<T>> PostAsync<T>(string url, object? body, CancellationToken ct = default)
        => SendAsync<T>(HttpMethod.Post, url, body, ct);

    public Task<ApiResult> PostAsync(string url, object? body, CancellationToken ct = default)
        => SendAsync(HttpMethod.Post, url, body, ct);

    public Task<ApiResult> PutAsync(string url, object? body, CancellationToken ct = default)
        => SendAsync(HttpMethod.Put, url, body, ct);

    public Task<ApiResult> DeleteAsync(string url, CancellationToken ct = default)
        => SendAsync(HttpMethod.Delete, url, null, ct);

    /// <summary>?key=value&amp;... აგება; ცარიელი/null მნიშვნელობები გამოტოვებულია.</summary>
    public static string Url(string path, params (string Key, object? Value)[] query)
    {
        var parts = query
            .Where(q => q.Value is not null && !string.IsNullOrWhiteSpace(q.Value.ToString()))
            .Select(q => $"{Uri.EscapeDataString(q.Key)}={Uri.EscapeDataString(Format(q.Value!))}")
            .ToList();

        return parts.Count == 0 ? path : $"{path}?{string.Join("&", parts)}";
    }

    private static string Format(object value) => value switch
    {
        DateOnly d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        Enum e => e.ToString(),
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString()!
    };

    private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string url, object? body, CancellationToken ct)
    {
        using var response = await SendRawAsync(method, url, body, ct);

        if (!response.IsSuccessStatusCode)
        {
            return new ApiResult<T>
            {
                Success = false,
                StatusCode = (int)response.StatusCode,
                Errors = await ReadErrorsAsync(response, ct)
            };
        }

        T? data = default;
        if (response.StatusCode != HttpStatusCode.NoContent && response.Content.Headers.ContentLength != 0)
            data = await response.Content.ReadFromJsonAsync<T>(Json, ct);

        return new ApiResult<T> { Success = true, StatusCode = (int)response.StatusCode, Data = data };
    }

    private async Task<ApiResult> SendAsync(HttpMethod method, string url, object? body, CancellationToken ct)
    {
        using var response = await SendRawAsync(method, url, body, ct);

        return new ApiResult
        {
            Success = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode,
            Errors = response.IsSuccessStatusCode ? new() : await ReadErrorsAsync(response, ct)
        };
    }

    private const string FreshTokenKey = "fz_fresh_access_token";

    private async Task<HttpResponseMessage> SendRawAsync(HttpMethod method, string url, object? body, CancellationToken ct)
    {
        var token = await CurrentAccessTokenAsync();
        var response = await SendOnceAsync(method, url, body, token, ct);

        // 401 + ტოკენი = access token (15 წთ) ვადაგასულია. ჯერ refresh token-ით ვცდილობთ,
        // მხოლოდ მერე ვუშვებთ მომხმარებელს Login-ზე. (Login-ზე ტოკენი არ არის, იქ 401 ჩვეულებრივი შეცდომაა.)
        if (response.StatusCode == HttpStatusCode.Unauthorized && !string.IsNullOrEmpty(token))
        {
            response.Dispose();

            var fresh = await TryRefreshAsync(ct);
            if (fresh is null) throw new UnauthorizedApiException();

            response = await SendOnceAsync(method, url, body, fresh, ct);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Dispose();
                throw new UnauthorizedApiException();
            }
        }

        return response;
    }

    private async Task<HttpResponseMessage> SendOnceAsync(
        HttpMethod method, string url, object? body, string? token, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, url);

        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (body is not null)
            request.Content = JsonContent.Create(body, body.GetType(), options: Json);

        return await _http.SendAsync(request, ct);
    }

    private async Task<string?> CurrentAccessTokenAsync()
    {
        var http = _context.HttpContext;
        if (http is null) return null;

        // თუ ამ მოთხოვნისას token უკვე განახლდა, cookie-ში ძველი ჯერ კიდევ წერია; ახალს Items-იდან ვიღებთ.
        if (http.Items[FreshTokenKey] is string fresh) return fresh;

        return await http.GetTokenAsync("access_token");
    }

    /// <summary>refresh token-ით ახალ წყვილს იღებს და cookie-ს ანახლებს. წარუმატებლობისას null.</summary>
    private async Task<string?> TryRefreshAsync(CancellationToken ct)
    {
        var http = _context.HttpContext;
        if (http is null) return null;

        var auth = await http.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!auth.Succeeded || auth.Principal is null || auth.Properties is null) return null;

        var refresh = auth.Properties.GetTokenValue("refresh_token");
        if (string.IsNullOrEmpty(refresh)) return null;

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/refresh")
        {
            Content = JsonContent.Create(new { RefreshToken = refresh }, options: Json)
        };
        using var response = await _http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;

        var data = await response.Content.ReadFromJsonAsync<AuthResponse>(Json, ct);
        if (data is null || string.IsNullOrEmpty(data.AccessToken) || string.IsNullOrEmpty(data.RefreshToken))
            return null;

        auth.Properties.StoreTokens(new[]
        {
            new AuthenticationToken { Name = "access_token", Value = data.AccessToken },
            new AuthenticationToken { Name = "refresh_token", Value = data.RefreshToken }
        });
        await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, auth.Principal, auth.Properties);

        http.Items[FreshTokenKey] = data.AccessToken;
        return data.AccessToken;
    }

    private static async Task<List<string>> ReadErrorsAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var messages = new List<string>();

        try
        {
            var text = await response.Content.ReadAsStringAsync(ct);
            if (!string.IsNullOrWhiteSpace(text))
            {
                using var doc = JsonDocument.Parse(text);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    // ValidationProblemDetails: { "errors": { "Field": ["msg", ...] } }
                    if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var field in errors.EnumerateObject())
                        {
                            if (field.Value.ValueKind != JsonValueKind.Array) continue;
                            foreach (var item in field.Value.EnumerateArray())
                            {
                                var message = item.GetString();
                                if (!string.IsNullOrWhiteSpace(message)) messages.Add(message);
                            }
                        }
                    }

                    // ProblemDetails: { "detail": "...", "title": "..." }
                    if (messages.Count == 0 && root.TryGetProperty("detail", out var detail)
                        && detail.ValueKind == JsonValueKind.String)
                        messages.Add(detail.GetString()!);

                    if (messages.Count == 0 && root.TryGetProperty("title", out var title)
                        && title.ValueKind == JsonValueKind.String)
                        messages.Add(title.GetString()!);
                }
            }
        }
        catch (JsonException)
        {
            // პასუხი JSON არ იყო; ქვემოთ ზოგადი ტექსტი გამოვა.
        }

        if (messages.Count == 0)
            messages.Add($"Request failed ({(int)response.StatusCode}).");

        return messages.Distinct().ToList();
    }
}
