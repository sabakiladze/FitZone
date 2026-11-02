# FitZone

სპორტდარბაზის მართვის სისტემა: REST API (ASP.NET Core 8, EF Core, SQL Server, JWT) და ვებ-ინტერფეისი (ASP.NET Core MVC + Razor, HTML/CSS/Bootstrap), რომელიც API-სთან მუშაობს.

მომხმარებელს შეუძლია ტრენერებისა და ვარჯიშის სესიების ნახვა, აბონემენტის შეძენა, სესიების დაჯავშნა და დარბაზში check-in. ადმინისტრატორი მართავს ტრენერებს, სესიებს, აბონემენტის გეგმებს და ხედავს ყველა ჯავშანს.

## სოლუშენის სტრუქტურა

| პროექტი | დანიშნულება |
|---|---|
| `FitZone.Domain` | ენტიტები, enum-ები, რეპოზიტორიების ინტერფეისები, საკუთარი გამონაკლისები (დამოკიდებულებების გარეშე) |
| `FitZone.Application` | სერვისები (ბიზნეს-წესები), DTO-ები, FluentValidation ვალიდატორები, AutoMapper პროფილი |
| `FitZone.Infrastructure` | EF Core `DbContext`, მიგრაციები, რეპოზიტორიები, JWT სერვისი, ელფოსტის გამგზავნები |
| `FitZone.WebApi` | კონტროლერები, Swagger, ავტორიზაცია, გლობალური შეცდომების დამმუშავებელი |
| `FitZone.Web` | **UI**: Razor views, cookie ავტორიზაცია, ტიპიზებული `HttpClient` API-სთვის |
| `FitZone.Testing` | xUnit ტესტები (სერვისები Moq-ით, რეპოზიტორიები EF InMemory-ით, AutoMapper-ის კონფიგურაცია) |

დამოკიდებულებები შიგნით არის მიმართული: `WebApi → Application → Domain` და `Infrastructure → Application/Domain`. `FitZone.Web` API-ს მხოლოდ HTTP-ით იცნობს და ბაზას პირდაპირ არასოდეს ეხება.

## მოთხოვნები

- .NET 8 SDK
- SQL Server (LocalDB, Express ან Docker)
- `dotnet-ef` ხელსაწყო: `dotnet tool install --global dotnet-ef`
- სანდო HTTPS dev სერტიფიკატი: `dotnet dev-certs https --trust`

## გაშვება

### 1. API-ს კონფიგურაცია

Development გარემოში API დამატებითი მოქმედებების გარეშე ეშვება: `FitZone.WebApi/appsettings.Development.json` შეიცავს LocalDB კავშირს (`FitZoneDb`), მხოლოდ dev-ისთვის განკუთვნილ JWT გასაღებს და `UseRealEmail: false`. ეს მნიშვნელობები საიდუმლო არ არის. საკუთარი SQL Server-ის ან გასაღების გამოსაყენებლად გადააწერე user secrets-ით (რეალური საიდუმლო მნიშვნელობები git-ში არასოდეს ჩასვა). repo-ს ძირიდან:

```bash
cd FitZone.WebApi
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\MSSQLLocalDB;Database=FitZoneDb;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "JwtTokenSettings:SecretKey" "ჩაწერე-მინიმუმ-32-სიმბოლოიანი-შემთხვევითი-ტექსტი"
cd ..
```

### 2. ბაზის შექმნა

```bash
dotnet ef database update --project FitZone.Infrastructure --startup-project FitZone.WebApi
```

ეს გაუშვებს ყველა მიგრაციას და ჩაწერს საწყის მონაცემებს: როლები, ერთი ადმინი, 5 ტრენერი და 5 აბონემენტის გეგმა.

### 3. API-ს გაშვება

```bash
dotnet run --project FitZone.WebApi --launch-profile https
```

Swagger: <https://localhost:7284/swagger>

### 4. UI-ს გაშვება (მეორე ტერმინალში)

```bash
dotnet run --project FitZone.Web --launch-profile https
```

UI: <https://localhost:7300>

Visual Studio-ში: *Solution → Configure Startup Projects → Multiple startup projects* და `FitZone.WebApi` და `FitZone.Web` ორივე დააყენე *Start*-ზე.

თუ API სხვა მისამართზე ეშვება, შეცვალე `Api:BaseUrl` ფაილში `FitZone.Web/appsettings.json`.

### ანგარიშები

| როლი | ლოგინი | პაროლი |
|---|---|---|
| ადმინი (საწყისი მონაცემებიდან) | `admin@fitzone.local` | `Admin123!` |
| მომხმარებელი | დარეგისტრირდი საიტზე | შენი არჩევანი (მინ. 8 სიმბოლო, მხოლოდ ლათინური ასოები, ერთი დიდი ასო, ერთი სიმბოლო) |

### ელფოსტის დადასტურება

ახალმა მომხმარებელმა ლოგინამდე ელფოსტა უნდა დაადასტუროს. Development-ში წერილები **არ იგზავნება**: `"UseRealEmail": false` (ნაგულისხმევი) დროს API შეტყობინებას კონსოლში ბეჭდავს. მოძებნე `FAKE EMAIL to ...` და ტექსტიდან ააკრიფე 6-ციფრიანი კოდი. კოდი 15 წუთი მოქმედებს. ახალი კოდის მოთხოვნა შესაძლებელია 60 წამის შემდეგ.

ნამდვილი წერილების გასაგზავნად `UseRealEmail` გახადე `true` და Gmail-ის მონაცემები ჩაწერე user secrets-ში (git-ში არასოდეს):

```bash
cd FitZone.WebApi
dotnet user-secrets set "EmailSettings:SenderEmail" "you@gmail.com"
dotnet user-secrets set "EmailSettings:AppPassword" "<16-სიმბოლოიანი Gmail App Password>"
```

## UI რუკა

ყოველ API endpoint-ს UI-ში ერთი ლოგიკური ადგილი აქვს.

| გვერდი (URL) | ვის | გამოყენებული API endpoint-ები |
|---|---|---|
| მთავარი `/` | ყველას | – |
| Register `/Account/Register`, Verify `/Account/Verify`, Login `/Account/Login` | სტუმრებს | `POST auth/register`, `auth/send-verification-code`, `auth/verify-email`, `auth/login` |
| Logout (მომხმარებლის მენიუ) | შესულს | `POST auth/logout`; `auth/refresh` ავტომატურად გამოიძახება, როცა 15-წუთიანი access token ამოიწურება |
| **Trainers** `/Trainers` (ბარათები, ძებნა) | ყველას | `GET trainers`, `GET trainers/search` |
| ტრენერის გვერდი `/Trainers/Details/{id}` (ინფორმაცია + მომავალი სესიები) | ყველას | `GET trainers/{id}`, `GET training-sessions?trainerId=` |
| ტრენერის დამატება / რედაქტირება / წაშლა | ადმინს | `POST`, `PUT`, `DELETE trainers` |
| **Workout sessions** `/Sessions` (ბარათები, ფილტრი ტრენერისა და თარიღით) | შესულს* | `GET training-sessions` |
| სესიის გვერდი `/Sessions/Details/{id}` (დაჯავშნა / ჯავშნის გაუქმება) | შესულს* | `GET training-sessions/{id}`, `POST bookings`, `DELETE bookings/{id}`, `GET bookings/my` |
| სესიის დამატება / რედაქტირება / წაშლა; სესიის ჯავშნები | ადმინს | `POST`, `PUT`, `DELETE training-sessions`, `GET bookings?workoutSessionId=` |
| **Memberships** `/Plans` (ბარათები) და `/Plans/Details/{id}` (გააქტიურება) | ყველას | `GET memberships/plans`, `POST memberships/activate` (მომხმარებელი) |
| გეგმის დამატება / რედაქტირება / წაშლა | ადმინს | `POST`, `PUT`, `DELETE memberships/plans` |
| **My membership** `/Membership` (დარჩენილი დღეები/დრო/ვიზიტები, ფასი, შეძენის დრო, check-in, ვიზიტების ისტორია, გაუქმება) | მომხმარებელს | `GET memberships/my`, `DELETE memberships/my`, `POST visits/check-in`, `GET visits/my` |
| **My bookings** `/Bookings/My` | მომხმარებელს | `GET bookings/my`, `DELETE bookings/{id}` |
| **All bookings** `/Bookings/All` (ფილტრები, გვერდები) | ადმინს | `GET bookings` |

\* API ახლა სესიების განრიგს ლოგინის გარეშე არ აჩვენებს. სტუმარი ამ გვერდებზე "გთხოვთ, შეხვიდეთ" პანელს ხედავს.

## როლები და წესები

- **სტუმარი**: ტრენერები, აბონემენტის გეგმები, რეგისტრაცია, ლოგინი.
- **მომხმარებელი**: ყველაფერი ზემოთ ჩამოთვლილი, დამატებით სესიები, ჯავშნები, აბონემენტი, check-in.
- **ადმინი**: მართავს ტრენერებს, გეგმებსა და სესიებს, ხედავს ყველა ჯავშანს. აბონემენტი, check-in და დაჯავშნა UI-ში მხოლოდ *მომხმარებლის* როლს აქვს.

ბიზნეს-წესები, რომლებსაც API ამოწმებს (UI წარუმატებლობისას API-ს შეტყობინებას აჩვენებს):

- დაჯავშნისთვის საჭიროა აქტიური აბონემენტი დარჩენილი ვიზიტებით, სესია, რომელიც ჯერ არ დაწყებულა, თავისუფალი ადგილი, ამ სესიაზე არსებული ჯავშნის არქონა და იმავე მომხმარებლის სხვა ჯავშანთან დროში გადაფარვის არქონა.
- ჯავშანი უარყოფილია, თუ სესია აბონემენტის ამოწურვის შემდეგ იწყება (400).
- მომხმარებელს შეიძლება ჰქონდეს ერთი აქტიური აბონემენტი. ვიზიტების ლიმიტიან გეგმაზე ყოველი check-in ერთ ვიზიტს აკლებს, შეუზღუდავზე არა. როცა ყველა ვიზიტი გამოყენებულია, გეგმა დასრულებულად ითვლება და ახალი გეგმის გააქტიურება შესაძლებელია გაუქმების გარეშე.
- check-in შეიძლება 12 საათში ერთხელ (სხვა შემთხვევაში 409).
- თარიღის ფილტრის გარეშე სესიების სია მხოლოდ მომავალ სესიებს აჩვენებს; თარიღის მითითებისას ამ დღის სესიებს.
- ტრენერს ორი ერთმანეთზე გადაფარული სესია ვერ ექნება.
- წაშლა: ტრენერები soft-delete-ით იშლება და მხოლოდ მაშინ, როცა მომავალი სესიები არ აქვთ (სხვა შემთხვევაში 409). შეძენილი გეგმები და სესიები, რომლებსაც ჯავშნები აქვთ ან ჰქონდათ, არ იშლება (409). ჯავშნები უქმდება და არასოდეს იშლება.
- შეცდომები ბრუნდება RFC 7807 problem details ფორმატით შესაბამისი სტატუსით (400 ვალიდაცია, 401/403 ავტორიზაცია, 404 ვერ მოიძებნა, 409 კონფლიქტი).

## დროის სარტყელი და ვალუტა

API ინახავს და აბრუნებს **UTC** დროს. UI ამას გარდაქმნის `FitZone.Web/appsettings.json`-ში მითითებულ სარტყელზე (`Display:TimeZone`, ნაგულისხმევი `Asia/Tbilisi`) და ფორმიდან შეყვანილ დროს უკან UTC-ში აბრუნებს. ვალუტის სიმბოლო არის `Display:Currency`. სესიებისა და ჯავშნების გვერდზე თარიღის ფილტრები UTC კალენდარულ დღეს იყენებს.

## ტესტები

```bash
dotnet test
```

## ცნობილი შეზღუდვები

- ადგილებისა და ვიზიტების მრიცხველებს პარალელური მოთხოვნებისგან დაცვა არ აქვს: ორმა ერთდროულმა მოთხოვნამ შეიძლება ბოლო ადგილი ან ბოლო ვიზიტი ორივემ აიღოს.
- მომხმარებელზე მხოლოდ ერთი refresh token ინახება: მეორე მოწყობილობიდან შესვლისას პირველი მოწყობილობა გამოდის, როცა მისი access token ამოიწურება.
- აბონემენტის ჩანაწერი არ ინახავს გადახდილ ფასს; *My membership* გვერდი იმავე სახელის გეგმის მიმდინარე ფასს აჩვენებს.
- ტრენერს აქვს სრული სახელი, სპეციალიზაცია და გამოცდილების წლები; API-ში ცალკე გვარი და აღწერა არ არის.
- UI-დან მომხმარებლის ანგარიშის წაშლის endpoint არ არსებობს.
