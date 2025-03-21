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

        // GET: GuidanceTeacher Dashboard
        public ActionResult GuidanceTeacherDash()
        {
            var teacherId = User.Identity.GetUserId();

            var issues = db.Issues
                .Include("Student")
                .Include("Lecturer")
                .Where(i => i.GuidanceTeacherId == teacherId)
                .OrderByDescending(i => i.CreatedAt)
                .ToList();

            return View("GuidanceTeacherDash", issues);
        }

        // View all students
        public ActionResult ViewAllStudents()
        {
            var students = db.Students.OrderBy(s => s.RegistredAt).ToList();
            return View(students);
        }

        // Create a new student (GET)
        public ActionResult CreateStudent()
        {
            CreateStudentViewModel student = new CreateStudentViewModel
            {
                Classes = db.Classes
                    .Select(c => new SelectListItem
                    {
                        Value = c.ClassId.ToString(),
                        Text = c.ClassName
                    })
                    .ToList()
            };
            return View(student);
        }

        // Create a new student (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateStudent(CreateStudentViewModel model)
        {
            if (ModelState.IsValid)
            {
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
                    ClassId = model.ClassId
                };

                var result = UserManager.Create(newStudent, model.Password);
                if (result.Succeeded)
                {
                    UserManager.AddToRole(newStudent.Id, "Student");
                    return RedirectToAction("ViewAllStudents", "GuidanceTeacher");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
            }

            // Repopulate the Classes list before returning the view
            model.Classes = db.Classes
                .Select(c => new SelectListItem
                {
                    Value = c.ClassId.ToString(),
                    Text = c.ClassName
                })
                .ToList();

            return View(model);
        }
    }
}