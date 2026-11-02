namespace FitZone.Web.Models;

// API-ს DTO-ების სარკე (მხოლოდ ის ველები, რაც UI-ს სჭირდება).

public enum BookingStatus { Active, Pending, Cancelled, Attended, NoShow }

public class PagedResult<T>
{
    public int TotalCount { get; set; }
    public List<T> Values { get; set; } = new();
}

public class AuthResponse
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public string AccessToken { get; set; } = "";
    public DateTime AccessTokenExpiresAt { get; set; }
    public string RefreshToken { get; set; } = "";
}

public class MembershipPlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int DurationInDays { get; set; }
    public int? VisitLimit { get; set; }
}

public class UserMembershipDto
{
    public Guid Id { get; set; }
    public string PlanName { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? RemainingVisits { get; set; }
    public bool IsActive { get; set; }
}

public class TrainerDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = "";
    public string Specialization { get; set; } = "";
    public int ExperienceYears { get; set; }
}

public class WorkoutSessionDto
{
    public Guid Id { get; set; }
    public Guid TrainerId { get; set; }
    public string TrainerName { get; set; } = "";
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public int Capacity { get; set; }
    public int FreeSpots { get; set; }
}

public class BookingDto
{
    public Guid Id { get; set; }
    public Guid WorkoutSessionId { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime BookedAt { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public string TrainerName { get; set; } = "";
}

public class VisitDto
{
    public Guid Id { get; set; }
    public DateTime CheckedInAt { get; set; }
    public string PlanName { get; set; } = "";
}

public class CheckInResponse
{
    public Guid VisitId { get; set; }
    public DateTime CheckedInAt { get; set; }
    public int? RemainingVisits { get; set; }
}
