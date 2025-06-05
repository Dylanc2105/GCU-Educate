using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Data.Entity.Validation;
using System.Text;
using System.Net.Sockets;
using GuidanceTracker.Models;

namespace GuidanceTracker.Models
{
    public class DatabaseInitializer : DropCreateDatabaseAlways<GuidanceTrackerDbContext>
    //public class DatabaseInitializer : DropCreateDatabaseIfModelChanges<GuidanceTrackerDbContext>
    {
        protected override void Seed(GuidanceTrackerDbContext context)
        {
            base.Seed(context);

            //if no records in users table
            if (!context.Users.Any())
            {
                //create roles and store them in a table

                //to create and store roles we need usermanager
                RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

                //if role doesn't exist
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

                if (!roleManager.RoleExists("CurriculumHead"))
                {
                    //then create one
                    roleManager.Create(new IdentityRole("CurriculumHead"));
                }

                context.SaveChanges();
            }


            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(context));

            GuidanceTeacher guidance = null;
            GuidanceTeacher guidance2 = null;
            GuidanceTeacher newGuidanceTeacher = null;
            //GuidanceTeacher guidance3 = null;
            CurriculumHead curriculumHead1 = null;
            Lecturer lecturer1 = null;
            Lecturer lecturer2 = null;
            Lecturer lecturer3 = null;
            Lecturer lecturer4 = null;
            Lecturer lecturer5 = null;
            Lecturer lecturer6 = null;
            //Lecturer lecturer7 = null;
            //Lecturer lecturer8 = null;
            //Lecturer lecturer9 = null;
            //Lecturer lecturer10 = null;
            Lecturer financeLecturer = null;
            Student student1 = null;
            Student student2 = null;
            Student student3 = null;
            Student student4 = null;
            Student student5 = null;
            Student student6 = null;
            Student student7 = null;
            Student student8 = null;
            Student student9 = null;
            Student student10 = null;
            Student student11 = null;
            Student student12 = null;
            Student student13 = null;
            Student student14 = null;
            Student student15 = null;
            Student student16 = null;
            Student student17 = null;
            Student student18 = null;
            Student student19 = null;
            Student student20 = null;
            Student student21 = null;
            Student student22 = null;
            Student student23 = null;
            Student student24 = null;
            Student student25 = null;
            Student student26 = null;
            Student student27 = null;
            Student student28 = null;
            Student student29 = null;
            Student financeStudent2 = null;
            //create guidance teacher
            //first check if admin exists in db
            if (userManager.FindByName("Jamie.Stewart@cityofglasgowcollege.ac.uk") == null)
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
                //create an admin employee

                guidance = new GuidanceTeacher
                {
                    UserName = "Jamie.Stewart@cityofglasgowcollege.ac.uk",
                    Email = "Jamie.Stewart@cityofglasgowcollege.ac.uk",
                    FirstName = "Jamie",
                    LastName = "Stewart",
                    Street = "35 Washington st",
                    City = "London",
                    Postcode = "E12 8UP",
                    RegistredAt = DateTime.UtcNow.AddYears(-5),
                    EmailConfirmed = true,
                };

                //add admin to users table
                userManager.Create(guidance, "123");
                //assign it to the guidance teacher role
                userManager.AddToRole(guidance.Id, "GuidanceTeacher");
            }

            // Seed Guidance Teacher 1
            if (userManager.FindByName("Denise.Doyle@cityofglasgowcollege.ac.uk") == null)
            {
                newGuidanceTeacher = new GuidanceTeacher
                {
                    UserName = "Denise.Doyle@cityofglasgowcollege.ac.uk",
                    Email = "Denise.Doyle@cityofglasgowcollege.ac.uk",
                    FirstName = "Denise",
                    LastName = "Doyle",
                    Street = "123 Oak Avenue",
                    City = "London",
                    Postcode = "SW1A 0AA", 
                    RegistredAt = DateTime.UtcNow.AddYears(-3),
                    EmailConfirmed = true,
                };

                userManager.Create(newGuidanceTeacher, "123"); 
                userManager.AddToRole(newGuidanceTeacher.Id, "GuidanceTeacher");
            }

            // Seed Guidance Teacher 2
            if (userManager.FindByName("guidance2@email.com") == null)
            {
                guidance2 = new GuidanceTeacher
                {
                    UserName = "guidance2@email.com",
                    Email = "guidance2@email.com",
                    FirstName = "Michael",
                    LastName = "Jordan",
                    Street = "45 Elm Street",
                    City = "London",
                    Postcode = "N1 9GU", 
                    RegistredAt = DateTime.UtcNow.AddYears(-2), 
                    EmailConfirmed = true,
                };

                userManager.Create(guidance2, "123"); 
                userManager.AddToRole(guidance2.Id, "GuidanceTeacher");
            }

            //if (userManager.FindByName("samir.zarrug@cogcgt.com") == null)
            //{
            //    guidance3 = new GuidanceTeacher
            //    {
            //        UserName = "samir.zarrug@cogcgt.com",
            //        Email = "samir.zarrug@cogcgt.com",
            //        FirstName = "Samir",
            //        LastName = "Zarrug",
            //        Street = "10 Boss St",
            //        City = "Fife",
            //        Postcode = "1L 8GT",
            //        RegistredAt = DateTime.UtcNow.AddYears(-8),
            //        EmailConfirmed = true,
            //    };
            //    userManager.Create(guidance3, "123");
            //    userManager.AddToRole(guidance3.Id, "GuidanceTeacher");
            //}

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
                    RegistredAt = DateTime.UtcNow.AddYears(-5),
                    EmailConfirmed = true,
                };
                userManager.Create(lecturer3, "123");
                userManager.AddToRole(lecturer3.Id, "Lecturer");
            }
            context.SaveChanges();

            if (userManager.FindByName("Samir.Zarrug@cityofglasgowcollege.ac.uk") == null)
            {
                curriculumHead1 = new CurriculumHead
                {
                    UserName = "Samir.Zarrug@cityofglasgowcollege.ac.uk",
                    Email = "Samir.Zarrug@cityofglasgowcollege.ac.uk",
                    FirstName = "Samir",
                    LastName = "Zarrug",
                    Street = "25 glasgow road",
                    City = "Glasgow",
                    Postcode = "G31 29H",
                    RegistredAt = DateTime.UtcNow.AddYears(-3),
                    EmailConfirmed = true,
                    DepartmentId = "CR-CTC"
                };
                userManager.Create(curriculumHead1, "123");
                userManager.AddToRole(curriculumHead1.Id, "CurriculumHead");
            }
            context.SaveChanges();

            // Create a new curriculum head for the new department
            CurriculumHead CurriculumHead2 = null;
            if (userManager.FindByName("financeHead@email.com") == null)
            {
                CurriculumHead2 = new CurriculumHead
                {
                    UserName = "financeHead@email.com",
                    Email = "financeHead@email.com",
                    FirstName = "Margaret",
                    LastName = "Thompson",
                    Street = "42 Finance Street",
                    City = "Edinburgh",
                    Postcode = "EH4 7LP",
                    RegistredAt = DateTime.UtcNow.AddYears(-2),
                    EmailConfirmed = true,
                    DepartmentId = "BU-FIN"
                };
                userManager.Create(CurriculumHead2, "123");
                userManager.AddToRole(CurriculumHead2.Id, "CurriculumHead");
            }
            context.SaveChanges();

            // First create Department
            var department = new Department
            {
                DepartmentId = "CR-CTC",
                DepartmentName = "Creative & Computing Technologies",
                CurriculumHead = curriculumHead1
            };
            context.Departments.Add(department);
            context.SaveChanges();

            var newDepartment = new Department
            {
                DepartmentId = "BU-FIN",
                DepartmentName = "Business & Finance",
                CurriculumHead = CurriculumHead2
            };
            context.Departments.Add(newDepartment);
            context.SaveChanges();


            //add some lecturers
            //first check if lecturer already exists in db
            if (userManager.FindByName("Dana.Carson@cityofglasgowcollege.ac.uk") == null)
            {
                //if no then create him
                lecturer1 = new Lecturer
                {
                    UserName = "Dana.Carson@cityofglasgowcollege.ac.uk",
                    Email = "Dana.Carson@cityofglasgowcollege.ac.uk",
                    FirstName = "Dana",
                    LastName = "Carson",
                    Street = "25 LA st",
                    City = "London",
                    Postcode = "E52 9UP",
                    RegistredAt = DateTime.UtcNow.AddYears(-3),
                    EmailConfirmed = true,
                };
                //add jeff to users table
                userManager.Create(lecturer1, "123");
                //assign it to the manager role
                userManager.AddToRole(lecturer1.Id, "Lecturer");
            }
            context.SaveChanges();

            if (userManager.FindByName("James.Hood@cityofglasgowcollege.ac.uk") == null)
            {
                lecturer2 = new Lecturer
                {
                    UserName = "James.Hood@cityofglasgowcollege.ac.uk",
                    Email = "James.Hood@cityofglasgowcollege.ac.uk",
                    FirstName = "James",
                    LastName = "Hood",
                    Street = "33 Oxford st",
                    City = "London",
                    Postcode = "SW1 2AA",
                    RegistredAt = DateTime.UtcNow.AddYears(-5),
                    EmailConfirmed = true,
                };
                userManager.Create(lecturer2, "123");
                userManager.AddToRole(lecturer2.Id, "Lecturer");
            }
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
                    RegistredAt = DateTime.UtcNow.AddYears(-5),
                    EmailConfirmed = true,
                };
                userManager.Create(lecturer3, "123");
                userManager.AddToRole(lecturer3.Id, "Lecturer");
            }
            context.SaveChanges();

            if (userManager.FindByName("Garry.Kelly@cityofglasgowcollege.ac.uk") == null)
            {
                lecturer4 = new Lecturer
                {
                    UserName = "Garry.Kelly@cityofglasgowcollege.ac.uk",
                    Email = "Garry.Kelly@cityofglasgowcollege.ac.uk",
                    FirstName = "Garry",
                    LastName = "Kelly",
                    Street = "27 King Street",
                    City = "Manchester",
                    Postcode = "M2 6LE",
                    RegistredAt = DateTime.UtcNow.AddYears(-2),
                    EmailConfirmed = true,
                };
                userManager.Create(lecturer4, "123");
                userManager.AddToRole(lecturer4.Id, "Lecturer");
            }
            context.SaveChanges();

            //if (userManager.FindByName("dana.carson@cogcgt.com") == null)
            //{
            //    lecturer5 = new Lecturer
            //    {
            //        UserName = "dana.carson@cogcgt.com",
            //        Email = "dana.carson@cogcgt.com",
            //        FirstName = "Dana",
            //        LastName = "Carson",
            //        Street = "45 Baloney St",
            //        City = "Antarctica",
            //        Postcode = "GB 45A",
            //        RegistredAt = DateTime.UtcNow.AddYears(-15),
            //        EmailConfirmed = true,
            //    };
            //    userManager.Create(lecturer5, "123");
            //    userManager.AddToRole(lecturer5.Id, "Lecturer");
            //}
            //context.SaveChanges();

            //if (userManager.FindByName("garry.kelly@cogcgt.com") == null)
            //{
            //    lecturer6 = new Lecturer
            //    {
            //        UserName = "garry.kelly@cogcgt.com",
            //        Email = "garry.kelly@cogcgt.com",
            //        FirstName = "Garry",
            //        LastName = "Kelly",
            //        Street = "55 Baloney St",
            //        City = "Antarctica",
            //        Postcode = "GB 55A",
            //        RegistredAt = DateTime.UtcNow.AddYears(-25),
            //        EmailConfirmed = true,
            //    };
            //    userManager.Create(lecturer6, "123");
            //    userManager.AddToRole(lecturer6.Id, "Lecturer");
            //}
            //context.SaveChanges();

            if (userManager.FindByName("Asmat.Ullah@cityofglasgowcollege.ac.uk") == null)
            {
                lecturer5 = new Lecturer
                {
                    UserName = "Asmat.Ullah@cityofglasgowcollege.ac.uk",
                    Email = "Asmat.Ullah@cityofglasgowcollege.ac.uk",
                    FirstName = "Asmat",
                    LastName = "Ullah",
                    Street = "65 Baloney St",
                    City = "Antarctica",
                    Postcode = "GB 65A",
                    RegistredAt = DateTime.UtcNow.AddYears(-35),
                    EmailConfirmed = true,
                };
                userManager.Create(lecturer5, "123");
                userManager.AddToRole(lecturer5.Id, "Lecturer");
            }
            context.SaveChanges();

            //if (userManager.FindByName("james.hood@cogcgt.com") == null)
            //{
            //    lecturer8 = new Lecturer
            //    {
            //        UserName = "james.hood@cogcgt.com",
            //        Email = "james.hood@cogcgt.com",
            //        FirstName = "James",
            //        LastName = "Hood",
            //        Street = "75 Baloney St",
            //        City = "Antarctica",
            //        Postcode = "GB 75A",
            //        RegistredAt = DateTime.UtcNow.AddYears(-5),
            //        EmailConfirmed = true,
            //    };
            //    userManager.Create(lecturer8, "123");
            //    userManager.AddToRole(lecturer8.Id, "Lecturer");
            //}
            //context.SaveChanges();

            //if (userManager.FindByName("jamie.stewart@cogcgt.com") == null)
            //{
            //    lecturer9 = new Lecturer
            //    {
            //        UserName = "jamie.stewart@cogcgt.com",
            //        Email = "jamie.stewart@cogcgt.com",
            //        FirstName = "Jamie",
            //        LastName = "Stewart",
            //        Street = "85 Baloney St",
            //        City = "Antarctica",
            //        Postcode = "GB 85A",
            //        RegistredAt = DateTime.UtcNow.AddYears(-18),
            //        EmailConfirmed = true,
            //    };
            //    userManager.Create(lecturer9, "123");
            //    userManager.AddToRole(lecturer9.Id, "Lecturer");
            //}
            //context.SaveChanges();

            if (userManager.FindByName("Stella.Martin@cityofglasgowcollege.ac.uk") == null)
            {
                lecturer6 = new Lecturer
                {
                    UserName = "Stella.Martin@cityofglasgowcollege.ac.uk",
                    Email = "Stella.Martin@cityofglasgowcollege.ac.uk",
                    FirstName = "Stella",
                    LastName = "Martin",
                    Street = "95 Baloney St",
                    City = "Antarctica",
                    Postcode = "GB 95A",
                    RegistredAt = DateTime.UtcNow.AddYears(-40),
                    EmailConfirmed = true,
                };
                userManager.Create(lecturer6, "123");
                userManager.AddToRole(lecturer6.Id, "Lecturer");
            }
            context.SaveChanges();


            if (userManager.FindByName("financeLecturer@email.com") == null)
            {
                //if no then create him
                financeLecturer = new Lecturer
                {
                    UserName = "financeLecturer@email.com",
                    Email = "financeLecturer@email.com",
                    FirstName = "Lenny",
                    LastName = "Morgan",
                    Street = "25 LA st",
                    City = "London",
                    Postcode = "E52 9UP",
                    RegistredAt = DateTime.UtcNow.AddYears(-3),
                    EmailConfirmed = true,
                };
                //add jeff to users table
                userManager.Create(financeLecturer, "123");
                //assign it to the manager role
                userManager.AddToRole(financeLecturer.Id, "Lecturer");
            }
            context.SaveChanges();

            

            // Create classes
            var classes1 = new Class
            {
                ClassId = 1,
                ClassName = "HNC Computing Class A",
                MaxCapacity = 24,
                GuidanceTeacherId = guidance.Id
            };
            context.Classes.Add(classes1);
            context.SaveChanges();

            var classes2 = new Class
            {
                ClassId = 2,
                ClassName = "HNC Computing Class B",
                MaxCapacity = 24,
                GuidanceTeacherId = guidance.Id
            };
            context.Classes.Add(classes2);
            context.SaveChanges();

            var classes3 = new Class
            {
                ClassId = 3,
                ClassName = "HNC Software Development Class A",
                MaxCapacity = 24,
                GuidanceTeacherId = guidance.Id
            };
            context.Classes.Add(classes3);
            context.SaveChanges();

            var classes4 = new Class
            {
                ClassId = 4,
                ClassName = "HNC Software Development Class B",
                MaxCapacity = 24,
                GuidanceTeacherId = guidance.Id
            };
            context.Classes.Add(classes4);
            context.SaveChanges();

            var classes5 = new Class
            {
                ClassId = 5,
                ClassName = "HND Computer Science Class",
                MaxCapacity = 16,
                GuidanceTeacherId = guidance.Id
            };
            context.Classes.Add(classes5);
            context.SaveChanges();

            var classes6 = new Class
            {
                ClassId = 6,
                ClassName = "HND Software Development Class A",
                MaxCapacity = 24,
                GuidanceTeacherId = guidance.Id
            };

            context.Classes.Add(classes6);
            context.SaveChanges();

            var classes7 = new Class
            {
                ClassId = 7,
                ClassName = "HND Software Development Class B",
                MaxCapacity = 16,
                GuidanceTeacherId = guidance.Id
            };
            context.Classes.Add(classes7);
            context.SaveChanges();

            var classes8 = new Class
            {
                ClassId = 8,
                ClassName = "NQ Computing Class A",
                MaxCapacity = 24,
                GuidanceTeacherId = guidance.Id
            };

            context.Classes.Add(classes8);
            context.SaveChanges();

            var classes9 = new Class
            {
                ClassId = 9,
                ClassName = "NQ Computing Class B",
                MaxCapacity = 24,
                GuidanceTeacherId = guidance.Id
            };

            context.Classes.Add(classes9);
            context.SaveChanges();

            var financeClass = new Class
            {
                ClassId = 11, // Make sure this ID doesn't conflict with existing ones
                ClassName = "HNC Finance Class A",
                MaxCapacity = 20,
                GuidanceTeacherId = newGuidanceTeacher.Id
            };
            context.Classes.Add(financeClass);
            context.SaveChanges();

            // Create courses

            var courses1 = new Course
            {
                CourseId = 1,
                CourseName = "HNC Computing / HNC Computer Science",
                CourseReference = "CRHNCCOMSC/F241A",
                ModeOfStudy = "17: Full-Time",
                DurationInWeeks = 37,
                SCQFLevel = 7,
                Site = "City Campus",
                StartDate = DateTime.Parse("2024-08-26"),
                EndDate = DateTime.Parse("2025-06-13"),
                DepartmentId = department.DepartmentId
            };
            context.Courses.Add(courses1);
            context.SaveChanges();

            var course2 = new Course
            {
                CourseId = 2,
                CourseName = "HNC Computing / HNC Computer Science",
                CourseReference = "CRHNCCOMSC/F241B",
                ModeOfStudy = "17: Full-Time",
                DurationInWeeks = 37,
                SCQFLevel = 7,
                Site = "City Campus",
                StartDate = DateTime.Parse("2024-08-26"),
                EndDate = DateTime.Parse("2025-06-13"),
                DepartmentId = department.DepartmentId
            };
            context.Courses.Add(course2);
            context.SaveChanges();

            var courses3 = new Course
            {
                CourseId = 3,
                CourseName = "HNC Computing/HNC Computing: Software Development",
                CourseReference = "CRHNCCOMSD/F241A",
                ModeOfStudy = "17: Full-Time",
                DurationInWeeks = 37,
                SCQFLevel = 7,
                Site = "City Campus",
                StartDate = DateTime.Parse("2024-08-26"),
                EndDate = DateTime.Parse("2025-06-13"),
                DepartmentId = department.DepartmentId
            };
            context.Courses.Add(courses3);
            context.SaveChanges();

            var courses4 = new Course
            {
                CourseId = 4,
                CourseName = "HNC Computing/HNC Computing: Software Development",
                CourseReference = "CRHNCCOMSD/F241B",
                ModeOfStudy = "17: Full-Time",
                DurationInWeeks = 37,
                SCQFLevel = 7,
                Site = "City Campus",
                StartDate = DateTime.Parse("2024-08-26"),
                EndDate = DateTime.Parse("2025-06-13"),
                DepartmentId = department.DepartmentId
            };
            context.Courses.Add(courses4);
            context.SaveChanges();

            var courses5 = new Course
            {
                CourseId = 5,
                CourseName = "HND Computer Science",
                CourseReference = "CRHNDCOMSC/F242A",
                ModeOfStudy = "17: Full-Time",
                DurationInWeeks = 37,
                SCQFLevel = 8,
                Site = "City Campus",
                StartDate = DateTime.Parse("2024-08-26"),
                EndDate = DateTime.Parse("2025-06-13"),
                DepartmentId = department.DepartmentId
            };
            context.Courses.Add(courses5);
            context.SaveChanges();

            var course6 = new Course
            {
                CourseId = 6,
                CourseName = "HND Computing: Software Development",
                CourseReference = "CRHNDCOMSD/F241A",
                ModeOfStudy = "17: Full-Time",
                DurationInWeeks = 37,
                SCQFLevel = 8,
                Site = "City Campus",
                StartDate = DateTime.Parse("2024-08-26"),
                EndDate = DateTime.Parse("2025-06-13"),
                DepartmentId = department.DepartmentId

            };
            context.Courses.Add(course6);
            context.SaveChanges();

            var courses7 = new Course
            {
                CourseId = 7,
                CourseName = "HND Computing: Software Development",
                CourseReference = "CRHNDCOMSD/F242A",
                ModeOfStudy = "17: Full-Time",
                DurationInWeeks = 37,
                SCQFLevel = 8,
                Site = "City Campus",
                StartDate = DateTime.Parse("2024-08-26"),
                EndDate = DateTime.Parse("2025-06-13"),
                DepartmentId = department.DepartmentId
            };
            context.Courses.Add(courses7);
            context.SaveChanges();

            var courses8 = new Course
            {
                CourseId = 8,
                CourseName = "HND Computing: Software Development",
                CourseReference = "CRHNDCOMSD/F242B",
                ModeOfStudy = "17: Full-Time",
                DurationInWeeks = 37,
                SCQFLevel = 8,
                Site = "City Campus",
                StartDate = DateTime.Parse("2024-08-26"),
                EndDate = DateTime.Parse("2025-06-13"),
                DepartmentId = department.DepartmentId
            };
            context.Courses.Add(courses8);
            context.SaveChanges();

            var courses9 = new Course
            {
                CourseId = 9,
                CourseName = "NQ Computing",
                CourseReference = "CRNQUCOMP6/F241A",
                ModeOfStudy = "17: Full-Time",
                DurationInWeeks = 37,
                SCQFLevel = 6,
                Site = "City Campus",
                StartDate = DateTime.Parse("2024-08-26"),
                EndDate = DateTime.Parse("2025-06-13"),
                DepartmentId = department.DepartmentId
            };
            context.Courses.Add(courses9);
            context.SaveChanges();

            // Create a new course
            var financeCourse = new Course
            {
                CourseId = 10, // Make sure this doesn't conflict with existing course IDs
                CourseName = "HNC Business & Finance",
                CourseReference = "BUHNFIN/F241A",
                ModeOfStudy = "17: Full-Time",
                DurationInWeeks = 37,
                SCQFLevel = 7,
                Site = "Edinburgh Campus",
                StartDate = DateTime.Parse("2024-08-26"),
                EndDate = DateTime.Parse("2025-06-13"),
                DepartmentId = newDepartment.DepartmentId
            };
            context.Courses.Add(financeCourse);
            context.SaveChanges();

            // Create a new enrollment
            var financeEnrollment = new Enrollment
            {
                EnrollmentDate = DateTime.Parse("2024-08-26"),
                Status = EnrollmentStatus.Active,
                CourseId = financeCourse.CourseId,
                ClassId = financeClass.ClassId
            };
            context.Enrollments.Add(financeEnrollment);
            context.SaveChanges();

            // Create enrollments to link courses with classes
            // HNC Computing / HNC Computer Science - CRHNCCOMSC/F241A with HNC Computing Class A
            var enrollment1 = new Enrollment
            {
                EnrollmentDate = DateTime.Parse("2024-08-26"),
                Status = EnrollmentStatus.Active,
                CourseId = courses1.CourseId, // CRHNCCOMSC/F241A
                ClassId = classes1.ClassId // HNC Computing Class A
            };
            context.Enrollments.Add(enrollment1);
            context.SaveChanges();

            // HNC Computing / HNC Computer Science - CRHNCCOMSC/F241B with HNC Computing Class B
            var enrollment2 = new Enrollment
            {
                EnrollmentDate = DateTime.Parse("2024-08-26"),
                Status = EnrollmentStatus.Active,
                CourseId = course2.CourseId, // CRHNCCOMSC/F241B
                ClassId = classes2.ClassId // HNC Computing Class B
            };
            context.Enrollments.Add(enrollment2);
            context.SaveChanges();

            // HNC Computing/HNC Computing: Software Development - CRHNCCOMSD/F241A with HNC Software Development Class A
            var enrollment3 = new Enrollment
            {
                EnrollmentDate = DateTime.Parse("2024-08-26"),
                Status = EnrollmentStatus.Active,
                CourseId = courses3.CourseId, // CRHNCCOMSD/F241A
                ClassId = classes3.ClassId // HNC Software Development Class A
            };
            context.Enrollments.Add(enrollment3);
            context.SaveChanges();

            // HNC Computing/HNC Computing: Software Development - CRHNCCOMSD/F241B with HNC Software Development Class B
            var enrollment4 = new Enrollment
            {
                EnrollmentDate = DateTime.Parse("2024-08-26"),
                Status = EnrollmentStatus.Active,
                CourseId = courses4.CourseId, // CRHNCCOMSD/F241B
                ClassId = classes4.ClassId // HNC Software Development Class B
            };
            context.Enrollments.Add(enrollment4);
            context.SaveChanges();

            // HND Computer Science - CRHNDCOMSC/F242A with HND Computer Science Class
            var enrollment5 = new Enrollment
            {
                EnrollmentDate = DateTime.Parse("2024-08-26"),
                Status = EnrollmentStatus.Active,
                CourseId = courses5.CourseId, // CRHNDCOMSC/F242A
                ClassId = classes5.ClassId // HND Computer Science Class
            };
            context.Enrollments.Add(enrollment5);
            context.SaveChanges();

            // HND Computing: Software Development - CRHNDCOMSD/F241A with HND Software Development Class A
            var enrollment6 = new Enrollment
            {
                EnrollmentDate = DateTime.Parse("2024-08-26"),
                Status = EnrollmentStatus.Active,
                CourseId = course6.CourseId, // CRHNDCOMSD/F241A
                ClassId = classes6.ClassId // HND Software Development Class A
            };
            context.Enrollments.Add(enrollment6);
            context.SaveChanges();

            // HND Computing: Software Development - CRHNDCOMSD/F242A with HND Software Development Class A (sharing the same class)
            var enrollment7 = new Enrollment
            {
                EnrollmentDate = DateTime.Parse("2024-08-26"),
                Status = EnrollmentStatus.Active,
                CourseId = courses7.CourseId, // CRHNDCOMSD/F242A
                ClassId = classes6.ClassId // HND Software Development Class A
            };
            context.Enrollments.Add(enrollment7);
            context.SaveChanges();

            // HND Computing: Software Development - CRHNDCOMSD/F242B with HND Software Development Class B
            var enrollment8 = new Enrollment
            {
                EnrollmentDate = DateTime.Parse("2024-08-26"),
                Status = EnrollmentStatus.Active,
                CourseId = courses8.CourseId, // CRHNDCOMSD/F242B
                ClassId = classes7.ClassId // HND Software Development Class B
            };
            context.Enrollments.Add(enrollment8);
            context.SaveChanges();

            // NQ Computing - CRNQUCOMP6/F241A with NQ Computing Class A
            var enrollment9 = new Enrollment
            {
                EnrollmentDate = DateTime.Parse("2024-08-26"),
                Status = EnrollmentStatus.Active,
                CourseId = courses9.CourseId, // CRNQUCOMP6/F241A
                ClassId = classes8.ClassId // NQ Computing Class A
            };
            context.Enrollments.Add(enrollment9);
            context.SaveChanges();


            // Create units for the new finance program
            var accountingUnit = new Unit
            {
                UnitName = "Principles of Accounting",
                UnitDescription = "Introduction to accounting concepts and practices",
                LecturerId = financeLecturer.Id,
                Classes = new List<Class> { financeClass } // Link to HNC Finance Class A
            };
            context.Units.Add(accountingUnit);
            context.SaveChanges();

            var financeUnit = new Unit
            {
                UnitName = "Business Finance",
                UnitDescription = "Understanding financial management in business contexts",
                LecturerId = financeLecturer.Id,
                Classes = new List<Class> { financeClass } // Link to HNC Finance Class A
            };
            context.Units.Add(financeUnit);
            context.SaveChanges();

            var economicsUnit = new Unit
            {
                UnitName = "Economics for Business",
                UnitDescription = "Application of economic principles in business decision-making",
                LecturerId = financeLecturer.Id,
                Classes = new List<Class> { financeClass } // Link to HNC Finance Class A
            };
            context.Units.Add(economicsUnit);
            context.SaveChanges();

            var investmentUnit = new Unit
            {
                UnitName = "Investment Analysis",
                UnitDescription = "Methods and principles of investment evaluation",
                LecturerId = financeLecturer.Id,
                Classes = new List<Class> { financeClass } // Link to HNC Finance Class A
            };
            context.Units.Add(investmentUnit);
            context.SaveChanges();

            var taxationUnit = new Unit
            {
                UnitName = "Taxation",
                UnitDescription = "Principles of business and personal taxation",
                LecturerId = financeLecturer.Id,
                Classes = new List<Class> { financeClass } // Link to HNC Finance Class A
            };
            context.Units.Add(taxationUnit);
            context.SaveChanges();

            // 1. HNC Computing / HNC Computer Science Units
            var computingUnit = new Unit
            {
                UnitName = "Computing",
                UnitDescription = "Core Computing unit for HNC",
                LecturerId = lecturer1.Id,
                Classes = new List<Class> { classes1, classes2 } // Link to HNC Computing Class A and B
            };
            context.Units.Add(computingUnit);
            context.SaveChanges();

            var databaseDesignUnit = new Unit
            {
                UnitName = "Database Design Fundamentals",
                UnitDescription = "Introduction to database design and implementation",
                LecturerId = lecturer2.Id,
                Classes = new List<Class> { classes1, classes2 } // Link to HNC Computing Class A and B
            };
            context.Units.Add(databaseDesignUnit);
            context.SaveChanges();

            var developingSoftwareUnit = new Unit
            {
                UnitName = "Developing Software: Introduction",
                UnitDescription = "Introduction to software development principles",
                LecturerId = lecturer3.Id,
                Classes = new List<Class> { classes1, classes2 } // Link to HNC Computing Class A and B
            };
            context.Units.Add(developingSoftwareUnit);
            context.SaveChanges();

            var computerSystemsUnit = new Unit
            {
                UnitName = "Computer Systems Fundamentals",
                UnitDescription = "Basic computer architecture and operating systems",
                LecturerId = lecturer4.Id,
                Classes = new List<Class> { classes1, classes2 } // Link to HNC Computing Class A and B
            };
            context.Units.Add(computerSystemsUnit);
            context.SaveChanges();

            var troubleshootingUnit = new Unit
            {
                UnitName = "Troubleshooting Computing Problems",
                UnitDescription = "Practical problem-solving in computing environments",
                LecturerId = lecturer4.Id,
                Classes = new List<Class> { classes1, classes2 } // Link to HNC Computing Class A and B
            };
            context.Units.Add(troubleshootingUnit);
            context.SaveChanges();

            var teamWorkingUnit = new Unit
            {
                UnitName = "Team Working in Computing",
                UnitDescription = "Collaborative work in computing projects",
                LecturerId = lecturer6.Id,
                Classes = new List<Class> { classes1, classes2 } // Link to HNC Computing Class A and B
            };
            context.Units.Add(teamWorkingUnit);
            context.SaveChanges();

            var ethicsUnit = new Unit
            {
                UnitName = "Professionalism and Ethics in Computing",
                UnitDescription = "Ethical considerations in computing industry",
                LecturerId = lecturer1.Id,
                Classes = new List<Class> { classes1, classes2 } // Link to HNC Computing Class A and B
            };
            context.Units.Add(ethicsUnit);
            context.SaveChanges();

            var bigDataUnit = new Unit
            {
                UnitName = "Big Data",
                UnitDescription = "Introduction to big data concepts and technologies",
                LecturerId = lecturer2.Id,
                Classes = new List<Class> { classes1, classes2 } // Link to HNC Computing Class A and B
            };
            context.Units.Add(bigDataUnit);
            context.SaveChanges();

            var webDevUnit = new Unit
            {
                UnitName = "Software Development: Developing Websites",
                UnitDescription = "Web development fundamentals",
                LecturerId = lecturer3.Id,
                Classes = new List<Class> { classes1, classes2 } // Link to HNC Computing Class A and B
            };
            context.Units.Add(webDevUnit);
            context.SaveChanges();

            var gradedUnit1 = new Unit
            {
                UnitName = "Computing: Graded Unit 1",
                UnitDescription = "Integration of knowledge across the HNC",
                LecturerId = lecturer4.Id,
                Classes = new List<Class> { classes1, classes2 } // Link to HNC Computing Class A and B
            };
            context.Units.Add(gradedUnit1);
            context.SaveChanges();

            var dataScienceUnit = new Unit
            {
                UnitName = "Data Science",
                UnitDescription = "Introduction to data science principles and practices",
                LecturerId = lecturer5.Id,
                Classes = new List<Class> { classes1, classes2 } // Link to HNC Computing Class A and B

            };
            context.Units.Add(dataScienceUnit);
            context.SaveChanges();

            var statisticsUnit = new Unit
            {
                UnitName = "Statistics for Science 1",
                UnitDescription = "Statistical methods for scientific applications",
                LecturerId = lecturer6.Id,
                Classes = new List<Class> { classes1, classes2 } // Link to HNC Computing Class A and B
            };
            context.Units.Add(statisticsUnit);
            context.SaveChanges();

            var mobileWebUnit = new Unit
            {
                UnitName = "Developing Mobile Web Based Applications",
                UnitDescription = "Mobile app development principles",
                LecturerId = lecturer1.Id,
                Classes = new List<Class> { classes1, classes2 } // Link to HNC Computing Class A and B
            };
            context.Units.Add(mobileWebUnit);
            context.SaveChanges();

            // 2. HNC Software Development specific units
            var systemsDevUnit = new Unit
            {
                UnitName = "Systems Development: Introduction",
                UnitDescription = "Introduction to systems analysis and design",
                LecturerId = lecturer2.Id,
                Classes = new List<Class> { classes3, classes4 } // Link to HNC Software Development Class A and B
            };
            context.Units.Add(systemsDevUnit);
            context.SaveChanges();

            var testingUnit = new Unit
            {
                UnitName = "Systems Development: Testing Software",
                UnitDescription = "Software testing methodologies",
                LecturerId = lecturer3.Id,
                Classes = new List<Class> { classes3, classes4 } // Link to HNC Software Development Class A and B
            };
            context.Units.Add(testingUnit);
            context.SaveChanges();

            var emergingTechUnit = new Unit
            {
                UnitName = "Emerging Technologies and Experiences",
                UnitDescription = "Current trends in software development",
                LecturerId = lecturer4.Id,
                Classes = new List<Class> { classes3, classes4 } // Link to HNC Software Development Class A and B
            };
            context.Units.Add(emergingTechUnit);
            context.SaveChanges();

            // 3. HND Computer Science units
            var computerScienceUnit = new Unit
            {
                UnitName = "Computer Science",
                UnitDescription = "Advanced computing principles",
                LecturerId = lecturer5.Id,
                Classes = new List<Class> { classes5 } // Link to HND Computer Science Class
            };
            context.Units.Add(computerScienceUnit);
            context.SaveChanges();

            var rdbmsUnit = new Unit
            {
                UnitName = "Relational Database Management Systems",
                UnitDescription = "Advanced database design and management",
                LecturerId = lecturer6.Id,
                Classes = new List<Class> { classes5 } // Link to HND Computer Science Class
            };
            context.Units.Add(rdbmsUnit);
            context.SaveChanges();

            var oopUnit = new Unit
            {
                UnitName = "Software Development: Object Oriented Programming",
                UnitDescription = "OOP techniques and patterns",
                LecturerId = lecturer1.Id,
                Classes = new List<Class> { classes5 } // Link to HND Computer Science Class
            };
            context.Units.Add(oopUnit);
            context.SaveChanges();

            var ooadUnit = new Unit
            {
                UnitName = "Systems Development: Object Oriented Analysis & Design",
                UnitDescription = "OO analysis methodologies",
                LecturerId = lecturer2.Id,
                Classes = new List<Class> { classes5 } // Link to HND Computer Science Class
            };
            context.Units.Add(ooadUnit);
            context.SaveChanges();

            var dataStructuresUnit = new Unit
            {
                UnitName = "Software Development: Data Structures",
                UnitDescription = "Advanced data structures and algorithms",
                LecturerId = lecturer3.Id,
                Classes = new List<Class> { classes5 } // Link to HND Computer Science Class
            };
            context.Units.Add(dataStructuresUnit);
            context.SaveChanges();

            var devApplicationsUnit = new Unit
            {
                UnitName = "Software Development: Developing Applications",
                UnitDescription = "Enterprise application development",
                LecturerId = lecturer4.Id,
                Classes = new List<Class> { classes5 } // Link to HND Computer Science Class
            };
            context.Units.Add(devApplicationsUnit);
            context.SaveChanges();

            var webServerUnit = new Unit
            {
                UnitName = "Managing a Web Server",
                UnitDescription = "Web server administration and optimization",
                LecturerId = lecturer5.Id,
                Classes = new List<Class> { classes5 } // Link to HND Computer Science Class
            };
            context.Units.Add(webServerUnit);
            context.SaveChanges();

            var gradedUnit2 = new Unit
            {
                UnitName = "Computer Science: Graded Unit 2",
                UnitDescription = "Project-based assessment for HND",
                LecturerId = lecturer6.Id,
                Classes = new List<Class> { classes5 } // Link to HND Computer Science Class
            };
            context.Units.Add(gradedUnit2);
            context.SaveChanges();

            var aiUnit = new Unit
            {
                UnitName = "Artificial Intelligence",
                UnitDescription = "AI concepts and applications",
                LecturerId = lecturer1.Id,
                Classes = new List<Class> { classes5 } // Link to HND Computer Science Class
            };
            context.Units.Add(aiUnit);
            context.SaveChanges();

            // 4. HND Software Development specific units
            var softwareDevUnit = new Unit
            {
                UnitName = "Computing: Software Development",
                UnitDescription = "Advanced software development concepts",
                LecturerId = lecturer2.Id,
                Classes = new List<Class> { classes6, classes7 } // Link to HNC Software Development Class A and B
            };
            context.Units.Add(softwareDevUnit);
            context.SaveChanges();

            var multiUserOsUnit = new Unit
            {
                UnitName = "Multi User Operating Systems",
                UnitDescription = "Operating systems for software developers",
                LecturerId = lecturer3.Id,
                Classes = new List<Class> { classes6, classes7 } // Link to HND Software Development Class A and B
            };
            context.Units.Add(multiUserOsUnit);
            context.SaveChanges();

            var projectMgmtUnit = new Unit
            {
                UnitName = "Project Management for IT",
                UnitDescription = "IT project management methodologies",
                LecturerId = lecturer4.Id,
                Classes = new List<Class> { classes6, classes7 } // Link to HND Software Development Class A and B
            };
            context.Units.Add(projectMgmtUnit);
            context.SaveChanges();

            var softwareDevGradedUnit = new Unit
            {
                UnitName = "Computing: Software Development Graded Unit",
                UnitDescription = "Project-based assessment",
                LecturerId = lecturer5.Id,
                Classes = new List<Class> { classes6, classes7 } // Link to HND Software Development Class A and B
            };
            context.Units.Add(softwareDevGradedUnit);
            context.SaveChanges();

            // 5. NQ Computing units
            var introToProgrammingUnit = new Unit
            {
                UnitName = "Introduction to Computer Programming",
                UnitDescription = "Fundamentals of programming",
                LecturerId = lecturer6.Id,
                Classes = new List<Class> { classes8, classes9 } // Link to NQ Computing Class A
            };
            context.Units.Add(introToProgrammingUnit);
            context.SaveChanges();

            var digitalMediaUnit = new Unit
            {
                UnitName = "Computing: Digital Media Elements",
                UnitDescription = "Working with digital media",
                LecturerId = lecturer1.Id,
                Classes = new List<Class> { classes8, classes9 } // Link to NQ Computing Class A
            };
            context.Units.Add(digitalMediaUnit);
            context.SaveChanges();

            var compSysArchUnit = new Unit
            {
                UnitName = "Computer Systems Architecture",
                UnitDescription = "Understanding computer hardware and systems",
                LecturerId = lecturer2.Id,
                Classes = new List<Class> { classes8, classes9 } // Link to NQ Computing Class A
            };
            context.Units.Add(compSysArchUnit);
            context.SaveChanges();

            var networkingUnit = new Unit
            {
                UnitName = "Computing: Networking Technologies",
                UnitDescription = "Introduction to computer networks",
                LecturerId = lecturer3.Id,
                Classes = new List<Class> { classes8, classes9 } // Link to NQ Computing Class A
            };
            context.Units.Add(networkingUnit);
            context.SaveChanges();

            var desktopTroubleshootUnit = new Unit
            {
                UnitName = "Computing: Troubleshoot Desktop Problems",
                UnitDescription = "Basic computer troubleshooting",
                LecturerId = lecturer4.Id,
                Classes = new List<Class> { classes8, classes9 } // Link to NQ Computing Class A
            };
            context.Units.Add(desktopTroubleshootUnit);
            context.SaveChanges();

            var dataSecurityUnit = new Unit
            {
                UnitName = "Data Security",
                UnitDescription = "Introduction to cybersecurity",
                LecturerId = lecturer5.Id,
                Classes = new List<Class> { classes8, classes9 } // Link to NQ Computing Class A

            };
            context.Units.Add(dataSecurityUnit);
            context.SaveChanges();

            var numeracyUnit = new Unit
            {
                UnitName = "Numeracy",
                UnitDescription = "Essential math for computing",
                LecturerId = lecturer6.Id,
                Classes = new List<Class> { classes8, classes9 } // Link to NQ Computing Class A
            };
            context.Units.Add(numeracyUnit);
            context.SaveChanges();

            var hardwareUnit = new Unit
            {
                UnitName = "Computing: Installing and Maintaining Hardware",
                UnitDescription = "Hardware maintenance fundamentals",
                LecturerId = lecturer1.Id,
                Classes = new List<Class> { classes8, classes9 } // Link to NQ Computing Class A
            };
            context.Units.Add(hardwareUnit);
            context.SaveChanges();

            var authoringWebsiteUnit = new Unit
            {
                UnitName = "Computing: Authoring a Website",
                UnitDescription = "Basic web development",
                LecturerId = lecturer2.Id,
                Classes = new List<Class> { classes8, classes9 } // Link to NQ Computing Class A
            };
            context.Units.Add(authoringWebsiteUnit);
            context.SaveChanges();

            var greenITUnit = new Unit
            {
                UnitName = "Green IT",
                UnitDescription = "Sustainable computing practices",
                LecturerId = lecturer3.Id,
                Classes = new List<Class> { classes8, classes9 } // Link to NQ Computing Class A
            };
            context.Units.Add(greenITUnit);
            context.SaveChanges();

            var computingProjectUnit = new Unit
            {
                UnitName = "Computing: Project",
                UnitDescription = "Practical computing project",
                LecturerId = lecturer4.Id,
                Classes = new List<Class> { classes8, classes9 } // Link to NQ Computing Class A
            };
            context.Units.Add(computingProjectUnit);
            context.SaveChanges();

            var appDevUnit = new Unit
            {
                UnitName = "Computing: Applications Development",
                UnitDescription = "Introduction to app development",
                LecturerId = lecturer5.Id,
                Classes = new List<Class> { classes8, classes9 } // Link to NQ Computing Class A
            };
            context.Units.Add(appDevUnit);
            context.SaveChanges();

            var structuredMethodsUnit = new Unit
            {
                UnitName = "Develop Software Using Structured Methods",
                UnitDescription = "Structured programming approaches",
                LecturerId = lecturer6.Id,
                Classes = new List<Class> { classes8, classes9 } // Link to NQ Computing Class A
            };
            context.Units.Add(structuredMethodsUnit);
            context.SaveChanges();


            //create guidance sessions

            var guidanceSession1 = new GuidanceSession
            {
                ClassId = 1,
                Room = "05.005",
                Time = new TimeSpan(11, 00, 0),
                Class = classes1,
                Day = DateTime.UtcNow,
            };
            context.GuidanceSessions.Add(guidanceSession1);

            var guidanceSession2 = new GuidanceSession
            {
                ClassId = 2,
                Room = "05.007",
                Time = new TimeSpan(13, 00, 0),
                Class = classes2,
                Day = DateTime.UtcNow.AddDays(2),
            };
            context.GuidanceSessions.Add(guidanceSession2);

            var guidanceSession3 = new GuidanceSession
            {
                ClassId = 3,
                Room = "05.012",
                Time = new TimeSpan(11, 00, 0),
                Class = classes3,
                Day = DateTime.UtcNow.AddDays(1),
            };
            context.GuidanceSessions.Add(guidanceSession3);

            var guidanceSession4 = new GuidanceSession
            {
                ClassId = 4,
                Room = "05.005",
                Time = new TimeSpan(11, 00, 0),
                Class = classes4,
                Day = DateTime.UtcNow.AddDays(3),
            };
            context.GuidanceSessions.Add(guidanceSession4);

            var guidanceSession5 = new GuidanceSession
            {
                ClassId = 5,
                Room = "05.010",
                Time = new TimeSpan(13, 00, 0),
                Class = classes5,
                Day = DateTime.UtcNow.AddDays(4),
            };
            context.GuidanceSessions.Add(guidanceSession5);

            var guidanceSession6 = new GuidanceSession
            {
                ClassId = 6,
                Room = "05.007",
                Time = new TimeSpan(11, 00, 0),
                Class = classes6,
                Day = DateTime.UtcNow.AddDays(5)
            };
            context.GuidanceSessions.Add(guidanceSession6);

            var guidanceSession7 = new GuidanceSession
            {
                ClassId = 7,
                Room = "05.009",
                Time = new TimeSpan(14, 00, 0),
                Class = classes7,
                Day = DateTime.UtcNow.AddDays(4)
            };
            context.GuidanceSessions.Add(guidanceSession7);

            var guidanceSession8 = new GuidanceSession
            {
                ClassId = 7,
                Room = "05.009",
                Time = new TimeSpan(14, 00, 0),
                Class = classes7,
                Day = DateTime.UtcNow.AddDays(2),
            };
            context.GuidanceSessions.Add(guidanceSession8);

            var guidanceSession9 = new GuidanceSession
            {
                ClassId = 9,
                Room = "05.005",
                Time = new TimeSpan(13, 00, 0),
                Class = classes9,
                Day = DateTime.UtcNow.AddDays(3)
            };
            context.GuidanceSessions.Add(guidanceSession9);
            context.SaveChanges();


            // Create a finance student
            Student financeStudent = null;
            if (userManager.FindByName("finstudent@email.com") == null)
            {
                financeStudent = new Student
                {
                    UserName = "finstudent@email.com",
                    Email = "finstudent@email.com",
                    FirstName = "Samantha",
                    LastName = "Fisher",
                    Street = "53 Accounting Lane",
                    City = "Edinburgh",
                    Postcode = "EH7 4QP",
                    RegistredAt = DateTime.UtcNow.AddMonths(-1),
                    EmailConfirmed = true,
                    GuidanceTeacherId = newGuidanceTeacher.Id,
                    ClassId = financeClass.ClassId,
                    StudentNumber = "42000123",
                    IsClassRep = true
                };
                userManager.Create(financeStudent, "123");
                userManager.AddToRole(financeStudent.Id, "Student");
            }
            context.SaveChanges();

            // Create a second finance student

            if (userManager.FindByName("finstudent2@email.com") == null)
            {
                financeStudent2 = new Student
                {
                    UserName = "finstudent2@email.com",
                    Email = "finstudent2@email.com",
                    FirstName = "Thomas",
                    LastName = "Morgan",
                    Street = "18 Commerce Road",
                    City = "Edinburgh",
                    Postcode = "EH7 4QP",
                    RegistredAt = DateTime.UtcNow.AddMonths(-1),
                    EmailConfirmed = true,
                    GuidanceTeacherId = newGuidanceTeacher.Id,
                    ClassId = financeClass.ClassId,
                    StudentNumber = "42000124"
                };
                userManager.Create(financeStudent2, "123");
                userManager.AddToRole(financeStudent2.Id, "Student");
            }
            context.SaveChanges();


            // Student for HNC Computing Class A
            if (userManager.FindByName("student1@email.com") == null)
            {
                student1 = new Student
                {
                    UserName = "student1@email.com",
                    Email = "student1@email.com",
                    FirstName = "Billy",
                    LastName = "Mclean",
                    Street = "45 High Street",
                    City = "London",
                    Postcode = "E18 2XP",
                    RegistredAt = DateTime.UtcNow.AddMonths(-2),
                    EmailConfirmed = true,
                    IsClassRep = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes1.ClassId // HNC Computing Class A
                };
                userManager.Create(student1, "123");
                userManager.AddToRole(student1.Id, "Student");
            }
            context.SaveChanges();

            // Student for HNC Computing Class B
            if (userManager.FindByName("student2@email.com") == null)
            {
                student2 = new Student
                {
                    UserName = "student2@email.com",
                    Email = "student2@email.com",
                    FirstName = "Emma",
                    LastName = "Brown",
                    Street = "78 Park Avenue",
                    City = "Manchester",
                    Postcode = "M1 5QD",
                    RegistredAt = DateTime.UtcNow.AddMonths(-3),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes2.ClassId // HNC Computing Class B
                };
                userManager.Create(student2, "123");
                userManager.AddToRole(student2.Id, "Student");
            }
            context.SaveChanges();

            // Student for HNC Software Development Class A
            if (userManager.FindByName("student3@email.com") == null)
            {
                student3 = new Student
                {
                    UserName = "student3@email.com",
                    Email = "student3@email.com",
                    FirstName = "James",
                    LastName = "Taylor",
                    Street = "15 Oak Road",
                    City = "Birmingham",
                    Postcode = "B1 8LS",
                    RegistredAt = DateTime.UtcNow.AddMonths(-1),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes3.ClassId // HNC Software Development Class A
                };
                userManager.Create(student3, "123");
                userManager.AddToRole(student3.Id, "Student");
            }
            context.SaveChanges();

            // Student for HNC Software Development Class B
            if (userManager.FindByName("student4@email.com") == null)
            {
                student4 = new Student
                {
                    UserName = "student4@email.com",
                    Email = "student4@email.com",
                    FirstName = "Sophia",
                    LastName = "Garcia",
                    Street = "32 Maple Drive",
                    City = "Glasgow",
                    Postcode = "G1 2RS",
                    RegistredAt = DateTime.UtcNow.AddMonths(-4),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes4.ClassId // HNC Software Development Class B
                };
                userManager.Create(student4, "123");
                userManager.AddToRole(student4.Id, "Student");
            }
            context.SaveChanges();

            // Student for HND Computer Science Class
            if (userManager.FindByName("student5@email.com") == null)
            {
                student5 = new Student
                {
                    UserName = "student5@email.com",
                    Email = "student5@email.com",
                    FirstName = "William",
                    LastName = "Martin",
                    Street = "67 Church Street",
                    City = "Edinburgh",
                    Postcode = "EH1 3QP",
                    RegistredAt = DateTime.UtcNow.AddMonths(-5),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes5.ClassId // HND Computer Science Class
                };
                userManager.Create(student5, "123");
                userManager.AddToRole(student5.Id, "Student");
            }
            context.SaveChanges();

            // Student for HND Software Development Class A
            if (userManager.FindByName("student6@email.com") == null)
            {
                student6 = new Student
                {
                    UserName = "student6@email.com",
                    Email = "student6@email.com",
                    FirstName = "Olivia",
                    LastName = "Anderson",
                    Street = "92 Station Road",
                    City = "Leeds",
                    Postcode = "LS1 4BT",
                    RegistredAt = DateTime.UtcNow.AddMonths(-2),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes6.ClassId // HND Software Development Class A
                };
                userManager.Create(student6, "123");
                userManager.AddToRole(student6.Id, "Student");
            }
            context.SaveChanges();

            // Student for HND Software Development Class B
            if (userManager.FindByName("student7@email.com") == null)
            {
                student7 = new Student
                {
                    UserName = "student7@email.com",
                    Email = "student7@email.com",
                    FirstName = "Benjamin",
                    LastName = "Thomas",
                    Street = "14 Queen Avenue",
                    City = "Liverpool",
                    Postcode = "L1 5TD",
                    RegistredAt = DateTime.UtcNow.AddMonths(-3),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes7.ClassId // HND Software Development Class B
                };
                userManager.Create(student7, "123");
                userManager.AddToRole(student7.Id, "Student");
            }
            context.SaveChanges();

            // Student for NQ Computing Class A
            if (userManager.FindByName("student8@email.com") == null)
            {
                student8 = new Student
                {
                    UserName = "student8@email.com",
                    Email = "student8@email.com",
                    FirstName = "Charlotte",
                    LastName = "Harris",
                    Street = "51 London Road",
                    City = "Cardiff",
                    Postcode = "CF10 2EQ",
                    RegistredAt = DateTime.UtcNow.AddMonths(-1),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes8.ClassId // NQ Computing Class A
                };
                userManager.Create(student8, "123");
                userManager.AddToRole(student8.Id, "Student");
            }
            context.SaveChanges();

            // Student for NQ Computing Class B
            if (userManager.FindByName("student9@email.com") == null)
            {
                student9 = new Student
                {
                    UserName = "student9@email.com",
                    Email = "student9@email.com",
                    FirstName = "Daniel",
                    LastName = "Robinson",
                    Street = "29 York Avenue",
                    City = "Belfast",
                    Postcode = "BT1 6GH",
                    RegistredAt = DateTime.UtcNow.AddMonths(-4),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes9.ClassId // NQ Computing Class B
                };
                userManager.Create(student9, "123");
                userManager.AddToRole(student9.Id, "Student");
            }
            context.SaveChanges();

            if (userManager.FindByName("student10@email.com") == null)
            {
                student10 = new Student
                {
                    UserName = "student10@email.com",
                    Email = "student10@email.com",
                    FirstName = "Dylan",
                    LastName = "Campbell",
                    Street = "57 Station Rd",
                    City = "Glasgow",
                    Postcode = "DD1 3CC",
                    RegistredAt = DateTime.UtcNow.AddMonths(-2),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes1.ClassId,
                    StudentNumber = "42000125"
                };
                userManager.Create(student10, "123");
                userManager.AddToRole(student10.Id, "Student");
            }

            if (userManager.FindByName("student11@email.com") == null)
            {
                student11 = new Student
                {
                    UserName = "student11@email.com",
                    Email = "student11@email.com",
                    FirstName = "Ross",
                    LastName = "Gibb",
                    Street = "34 West End",
                    City = "Aberdeen",
                    Postcode = "FK8 1QZ",
                    RegistredAt = DateTime.UtcNow.AddMonths(-2),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes2.ClassId,
                    StudentNumber = "42000126"
                };
                userManager.Create(student11, "123");
                userManager.AddToRole(student11.Id, "Student");
            }

            if (userManager.FindByName("student12@email.com") == null)
            {
                student12 = new Student
                {
                    UserName = "student12@email.com",
                    Email = "student12@email.com",
                    FirstName = "Garry",
                    LastName = "Grant",
                    Street = "57 Station Rd",
                    City = "Glasgow",
                    Postcode = "DD1 3CC",
                    RegistredAt = DateTime.UtcNow.AddMonths(-2),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes3.ClassId,
                    StudentNumber = "42000127"
                };
                userManager.Create(student12, "123");
                userManager.AddToRole(student12.Id, "Student");
            }

            if (userManager.FindByName("student13@email.com") == null)
            {
                student13 = new Student
                {
                    UserName = "student13@email.com",
                    Email = "student13@email.com",
                    FirstName = "Amy",
                    LastName = "Medineli",
                    Street = "34 West End",
                    City = "Stirling",
                    Postcode = "EH2 2BB",
                    RegistredAt = DateTime.UtcNow.AddMonths(-4),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes4.ClassId,
                    StudentNumber = "42000128"
                };
                userManager.Create(student13, "123");
                userManager.AddToRole(student13.Id, "Student");
            }
            if (userManager.FindByName("student14@email.com") == null)
            {
                student14 = new Student
                {
                    UserName = "student14@email.com",
                    Email = "student14@email.com",
                    FirstName = "Grant",
                    LastName = "Smith",
                    Street = "9 George Sq",
                    City = "Stirling",
                    Postcode = "FK8 1QZ",
                    RegistredAt = DateTime.Parse("2025-03-07T10:40:08"),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes5.ClassId,
                    StudentNumber = "42000129"
                };
                userManager.Create(student14, "123");
                userManager.AddToRole(student14.Id, "Student");
            }

            if (userManager.FindByName("student15@email.com") == null)
            {
                student15 = new Student
                {
                    UserName = "student15@email.com",
                    Email = "student15@email.com",
                    FirstName = "Amy",
                    LastName = "Johnson",
                    Street = "5 Greenhill Ave",
                    City = "Glasgow",
                    Postcode = "AB10 1XY",
                    RegistredAt = DateTime.Parse("2025-02-27T10:40:08"),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes6.ClassId,
                    StudentNumber = "42000130"
                };
                userManager.Create(student15, "123");
                userManager.AddToRole(student15.Id, "Student");
            }

            if (userManager.FindByName("student16@email.com") == null)
            {
                student16 = new Student
                {
                    UserName = "student16@email.com",
                    Email = "student16@email.com",
                    FirstName = "Megan",
                    LastName = "Brown",
                    Street = "73 Oxford St",
                    City = "Edinburgh",
                    Postcode = "DD1 3CC",
                    RegistredAt = DateTime.Parse("2025-02-17T10:40:08"),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes7.ClassId,
                    StudentNumber = "42000131"
                };
                userManager.Create(student16, "123");
                userManager.AddToRole(student16.Id, "Student");
            }

            if (userManager.FindByName("student17@email.com") == null)
            {
                student17 = new Student
                {
                    UserName = "student17@email.com",
                    Email = "student17@email.com",
                    FirstName = "Liam",
                    LastName = "Taylor",
                    Street = "1 Queen St",
                    City = "Edinburgh",
                    Postcode = "FK8 1QZ",
                    RegistredAt = DateTime.Parse("2025-03-08T10:40:08"),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes8.ClassId,
                    StudentNumber = "42000132"
                };
                userManager.Create(student17, "123");
                userManager.AddToRole(student17.Id, "Student");
            }

            if (userManager.FindByName("student18@email.com") == null)
            {
                student18 = new Student
                {
                    UserName = "student18@email.com",
                    Email = "student18@email.com",
                    FirstName = "Ethan",
                    LastName = "Anderson",
                    Street = "42 Main Road",
                    City = "Dundee",
                    Postcode = "FK8 1QZ",
                    RegistredAt = DateTime.Parse("2025-01-25T10:40:08"),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes9.ClassId,
                    StudentNumber = "42000133"
                };
                userManager.Create(student18, "123");
                userManager.AddToRole(student18.Id, "Student");
            }
            if (userManager.FindByName("student19@email.com") == null)
            {
                student19 = new Student
                {
                    UserName = "student19@email.com",
                    Email = "student19@email.com",
                    FirstName = "Ben",
                    LastName = "Atagan",
                    Street = "9 George Sq",
                    City = "Stirling",
                    Postcode = "DD1 3CC",
                    RegistredAt = DateTime.Parse("2025-01-29T10:40:08"),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes1.ClassId,
                    StudentNumber = "42000134"
                };
                userManager.Create(student19, "123");
                userManager.AddToRole(student19.Id, "Student");
            }

            if (userManager.FindByName("student20@email.com") == null)
            {
                student20 = new Student
                {
                    UserName = "student20@email.com",
                    Email = "student20@email.com",
                    FirstName = "Zara",
                    LastName = "Lewis",
                    Street = "9 George Sq",
                    City = "Stirling",
                    Postcode = "AB10 1XY",
                    RegistredAt = DateTime.Parse("2025-01-24T10:40:08"),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes2.ClassId,
                    StudentNumber = "42000135"
                };
                userManager.Create(student20, "123");
                userManager.AddToRole(student20.Id, "Student");
            }

            if (userManager.FindByName("student21@email.com") == null)
            {
                student21 = new Student
                {
                    UserName = "student21@email.com",
                    Email = "student21@email.com",
                    FirstName = "Kyle",
                    LastName = "Walker",
                    Street = "88 Commerce St",
                    City = "Edinburgh",
                    Postcode = "DD1 3CC",
                    RegistredAt = DateTime.Parse("2025-03-08T10:40:08"),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes3.ClassId,
                    StudentNumber = "42000136"
                };
                userManager.Create(student21, "123");
                userManager.AddToRole(student21.Id, "Student");
            }

            if (userManager.FindByName("student22@email.com") == null)
            {
                student22 = new Student
                {
                    UserName = "student22@email.com",
                    Email = "student22@email.com",
                    FirstName = "Ava",
                    LastName = "Hall",
                    Street = "9 George Sq",
                    City = "Aberdeen",
                    Postcode = "AB10 1XY",
                    RegistredAt = DateTime.Parse("2025-04-09T10:40:08"),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes4.ClassId,
                    StudentNumber = "42000137"
                };
                userManager.Create(student22, "123");
                userManager.AddToRole(student22.Id, "Student");
            }

            if (userManager.FindByName("student23@email.com") == null)
            {
                student23 = new Student
                {
                    UserName = "student23@email.com",
                    Email = "student23@email.com",
                    FirstName = "Logan",
                    LastName = "Allen",
                    Street = "9 George Sq",
                    City = "Aberdeen",
                    Postcode = "AB10 1XY",
                    RegistredAt = DateTime.Parse("2025-04-01T10:40:08"),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes5.ClassId,
                    StudentNumber = "42000138"
                };
                userManager.Create(student23, "123");
                userManager.AddToRole(student23.Id, "Student");
            }

            if (userManager.FindByName("student24@email.com") == null)
            {
                student24 = new Student
                {
                    UserName = "student24@email.com",
                    Email = "student24@email.com",
                    FirstName = "Emma",
                    LastName = "Young",
                    Street = "1 Queen St",
                    City = "Stirling",
                    Postcode = "FK8 1QZ",
                    RegistredAt = DateTime.Parse("2025-03-22T10:40:08"),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes6.ClassId,
                    StudentNumber = "42000139"
                };
                userManager.Create(student24, "123");
                userManager.AddToRole(student24.Id, "Student");
            }

            if (userManager.FindByName("student25@email.com") == null)
            {
                student25 = new Student
                {
                    UserName = "student25@email.com",
                    Email = "student25@email.com",
                    FirstName = "Ben",
                    LastName = "Scott",
                    Street = "34 West End",
                    City = "Glasgow",
                    Postcode = "DD1 3CC",
                    RegistredAt = DateTime.Parse("2025-03-16T10:40:08"),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes7.ClassId,
                    StudentNumber = "42000140"
                };  
                userManager.Create(student25, "123");
                userManager.AddToRole(student25.Id, "Student");
            }

            if (userManager.FindByName("student26@email.com") == null)
            {
                student26 = new Student
                {
                    UserName = "student26@email.com",
                    Email = "student26@email.com",
                    FirstName = "Lucy",
                    LastName = "Adams",
                    Street = "34 West End",
                    City = "Aberdeen",
                    Postcode = "AB10 1XY",
                    RegistredAt = DateTime.Parse("2025-01-31T10:40:08"),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes8.ClassId,
                    StudentNumber = "42000141"
                };
                userManager.Create(student26, "123");
                userManager.AddToRole(student26.Id, "Student");
            }

            if (userManager.FindByName("student27@email.com") == null)
            {
                student27 = new Student
                {
                    UserName = "student27@email.com",
                    Email = "student27@email.com",
                    FirstName = "Noah",
                    LastName = "Hughes",
                    Street = "1 Queen St",
                    City = "Stirling",
                    Postcode = "DD1 3CC",
                    RegistredAt = DateTime.Parse("2025-02-17T10:40:08"),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes9.ClassId,
                    StudentNumber = "42000142"
                };
                userManager.Create(student27, "123");
                userManager.AddToRole(student27.Id, "Student");
            }

            if (userManager.FindByName("student28@email.com") == null)
            {
                student28 = new Student
                {
                    UserName = "student28@email.com",
                    Email = "student28@email.com",
                    FirstName = "Mykailo",
                    LastName = "Shavlin",
                    Street = "57 Station Rd",
                    City = "Edinburgh",
                    Postcode = "AB10 1XY",
                    RegistredAt = DateTime.Parse("2025-02-18T10:40:08"),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes1.ClassId,
                    StudentNumber = "42000143"
                };
                userManager.Create(student28, "123");
                userManager.AddToRole(student28.Id, "Student");
            }

            if (userManager.FindByName("student29@email.com") == null)
            {
                student29 = new Student
                {
                    UserName = "student29@email.com",
                    Email = "student29@email.com",
                    FirstName = "Karina",
                    LastName = "Fatkullina",
                    Street = "24 Union St",
                    City = "Edinburgh",
                    Postcode = "DD1 3CC",
                    RegistredAt = DateTime.Parse("2025-04-13T10:40:08"),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes1.ClassId,
                    StudentNumber = "42000144"
                };
                userManager.Create(student29, "123");
                userManager.AddToRole(student29.Id, "Student");
            }

            if (userManager.FindByName("student35@email.com") == null)
            {
                student29 = new Student
                {
                    UserName = "student35@email.com",
                    Email = "student35@email.com",
                    FirstName = "Mark",
                    LastName = "Rossiter",
                    Street = "24 Union St",
                    City = "Edinburgh",
                    Postcode = "DD1 3CC",
                    RegistredAt = DateTime.Parse("2025-04-13T10:40:08"),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes2.ClassId,
                    StudentNumber = "42000144"
                };
                userManager.Create(student29, "123");
                userManager.AddToRole(student29.Id, "Student");
            }


            // Create a Issue
            var issue = new Issue
            {
                IssueTitle = IssueTitle.LateAttendance,
                IssueDescription = "Student has missed multiple classes.",
                IssueStatus = IssueStatus.New,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LecturerId = lecturer1.Id,
                GuidanceTeacherId = guidance.Id,
                StudentId = student1.Id
            };
            context.Issues.Add(issue);
            context.SaveChanges();

            var issue2 = new Issue
            {
                IssueTitle = IssueTitle.MissingAttendance,
                IssueDescription = "Student has failed my class.",
                IssueStatus = IssueStatus.InProgress,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LecturerId = lecturer1.Id,
                GuidanceTeacherId = guidance.Id,
                StudentId = student2.Id
            };
            context.Issues.Add(issue2);
            context.SaveChanges();

            var issue3 = new Issue
            {
                IssueTitle = IssueTitle.Medical,
                IssueDescription = "Student has fractured their arm and is unable to type",
                IssueStatus = IssueStatus.Archived,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LecturerId = lecturer1.Id,
                GuidanceTeacherId = guidance.Id,
                StudentId = student3.Id
            };
            context.Issues.Add(issue3);
            context.SaveChanges();

            var issue4 = new Issue
            {
                IssueTitle = IssueTitle.LateAttendance,
                IssueDescription = "Student has been consistently late to class.",
                IssueStatus = IssueStatus.New,
                CreatedAt = DateTime.UtcNow.AddYears(-3),
                UpdatedAt = DateTime.UtcNow,
                LecturerId = lecturer1.Id,
                GuidanceTeacherId = guidance.Id,
                StudentId = student4.Id
            };
            context.Issues.Add(issue4);
            context.SaveChanges();

            var issue5 = new Issue
            {
                IssueTitle = IssueTitle.MissingAttendance,
                IssueDescription = "Student has missed several classes without valid reasons.",
                IssueStatus = IssueStatus.New,
                CreatedAt = DateTime.UtcNow.AddMonths(-2),
                UpdatedAt = DateTime.UtcNow,
                LecturerId = lecturer2.Id,
                GuidanceTeacherId = guidance.Id,
                StudentId = student5.Id
            };
            context.Issues.Add(issue5);
            context.SaveChanges();

            var issue6 = new Issue
            {
                IssueTitle = IssueTitle.Behaviour,
                IssueDescription = "Student has been disruptive in class.",
                IssueStatus = IssueStatus.InProgress,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow,
                LecturerId = lecturer3.Id,
                GuidanceTeacherId = guidance.Id,
                StudentId = student6.Id
            };
            context.Issues.Add(issue6);
            context.SaveChanges();

            var issue7 = new Issue
            {
                IssueTitle = IssueTitle.Deadlines,
                IssueDescription = "Student has missed multiple assignment deadlines.",
                IssueStatus = IssueStatus.New,
                CreatedAt = DateTime.UtcNow.AddDays(-14),
                UpdatedAt = DateTime.UtcNow,
                LecturerId = lecturer4.Id,
                GuidanceTeacherId = guidance.Id,
                StudentId = student7.Id
            };
            context.Issues.Add(issue7);
            context.SaveChanges();

            var issue8 = new Issue
            {
                IssueTitle = IssueTitle.Communication,
                IssueDescription = "Student has failed to respond to several important communications.",
                IssueStatus = IssueStatus.New,
                CreatedAt = DateTime.UtcNow.AddYears(-2),
                UpdatedAt = DateTime.UtcNow,
                LecturerId = lecturer1.Id,
                GuidanceTeacherId = guidance.Id,
                StudentId = student8.Id
            };
            context.Issues.Add(issue8);
            context.SaveChanges();

            var issue9 = new Issue
            {
                IssueTitle = IssueTitle.Performance,
                IssueDescription = "Student's performance has been below expectations for the semester.",
                IssueStatus = IssueStatus.InProgress,
                CreatedAt = DateTime.UtcNow.AddMonths(-1),
                UpdatedAt = DateTime.UtcNow,
                LecturerId = lecturer2.Id,
                GuidanceTeacherId = guidance.Id,
                StudentId = student9.Id
            };
            context.Issues.Add(issue9);
            context.SaveChanges();

            var issue10 = new Issue
            {
                IssueTitle = IssueTitle.Medical,
                IssueDescription = "Student has been ill and unable to attend classes.",
                IssueStatus = IssueStatus.Archived,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow,
                LecturerId = lecturer3.Id,
                GuidanceTeacherId = guidance.Id,
                StudentId = student1.Id
            };
            context.Issues.Add(issue10);
            context.SaveChanges();

            var issue11 = new Issue
            {
                IssueTitle = IssueTitle.AcademicDishonesty,
                IssueDescription = "Student was caught plagiarizing during the last exam.",
                IssueStatus = IssueStatus.New,
                CreatedAt = DateTime.UtcNow.AddYears(-1),
                UpdatedAt = DateTime.UtcNow,
                LecturerId = lecturer4.Id,
                GuidanceTeacherId = guidance.Id,
                StudentId = student2.Id
            };
            context.Issues.Add(issue11);
            context.SaveChanges();

            var issue12 = new Issue
            {
                IssueTitle = IssueTitle.CustomIssue,
                IssueDescription = "Student has been requesting extensions for every assignment.",
                IssueStatus = IssueStatus.InProgress,
                CreatedAt = DateTime.UtcNow.AddMonths(-4),
                UpdatedAt = DateTime.UtcNow,
                LecturerId = lecturer1.Id,
                GuidanceTeacherId = guidance.Id,
                StudentId = student3.Id
            };
            context.Issues.Add(issue12);
            context.SaveChanges();

            var issue13 = new Issue
            {
                IssueTitle = IssueTitle.LateAttendance,
                IssueDescription = "Student arrives late to class almost every session.",
                IssueStatus = IssueStatus.New,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow,
                LecturerId = lecturer2.Id,
                GuidanceTeacherId = guidance.Id,
                StudentId = student4.Id
            };
            context.Issues.Add(issue13);
            context.SaveChanges();

            var issue14 = new Issue
            {
                IssueTitle = IssueTitle.MissingAttendance,
                IssueDescription = "Student has missed a significant portion of the semester.",
                IssueStatus = IssueStatus.InProgress,
                CreatedAt = DateTime.UtcNow.AddMonths(-3),
                UpdatedAt = DateTime.UtcNow,
                LecturerId = lecturer3.Id,
                GuidanceTeacherId = guidance.Id,
                StudentId = student5.Id
            };
            context.Issues.Add(issue14);
            context.SaveChanges();

            var issue15 = new Issue
            {
                IssueTitle = IssueTitle.Behaviour,
                IssueDescription = "Student has been repeatedly disrespectful to classmates.",
                IssueStatus = IssueStatus.New,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow,
                LecturerId = lecturer4.Id,
                GuidanceTeacherId = guidance.Id,
                StudentId = student6.Id
            };
            context.Issues.Add(issue15);
            context.SaveChanges();

            var issue16 = new Issue
            {
                IssueTitle = IssueTitle.LateAttendance,
                IssueDescription = "Student is consistently 40 minutes late to class.",
                IssueStatus = IssueStatus.New,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow,
                LecturerId = financeLecturer.Id,
                GuidanceTeacherId = newGuidanceTeacher.Id,
                StudentId = financeStudent.Id
            };
            context.Issues.Add(issue16);
            context.SaveChanges();

            var issue17 = new Issue
            {
                IssueTitle = IssueTitle.Behaviour,
                IssueDescription = "Student is consistently talking class.",
                IssueStatus = IssueStatus.New,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow,
                LecturerId = financeLecturer.Id,
                GuidanceTeacherId = newGuidanceTeacher.Id,
                StudentId = financeStudent2.Id
            };
            context.Issues.Add(issue17);
            context.SaveChanges();




            //add appointemnts
            // First appointment for student1
            var session1 = new Appointment
            {
                AppointmentDate = guidanceSession1.Day,
                AppointmentStatus = AppointmentStatus.Scheduled,
                AppointmentNotes = "Initial consultation",
                StudentId = student1.Id,
                GuidanceTeacherId = guidance.Id,
                GuidanceSessionId = 1, // Original value
                Room = guidanceSession1.Room,
                IssueId = null,
                StartTime = guidanceSession1.Time
            };
            context.Appointments.Add(session1);

            // Second appointment for student1 (with issue)
            var session01 = new Appointment
            {
                AppointmentDate = guidanceSession1.Day,
                AppointmentStatus = AppointmentStatus.Scheduled,
                AppointmentNotes = "Initial consultation",
                StudentId = student1.Id,
                GuidanceTeacherId = guidance.Id,
                GuidanceSessionId = 1, // Original value
                Room = guidanceSession1.Room,
                IssueId = issue.IssueId,
                StartTime = guidanceSession1.Time.Add(TimeSpan.FromMinutes(10))
            };
            context.Appointments.Add(session01);

            // Appointment for student2
            var session2 = new Appointment
            {
                AppointmentDate = guidanceSession2.Day,
                AppointmentStatus = AppointmentStatus.Scheduled,
                AppointmentNotes = "Progress review",
                StudentId = student2.Id,
                GuidanceTeacherId = guidance.Id,
                GuidanceSessionId = 2, // Original value
                Room = guidanceSession2.Room,
                IssueId = null,
                StartTime = guidanceSession2.Time
            };
            context.Appointments.Add(session2);

            // Appointment for student3
            var session3 = new Appointment
            {
                AppointmentDate = guidanceSession1.Day,
                AppointmentStatus = AppointmentStatus.Scheduled,
                AppointmentNotes = "Career advice",
                StudentId = student3.Id,
                GuidanceTeacherId = guidance.Id,
                GuidanceSessionId = 1, // Original value
                Room = guidanceSession1.Room,
                IssueId = null,
                StartTime = guidanceSession1.Time.Add(TimeSpan.FromMinutes(20))
            };
            context.Appointments.Add(session3);

            // Appointment for student4
            var session4 = new Appointment
            {
                AppointmentDate = guidanceSession3.Day,
                AppointmentStatus = AppointmentStatus.Requested,
                AppointmentNotes = "Academic support",
                StudentId = student4.Id,
                GuidanceTeacherId = guidance.Id,
                GuidanceSessionId = 3, // Original value
                Room = guidanceSession3.Room,
                IssueId = null,
                StartTime = guidanceSession3.Time.Add(TimeSpan.FromMinutes(10))
            };
            context.Appointments.Add(session4);

            // Appointment for student5
            var session5 = new Appointment
            {
                AppointmentDate = guidanceSession4.Day,
                AppointmentStatus = AppointmentStatus.Scheduled,
                AppointmentNotes = "Personal issues",
                StudentId = student5.Id,
                GuidanceTeacherId = guidance.Id,
                GuidanceSessionId = 4, // Original value
                Room = guidanceSession4.Room,
                IssueId = null,
                StartTime = guidanceSession4.Time
            };
            context.Appointments.Add(session5);

            // Appointment for student6
            var session6 = new Appointment
            {
                AppointmentDate = guidanceSession5.Day,
                AppointmentStatus = AppointmentStatus.Scheduled,
                AppointmentNotes = "Study plan",
                StudentId = student6.Id,
                GuidanceTeacherId = guidance.Id,
                GuidanceSessionId = 5, // Original value
                Room = guidanceSession5.Room,
                IssueId = null,
                StartTime = guidanceSession5.Time.Add(TimeSpan.FromMinutes(20))
            };
            context.Appointments.Add(session6);

            // Appointment for student7
            var session7 = new Appointment
            {
                AppointmentDate = guidanceSession6.Day,
                AppointmentStatus = AppointmentStatus.Cancelled,
                AppointmentNotes = "Exam preparation",
                StudentId = student7.Id,
                GuidanceTeacherId = guidance.Id,
                GuidanceSessionId = 6, // Original value
                Room = guidanceSession6.Room,
                IssueId = null,
                StartTime = guidanceSession6.Time.Add(TimeSpan.FromMinutes(30))
            };
            context.Appointments.Add(session7);

            // Appointment for student8
            var session8 = new Appointment
            {
                AppointmentDate = guidanceSession7.Day, // Using Day from guidanceSession7 to match GuidanceSessionId
                AppointmentStatus = AppointmentStatus.Scheduled,
                AppointmentNotes = "Course selection",
                StudentId = student8.Id,
                GuidanceTeacherId = guidance.Id,
                GuidanceSessionId = 7, // Original value
                Room = guidanceSession7.Room,
                IssueId = null,
                StartTime = guidanceSession7.Time.Add(TimeSpan.FromMinutes(10))
            };
            context.Appointments.Add(session8);

            // Appointment for student9
            var session9 = new Appointment
            {
                AppointmentDate = guidanceSession8.Day,
                AppointmentStatus = AppointmentStatus.Requested,
                AppointmentNotes = "Internship advice",
                StudentId = student9.Id,
                GuidanceTeacherId = guidance.Id,
                GuidanceSessionId = 8, // Original value
                Room = guidanceSession8.Room,
                IssueId = null,
                StartTime = guidanceSession8.Time.Add(TimeSpan.FromMinutes(20))
            };
            context.Appointments.Add(session9);

            // Another appointment for student9
            var session10 = new Appointment
            {
                AppointmentDate = guidanceSession1.Day,
                AppointmentStatus = AppointmentStatus.Scheduled,
                AppointmentNotes = "General support",
                StudentId = student9.Id,
                GuidanceTeacherId = guidance.Id,
                GuidanceSessionId = 1, // Original value
                Room = guidanceSession1.Room,
                IssueId = null,
                StartTime = guidanceSession1.Time.Add(TimeSpan.FromMinutes(30))
            };
            context.Appointments.Add(session10);

            // Requested appointment for student3
            var requested1 = new Appointment
            {
                AppointmentDate = guidanceSession3.Day,
                AppointmentStatus = AppointmentStatus.Requested,
                AppointmentNotes = "Requesting help with essay writing.",
                StudentId = student3.Id,
                GuidanceTeacherId = guidance.Id,
                GuidanceSessionId = 3, // Original value
                Room = guidanceSession3.Room,
                IssueId = null,
                StartTime = guidanceSession3.Time.Add(TimeSpan.FromMinutes(20))
            };
            context.Appointments.Add(requested1);

            // Another requested appointment for student3
            var requested2 = new Appointment
            {
                AppointmentDate = guidanceSession3.Day,
                AppointmentStatus = AppointmentStatus.Requested,
                AppointmentNotes = "need help with calculus.",
                StudentId = student3.Id,
                GuidanceTeacherId = guidance.Id,
                GuidanceSessionId = 3, // Original value
                Room = guidanceSession3.Room,
                IssueId = null,
                StartTime = guidanceSession3.Time.Add(TimeSpan.FromMinutes(30))
            };
            context.Appointments.Add(requested2);

            // Another requested appointment for student3
            var requested3 = new Appointment
            {
                AppointmentDate = guidanceSession3.Day,
                AppointmentStatus = AppointmentStatus.Requested,
                AppointmentNotes = "need help with coding.",
                StudentId = student3.Id,
                GuidanceTeacherId = guidance.Id,
                GuidanceSessionId = 3, // Original value
                Room = guidanceSession3.Room,
                IssueId = null,
                StartTime = guidanceSession3.Time.Add(TimeSpan.FromMinutes(40))
            };
            context.Appointments.Add(requested3);

            context.SaveChanges();

            // Global posts
            var Post1 = new Post
            {
                PostId = Guid.NewGuid().ToString(),
                Title = "Welcome to the Guidance Tracker System",
                Content = "This is a global announcement for all users. The new Guidance Tracker System is now live! Please explore the features and let us know if you have any feedback.",
                PostDate = DateTime.UtcNow.AddDays(-10),
                AuthorId = curriculumHead1.Id,
                Visibility = VisibilityType.Global
            };
            context.Posts.Add(Post1);

            var post2 = new Post
            {
                PostId = Guid.NewGuid().ToString(),
                Title = "Welcome to the Guidance Tracker System",
                Content = "This is a global announcement for all users. The new Guidance Tracker System is now live! Please explore the features and let us know if you have any feedback.",
                PostDate = DateTime.UtcNow.AddDays(-10),
                AuthorId = curriculumHead1.Id,
                Visibility = VisibilityType.Global
            };
            context.Posts.Add(post2);

            var Post3 = new Post

            {
                PostId = Guid.NewGuid().ToString(),
                Title = "System Maintenance Notice",
                Content = "The system will be undergoing maintenance this weekend from Saturday 22:00 to Sunday 02:00. Please save your work before this time to avoid any data loss.",
                PostDate = DateTime.UtcNow.AddDays(-5),
                AuthorId = curriculumHead1.Id,
                Visibility = VisibilityType.Global
            };
            context.Posts.Add(Post3);

            // Student-only posts
            var Post4 = new Post
            {
                PostId = Guid.NewGuid().ToString(),
                Title = "Student Council Meeting",
                Content = "The next Student Council meeting will be held on Friday at 15:00 in Room 302. All class representatives are required to attend. The agenda includes discussion on upcoming events and academic support services.",
                PostDate = DateTime.UtcNow.AddDays(-3),
                AuthorId = guidance.Id,
                Visibility = VisibilityType.Student
            };
            context.Posts.Add(Post4);
            var Post5 = new Post
            {
                PostId = Guid.NewGuid().ToString(),
                Title = "Study Resources Now Available",
                Content = "New study resources for all courses have been uploaded to the student portal. These include practice exams, study guides, and video tutorials. Access them through your student dashboard.",
                PostDate = DateTime.UtcNow.AddDays(-2),
                AuthorId = lecturer1.Id,
                Visibility = VisibilityType.Student
            };
            context.Posts.Add(Post5);

            // Lecturer-only posts
            var Post6 = new Post
            {
                PostId = Guid.NewGuid().ToString(),
                Title = "Staff Meeting Reminder",
                Content = "This is a reminder that we have our monthly staff meeting tomorrow at 14:00 in the conference room. Please prepare your department updates and bring any questions or concerns you'd like to discuss.",
                PostDate = DateTime.UtcNow.AddDays(-1),
                AuthorId = curriculumHead1.Id,
                Visibility = VisibilityType.Staff
            };
            context.Posts.Add(Post6);

            var Post7 = new Post
            {
                PostId = Guid.NewGuid().ToString(),
                Title = "Grading Policy Update",
                Content = "Please note that there have been updates to the grading policy for the current semester. All assignments must now include detailed feedback, and final grades must be submitted within 10 days of the assessment date. See the attached document for more details.",
                PostDate = DateTime.UtcNow.AddHours(-12),
                AuthorId = guidance.Id,
                Visibility = VisibilityType.Staff
            };
            context.Posts.Add(Post7);
            try
            {
                context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                StringBuilder errorMessage = new StringBuilder("Entity Validation Failed - Errors: ");

                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    errorMessage.AppendLine($"\nEntity of type '{validationErrors.Entry.Entity.GetType().Name}' in state '{validationErrors.Entry.State}' has the following validation errors:");

                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        var propertyValue = validationErrors.Entry.CurrentValues[validationError.PropertyName];
                        errorMessage.AppendLine($"- Property: '{validationError.PropertyName}', Value: '{propertyValue}', Error: '{validationError.ErrorMessage}'");
                    }
                }

                // Output to debug window
                System.Diagnostics.Debug.WriteLine(errorMessage.ToString());

                // If you're using logging

                // You might want to throw a more informative exception or handle it differently
                throw new Exception($"Validation failed: {errorMessage.ToString()}", ex);
            }

            //var feedbackRequest1 = new RequestedDetailedForm
            //{
            //    RequestId = Guid.NewGuid().ToString(),
            //    ClassId = classes1.ClassId,
            //    StudentId = student1.Id,
            //    CreatorId = guidance.Id,
            //    DateRequested = DateTime.UtcNow,

            //};
            //var feedbackRequest2 = new RequestedDetailedForm
            //{
            //    RequestId = Guid.NewGuid().ToString(),
            //    ClassId = classes1.ClassId,
            //    StudentId = student11.Id,
            //    CreatorId = guidance.Id,
            //    DateRequested = DateTime.UtcNow,

            //};
            //var feedbackRequest3 = new RequestedDetailedForm
            //{
            //    RequestId = Guid.NewGuid().ToString(),
            //    ClassId = classes1.ClassId,
            //    StudentId = student20.Id,
            //    CreatorId = guidance.Id,
            //    DateRequested = DateTime.UtcNow,
            //};

            //var feedbackRequest4 = new RequestedDetailedForm
            //{
            //    RequestId = Guid.NewGuid().ToString(),
            //    ClassId = classes1.ClassId,
            //    StudentId = student29.Id,
            //    CreatorId = guidance.Id,
            //    DateRequested = DateTime.UtcNow,
            //};
            //context.RequestedDetailedForms.Add(feedbackRequest1);
            //context.RequestedDetailedForms.Add(feedbackRequest2);
            //context.RequestedDetailedForms.Add(feedbackRequest3);
            //context.RequestedDetailedForms.Add(feedbackRequest4);
            //context.SaveChanges();



            //var detailedFeedback1 = new DetailedFeedback
            //{
            //    FeedbackId = Guid.NewGuid().ToString(),
            //    Course = courses1.CourseName,
            //    Class = classes1.ClassName,
            //    ClassId = classes1.ClassId,
            //    IsSubmitted = true,
            //    IsReadByClassRep = false,
            //    StudentId = student1.Id,
            //    CreatorId = guidance.Id,
            //    ClassRepId = student1.Id,
            //    MeetsExpectations = true,
            //    MeetsExpectationsNotes = null,
            //    WouldRecommend = true,
            //    WouldRecommendNotes = null,
            //    WorkloadManageable = false,
            //    WorkloadManageableNotes = "The workload was quite heavy, especially during assessment periods.",
            //    LearningExperienceKeyIssues = "Some lectures could be more engaging",
            //    LearningExperienceStrengths = "The course content was very relevant and useful.",
            //    LearningExperienceImprovements = "More interactive sessions would be beneficial.",
            //    LearningExperienceComments = "Overall, a good learning experience with room for improvement.",
            //    LearningExperienceRating = 4,
            //    ConceptsPresented = true,
            //    ConceptsPresentedNotes = null,
            //    MaterialsAvailable = true,
            //    MaterialsAvailableNotes = null,
            //    AccommodatesStyles = false,
            //    AccommodatesStylesNotes = "The course was primarily lecture-based and didn't cater well to visual or kinesthetic learners.",
            //    LecturerResponsive = true,
            //    LecturerResponsiveNotes = null,
            //    LearningTeachingKeyIssues = "Limited variety in teaching methods",
            //    LearningTeachingStrengths = "The teaching methods were clear and structured.",
            //    LearningTeachingImprovements = "More group work and interactive activities could enhance learning.",
            //    LearningTeachingComments = "The teaching was good but could be more diverse.",
            //    LearningTeachingRating = 4,
            //    AssessmentConfidence = true,
            //    AssessmentConfidenceNotes = null,
            //    TimelyFeedback = false,
            //    TimelyFeedbackNotes = "Feedback was often delayed by 2-3 weeks, which impacted learning progression.",
            //    SpecificFeedback = true,
            //    SpecificFeedbackNotes = null,
            //    AssessmentsAligned = true,
            //    AssessmentsAlignedNotes = null,
            //    SufficientTime = false,
            //    SufficientTimeNotes = "The final assessment period was rushed with multiple deadlines overlapping.",
            //    AssessmentKeyIssues = "Timing of assessments could be better spaced",
            //    AssessmentStrengths = "The assessments were fair and relevant.",
            //    AssessmentImprovements = "More practice assessments and better scheduling would be helpful.",
            //    AssessmentComments = "Overall, the assessment process was decent but needs timing improvements.",
            //    AssessmentRating = 3,
            //    MaterialsAccessible = true,
            //    MaterialsAccessibleNotes = null,
            //    PlatformOrganised = true,
            //    PlatformOrganisedNotes = null,
            //    EquipmentWorking = false,
            //    EquipmentWorkingNotes = "Some lab equipment was outdated and frequently malfunctioned during practical sessions.",
            //    SupplementaryResources = true,
            //    SupplementaryResourcesNotes = null,
            //    SpecializedEquipment = false,
            //    SpecializedEquipmentNotes = "Some specialized equipment was not available when needed for advanced projects.",
            //    LibraryResources = true,
            //    LibraryResourcesNotes = null,
            //    ResourcesKeyIssues = "Equipment reliability and availability",
            //    ResourcesStrengths = "The digital resources and library materials were excellent.",
            //    ResourcesImprovements = "Equipment maintenance and specialized tool availability need improvement.",
            //    ResourcesComments = "Good digital resources but physical equipment needs attention.",
            //    ResourcesRating = 4,
            //    StaffResponsive = true,
            //    StaffResponsiveNotes = null,
            //    AdditionalHelpAvailable = true,
            //    AdditionalHelpAvailableNotes = null,
            //    AccommodationsProvided = true,
            //    AccommodationsProvidedNotes = null,
            //    ClearPointsOfContact = false,
            //    ClearPointsOfContactNotes = "It was sometimes unclear who to contact for specific issues, leading to delays in getting help.",
            //    SupportEffectivenesssKeyIssues = "Communication clarity could be improved",
            //    SupportEffectivenessStrengths = "Support services were helpful when accessed.",
            //    SupportEffectivenessImprovements = "Clearer communication channels and contact information would be beneficial.",
            //    SupportEffectivenessComments = "Good support services but communication could be clearer.",
            //    SupportEffectivenessRating = 4,
            //    DevelopingCriticalThinking = true,
            //    DevelopingCriticalThinkingNotes = null,
            //    EnhancingProblemSolving = true,
            //    EnhancingProblemSolvingNotes = null,
            //    GainingPracticalSkills = false,
            //    GainingPracticalSkillsNotes = "The course was too theoretical and lacked sufficient hands-on practice opportunities.",
            //    ImprovingCommunication = true,
            //    ImprovingCommunicationNotes = null,
            //    DevelopingResearchSkills = true,
            //    DevelopingResearchSkillsNotes = null,
            //    SkillsDevelopmentKeyIssues = "Lack of practical application opportunities",
            //    SkillsDevelopmentStrengths = "The course was very effective in developing analytical and research skills.",
            //    SkillsDevelopmentImprovements = "More practical workshops and real-world applications would be beneficial.",
            //    SkillsDevelopmentComments = "Strong theoretical skills development but needs more practical focus.",
            //    SkillsDevelopmentRating = 4,
            //    BestFeatures = "The course content was comprehensive and the lecturer was knowledgeable and approachable.",
            //    AreasForImprovement = "Better equipment maintenance, more practical sessions, and improved assessment timing.",
            //    OverallRating = 4
            //};
            //context.DetailedFeedbacks.Add(detailedFeedback1);
            //context.SaveChanges();

            //var detailedfeedback2 = new DetailedFeedback
            //{
            //    FeedbackId = Guid.NewGuid().ToString(),
            //    Course = courses1.CourseName,
            //    Class = classes1.ClassName,
            //    ClassId = classes1.ClassId,
            //    IsSubmitted = true,
            //    IsReadByClassRep = false,
            //    StudentId = student11.Id,
            //    CreatorId = guidance.Id,
            //    ClassRepId = student1.Id,
            //    MeetsExpectations = false,
            //    MeetsExpectationsNotes = "The course content was more basic than expected and didn't cover advanced topics mentioned in the syllabus.",
            //    WouldRecommend = false,
            //    WouldRecommendNotes = "I wouldn't recommend this course due to outdated content and poor organization.",
            //    WorkloadManageable = true,
            //    WorkloadManageableNotes = null,
            //    LearningExperienceKeyIssues = "Outdated curriculum and lack of industry relevance",
            //    LearningExperienceStrengths = "The basic concepts were explained clearly.",
            //    LearningExperienceImprovements = "Update curriculum to include current industry practices and technologies.",
            //    LearningExperienceComments = "The course needs significant updates to remain relevant.",
            //    LearningExperienceRating = 2,
            //    ConceptsPresented = false,
            //    ConceptsPresentedNotes = "Some key concepts were glossed over or not covered adequately.",
            //    MaterialsAvailable = false,
            //    MaterialsAvailableNotes = "Course materials were often unavailable or provided late in the semester.",
            //    AccommodatesStyles = true,
            //    AccommodatesStylesNotes = null,
            //    LecturerResponsive = false,
            //    LecturerResponsiveNotes = "The lecturer was often unavailable and slow to respond to emails and questions.",
            //    LearningTeachingKeyIssues = "Poor communication and outdated teaching methods",
            //    LearningTeachingStrengths = "The lecturer had good subject knowledge.",
            //    LearningTeachingImprovements = "Improve communication and modernize teaching approaches.",
            //    LearningTeachingComments = "Teaching methods need significant improvement.",
            //    LearningTeachingRating = 2,
            //    AssessmentConfidence = false,
            //    AssessmentConfidenceNotes = "Assessment criteria were unclear and expectations were not well communicated.",
            //    TimelyFeedback = false,
            //    TimelyFeedbackNotes = "Feedback was extremely delayed, sometimes taking over a month to receive.",
            //    SpecificFeedback = false,
            //    SpecificFeedbackNotes = "Feedback was vague and didn't provide clear guidance for improvement.",
            //    AssessmentsAligned = false,
            //    AssessmentsAlignedNotes = "Assessments often tested material not covered in class or emphasized in lectures.",
            //    SufficientTime = true,
            //    SufficientTimeNotes = null,
            //    AssessmentKeyIssues = "Poor alignment with course content and unclear criteria",
            //    AssessmentStrengths = "Assessment topics were relevant when they aligned with course content.",
            //    AssessmentImprovements = "Better alignment with course content and clearer assessment criteria needed.",
            //    AssessmentComments = "Assessment process needs major improvements.",
            //    AssessmentRating = 2,
            //    MaterialsAccessible = true,
            //    MaterialsAccessibleNotes = null,
            //    PlatformOrganised = false,
            //    PlatformOrganisedNotes = "The online platform was poorly organized with files scattered across different sections.",
            //    EquipmentWorking = true,
            //    EquipmentWorkingNotes = null,
            //    SupplementaryResources = false,
            //    SupplementaryResourcesNotes = "Very limited supplementary resources were provided, making independent study difficult.",
            //    SpecializedEquipment = true,
            //    SpecializedEquipmentNotes = null,
            //    LibraryResources = true,
            //    LibraryResourcesNotes = null,
            //    ResourcesKeyIssues = "Poor platform organization and limited supplementary materials",
            //    ResourcesStrengths = "Library resources were adequate.",
            //    ResourcesImprovements = "Better platform organization and more supplementary resources needed.",
            //    ResourcesComments = "Resources were adequate but poorly organized.",
            //    ResourcesRating = 3,
            //    StaffResponsive = false,
            //    StaffResponsiveNotes = "Staff were often unresponsive and difficult to reach when help was needed.",
            //    AdditionalHelpAvailable = false,
            //    AdditionalHelpAvailableNotes = "Additional help was rarely available and hard to access when needed.",
            //    AccommodationsProvided = true,
            //    AccommodationsProvidedNotes = null,
            //    ClearPointsOfContact = true,
            //    ClearPointsOfContactNotes = null,
            //    SupportEffectivenesssKeyIssues = "Staff availability and responsiveness",
            //    SupportEffectivenessStrengths = "Contact information was clearly provided.",
            //    SupportEffectivenessImprovements = "Staff need to be more responsive and available for student support.",
            //    SupportEffectivenessComments = "Support services were inadequate.",
            //    SupportEffectivenessRating = 2,
            //    DevelopingCriticalThinking = false,
            //    DevelopingCriticalThinkingNotes = "The course was too focused on memorization rather than developing critical thinking skills.",
            //    EnhancingProblemSolving = false,
            //    EnhancingProblemSolvingNotes = "Limited opportunities for problem-solving exercises and practical application.",
            //    GainingPracticalSkills = false,
            //    GainingPracticalSkillsNotes = "The course was entirely theoretical with no practical components.",
            //    ImprovingCommunication = true,
            //    ImprovingCommunicationNotes = null,
            //    DevelopingResearchSkills = false,
            //    DevelopingResearchSkillsNotes = "Research components were minimal and not well-integrated into the curriculum.",
            //    SkillsDevelopmentKeyIssues = "Lack of practical application and skill development opportunities",
            //    SkillsDevelopmentStrengths = "Some improvement in written communication skills.",
            //    SkillsDevelopmentImprovements = "Include more practical exercises and skill development opportunities.",
            //    SkillsDevelopmentComments = "Skills development was inadequate.",
            //    SkillsDevelopmentRating = 2,
            //    BestFeatures = "The course schedule was flexible and accommodated different learning needs.",
            //    AreasForImprovement = "Complete curriculum overhaul needed, better staff training, and improved resource organization.",
            //    OverallRating = 2
            //};
            //context.DetailedFeedbacks.Add(detailedfeedback2);
            //context.SaveChanges();

            //var detailedFeedback3 = new DetailedFeedback
            //{
            //    FeedbackId = Guid.NewGuid().ToString(),
            //    Course = courses1.CourseName,
            //    Class = classes1.ClassName,
            //    IsSubmitted = true,
            //    IsReadByClassRep = false,
            //    ClassId = classes1.ClassId,
            //    StudentId = student20.Id,
            //    CreatorId = guidance.Id,
            //    ClassRepId = student1.Id,
            //    MeetsExpectations = true,
            //    MeetsExpectationsNotes = null,
            //    WouldRecommend = true,
            //    WouldRecommendNotes = null,
            //    WorkloadManageable = true,
            //    WorkloadManageableNotes = null,
            //    LearningExperienceKeyIssues = "None",
            //    LearningExperienceStrengths = "Excellent balance of theory and practice with engaging delivery.",
            //    LearningExperienceImprovements = "Perhaps more guest speakers from industry would add value.",
            //    LearningExperienceComments = "Outstanding learning experience that exceeded expectations.",
            //    LearningExperienceRating = 5,
            //    ConceptsPresented = true,
            //    ConceptsPresentedNotes = null,
            //    MaterialsAvailable = true,
            //    MaterialsAvailableNotes = null,
            //    AccommodatesStyles = true,
            //    AccommodatesStylesNotes = null,
            //    LecturerResponsive = true,
            //    LecturerResponsiveNotes = null,
            //    LearningTeachingKeyIssues = "None",
            //    LearningTeachingStrengths = "Innovative teaching methods and excellent use of technology.",
            //    LearningTeachingImprovements = "Already excellent, minor improvements could include more peer learning opportunities.",
            //    LearningTeachingComments = "Exemplary teaching that sets the standard for other courses.",
            //    LearningTeachingRating = 5,
            //    AssessmentConfidence = true,
            //    AssessmentConfidenceNotes = null,
            //    TimelyFeedback = true,
            //    TimelyFeedbackNotes = null,
            //    SpecificFeedback = true,
            //    SpecificFeedbackNotes = null,
            //    AssessmentsAligned = true,
            //    AssessmentsAlignedNotes = null,
            //    SufficientTime = true,
            //    SufficientTimeNotes = null,
            //    AssessmentKeyIssues = "None",
            //    AssessmentStrengths = "Perfectly aligned assessments with clear criteria and excellent feedback.",
            //    AssessmentImprovements = "Assessment process is already excellent.",
            //    AssessmentComments = "Outstanding assessment process that enhanced learning.",
            //    AssessmentRating = 5,
            //    MaterialsAccessible = true,
            //    MaterialsAccessibleNotes = null,
            //    PlatformOrganised = true,
            //    PlatformOrganisedNotes = null,
            //    EquipmentWorking = true,
            //    EquipmentWorkingNotes = null,
            //    SupplementaryResources = true,
            //    SupplementaryResourcesNotes = null,
            //    SpecializedEquipment = true,
            //    SpecializedEquipmentNotes = null,
            //    LibraryResources = true,
            //    LibraryResourcesNotes = null,
            //    ResourcesKeyIssues = "None",
            //    ResourcesStrengths = "Comprehensive and well-organized resources that enhanced learning.",
            //    ResourcesImprovements = "Resources are already excellent.",
            //    ResourcesComments = "Outstanding resource provision and organization.",
            //    ResourcesRating = 5,
            //    StaffResponsive = true,
            //    StaffResponsiveNotes = null,
            //    AdditionalHelpAvailable = true,
            //    AdditionalHelpAvailableNotes = null,
            //    AccommodationsProvided = true,
            //    AccommodationsProvidedNotes = null,
            //    ClearPointsOfContact = true,
            //    ClearPointsOfContactNotes = null,
            //    SupportEffectivenesssKeyIssues = "None",
            //    SupportEffectivenessStrengths = "Proactive and highly effective support services.",
            //    SupportEffectivenessImprovements = "Support services are already excellent.",
            //    SupportEffectivenessComments = "Exemplary support that went above and beyond expectations.",
            //    SupportEffectivenessRating = 5,
            //    DevelopingCriticalThinking = true,
            //    DevelopingCriticalThinkingNotes = null,
            //    EnhancingProblemSolving = true,
            //    EnhancingProblemSolvingNotes = null,
            //    GainingPracticalSkills = true,
            //    GainingPracticalSkillsNotes = null,
            //    ImprovingCommunication = true,
            //    ImprovingCommunicationNotes = null,
            //    DevelopingResearchSkills = true,
            //    DevelopingResearchSkillsNotes = null,
            //    SkillsDevelopmentKeyIssues = "None",
            //    SkillsDevelopmentStrengths = "Comprehensive skills development across all areas.",
            //    SkillsDevelopmentImprovements = "Skills development is already excellent.",
            //    SkillsDevelopmentComments = "Outstanding skills development that prepared me well for future challenges.",
            //    SkillsDevelopmentRating = 5,
            //    BestFeatures = "Exceptional teaching quality, comprehensive resources, and outstanding support services.",
            //    AreasForImprovement = "Course is already excellent - perhaps more industry guest speakers could add value.",
            //    OverallRating = 5
            //};
            //context.DetailedFeedbacks.Add(detailedFeedback3);
            //context.SaveChanges();

            //var detailedFeedback4 = new DetailedFeedback
            //{
            //    FeedbackId = Guid.NewGuid().ToString(),
            //    Course = courses1.CourseName,
            //    Class = classes1.ClassName,
            //    IsSubmitted = true,
            //    IsReadByClassRep = false,
            //    ClassId = classes1.ClassId,
            //    StudentId = student29.Id,
            //    CreatorId = guidance.Id,
            //    ClassRepId = student1.Id,
            //    MeetsExpectations = true,
            //    MeetsExpectationsNotes = null,
            //    WouldRecommend = true,
            //    WouldRecommendNotes = null,
            //    WorkloadManageable = false,
            //    WorkloadManageableNotes = "The workload was challenging but manageable with good time management.",
            //    LearningExperienceKeyIssues = "Some topics could have been covered in more depth",
            //    LearningExperienceStrengths = "Good variety of learning activities and practical applications.",
            //    LearningExperienceImprovements = "More time allocated to complex topics would be beneficial.",
            //    LearningExperienceComments = "Solid learning experience with good practical elements.",
            //    LearningExperienceRating = 1,
            //    ConceptsPresented = true,
            //    ConceptsPresentedNotes = null,
            //    MaterialsAvailable = true,
            //    MaterialsAvailableNotes = null,
            //    AccommodatesStyles = true,
            //    AccommodatesStylesNotes = null,
            //    LecturerResponsive = true,
            //    LecturerResponsiveNotes = null,
            //    LearningTeachingKeyIssues = "Pace could be adjusted for complex topics",
            //    LearningTeachingStrengths = "Clear explanations and good use of examples.",
            //    LearningTeachingImprovements = "Slower pace for difficult concepts would help understanding.",
            //    LearningTeachingComments = "Good teaching with room for pacing improvements.",
            //    LearningTeachingRating = 1,
            //    AssessmentConfidence = true,
            //    AssessmentConfidenceNotes = null,
            //    TimelyFeedback = true,
            //    TimelyFeedbackNotes = null,
            //    SpecificFeedback = false,
            //    SpecificFeedbackNotes = "Feedback was helpful but could have been more detailed for complex assignments.",
            //    AssessmentsAligned = true,
            //    AssessmentsAlignedNotes = null,
            //    SufficientTime = true,
            //    SufficientTimeNotes = null,
            //    AssessmentKeyIssues = "Feedback detail could be improved",
            //    AssessmentStrengths = "Fair assessments that tested understanding well.",
            //    AssessmentImprovements = "More detailed feedback would help with learning progression.",
            //    AssessmentComments = "Good assessment process with room for feedback improvement.",
            //    AssessmentRating = 1,
            //    MaterialsAccessible = true,
            //    MaterialsAccessibleNotes = null,
            //    PlatformOrganised = true,
            //    PlatformOrganisedNotes = null,
            //    EquipmentWorking = true,
            //    EquipmentWorkingNotes = null,
            //    SupplementaryResources = false,
            //    SupplementaryResourcesNotes = "Additional resources for advanced topics would have been helpful.",
            //    SpecializedEquipment = true,
            //    SpecializedEquipmentNotes = null,
            //    LibraryResources = true,
            //    LibraryResourcesNotes = null,
            //    ResourcesKeyIssues = "Limited advanced supplementary materials",
            //    ResourcesStrengths = "Good basic resources and well-organized platform.",
            //    ResourcesImprovements = "More advanced supplementary resources would be beneficial.",
            //    ResourcesComments = "Good resource provision with room for expansion.",
            //    ResourcesRating = 1,
            //    StaffResponsive = true,
            //    StaffResponsiveNotes = null,
            //    AdditionalHelpAvailable = true,
            //    AdditionalHelpAvailableNotes = null,
            //    AccommodationsProvided = true,
            //    AccommodationsProvidedNotes = null,
            //    ClearPointsOfContact = true,
            //    ClearPointsOfContactNotes = null,
            //    SupportEffectivenesssKeyIssues = "None",
            //    SupportEffectivenessStrengths = "Responsive and helpful support services.",
            //    SupportEffectivenessImprovements = "Support services are working well.",
            //    SupportEffectivenessComments = "Excellent support services that were always available when needed.",
            //    SupportEffectivenessRating = 5,
            //    DevelopingCriticalThinking = true,
            //    DevelopingCriticalThinkingNotes = null,
            //    EnhancingProblemSolving = true,
            //    EnhancingProblemSolvingNotes = null,
            //    GainingPracticalSkills = true,
            //    GainingPracticalSkillsNotes = null,
            //    ImprovingCommunication = false,
            //    ImprovingCommunicationNotes = "Limited opportunities for presentation and communication skill development.",
            //    DevelopingResearchSkills = true,
            //    DevelopingResearchSkillsNotes = null,
            //    SkillsDevelopmentKeyIssues = "Limited communication skill development opportunities",
            //    SkillsDevelopmentStrengths = "Good development of analytical and practical skills.",
            //    SkillsDevelopmentImprovements = "More opportunities for presentation and communication skill development.",
            //    SkillsDevelopmentComments = "Strong skills development with room for communication improvements.",
            //    SkillsDevelopmentRating = 4,
            //    BestFeatures = "Good balance of theory and practice with responsive support services.",
            //    AreasForImprovement = "Better pacing for complex topics and more communication skill development opportunities.",
            //    OverallRating = 4
            //};
            //context.DetailedFeedbacks.Add(detailedFeedback4);
            //context.SaveChanges();




            // Seed Conversations
            var conversation1 = new Conversation
            {
                UserOneId = lecturer1.Id,
                UserTwoId = guidance.Id,
                LastUpdated = DateTime.UtcNow
            };

            var conversation2 = new Conversation
            {
                UserOneId = lecturer2.Id,
                UserTwoId = guidance2.Id,
                LastUpdated = DateTime.UtcNow
            };

            var conversation3 = new Conversation
            {
                UserOneId = student1.Id,
                UserTwoId = guidance.Id,
                LastUpdated = DateTime.UtcNow
            };

            var conversation4 = new Conversation
            {
                UserOneId = financeStudent.Id,
                UserTwoId = newGuidanceTeacher.Id,
                LastUpdated = DateTime.UtcNow
            };

            context.Conversations.Add(conversation1);
            context.Conversations.Add(conversation2);
            context.Conversations.Add(conversation3);
            context.Conversations.Add(conversation4);
            context.SaveChanges();

            // Seed Messages
            var messages = new List<Message>
{
    new Message
    {
        ConversationId = conversation1.Id,
        SenderId = lecturer1.Id,
        ReceiverId = guidance.Id,
        Content = "Hi, can we discuss David’s attendance?",
        SentAt = DateTime.UtcNow.AddMinutes(-25),
        IsRead = false
    },
    new Message
    {
        ConversationId = conversation1.Id,
        SenderId = guidance.Id,
        ReceiverId = lecturer1.Id,
        Content = "Sure, I'll pull up his records now.",
        SentAt = DateTime.UtcNow.AddMinutes(-20),
        IsRead = false
    },
    new Message
    {
        ConversationId = conversation3.Id,
        SenderId = student1.Id,
        ReceiverId = guidance.Id,
        Content = "Hi, I need help with my schedule.",
        SentAt = DateTime.UtcNow.AddHours(-2),
        IsRead = false
    },
    new Message
    {
        ConversationId = conversation3.Id,
        SenderId = guidance.Id,
        ReceiverId = student1.Id,
        Content = "Let’s book an appointment to go through it.",
        SentAt = DateTime.UtcNow.AddHours(-1),
        IsRead = false
    }
};

            context.Messages.AddRange(messages);
            context.SaveChanges();


        }

    }
}
       
