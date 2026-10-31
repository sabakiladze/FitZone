namespace FitZone.Domain.CustomExceptions
{
    public class MembershipPlanInUseException : AppException
    {
        public MembershipPlanInUseException()
            : base("Membership plan is used by users and cannot be deleted.", 409) { }
    }
}