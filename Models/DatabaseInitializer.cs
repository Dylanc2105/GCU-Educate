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

                var guidance = new GuidanceTeacher
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
                var lecturer = new Lecturer
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
                userManager.Create(lecturer, "123");
                //assign it to the manager role
                userManager.AddToRole(lecturer.Id, "Lecturer");
            }

            if (userManager.FindByName("student@email.com") == null)
            {
                var student = new Student
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
                };
                userManager.Create(student, "123");
                userManager.AddToRoles(student.Id, "Student");
            }
      
            context.SaveChanges();
        }
    }

}