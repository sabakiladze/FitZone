using System.Globalization;

namespace FitZone.Web.Services;

/// <summary>
/// API ყველა დროს UTC-ში აბრუნებს. ეს კლასი ერთ ადგილას აკეთებს:
/// UTC -> ადგილობრივი დრო ჩვენებისთვის, ადგილობრივი -> UTC API-ზე გასაგზავნად, ფასისა და დარჩენილი დროის ტექსტებს.
/// ზონა და ვალუტა appsettings.json-ის "Display" სექციიდან მოდის.
/// </summary>
public static class Fmt
{
    private static TimeZoneInfo _zone = TimeZoneInfo.Utc;
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static string ZoneName { get; private set; } = "UTC";
    public static string Currency { get; private set; } = "";

    public static void Configure(string? timeZoneId, string? currency)
    {
        Currency = currency ?? "";

        if (string.IsNullOrWhiteSpace(timeZoneId)) return;

        try
        {
            _zone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            ZoneName = timeZoneId;
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            // უცნობი ზონა -> UTC; გვერდები მაინც მუშაობს.
            _zone = TimeZoneInfo.Utc;
            ZoneName = "UTC";
        }
    }

    /// <summary>API-ს DateTime (Kind უმეტესად Unspecified) ყოველთვის UTC-დ მიაჩნია.</summary>
    public static DateTime AsUtc(DateTime d) => d.Kind switch
    {
        DateTimeKind.Utc => d,
        DateTimeKind.Local => d.ToUniversalTime(),
        _ => DateTime.SpecifyKind(d, DateTimeKind.Utc)
    };

    public static DateTime ToLocal(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(AsUtc(utc), _zone);

    /// <summary>ფორმიდან მოსული ადგილობრივი დრო -> UTC (API-სთვის).</summary>
    public static DateTime ToUtc(DateTime local)
    {
        var unspecified = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        if (_zone.IsInvalidTime(unspecified)) unspecified = unspecified.AddHours(1);
        return TimeZoneInfo.ConvertTimeToUtc(unspecified, _zone);
    }

    public static bool IsPast(DateTime utc) => AsUtc(utc) <= DateTime.UtcNow;

    // ---------- ტექსტები ----------

    public static string DateTimeText(DateTime utc) => ToLocal(utc).ToString("ddd, dd MMM yyyy, HH:mm", Inv);
    public static string DateText(DateTime utc) => ToLocal(utc).ToString("ddd, dd MMM yyyy", Inv);
    public static string TimeText(DateTime utc) => ToLocal(utc).ToString("HH:mm", Inv);
    public static string TimeRange(DateTime startUtc, DateTime endUtc) => $"{TimeText(startUtc)} – {TimeText(endUtc)}";

    /// <summary>Value for &lt;input type="datetime-local"&gt;.</summary>
    public static string InputValue(DateTime local) => local.ToString("yyyy-MM-dd'T'HH:mm", Inv);

    public static string Money(decimal price) =>
        string.IsNullOrEmpty(Currency) ? price.ToString("0.##", Inv) : $"{price.ToString("0.##", Inv)} {Currency}";

    public static string Visits(int? limit) => limit is null ? "Unlimited" : limit.Value.ToString(Inv);

    public static string Remaining(TimeSpan span)
    {
        if (span <= TimeSpan.Zero) return "expired";
        return $"{(int)span.TotalDays}d {span.Hours:00}h {span.Minutes:00}m";
    }

    public static int RemainingDays(DateTime endUtc)
    {
        var span = AsUtc(endUtc) - DateTime.UtcNow;
        return span <= TimeSpan.Zero ? 0 : (int)Math.Ceiling(span.TotalDays);
    }
}
