using System.ComponentModel.DataAnnotations;

namespace FitZone.Web.Models;

// ---------- ფორმები ----------

public class RegisterVm
{
    [Required, StringLength(50, MinimumLength = 3)]
    [Display(Name = "Username")]
    public string UserName { get; set; } = "";

    [Required, StringLength(50)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = "";

    [Required, StringLength(50)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = "";

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = "";

    [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required, DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = "";
}

public class LoginVm
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
    public string? ReturnUrl { get; set; }
}

public class VerifyVm
{
    [Required, EmailAddress] public string Email { get; set; } = "";

    [Required, StringLength(6, MinimumLength = 6, ErrorMessage = "The code has 6 digits.")]
    public string Code { get; set; } = "";
}

public class PlanForm
{
    public Guid? Id { get; set; }

    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = "";

    [Range(1, 365, ErrorMessage = "Duration must be between 1 and 365 days.")]
    [Display(Name = "Duration (days)")]
    public int DurationInDays { get; set; } = 30;

    [Range(1, 10000, ErrorMessage = "Visit limit must be at least 1 (leave empty for unlimited).")]
    [Display(Name = "Visit limit")]
    public int? VisitLimit { get; set; }

    [Range(0.01, 1000000, ErrorMessage = "Price must be greater than 0.")]
    public decimal Price { get; set; }
}

public class TrainerForm
{
    public Guid? Id { get; set; }

    [Required, StringLength(100, MinimumLength = 2)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = "";

    [Required, StringLength(500)]
    public string Specialization { get; set; } = "";

    [Range(0, 60)]
    [Display(Name = "Experience (years)")]
    public int ExperienceYears { get; set; }
}

public class SessionForm
{
    public Guid? Id { get; set; }

    public Guid TrainerId { get; set; }

    /// <summary>Edit-ისას trainer-ის შეცვლა API-ში შეუძლებელია, ამიტომ მხოლოდ ვაჩვენებთ.</summary>
    public string? TrainerName { get; set; }

    // ფორმაში ადგილობრივი დროა (Display:TimeZone); API-ზე UTC-დ გადადის.
    [Required] public DateTime StartsAt { get; set; }
    [Required] public DateTime EndsAt { get; set; }

    [Range(1, 200)]
    public int Capacity { get; set; } = 10;

    public List<TrainerDto> Trainers { get; set; } = new();
}

// ---------- გვერდები ----------

public class PagerModel
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalCount { get; set; }
}

public class HasActivePlan
{
    public UserMembershipDto? MyMembership { get; set; }
}

public class PlansIndexVm : HasActivePlan
{
    public List<MembershipPlanDto> Plans { get; set; } = new();
}

public class PlanDetailsVm : HasActivePlan
{
    public MembershipPlanDto Plan { get; set; } = new();
}

public class TrainersIndexVm
{
    public List<TrainerDto> Items { get; set; } = new();
    public string? Name { get; set; }
    public string? Specialization { get; set; }
}

public class TrainerDetailsVm
{
    public TrainerDto Trainer { get; set; } = new();
    public List<WorkoutSessionDto> UpcomingSessions { get; set; } = new();
    public bool SessionsNeedLogin { get; set; }
    public string? SessionsError { get; set; }
}

public class SessionsIndexVm
{
    public List<WorkoutSessionDto> Items { get; set; } = new();
    public PagerModel Pager { get; set; } = new();
    public List<TrainerDto> Trainers { get; set; } = new();
    public Guid? TrainerId { get; set; }
    public DateOnly? Date { get; set; }
    public bool NeedsLogin { get; set; }
}

public class SessionDetailsVm
{
    public WorkoutSessionDto Session { get; set; } = new();

    /// <summary>User-ის აქტიური ჯავშანი ამ სესიაზე (თუ აქვს).</summary>
    public BookingDto? MyBooking { get; set; }

    /// <summary>მხოლოდ Admin-ისთვის: ამ სესიის ჯავშნები.</summary>
    public List<BookingDto> Bookings { get; set; } = new();

    public bool NeedsLogin { get; set; }
}

public class BookingsListVm
{
    public List<BookingDto> Items { get; set; } = new();
    public PagerModel Pager { get; set; } = new();

    // მხოლოდ Admin-ის ფილტრებისთვის
    public Guid? SessionId { get; set; }
    public string? TrainerName { get; set; }
    public DateOnly? Date { get; set; }
    public BookingStatus? Status { get; set; }
}

public class MyMembershipVm
{
    public UserMembershipDto? Membership { get; set; }

    /// <summary>API-ს UserMembershipDto-ში ფასი არ არის; plan-ის სახელით ვპოულობთ (მიმდინარე ფასი).</summary>
    public MembershipPlanDto? Plan { get; set; }

    public List<VisitDto> Visits { get; set; } = new();
}

public class ErrorVm
{
    public string? Message { get; set; }
}

public class UsersIndexVm
{
    public List<UserListItemDto> Items { get; set; } = new();
    public PagerModel Pager { get; set; } = new();
    public string? Search { get; set; }
}

public class UserDetailsVm
{
    public ProfileDto Profile { get; set; } = new();
    public UserMembershipDto? Membership { get; set; }
    public List<BookingDto> Bookings { get; set; } = new();
    public PagerModel Pager { get; set; } = new();
    public List<VisitDto> Visits { get; set; } = new();
    public bool IsUser => Profile.Role == "User";
}

public class ProfileVm
{
    public ProfileDto? Profile { get; set; }
    public bool IsUser { get; set; }

    /// <summary>მოქმედი აბონემენტი (null = არ აქვს).</summary>
    public UserMembershipDto? Membership { get; set; }
    public List<UserMembershipDto> MembershipHistory { get; set; } = new();

    /// <summary>მომავალი და მიმდინარე ჯავშნები.</summary>
    public List<BookingDto> Upcoming { get; set; } = new();

    /// <summary>ჯავშნების ისტორია გვერდებად.</summary>
    public List<BookingDto> Bookings { get; set; } = new();
    public PagerModel Pager { get; set; } = new();

    public List<VisitDto> Visits { get; set; } = new();
}
