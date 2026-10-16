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

                builder.HasMany(u=>u.Memberships)
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
                builder.ToTable("Trainers");

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

            });

            modelBuilder.Entity<MembershipPlan>(builder =>
            {
                builder.ToTable("MembershipPlans");
                builder.HasKey(m => m.Id);
                builder.Property(m => m.Id);

                builder.Property(m => m.Name).HasMaxLength(100);
                builder.HasIndex(m => m.Name).IsUnique();
                builder.Property(m => m.Price);
                builder.Property(m => m.VisitLimit);
                builder.Property(m => m.DurationInDays);

                /// aq constraints დამჭირდება, რომ ერთნაირი სახელის მქონე გეგმები არ იყოს ბაზაში, ანუ უნიკალური უნდა იყოს Name.

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
                builder.ToTable("WorkoutSessions");

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





        }
    }
}
