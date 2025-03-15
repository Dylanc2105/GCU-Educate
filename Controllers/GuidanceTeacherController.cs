using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using GuidanceTracker.Models.ViewModels;
using GuidanceTracker.Models;
using System.Security.Cryptography.X509Certificates;

namespace GuidanceTracker.Controllers
{
    [Authorize(Roles = "GuidanceTeacher")]
    public class GuidanceTeacherController : AccountController
    {
        private GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();
        public GuidanceTeacherController() : base()
        {

        }

        public GuidanceTeacherController(ApplicationUserManager userManager, ApplicationSignInManager signInManager) :
            base(userManager, signInManager)
        {

        }

        // GET: GuidanceTeacher
        public ActionResult GuidanceTeacherDash()
        {
            var tickets = db.Tickets
                .Include("Student")
                .Include("Lecturer")
                .OrderByDescending(t => t.CreatedAt)
                .ToList();

            return View("GuidanceTeacherDash", tickets); 
        }

        public ActionResult ViewAllStudents()
        {
            var students = db.Students.OrderBy(s => s.RegistredAt).ToList();

            return View(students);
        }

        //2: CREATE A NEW Student
        //***************************************************************************************

        public ActionResult CreateStudent()
        {
            CreateStudentViewModel student = new CreateStudentViewModel();
            // Get all courses from the database and convert to SelectListItems
            student.Courses = db.Courses
                .Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(), // Use CourseId as the value
                    Text = c.CourseName
                })
                .ToList();
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateStudent(CreateStudentViewModel model)
        {
            if (ModelState.IsValid)
            {
                //build the student
                Student newStudent = new Student
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Street = model.Street,
                    City = model.City,
                    Postcode = model.Postcode,
                    PhoneNumberConfirmed = true,
                    EmailConfirmed = model.EmailConfirm,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    RegistredAt = DateTime.Now,
                    CourseId = model.CourseId // Set the CourseId here
                };

                //create user, and store in the database and pass the password to be hashed
                var result = UserManager.Create(newStudent, model.Password);
                //if user was stored in the database successfully
                if (result.Succeeded)
                {
                    //then add user to the role selected
                    UserManager.AddToRole(newStudent.Id, "Student");

                    return RedirectToAction("ViewAllStudents", "GuidanceTeacher");
                }
                else
                {
                    // Add identity errors to ModelState
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
            }
            //something is wrong so go back to the create student view
            return View(model);



        }
    }
}