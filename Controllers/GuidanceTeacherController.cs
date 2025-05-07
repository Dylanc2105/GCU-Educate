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

            // counts the posts that don't have a row in the PostRead table for the current user
            var visiblePosts = PostVisibilityHelper.GetVisiblePosts(userId, db, User);

            var newAnnouncementsCount = visiblePosts
                .Where(p => !db.PostReads.Any(pr => pr.PostId == p.PostId && pr.UserId == userId))
                .Count();

            var model = new GuidanceDashViewModel
            {
                FirstName = user.FirstName,
                NewIssuesCount = db.Issues.Where(i => i.IssueStatus == IssueStatus.New && i.GuidanceTeacherId == userId).Count(),
                //NewMessagesCount = db.Messages.Where(n => n.IsRead == false).Count(),
                AppointmentsTodayCount = db.Appointments.Where(a => DbFunctions.TruncateTime(a.AppointmentDate) == today).Count(),
                NewAnnouncementsCount = newAnnouncementsCount


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
                Where(a => a.AppointmentDate.Day == date.Day).
                Where(a => a.AppointmentDate.Month == date.Month).
                Where(a => a.AppointmentDate.Year == date.Year).
                ToList();


            return PartialView("_AppointmentsForDay", appointments);
        }

        //partial view with appiontments for current week
        public ActionResult AppointmentsForWeekPartial()
        {


            string guidanceTeacherId = User.Identity.GetUserId();

            DateTime baseDate = DateTime.Now;
            var thisWeekStart = baseDate.AddDays(-(int)baseDate.DayOfWeek);
            var thisWeekEnd = thisWeekStart.AddDays(7).AddSeconds(-1);

            var appointments = db.Appointments.
                Include(a => a.Student).
                Include(a => a.GuidanceTeacher).
                Where(a => a.GuidanceTeacher.Id == guidanceTeacherId).
                Where(a => a.AppointmentDate > thisWeekStart).
                Where(a => a.AppointmentDate < thisWeekEnd).
                OrderBy(a => a.AppointmentDate).ToList();


            return PartialView("_AppointmentsForWeek", appointments);
        }


        public ActionResult StudentDetails(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Student student = db.Students.Find(id);
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            // Count current issues
            var currentIssues = db.Issues
                .Where(i => i.StudentId == id)
                .Count();

            // Count archived issues
            var archivedIssues = db.ArchivedTickets
                .Include(at => at.Issue)
                .Where(at => at.Issue.StudentId == id)
                .Count();

            // Count appointments by status
            var scheduledAppointments = db.Appointments
                .Where(a => a.StudentId == id &&
                       (a.AppointmentStatus == AppointmentStatus.Scheduled ||
                       a.AppointmentStatus == AppointmentStatus.Requested ||
                       a.AppointmentStatus == AppointmentStatus.Rescheduled))
                .Count();

            var completedAppointments = db.Appointments
                .Where(a => a.StudentId == id && a.AppointmentStatus == AppointmentStatus.Completed)
                .Count();

            var cancelledAppointments = db.Appointments
                .Where(a => a.StudentId == id && a.AppointmentStatus == AppointmentStatus.Cancelled)
                .Count();

            var totalAppointments = db.Appointments
                .Where(a => a.StudentId == id)
                .Count();

            // Set ViewBag properties
            ViewBag.CurrentIssueCount = currentIssues;
            ViewBag.ArchivedIssueCount = archivedIssues;
            ViewBag.TotalIssueCount = currentIssues + archivedIssues;

            ViewBag.ScheduledAppointments = scheduledAppointments;
            ViewBag.CompletedAppointments = completedAppointments;
            ViewBag.CancelledAppointments = cancelledAppointments;
            ViewBag.TotalAppointments = totalAppointments;

            var model = new Student
            {
                StudentNumber = student.StudentNumber,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Street = student.Street,
                City = student.City,
                Postcode = student.Postcode,
                PhoneNumber = student.PhoneNumber,
                Email = student.Email,
                Class = student.Class,
                IsClassRep = student.IsClassRep,
                IsDeputyClassRep = student.IsDeputyClassRep,
                GuidanceTeacher = student.GuidanceTeacher,
            };

            return View(model);
        }

        public ActionResult ElectStudentRep(string id)
        {
            Student student = db.Students.Find(id);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            if (student.IsClassRep == false)
            {
                student.IsClassRep = true;
                db.Entry(student).State = EntityState.Modified;
                db.SaveChanges();
            }
            var otherStudents = db.Students.Where(s => s.ClassId == student.ClassId && s.Id != student.Id);
            foreach (var otherStudent in otherStudents)
            {
                otherStudent.IsClassRep = false;
                db.Entry(otherStudent).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("ViewAllStudents", "GuidanceTeacher");

        }

        public ActionResult ElectDeputyStudentRep(string id)
        {
            Student student = db.Students.Find(id);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            if (student.IsDeputyClassRep == false)
            {
                student.IsDeputyClassRep = true;
                db.Entry(student).State = EntityState.Modified;
                db.SaveChanges();
            }
            var otherStudents = db.Students.Where(s => s.ClassId == student.ClassId && s.Id != student.Id);
            foreach (var otherStudent in otherStudents)
            {
                otherStudent.IsDeputyClassRep = false;
                db.Entry(otherStudent).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("ViewAllStudents", "GuidanceTeacher");
        }
    }
}