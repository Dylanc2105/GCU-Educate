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
    [Authorize(Roles = "GuidanceTeacher, CurriculumHead")]
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

            //count appointments with requested status for this user
            var appointmentsToBeApprovedCount = db.Appointments
                .Where(a => a.AppointmentStatus == AppointmentStatus.Requested && a.GuidanceTeacherId == userId)
                .Count();

            //count guidance sessions for this week for this user
            var guidanceSessionsForWeekCount = db.GuidanceSessions
                .Where(g => g.Class.GuidanceTeacherId == userId)
                .Count();

            var model = new GuidanceDashViewModel
            {
                FirstName = user.FirstName,
                NewIssuesCount = db.Issues.Where(i => i.IssueStatus == IssueStatus.New && i.GuidanceTeacherId == userId).Count(),
                AppointmentsTodayCount = db.Appointments.Where(a => DbFunctions.TruncateTime(a.AppointmentDate) == today && a.GuidanceTeacherId == userId).Count(),
                NewAnnouncementsCount = newAnnouncementsCount,
                UnreadMessagesCount = unreadMessagesCount,
                NewFeedbackCount = db.SimpleFeedbacks
                    .Where(fb => fb.IsReadByGuidanceTeacher == false && fb.GuidanceTeacherId == userId)
                    .Count(),
                AppointmentsToBeApprovedCount = appointmentsToBeApprovedCount,
                GuidanceSessionsForWeekCount = guidanceSessionsForWeekCount
            };

            ViewBag.Message = TempData["Message"] as string; // Retrieve the message from TempData

            return View(model);
        }

        //dashboard for appointemnts
        public ActionResult AppointmentsDash()
        {
            AppointmentsDashViewModel model = new AppointmentsDashViewModel();
            model.AppointmentsToBeApprovedCount = db.Appointments
                .Where(a => a.AppointmentStatus == AppointmentStatus.Requested).Count();

            model.AppointmentsTodayCount = db.Appointments
    .Where(a => a.AppointmentDate.Year == DateTime.Today.Year &&
                a.AppointmentDate.Month == DateTime.Today.Month &&
                a.AppointmentDate.Day == DateTime.Today.Day)
    .Count();

            string guidanceTeacherId = User.Identity.GetUserId();

            DateTime baseDate = DateTime.UtcNow;
            var thisWeekStart = baseDate.AddDays(-(int)baseDate.DayOfWeek);
            var thisWeekEnd = thisWeekStart.AddDays(7).AddSeconds(-1);

            model.AppointmentsForWeekCount = db.Appointments.
                Include(a => a.Student).
                Include(a => a.GuidanceTeacher).
                Where(a => a.GuidanceTeacher.Id == guidanceTeacherId).
                Where(a => a.AppointmentDate > thisWeekStart).
                Where(a => a.AppointmentDate < thisWeekEnd).
                OrderBy(a => a.AppointmentDate).Count();
            

            return View(model);
        }

        public ActionResult CalendarForDashPartial()
        {
            return PartialView("_CalendarPartial");
        }


        // View all students
        [Authorize (Roles = "GuidanceTeacher")]
        public ActionResult ViewAllStudents()
        {
            var students = db.Students.OrderBy(s => s.RegistredAt).ToList();
            return View(students);
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
        public ActionResult AppointmentsForWeek()
        {
            string guidanceTeacherId = User.Identity.GetUserId();

            DateTime baseDate = DateTime.UtcNow;
            var thisWeekStart = baseDate.AddDays(-(int)baseDate.DayOfWeek);
            var thisWeekEnd = thisWeekStart.AddDays(7).AddSeconds(-1);

            var appointments = db.Appointments.
                Include(a => a.Student).
                Include(a => a.GuidanceTeacher).
                Where(a => a.GuidanceTeacher.Id == guidanceTeacherId).
                Where(a => a.AppointmentDate > thisWeekStart).
                Where(a => a.AppointmentDate < thisWeekEnd).
                OrderBy(a => a.AppointmentDate).ToList();


            return View("AppointmentsForWeek", appointments);
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
            }
            var otherStudents = db.Students.Where(s => s.ClassId == student.ClassId && s.Id != student.Id)
                .ToList();
            foreach (var otherStudent in otherStudents)
            {
                otherStudent.IsClassRep = false;
            }
            db.SaveChanges();
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
            }
            var otherStudents = db.Students.Where(s => s.ClassId == student.ClassId && s.Id != student.Id)
                .ToList();
            foreach (var otherStudent in otherStudents)
            {
                otherStudent.IsDeputyClassRep = false;
            }
            db.SaveChanges();
            return RedirectToAction("ViewAllStudents", "GuidanceTeacher");
        }

        // GET: GuidanceTeacher/Create
        // Displays the form to create a new guidance teacher
        [Authorize(Roles = "CurriculumHead")]
        public ActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "CurriculumHead")]
        // POST: GuidanceTeacher/Create
        // Handles the submission of the guidance teacher creation form
        [HttpPost]
        [ValidateAntiForgeryToken] // Protects against Cross-Site Request Forgery attacks
        public async Task<ActionResult> Create(CreateGuidanceTeacherViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Create a new GuidanceTeacher user based on the ViewModel data
                var guidanceTeacherUser = new GuidanceTeacher
                {
                    UserName = model.Email, // Email is commonly used as the UserName for login
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Street = model.Street,
                    City = model.City,
                    Postcode = model.Postcode,
                    RegistredAt = DateTime.UtcNow // Set the registration date to the current UTC time
                };

                // Attempt to create the user in the ASP.NET Identity system with the provided password
                var result = await UserManager.CreateAsync(guidanceTeacherUser, model.Password);

                if (result.Succeeded)
                {
                    // If user creation is successful, assign the "GuidanceTeacher" role
                    var roleResult = await UserManager.AddToRoleAsync(guidanceTeacherUser.Id, "GuidanceTeacher");

                    if (roleResult.Succeeded)
                    {
                        // Set a success message to display on the next view
                        TempData["Message"] = "Guidance Teacher created successfully!";
                        // Redirect to a list of guidance teachers or the dashboard
                        return RedirectToAction("EnrollmentAcademicOperationsCenter", "CurriculumHead");
                    }
                    else
                    {
                        // If role assignment fails, add errors to ModelState
                        AddErrors(roleResult);
                        // Optionally, you might want to delete the user if role assignment fails
                        // await UserManager.DeleteAsync(guidanceTeacherUser);
                    }
                }
                else
                {
                    // If user creation fails (e.g., duplicate email, password policy), add errors to ModelState
                    AddErrors(result);
                }
            }

            // If ModelState is not valid or user/role creation failed, re-display the form with errors
            return View(model);
        }
                // Helper method to add IdentityResult errors to ModelState
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        // Dispose the UserManager when the controller is disposed to free resources
        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null) // Corrected from userManager to UserManager
            {
                UserManager.Dispose(); // Dispose the public property
                // No need to set _userManager = null explicitly if accessing via property
            }
            base.Dispose(disposing);
        }
    }
}