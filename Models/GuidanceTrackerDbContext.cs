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
        public DbSet<Issue> Issues { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<ArchivedTicket> ArchivedTickets { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<MessageBoard> MessageBoards { get; set; }
        public DbSet<ArchivedComment> ArchivedComments { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Timetable> Timetables { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Prevent cascade delete for the GuidanceTeacher relationship
            modelBuilder.Entity<Issue>()
                .HasRequired(t => t.GuidanceTeacher)
                .WithMany(g => g.Issues)
                .HasForeignKey(t => t.GuidanceTeacherId)
                .WillCascadeOnDelete(false);

            // You may also need to do this for other relationships
            modelBuilder.Entity<Issue>()
                .HasRequired(t => t.Student)
                .WithMany()
                .HasForeignKey(t => t.StudentId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Issue>()
                .HasRequired(t => t.Lecturer)
                .WithMany(L => L.Issues)
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
                                                 // disables cascade delete for the Appontments and USers relationship
            modelBuilder.Entity<Appointment>()
                .HasRequired(a => a.Student)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.StudentId)
                .WillCascadeOnDelete(false);


            // class has one guidance teacher and guidance teacher has many classes
            modelBuilder.Entity<Class>()
                .HasRequired(c => c.GuidanceTeacher)
                .WithMany(c => c.Classes)
                .HasForeignKey(c => c.GuidanceTeacherId)
                .WillCascadeOnDelete(false);

            // class has many students and a student belongs to a class
            modelBuilder.Entity<Student>()
                .HasRequired(s => s.Class)
                .WithMany(c => c.Students)
                .HasForeignKey(s => s.ClassId)
                .WillCascadeOnDelete(false);

            // unit has many classes and a class has many units
            modelBuilder.Entity<Unit>()
                .HasMany(u => u.Classes)
                .WithMany(c => c.Units)
                .Map(m =>
                {
                    m.ToTable("UnitClasses");
                    m.MapLeftKey("UnitId");
                    m.MapRightKey("ClassId");
                });

            // a class has many enrolments and an enrolment belongs to a class
            modelBuilder.Entity<Enrollment>()
                .HasRequired(e => e.Class)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.ClassId)
                .WillCascadeOnDelete(false);

            // one class has many timetables 
            modelBuilder.Entity<Timetable>()
                .HasRequired(t => t.Class)
                .WithMany(c => c.Timetables)
                .HasForeignKey(t => t.ClassId)
                .WillCascadeOnDelete(false);

            // Configure many-to-many relationships
            modelBuilder.Entity<Post>()
                .HasMany(p => p.Departments)
                .WithMany(d => d.Posts)
                .Map(m =>
                {
                    m.ToTable("PostDepartments");
                    m.MapLeftKey("PostId");
                    m.MapRightKey("DepartmentId");
                });

            modelBuilder.Entity<Post>()
                .HasMany(p => p.Courses)
                .WithMany(c => c.Posts)
                .Map(m =>
                {
                    m.ToTable("PostCourses");
                    m.MapLeftKey("PostId");
                    m.MapRightKey("CourseId");
                });

            modelBuilder.Entity<Post>()
                .HasMany(p => p.Classes)
                .WithMany(c => c.Posts)
                .Map(m =>
                {
                    m.ToTable("PostClasses");
                    m.MapLeftKey("PostId");
                    m.MapRightKey("ClassId");
                });

        }

        public static GuidanceTrackerDbContext Create()
        {
            return new GuidanceTrackerDbContext();
        }
    }
}