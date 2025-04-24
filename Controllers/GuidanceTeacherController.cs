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
using static GuidanceTracker.Controllers.PostController;

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
            var userId = User.Identity.GetUserId();
            var user = db.GuidanceTeachers.Find(userId);
            var today = DateTime.Today;

            // Get unread announcements
            var visiblePosts = PostVisibilityHelper.GetVisiblePosts(userId, db, User);

            var newAnnouncementsCount = visiblePosts
                .Where(p => !db.PostReads.Any(pr => pr.PostId == p.PostId && pr.UserId == userId))
                .Count();

            // COUNT UNREAD MESSAGES FOR THIS USER
            var unreadMessagesCount = db.Messages
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .Count();

            var model = new GuidanceDashViewModel
            {
                FirstName = user.FirstName,
                NewIssuesCount = db.Issues.Where(i => i.IssueStatus == IssueStatus.New && i.GuidanceTeacherId == userId).Count(),
                AppointmentsTodayCount = db.Appointments.Where(a => DbFunctions.TruncateTime(a.AppointmentDate) == today).Count(),
                NewAnnouncementsCount = newAnnouncementsCount,
                UnreadMessagesCount = unreadMessagesCount 
            };

            return View(model);
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

        public ActionResult Calendar() 
        {
            return View();
        }



 

        //partial view with appointments for cselected date
        public ActionResult GetAppointmentsForDatePartial(DateTime date)
        {
            string guidanceTeacherId = User.Identity.GetUserId();

            var appointments = db.Appointments.
                Include(a => a.Student).
                Include(a => a.GuidanceTeacher).
                Where(a => a.GuidanceTeacher.Id == guidanceTeacherId).
                Where(a=>a.AppointmentDate.Day == date.Day).
                Where(a => a.AppointmentDate.Month == date.Month).
                Where(a => a.AppointmentDate.Year == date.Year).
                ToList();


            return PartialView("_AppointmentsForDay", appointments);
        }

        //partial view with appiontments for current week
        public ActionResult AppointmentsForWeekPartial()
        {


            string guidanceTeacherId =  User.Identity.GetUserId();

            DateTime baseDate = DateTime.Now;
            var thisWeekStart = baseDate.AddDays(-(int)baseDate.DayOfWeek);
            var thisWeekEnd = thisWeekStart.AddDays(7).AddSeconds(-1);

            var appointments = db.Appointments.
                Include(a => a.Student).
                Include(a => a.GuidanceTeacher).
                Where(a => a.GuidanceTeacher.Id == guidanceTeacherId).
                Where(a => a.AppointmentDate > thisWeekStart).
                Where(a => a.AppointmentDate < thisWeekEnd).
                OrderBy(a=>a.AppointmentDate).ToList();


            return PartialView("_AppointmentsForWeek", appointments);
        }

    }
}