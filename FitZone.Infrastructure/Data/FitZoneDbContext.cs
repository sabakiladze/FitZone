using FitZone.Domain.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Infrastructure.Data
{
    public class FitZoneDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<MembershipPlan> MembershipPlans { get; set; }
        public DbSet<UserMembershipPlan> UserMembershipPlans { get; set; }
        public DbSet<WorkoutSession> WorkoutSessions { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Visit> Visits { get; set; }


        public FitZoneDbContext() { }
        public FitZoneDbContext(DbContextOptions<FitZoneDbContext> options) : base(options) { }





        // აქ უნდა დავამტო მერე ტესტირებისთის if() პირობა.
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();


            var _connectionString = configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>(builder =>
            {
                builder.ToTable("Roles");
                builder.HasKey(r => r.Id);
                builder.Property(r => r.Id);
                builder.Property(r => r.RoleName)
                .HasMaxLength(100)
                .IsRequired();

                builder.HasIndex(r => r.RoleName).IsUnique();

                builder.HasMany(r => r.Users)
                .WithOne(u => u.Role)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);


               private static readonly Guid AdminRoleId = Guid.NewGuid();

        builder.HasData(
                   new Role { Id = 33b55207-ab7c-474e-82e5-fd89bf9bab59, RoleName = "Admin" },
                   new Role { Id = 59cbfac4-9a3a-4cf8-93af-ba88f71d5719, RoleName = "User" }
               );

               

            });

            modelBuilder.Entity<User>(builder =>
            {
                builder.ToTable("Users");

                builder.HasKey(u => u.Id);
                builder.Property(u => u.Id)
                .ValueGeneratedOnAdd();

                builder.Property(u => u.UserName)
                .HasMaxLength(50)
                .IsRequired();
                builder.HasIndex(u => u.UserName)
                .IsUnique();

                builder.Property(u => u.FirstName)
                .HasMaxLength(50)
                .IsRequired();
                builder.Property(u=>u.LastName)
                .HasMaxLength(50)
                .IsRequired();

                builder.Property(u => u.Email)
                .HasMaxLength(256)
                .IsRequired();
                builder.HasIndex(u => u.Email)
                .IsUnique();

                builder.Property(u => u.PasswordHash)
                .HasMaxLength(500)
                .IsRequired();

                builder.Property(u => u.IsEmailVerified)
                .IsRequired()
                .HasDefaultValue(false);

                builder.Property(u => u.VerificationCode)
                .HasMaxLength(256);

                builder.Property(u => u.VerificationCodeExpiresAt);

                builder.Property(u => u.IsActive)
                    .IsRequired();

                builder.Property(u => u.CreatedAt)
                    .IsRequired();

                builder.Property(u => u.LastLoginAt);

                builder.HasOne(u=>u.Role)
                .WithMany(r=>r.Users)
                .HasForeignKey(u=>u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

                builder.HasMany(u => u.Bookings)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

                builder.HasMany(u=>u.UserMembershipPlans)
                .WithOne(m=>m.User)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);

                builder.HasMany(u => u.Visits)
                .WithOne(v=>v.User)
                .HasForeignKey(v=>v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            });


            modelBuilder.Entity<Trainer>(builder =>
            {
                builder.ToTable("Trainers", t =>
                {
                    t.HasCheckConstraint("Ck_Trainers_ExperienceYears", "[ExperienceYears] >= 0");
                });

                builder.HasKey(t => t.Id);
                builder.Property(t => t.Id);

                builder.Property(t => t.FullName)
                .HasMaxLength(100)
                .IsRequired();

                builder.Property(t => t.Specialization)
                .HasMaxLength(500)
                .IsRequired();

                builder.Property(t => t.ExperienceYears);

                builder.HasMany(t=>t.WorkoutSessions)
                .WithOne(w=>w.Trainer)
                .HasForeignKey(w=>w.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

                builder.HasData(
                new Trainer { Id = 063e3914-691e-4663-bbd4-5fb2acac5139, FullName = "Giorgi Beridze", Specialization = "Strength Training", ExperienceYears = 8 },
                new Trainer { Id = 4d64029d-2c85-47e7-9d37-8a5417dd6872, FullName = "Nino Kapanadze", Specialization = "Yoga", ExperienceYears = 5 },
                new Trainer { Id = 39a2d8bc-8c91-4a6a-965e-862c90c757ae, FullName = "Dato Maisuradze", Specialization = "Cardio & HIIT", ExperienceYears = 3 },
                new Trainer { Id = 05966c03-f1ee-4285-a055-518a29c368ea, FullLName = "Mariam Nozadze". Specialization = "Cardio $ HIIT", ExperienceYears = 4 },
                new Trainer { Id = 17f8c66d-6ec2-4b69-b7f5-1ee9c544e7f5, FullName = "Nika Donadze", Specialization = "Strength Training", ExperienceYears = 6 },
);

            });

            modelBuilder.Entity<MembershipPlan>(builder =>
            {
                builder.ToTable("MembershipPlans", t=>
                {
                    t.HasCheckConstraint("CK_MembershipPlans_Price", "[Price] > 0");
                    t.HasCheckConstraint("CK_MembershipPlans_VisitLimit", "[VisitLimit] IS NULL OR [VisitLimit] > 0");
                    t.HasCheckConstraint("CK_MembershipPlans_DurationInDays", "[DurationInDays] > 0");
                });
                builder.HasKey(m => m.Id);
                builder.Property(m => m.Id);

                builder.Property(m => m.Name).HasMaxLength(100);
                builder.HasIndex(m => m.Name).IsUnique();
                builder.Property(m => m.Price);
                builder.Property(m => m.VisitLimit);
                builder.Property(m => m.DurationInDays);

                builder.HasMany(m => m.UserMembershipPlans)
                .WithOne(u => u.MembershipPlan)
                .HasForeignKey(u => u.MembershipPlanId)
                .OnDelete(DeleteBehavior.Restrict);

                builder.HasData(
                   new MembershipPlan { Id = e056ea0c-d394-44d3-b188-7ab1c33922e0, Name = "Starter", Price = 30m, DurationInDays = 30, VisitLimit = 8 },
                   new MembershipPlan { Id = 82832180-3261-473e-a745-c251ec1cc799, Name = "Monthly", Price = 50m, DurationInDays = 30, VisitLimit = 12 },
                   new MembershipPlan { Id = da2a4441-855b-4c4a-b67f-2a10702bd60a, Name = "Monthly Unlimited", Price = 80m, DurationInDays = 30, VisitLimit = null },
                   new MembershipPlan { Id = c6986d8a-5af9-4f41-a1c1-606ef0f9fb92, Name = "Quarterly", Price = 130m, DurationInDays = 90, VisitLimit = 40 },
                   new MembershipPlan { Id = ea72fe03-414f-4c72-8cdd-1277f3279c45, Name = "Yearly", Price = 450m, DurationInDays = 365, VisitLimit = null });
            });

            modelBuilder.Entity<Booking>(builder =>
            {
                builder.ToTable("Bookings");
                builder.HasKey(b => b.Id);
                builder.Property(b => b.Id);
                builder.Property(b => b.BookedAt);
                builder.Property(b => b.Status);

                builder.HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

                builder.HasOne(b => b.WorkoutSession)
                .WithMany(w => w.Bookings)
                .HasForeignKey(b => b.WorkoutSessionId)
                .OnDelete(DeleteBehavior.Restrict);

            });

            modelBuilder.Entity<WorkoutSession>(builder =>
            {
                builder.ToTable("WorkoutSessions", t=>
                {
                    t.HasCheckConstraint("CK_WorkoutSessions_Capacity", "[Capacity]>0");
                });

                builder.HasKey(w => w.Id);
                builder.Property(w => w.Id);

                builder.Property(w => w.StartsAt);
                builder.Property(w => w.EndsAt);

                builder.Property(w => w.Capacity);

                builder.HasOne(w => w.Trainer)
                .WithMany(t => t.WorkoutSessions)
                .HasForeignKey(w => w.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

                builder.HasMany(w => w.Bookings)
                .WithOne(b => b.WorkoutSession)
                .HasForeignKey(b => b.WorkoutSessionId)
                .OnDelete(DeleteBehavior.Restrict);


            });


            modelBuilder.Entity<Visit>(builder =>
            {
                builder.ToTable("Visits");

                builder.HasKey(v => v.Id);
                builder.Property(v => v.Id);

                builder.Property(v => v.CheckedInAt);

                builder.HasOne(v => v.User)
                .WithMany(u => u.Visits)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

                builder.HasOne(v => v.UserMembershipPlan)
                .WithMany(m => m.Visits)
                .HasForeignKey(v => v.UserMembershipPlanId)
                .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<UserMembershipPlan>(builder =>
            {
                builder.ToTable("UserMembershipPlans");
                builder.HasKey(u => u.Id);
                builder.Property(u => u.Id);

                builder.Property(u => u.StartDate);
                builder.Property(u => u.EndDate);

                builder.Property(u => u.RemainingVisits);

                builder.HasOne(um=>um.User)
                .WithMany(us=>us.UserMembershipPlans)
                .HasForeignKey(um=>um.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            });



        }
    }
}
