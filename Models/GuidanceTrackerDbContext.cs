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
        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Ticket> Tickets { get; set; }


        public static GuidanceTrackerDbContext Create()
        {
            return new GuidanceTrackerDbContext();
        }
    }
}