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
                builder.Property(r => r.Id).HasDefaultValueSql("NewId()");
                builder.Property(r => r.RoleName)
                .HasMaxLength(100)
                .IsRequired();

                builder.HasIndex(r => r.RoleName).IsUnique();

                builder.HasMany(r => r.Users)
                .WithOne(u => u.Role)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

                builder.HasData(
        new Role { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), RoleName = "User" },
        new Role { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), RoleName = "Admin" });

            });






        }
    }
}
