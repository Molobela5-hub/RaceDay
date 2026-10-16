
using Microsoft.EntityFrameworkCore;
using RaceDay.Models;

namespace RaceDay.Data
{
    public class RaceDayContext : DbContext
    {
        public RaceDayContext(DbContextOptions<RaceDayContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("User", "dbo");
            modelBuilder.Entity<User>().HasKey(x => x.UserID);

            modelBuilder.Entity<Event>().ToTable("Event", "dbo");
            modelBuilder.Entity<Event>().HasKey(x => x.EventID);

            modelBuilder.Entity<EventType>().ToTable("EventType", "dbo");
            modelBuilder.Entity<EventType>().HasKey(x => x.EventTypeID);

            modelBuilder.Entity<Category>().ToTable("Category", "dbo");
            modelBuilder.Entity<Category>().HasKey(x => x.CategoryID);

            modelBuilder.Entity<Enrollment>().ToTable("Enrollment", "dbo");
            modelBuilder.Entity<Enrollment>().HasKey(x => x.EnrollmentID);

            modelBuilder.Entity<Result>().ToTable("Result", "dbo");
            modelBuilder.Entity<Result>().HasKey(x => x.ResultID);
        }
    }
}
