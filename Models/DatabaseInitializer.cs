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
    //public class DatabaseInitializer : DropCreateDatabaseIfModelChanges<GuidanceTrackerDbContext>
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

            // Create a Lecturer for the new units if not already created
            var lecturer4 = new Lecturer
            {
                UserName = "lecturer4@email.com",
                Email = "lecturer4@email.com",
                FirstName = "David",
                LastName = "Brown",
                Street = "20 Tech Ave",
                City = "London",
                Postcode = "E5 7UP",
                RegistredAt = DateTime.Now.AddYears(-2),
                EmailConfirmed = true,
            };
            userManager.Create(lecturer4, "123");
            userManager.AddToRole(lecturer4.Id, "Lecturer");

            // Modules for Computer Science
            var module1 = new Module
            {
                ModuleName = "Programming 101",
                ModuleDescription = "Introduction to Programming",
                CourseId = course.CourseId,
                LecturerId = lecturer1.Id
            };
            context.Modules.Add(module1);

            // New Module 1
            var module2 = new Module
            {
                ModuleName = "Data Structures & Algorithms",
                ModuleDescription = "Fundamentals of Data Structures and Algorithms",
                CourseId = course.CourseId,
                LecturerId = lecturer2.Id
            };
            context.Modules.Add(module2);

            // New Module 2
            var module3 = new Module
            {
                ModuleName = "Database Systems",
                ModuleDescription = "Introduction to SQL and Database Management",
                CourseId = course.CourseId,
                LecturerId = lecturer4.Id
            };
            context.Modules.Add(module3);

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

            // Create a second Course
            var course2 = new Course
            {
                CourseName = "Cyber Security",
                CourseDescription = "Introduction to Cyber Security"
            };
            context.Courses.Add(course2);
            context.SaveChanges();


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

            var ticket2 = new Ticket
            {
                TicketTitle = "Academic Issue",
                TicketDescription = "Student has failed my class.",
                TicketStatus = "Open",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                LecturerId = lecturer1.Id,
                StudentId = student1.Id,
                GuidanceTeacherId = guidance.Id
            };
            context.Tickets.Add(ticket2);
            context.SaveChanges();

            var ticket3 = new Ticket
            {
                TicketTitle = "Medical Issue",
                TicketDescription = "Student has fractured there arm and is unable to type ",
                TicketStatus = "Archived",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                LecturerId = lecturer1.Id,
                StudentId = student1.Id,
                GuidanceTeacherId = guidance.Id
            };
            context.Tickets.Add(ticket3);
            context.SaveChanges();


            //create an appointment
            var appointment = new Appointment
            {
                Time = DateTime.Now.AddDays(3),
                AppointmentComment = null,
                Room = "05.004",
                TicketId = ticket2.TicketId
            };
            context.Appointments.Add(appointment);
            context.SaveChanges();

            var appointment1 = new Appointment
            {
                Time = DateTime.Now.AddDays(-3),
                AppointmentComment = "student started preparing to reassessments",
                Room = "05.005",
                TicketId = ticket2.TicketId
            };
            context.Appointments.Add(appointment1);
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
