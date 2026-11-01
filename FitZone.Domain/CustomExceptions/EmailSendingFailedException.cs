namespace FitZone.Domain.CustomExceptions
{
    public class EmailSendingFailedException : AppException
    {
        public EmailSendingFailedException()
            : base("Email service is temporarily unavailable. Please try again later.", 503) { }
    }
}