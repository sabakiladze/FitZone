# FitZone.Web

Razor/MVC front end for the FitZone API. See the repository `README.md` for the full setup.

Quick start (API must be running first):

    dotnet run --launch-profile https      # https://localhost:7300

Settings (`appsettings.json`):

| Key | Meaning |
|---|---|
| `Api:BaseUrl` | Address of `FitZone.WebApi`, default `https://localhost:7284/` |
| `Display:TimeZone` | Time zone for showing and entering times (API itself uses UTC), default `Asia/Tbilisi` |
| `Display:Currency` | Currency symbol next to prices |

How it works: login calls `POST api/auth/login`; the JWT and refresh token are kept inside the encrypted auth cookie (never in JavaScript-readable storage). `Services/ApiClient.cs` adds the Bearer header, refreshes an expired access token automatically and turns API problem details into messages shown on the page. Dev builds trust the localhost dev certificate when calling the API.
