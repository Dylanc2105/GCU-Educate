using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models
{
    public class DatabaseInitializer : DropCreateDatabaseAlways<GuidanceTrackerDbContext>
    {


        protected override void Seed(GuidanceTrackerDbContext context)
        {
            base.Seed(context);

            //if no recodrs in useres table

            if (!context.Users.Any())
            {
                //create roles and store them in a table

                //to create amd store roles we need usermanager
                RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));




                //if admin role doesnt exist
                if (!roleManager.RoleExists("GuidanceTeacher"))
                {
                    //then create one
                    roleManager.Create(new IdentityRole("GuidanceTeacher"));
                }
                if (!roleManager.RoleExists("User"))
                {
                    //then create one
                    roleManager.Create(new IdentityRole("User"));
                }

                if (!roleManager.RoleExists("Lecturer"))
                {
                    //then create one
                    roleManager.Create(new IdentityRole("Lecturer"));
                }

                if (!roleManager.RoleExists("Student"))
                {
                    //then create one
                    roleManager.Create(new IdentityRole("Student"));
                }

                context.SaveChanges();
            }


            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(context));

            GuidanceTeacher guidance = null;
            Lecturer lecturer1 = null;
            Lecturer lecturer2 = null;
            Lecturer lecturer3 = null;
            Student student1 = null;

            //create guidance teacher
            //first check if admin exists in db
            if (userManager.FindByName("guidance@email.com") == null)
            {
                //very easy password validation 
                userManager.PasswordValidator = new PasswordValidator
                {
                    RequireDigit = false,
                    RequiredLength = 1,
                    RequireLowercase = false,
                    RequireNonLetterOrDigit = false,
                    RequireUppercase = false,

                };
                //crate an admin employee

                guidance = new GuidanceTeacher
                {
                    UserName = "guidance@email.com",
                    Email = "guidance@email.com",
                    FirstName = "John",
                    LastName = "Smith",
                    Street = "35 Washington st",
                    City = "London",
                    Postcode = "E12 8UP",
                    RegistredAt = DateTime.Now.AddYears(-5),
                    EmailConfirmed = true,

                };

                //add admin to users table
                userManager.Create(guidance, "123");
                //assign it to the guidance teacher role
                userManager.AddToRole(guidance.Id, "GuidanceTeacher");
            }

            //add some lecturers
            //first check if lecturer already exists in db
            if (userManager.FindByName("lecturer@email.com") == null)
            {
                //if no then create him
                lecturer1 = new Lecturer
                {
                    UserName = "lecturer@email.com",
                    Email = "lecturer@email.com",
                    FirstName = "Michael",
                    LastName = "Johnson",
                    Street = "25 LA st",
                    City = "London",
                    Postcode = "E52 9UP",
                    RegistredAt = DateTime.Now.AddYears(-3),
                    EmailConfirmed = true,
                };
                //add jeff to users table
                userManager.Create(lecturer1, "123");
                //assign it to the manager role
                userManager.AddToRole(lecturer1.Id, "Lecturer");
            }


            if (userManager.FindByName("lecturer2@email.com") == null)


            {
                lecturer2 = new Lecturer
                {
                    UserName = "lecturer2@email.com",
                    Email = "lecturer2@email.com",
                    FirstName = "Laura",
                    LastName = "Smith",
                    Street = "33 Oxford st",
                    City = "London",
                    Postcode = "SW1 2AA",
                    RegistredAt = DateTime.Now.AddYears(-5),
                    EmailConfirmed = true,
                };
                userManager.Create(lecturer2, "123");
                userManager.AddToRole(lecturer2.Id, "Lecturer");
            }

            // Create a Course
            var course = new Course
            {
                CourseName = "Computer Science",
                CourseDescription = "Introduction to Computer Science"
            };
            context.Courses.Add(course);
            context.SaveChanges();


            if (userManager.FindByName("student@email.com") == null)
            {
                student1 = new Student
                {
                    UserName = "student@email.com",
                    Email = "student@email.com",
                    FirstName = "Emily",
                    LastName = "Davis",
                    Street = "11 New York st",
                    City = "London",
                    Postcode = "E52 9UP",
                    RegistredAt = DateTime.Now.AddYears(-1),
                    EmailConfirmed = true,
                    CourseId = course.CourseId
                };
                userManager.Create(student1, "123");
                userManager.AddToRoles(student1.Id, "Student");

            }

            // Create a Module
            var module = new Module
            {
                ModuleName = "Programming 101",
                ModuleDescription = "Introduction to Programming",
                CourseId = course.CourseId,
                LecturerId = lecturer1.Id
            };
            context.Modules.Add(module);
            context.SaveChanges();

            // Create a Session
            var session = new Session
            {
                SessionDate = DateTime.Now.AddDays(7),
                SessionStatus = "Pending",
                SessionNotes = "Initial consultation",
                StudentId = student1.Id,
                GuidanceTeacherId = guidance.Id
            };
            context.Sessions.Add(session);
            context.SaveChanges();

            // Create a Ticket
            var ticket = new Ticket
            {
                TicketTitle = "Attendance Issue",
                TicketDescription = "Student has missed multiple classes.",
                TicketStatus = "Open",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                LecturerId = lecturer1.Id,
                StudentId = student1.Id,
                GuidanceTeacherId = guidance.Id
            };
            context.Tickets.Add(ticket);
            context.SaveChanges();

            if (userManager.FindByName("beno.atagan@gmail.com") == null)


            {
                lecturer3 = new Lecturer
                {
                    UserName = "beno.atagan@gmail.com",
                    Email = "beno.atagan@gmail.com",
                    FirstName = "Laura",
                    LastName = "Smith",
                    Street = "33 Oxford st",
                    City = "London",
                    Postcode = "SW1 2AA",
                    RegistredAt = DateTime.Now.AddYears(-5),
                    EmailConfirmed = true,
                };
                userManager.Create(lecturer3, "123");
                userManager.AddToRole(lecturer3.Id, "Lecturer");
            }
        }

    }
    

}

