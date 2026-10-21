using FitZone.Domain.CustomExceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FitZone.WebApi.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {

        private readonly ILogger<GlobalExceptionHandler> _logger;
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        // HttpContext httpContext ეს არის http მოთხონსის "შინაარსი", აქ იწერება რას აბრუნებს, რას იღებს და ასე შემდეგ.
        // მე ამ შემთხვევაში რას აბრუნებში ვცვლი ამ მეთოდით კონტექსტს, ანუ როდესაც შეცდომას დააბრუნებს ის ხომ თავისით წერს შინაარს
        // ამ შინაარსს ვცვლი მე ზუსტად, ანუ მე ვწერ შეცდომის შემთხვევაში რას დაააბრუნებს.
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
        {
            int status;
            string title;
            string details;


            // თუ შემავალი შეცდომა არის AppException ამ ტიპის (ანუ ჩემი ქუსთომ exception) 
            // მაშნ იფ ში შეიტანოს როგორც
            if (exception is AppException appException)
            {
                status = appException.StatusCode;
                title = "Rewuest failed";
                details = appException.Message;

                _logger.LogWarning("Handled error ({Status}): {Message}", status, details);

            }
            else
            {
                status = StatusCodes.Status500InternalServerError;
                title = "Server error";
                details = "An unexpected error occurred.";
                _logger.LogError(exception, "Unhandled exception");
            }


            // http კონტექსტს/პასუხს უწერს სტატუსკოდს(ჩემს კოდს)
            httpContext.Response.StatusCode = status;


            //რესპონსე ვწერთ ჯსონ ფორმატში


            /*
             * ამ კოდით ჩვენ ვეუბნებით აპლიკაციას:
             * „როდესაც სერვერზე რაიმე შეცდომა მოხდება,
             * ნუ ჩამოიშლება აპლიკაცია — დაიჭირე ის,
             * დააყენე შესაბამისი HTTP სტატუს კოდი,
             * შეფუთე სტანდარტულ JSON (ProblemDetails) ფორმატში და სუფთად დაუბრუნე კლიენტს“.*/
            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = details,
                Instance = httpContext.Request.Path  //ანუ ჰტტპ რექუესთის მისამართი, ანუ რომელი api მ გაჭედა
            }, ct);


            //წარმატებით დაიჭირა
            return true;
        }
    }
}
