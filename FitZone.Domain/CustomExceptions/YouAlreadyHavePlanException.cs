
namespace FitZone.Domain.CustomExceptions
{
    public class YouAlreadyHavePlanException:AppException
    {
        public YouAlreadyHavePlanException() : base("You already have a membership plan.", 409) { }
    }
}
