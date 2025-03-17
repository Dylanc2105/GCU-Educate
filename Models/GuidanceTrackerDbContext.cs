using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Sockets;
using System.Web;

namespace GuidanceTracker.Models
{
    public class GuidanceTrackerDbContext:IdentityDbContext<User>
    {


        public GuidanceTrackerDbContext():base("guidanceTrackerDB", throwIfV1Schema: false)
        {
            Database.SetInitializer(new DatabaseInitializer());
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<GuidanceTrackerDbContext, Migrations.Configuration>());
        }

        public DbSet<GuidanceTeacher> GuidanceTeachers { get; set; }
        public DbSet<CurriculumHead> CurriculumHeads { get; set; }
        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<ArchivedTicket> ArchivedTickets { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<MessageBoard> MessageBoards { get; set; }
        public DbSet<ArchivedComment> ArchivedComments { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Timetable> Timetables { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Prevent cascade delete for the GuidanceTeacher relationship
            modelBuilder.Entity<Ticket>()
                .HasRequired(t => t.GuidanceTeacher)
                .WithMany(g => g.Tickets)
                .HasForeignKey(t => t.GuidanceTeacherId)
                .WillCascadeOnDelete(false);

            // You may also need to do this for other relationships
            modelBuilder.Entity<Ticket>()
                .HasRequired(t => t.Student)
                .WithMany()
                .HasForeignKey(t => t.StudentId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Ticket>()
                .HasRequired(t => t.Lecturer)
                .WithMany(L => L.Tickets)
                .HasForeignKey(t => t.LecturerId)
                .WillCascadeOnDelete(false);

            // Configure CurriculumHead-Department relationship
            modelBuilder.Entity<Department>()
                .HasOptional(d => d.CurriculumHead)
                .WithOptionalDependent(c => c.Department);

            // Configure Feedback-Student relationship as one-to-one
            modelBuilder.Entity<Student>()
                .HasOptional(s => s.Feedback)    // Student can have one optional Feedback
                .WithRequired(f => f.Student);   // Feedback must have one Student

        }

        public static GuidanceTrackerDbContext Create()
        {
            return new GuidanceTrackerDbContext();
        }
    }
}