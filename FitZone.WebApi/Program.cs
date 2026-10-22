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
using FitZone.WebApi.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();


            if (builder.Configuration.GetValue<bool>("UseRealEmail"))
                builder.Services.AddScoped<IEmailSender, SmtpEmailSenderService>();
            else
                builder.Services.AddScoped<IEmailSender, DevelopmentEmailSender>();



            builder.Services.Configure<JwtSettings>(
               builder.Configuration.GetSection("JwtTokenSettings"));


            var jwtSettings = builder.Configuration.GetSection("JwtTokenSettings").Get<JwtSettings>()
               ?? throw new InvalidOperationException("JwtSettings section is missing.");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
           .AddJwtBearer(options =>
           {
               options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = true,
                   ValidateAudience = true,
                   ValidateLifetime = true,
                   ValidateIssuerSigningKey = true,
                   ValidIssuer = jwtSettings.Issuer,
                   ValidAudience = jwtSettings.Audience,
                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                   ClockSkew = TimeSpan.Zero
               };
               });
            // Add services to the container.


            builder.Services.AddAuthorization();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseExceptionHandler();


            app.MapControllers();

            app.Run();
        }
    }
}
