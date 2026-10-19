
using FitZone.Application;
using FitZone.Application.ServiceInterfaces;
using FitZone.Application.Services;
using FitZone.Domain.Interfaces;
using FitZone.Infrastructure;
using FitZone.Infrastructure.Data;
using FitZone.Infrastructure.Email;
using FitZone.Infrastructure.Repositories;
using FitZone.Infrastructure.Repositorys;
using FitZone.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitZone.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);



            builder.Services.AddDbContext<FitZoneDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddOptions<EmailSettings>()
                 .Bind(builder.Configuration.GetSection("EmailSettings"))
                    .ValidateDataAnnotations()
                      .ValidateOnStart();

           // builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<IAuthService, AuthService>();


            if (builder.Environment.IsDevelopment())
                builder.Services.AddScoped<IEmailSender, DevelopmentEmailSender>();
            else
                builder.Services.AddScoped<IEmailSender, SmtpEmailSenderService>();

            builder.Services.Configure<JwtSettings>(
               builder.Configuration.GetSection("JwtTokenSettings"));

            var jwtSettings = builder.Configuration.GetSection("JwtTokenSettings").Get<JwtSettings>()
               ?? throw new InvalidOperationException("JwtTokenSettings section is missing.");


            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
