namespace FitZone.Domain.CustomExceptions
{
    public class VisitNotFoundException : AppException
    {
        public VisitNotFoundException() : base("Visit was not found.", 404) { }
    }
}
