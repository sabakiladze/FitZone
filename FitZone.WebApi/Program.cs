using FitZone.Application;
using FitZone.Application.Implementations;
using FitZone.Application.ServiceInterfaces;
using FitZone.Domain.Interfaces;
using FitZone.Domain.RepositoryInterfaces;
using FitZone.Infrastructure;
using FitZone.Infrastructure.Data;
using FitZone.Infrastructure.Email;
using FitZone.Infrastructure.JwtTokenService;
using FitZone.Infrastructure.Repositories;
using FitZone.Infrastructure.Repositorys;
using FitZone.Infrastructure.Settings;
using FitZone.WebApi.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace FitZone.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            builder.Services.AddDbContext<FitZoneDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddOptions<EmailSettings>()
                 .Bind(builder.Configuration.GetSection("EmailSettings"))
                    .ValidateDataAnnotations()
                      .ValidateOnStart();


            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
            builder.Services.AddScoped<IMembershipService, MembershipService>();
            builder.Services.AddScoped<IMembershipPlanRepository, MembershipPlanRepository>();
            builder.Services.AddScoped<IUserMembershipRepository, UserMembershipPlanRepository>();
            builder.Services.AddScoped<IVisitRepository, VisitRepository>();
            builder.Services.AddScoped<IVisitService, VisitService>();
            builder.Services.AddScoped<ITrainerRepository, TrainerRepository>();
            builder.Services.AddScoped<ITrainerService, TrainerService>();
            builder.Services.AddScoped<IWorkOutSessionRepository, WorkOutSessioRepository>();
            builder.Services.AddScoped<IWorkOutSessionService, WorkoutSessionService>();

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

            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter JWT token without Bearer prefix."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
            });
            // Add services to the container.


            builder.Services.AddAuthorization();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseExceptionHandler();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            


            app.MapControllers();

            app.Run();
        }
    }
}
