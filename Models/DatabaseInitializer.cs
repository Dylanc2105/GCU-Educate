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
            GuidanceTeacher newGuidanceTeacher = null;
            CurriculumHead curriculumHead1 = null;
            Lecturer lecturer1 = null;
            Lecturer lecturer2 = null;
            Lecturer lecturer3 = null;
            Lecturer lecturer4 = null;
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
                //create an admin employee

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

            if (userManager.FindByName("newGuidance@email.com") == null)
            {
                newGuidanceTeacher = new GuidanceTeacher
                {
                    UserName = "guidance1@email.com",
                    Email = "guidance1@email.com",
                    FirstName = "james",
                    LastName = "dog",
                    Street = "35 Washington st",
                    City = "London",
                    Postcode = "E12 8UP",
                    RegistredAt = DateTime.Now.AddYears(-5),
                    EmailConfirmed = true,
                };

                //add admin to users table
                userManager.Create(newGuidanceTeacher, "123");
                //assign it to the guidance teacher role
                userManager.AddToRole(newGuidanceTeacher.Id, "GuidanceTeacher");
            }

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
            context.SaveChanges();

            if (userManager.FindByName("CurriculumHead@email.com") == null)
            {
                curriculumHead1 = new CurriculumHead
                {
                    UserName = "CurriculumHead@email.com",
                    Email = "CurriculumHead@email.com",
                    FirstName = "John",
                    LastName = "Johnson",
                    Street = "25 glasgow road",
                    City = "Glasgow",
                    Postcode = "G31 29H",
                    RegistredAt = DateTime.Now.AddYears(-3),
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
                    RegistredAt = DateTime.Now.AddYears(-2),
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
            context.SaveChanges();

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
            context.SaveChanges();

            if (userManager.FindByName("lecturer4@email.com") == null)
            {
                lecturer4 = new Lecturer
                {
                    UserName = "lecturer4@email.com",
                    Email = "lecturer4@email.com",
                    FirstName = "David",
                    LastName = "Brown",
                    Street = "27 King Street",
                    City = "Manchester",
                    Postcode = "M2 6LE",
                    RegistredAt = DateTime.Now.AddYears(-2),
                    EmailConfirmed = true,
                };
                userManager.Create(lecturer4, "123");
                userManager.AddToRole(lecturer4.Id, "Lecturer");
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
                    RegistredAt = DateTime.Now.AddYears(-3),
                    EmailConfirmed = true,
                };
                //add jeff to users table
                userManager.Create(financeLecturer, "123");
                //assign it to the manager role
                userManager.AddToRole(financeLecturer.Id, "Lecturer");
            }
            context.SaveChanges();

            // Create a new class for finance students
            var financeClass = new Class
            {
                ClassId = 10, // Make sure this ID doesn't conflict with existing ones
                ClassName = "HNC Finance Class A",
                MaxCapacity = 20,
                GuidanceTeacherId = newGuidanceTeacher.Id
            };
            context.Classes.Add(financeClass);
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

            // Create courses

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
                LecturerId = lecturer1.Id,
                Classes = new List<Class> { classes1, classes2 } // Link to HNC Computing Class A and B
            };
            context.Units.Add(troubleshootingUnit);
            context.SaveChanges();

            var teamWorkingUnit = new Unit
            {
                UnitName = "Team Working in Computing",
                UnitDescription = "Collaborative work in computing projects",
                LecturerId = lecturer2.Id,
                Classes = new List<Class> { classes1, classes2 } // Link to HNC Computing Class A and B
            };
            context.Units.Add(teamWorkingUnit);
            context.SaveChanges();

            var ethicsUnit = new Unit
            {
                UnitName = "Professionalism and Ethics in Computing",
                UnitDescription = "Ethical considerations in computing industry",
                LecturerId = lecturer3.Id,
                Classes = new List<Class> { classes1, classes2 } // Link to HNC Computing Class A and B
            };
            context.Units.Add(ethicsUnit);
            context.SaveChanges();

            var bigDataUnit = new Unit
            {
                UnitName = "Big Data",
                UnitDescription = "Introduction to big data concepts and technologies",
                LecturerId = lecturer4.Id,
                Classes = new List<Class> { classes1, classes2 } // Link to HNC Computing Class A and B
            };
            context.Units.Add(bigDataUnit);
            context.SaveChanges();

            var webDevUnit = new Unit
            {
                UnitName = "Software Development: Developing Websites",
                UnitDescription = "Web development fundamentals",
                LecturerId = lecturer1.Id,
                Classes = new List<Class> { classes1, classes2 } // Link to HNC Computing Class A and B
            };
            context.Units.Add(webDevUnit);
            context.SaveChanges();

            var gradedUnit1 = new Unit
            {
                UnitName = "Computing: Graded Unit 1",
                UnitDescription = "Integration of knowledge across the HNC",
                LecturerId = lecturer2.Id,
                Classes = new List<Class> { classes1, classes2 } // Link to HNC Computing Class A and B
            };
            context.Units.Add(gradedUnit1);
            context.SaveChanges();

            var dataScienceUnit = new Unit
            {
                UnitName = "Data Science",
                UnitDescription = "Introduction to data science principles and practices",
                LecturerId = lecturer3.Id,
                Classes = new List<Class> { classes1, classes2 } // Link to HNC Computing Class A and B

            };
            context.Units.Add(dataScienceUnit);
            context.SaveChanges();

            var statisticsUnit = new Unit
            {
                UnitName = "Statistics for Science 1",
                UnitDescription = "Statistical methods for scientific applications",
                LecturerId = lecturer4.Id,
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
                LecturerId = lecturer1.Id,
                Classes = new List<Class> { classes5 } // Link to HND Computer Science Class
            };
            context.Units.Add(computerScienceUnit);
            context.SaveChanges();

            var rdbmsUnit = new Unit
            {
                UnitName = "Relational Database Management Systems",
                UnitDescription = "Advanced database design and management",
                LecturerId = lecturer2.Id,
                Classes = new List<Class> { classes5 } // Link to HND Computer Science Class
            };
            context.Units.Add(rdbmsUnit);
            context.SaveChanges();

            var oopUnit = new Unit
            {
                UnitName = "Software Development: Object Oriented Programming",
                UnitDescription = "OOP techniques and patterns",
                LecturerId = lecturer3.Id,
                Classes = new List<Class> { classes5 } // Link to HND Computer Science Class
            };
            context.Units.Add(oopUnit);
            context.SaveChanges();

            var ooadUnit = new Unit
            {
                UnitName = "Systems Development: Object Oriented Analysis & Design",
                UnitDescription = "OO analysis methodologies",
                LecturerId = lecturer4.Id,
                Classes = new List<Class> { classes5 } // Link to HND Computer Science Class
            };
            context.Units.Add(ooadUnit);
            context.SaveChanges();

            var dataStructuresUnit = new Unit
            {
                UnitName = "Software Development: Data Structures",
                UnitDescription = "Advanced data structures and algorithms",
                LecturerId = lecturer1.Id,
                Classes = new List<Class> { classes5 } // Link to HND Computer Science Class
            };
            context.Units.Add(dataStructuresUnit);
            context.SaveChanges();

            var devApplicationsUnit = new Unit
            {
                UnitName = "Software Development: Developing Applications",
                UnitDescription = "Enterprise application development",
                LecturerId = lecturer2.Id,
                Classes = new List<Class> { classes5 } // Link to HND Computer Science Class
            };
            context.Units.Add(devApplicationsUnit);
            context.SaveChanges();

            var webServerUnit = new Unit
            {
                UnitName = "Managing a Web Server",
                UnitDescription = "Web server administration and optimization",
                LecturerId = lecturer3.Id,
                Classes = new List<Class> { classes5 } // Link to HND Computer Science Class
            };
            context.Units.Add(webServerUnit);
            context.SaveChanges();

            var gradedUnit2 = new Unit
            {
                UnitName = "Computer Science: Graded Unit 2",
                UnitDescription = "Project-based assessment for HND",
                LecturerId = lecturer4.Id,
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
                LecturerId = lecturer1.Id,
                Classes = new List<Class> { classes6, classes7 } // Link to HND Software Development Class A and B
            };
            context.Units.Add(softwareDevGradedUnit);
            context.SaveChanges();

            // 5. NQ Computing units
            var introToProgrammingUnit = new Unit
            {
                UnitName = "Introduction to Computer Programming",
                UnitDescription = "Fundamentals of programming",
                LecturerId = lecturer2.Id,
                Classes = new List<Class> { classes8, classes9 } // Link to NQ Computing Class A
            };
            context.Units.Add(introToProgrammingUnit);
            context.SaveChanges();

            var digitalMediaUnit = new Unit
            {
                UnitName = "Computing: Digital Media Elements",
                UnitDescription = "Working with digital media",
                LecturerId = lecturer3.Id,
                Classes = new List<Class> { classes8, classes9 } // Link to NQ Computing Class A
            };
            context.Units.Add(digitalMediaUnit);
            context.SaveChanges();

            var compSysArchUnit = new Unit
            {
                UnitName = "Computer Systems Architecture",
                UnitDescription = "Understanding computer hardware and systems",
                LecturerId = lecturer4.Id,
                Classes = new List<Class> { classes8, classes9 } // Link to NQ Computing Class A
            };
            context.Units.Add(compSysArchUnit);
            context.SaveChanges();

            var networkingUnit = new Unit
            {
                UnitName = "Computing: Networking Technologies",
                UnitDescription = "Introduction to computer networks",
                LecturerId = lecturer1.Id,
                Classes = new List<Class> { classes8, classes9 } // Link to NQ Computing Class A
            };
            context.Units.Add(networkingUnit);
            context.SaveChanges();

            var desktopTroubleshootUnit = new Unit
            {
                UnitName = "Computing: Troubleshoot Desktop Problems",
                UnitDescription = "Basic computer troubleshooting",
                LecturerId = lecturer2.Id,
                Classes = new List<Class> { classes8, classes9 } // Link to NQ Computing Class A
            };
            context.Units.Add(desktopTroubleshootUnit);
            context.SaveChanges();

            var dataSecurityUnit = new Unit
            {
                UnitName = "Data Security",
                UnitDescription = "Introduction to cybersecurity",
                LecturerId = lecturer3.Id,
                Classes = new List<Class> { classes8, classes9 } // Link to NQ Computing Class A

            };
            context.Units.Add(dataSecurityUnit);
            context.SaveChanges();

            var numeracyUnit = new Unit
            {
                UnitName = "Numeracy",
                UnitDescription = "Essential math for computing",
                LecturerId = lecturer4.Id,
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
                LecturerId = lecturer1.Id,
                Classes = new List<Class> { classes8, classes9 } // Link to NQ Computing Class A
            };
            context.Units.Add(appDevUnit);
            context.SaveChanges();

            var structuredMethodsUnit = new Unit
            {
                UnitName = "Develop Software Using Structured Methods",
                UnitDescription = "Structured programming approaches",
                LecturerId = lecturer2.Id,
                Classes = new List<Class> { classes8, classes9 } // Link to NQ Computing Class A
            };
            context.Units.Add(structuredMethodsUnit);
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
                    RegistredAt = DateTime.Now.AddMonths(-1),
                    EmailConfirmed = true,
                    GuidanceTeacherId = newGuidanceTeacher.Id,
                    ClassId = financeClass.ClassId,
                    StudentNumber = "42000123"
                };
                userManager.Create(financeStudent, "123");
                userManager.AddToRole(financeStudent.Id, "Student");
            }
            context.SaveChanges();

            // Create a second finance student
            if (userManager.FindByName("finstudent2@email.com") == null)
            {
                var financeStudent2 = new Student
                {
                    UserName = "finstudent2@email.com",
                    Email = "finstudent2@email.com",
                    FirstName = "Thomas",
                    LastName = "Morgan",
                    Street = "18 Commerce Road",
                    City = "Edinburgh",
                    Postcode = "EH6 5LT",
                    RegistredAt = DateTime.Now.AddMonths(-1),
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
                    FirstName = "David",
                    LastName = "Wilson",
                    Street = "45 High Street",
                    City = "London",
                    Postcode = "E18 2XP",
                    RegistredAt = DateTime.Now.AddMonths(-2),
                    EmailConfirmed = true,
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
                    RegistredAt = DateTime.Now.AddMonths(-3),
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
                    RegistredAt = DateTime.Now.AddMonths(-1),
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
                    RegistredAt = DateTime.Now.AddMonths(-4),
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
                    RegistredAt = DateTime.Now.AddMonths(-5),
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
                    RegistredAt = DateTime.Now.AddMonths(-2),
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
                    RegistredAt = DateTime.Now.AddMonths(-3),
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
                    RegistredAt = DateTime.Now.AddMonths(-1),
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
                    RegistredAt = DateTime.Now.AddMonths(-4),
                    EmailConfirmed = true,
                    GuidanceTeacherId = guidance.Id,
                    ClassId = classes9.ClassId // NQ Computing Class B
                };
                userManager.Create(student9, "123");
                userManager.AddToRole(student9.Id, "Student");
            }
            context.SaveChanges();



            // Create a Issue
            var issue = new Issue
            {
                IssueTitle = IssueTitle.LateAttendance,
                IssueDescription = "Student has missed multiple classes.",
                IssueStatus = IssueStatus.New,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
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
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
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
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
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
                CreatedAt = DateTime.Now.AddYears(-3),
                UpdatedAt = DateTime.Now,
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
                CreatedAt = DateTime.Now.AddMonths(-2),
                UpdatedAt = DateTime.Now,
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
                CreatedAt = DateTime.Now.AddDays(-7),
                UpdatedAt = DateTime.Now,
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
                CreatedAt = DateTime.Now.AddDays(-14),
                UpdatedAt = DateTime.Now,
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
                CreatedAt = DateTime.Now.AddYears(-2),
                UpdatedAt = DateTime.Now,
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
                CreatedAt = DateTime.Now.AddMonths(-1),
                UpdatedAt = DateTime.Now,
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
                CreatedAt = DateTime.Now.AddDays(-5),
                UpdatedAt = DateTime.Now,
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
                CreatedAt = DateTime.Now.AddYears(-1),
                UpdatedAt = DateTime.Now,
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
                CreatedAt = DateTime.Now.AddMonths(-4),
                UpdatedAt = DateTime.Now,
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
                CreatedAt = DateTime.Now.AddDays(-30),
                UpdatedAt = DateTime.Now,
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
                CreatedAt = DateTime.Now.AddMonths(-3),
                UpdatedAt = DateTime.Now,
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
                CreatedAt = DateTime.Now.AddDays(-10),
                UpdatedAt = DateTime.Now,
                LecturerId = lecturer4.Id,
                GuidanceTeacherId = guidance.Id,
                StudentId = student6.Id
            };
            context.Issues.Add(issue15);
            context.SaveChanges();


            // Create appointments
            var session1 = new Appointment
            {
                AppointmentDate = DateTime.Now.AddDays(1),
                AppointmentStatus = AppointmentStatus.Scheduled,
                AppointmentNotes = "Initial consultation",
                Room = "05.005",
                StudentId = student1.Id,
                GuidanceTeacherId = guidance.Id
            };
            context.Appointments.Add(session1);

            var session2 = new Appointment
            {
                AppointmentDate = DateTime.Now.AddDays(3),
                AppointmentStatus = AppointmentStatus.Scheduled,
                AppointmentNotes = "Progress review",
                Room = "05.002",
                StudentId = student2.Id,
                GuidanceTeacherId = guidance.Id
            };
            context.Appointments.Add(session2);

            var session3 = new Appointment
            {
                AppointmentDate = DateTime.Now.AddDays(2),
                AppointmentStatus = AppointmentStatus.Scheduled,
                AppointmentNotes = "Career advice",
                Room = "05.008",
                StudentId = student3.Id,
                GuidanceTeacherId = guidance.Id
            };
            context.Appointments.Add(session3);

            var session4 = new Appointment
            {
                AppointmentDate = DateTime.Now.AddDays(4),
                AppointmentStatus = AppointmentStatus.Requested,
                AppointmentNotes = "Academic support",
                Room = "05.010",
                StudentId = student4.Id,
                GuidanceTeacherId = guidance.Id
            };
            context.Appointments.Add(session4);

            var session5 = new Appointment
            {
                AppointmentDate = DateTime.Now.AddDays(1),
                AppointmentStatus = AppointmentStatus.Scheduled,
                AppointmentNotes = "Personal issues",
                Room = "05.001",
                StudentId = student5.Id,
                GuidanceTeacherId = guidance.Id
            };
            context.Appointments.Add(session5);

            var session6 = new Appointment
            {
                AppointmentDate = DateTime.Now.AddDays(5),
                AppointmentStatus = AppointmentStatus.Scheduled,
                AppointmentNotes = "Study plan",
                Room = "05.003",
                StudentId = student6.Id,
                GuidanceTeacherId = guidance.Id
            };
            context.Appointments.Add(session6);

            var session7 = new Appointment
            {
                AppointmentDate = DateTime.Now.AddDays(3),
                AppointmentStatus = AppointmentStatus.Cancelled,
                AppointmentNotes = "Exam preparation",
                Room = "05.006",
                StudentId = student7.Id,
                GuidanceTeacherId = guidance.Id
            };
            context.Appointments.Add(session7);

            var session8 = new Appointment
            {
                AppointmentDate = DateTime.Now.AddDays(2),
                AppointmentStatus = AppointmentStatus.Scheduled,
                AppointmentNotes = "Course selection",
                Room = "05.009",
                StudentId = student8.Id,
                GuidanceTeacherId = guidance.Id
            };
            context.Appointments.Add(session8);

            var session9 = new Appointment
            {
                AppointmentDate = DateTime.Now.AddDays(4),
                AppointmentStatus = AppointmentStatus.Requested,
                AppointmentNotes = "Internship advice",
                Room = "05.004",
                StudentId = student9.Id,
                GuidanceTeacherId = guidance.Id
            };
            context.Appointments.Add(session9);

            var session10 = new Appointment
            {
                AppointmentDate = DateTime.Now.AddDays(5),
                AppointmentStatus = AppointmentStatus.Scheduled,
                AppointmentNotes = "General support",
                Room = "05.007",
                StudentId = student9.Id,
                GuidanceTeacherId = guidance.Id
            };
            context.Appointments.Add(session10);
            context.SaveChanges();

            // Global posts
            var Post1 = new Post
            {
                PostId = Guid.NewGuid().ToString(),
                Title = "Welcome to the Guidance Tracker System",
                Content = "This is a global announcement for all users. The new Guidance Tracker System is now live! Please explore the features and let us know if you have any feedback.",
                PostDate = DateTime.Now.AddDays(-10),
                AuthorId = curriculumHead1.Id,
                Visibility = VisibilityType.Global
            };
            context.Posts.Add(Post1);

            var post2 = new Post
            {
                PostId = Guid.NewGuid().ToString(),
                Title = "Welcome to the Guidance Tracker System",
                Content = "This is a global announcement for all users. The new Guidance Tracker System is now live! Please explore the features and let us know if you have any feedback.",
                PostDate = DateTime.Now.AddDays(-10),
                AuthorId = curriculumHead1.Id,
                Visibility = VisibilityType.Global
            };
            context.Posts.Add(post2);

            var Post3 = new Post

            {
                PostId = Guid.NewGuid().ToString(),
                Title = "System Maintenance Notice",
                Content = "The system will be undergoing maintenance this weekend from Saturday 22:00 to Sunday 02:00. Please save your work before this time to avoid any data loss.",
                PostDate = DateTime.Now.AddDays(-5),
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
                PostDate = DateTime.Now.AddDays(-3),
                AuthorId = guidance.Id,
                Visibility = VisibilityType.Student
            };
            context.Posts.Add(Post4);
            var Post5 = new Post
            {
                PostId = Guid.NewGuid().ToString(),
                Title = "Study Resources Now Available",
                Content = "New study resources for all courses have been uploaded to the student portal. These include practice exams, study guides, and video tutorials. Access them through your student dashboard.",
                PostDate = DateTime.Now.AddDays(-2),
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
                PostDate = DateTime.Now.AddDays(-1),
                AuthorId = curriculumHead1.Id,
                Visibility = VisibilityType.Staff
            };
            context.Posts.Add(Post6);

            var Post7 = new Post
            {
                PostId = Guid.NewGuid().ToString(),
                Title = "Grading Policy Update",
                Content = "Please note that there have been updates to the grading policy for the current semester. All assignments must now include detailed feedback, and final grades must be submitted within 10 days of the assessment date. See the attached document for more details.",
                PostDate = DateTime.Now.AddHours(-12),
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

        }
    }
}
       
