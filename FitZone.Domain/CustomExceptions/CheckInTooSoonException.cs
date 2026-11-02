namespace FitZone.Domain.CustomExceptions
{
    public class CheckInTooSoonException : AppException
    {
        public CheckInTooSoonException() : base("You have already checked in recently. Please try again later.", 409) { }
    }
}
