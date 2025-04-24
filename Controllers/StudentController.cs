using GuidanceTracker.Models;
using GuidanceTracker.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static GuidanceTracker.Controllers.PostController;

namespace GuidanceTracker.Controllers
{
    public class StudentController : Controller
    {
        private GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();

        [Authorize(Roles = "Student")]
        // GET: Student
        public ActionResult StudentDash()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Students.Find(userId);

            // counts the posts that don't have a row in the PostRead table for the current user
            var visiblePosts = PostVisibilityHelper.GetVisiblePosts(userId, db, User);

            var newAnnouncementsCount = visiblePosts
                .Where(p => !db.PostReads.Any(pr => pr.PostId == p.PostId && pr.UserId == userId))
                .Count();

            var model = new StudentDashViewModel
            {
                FirstName = user.FirstName,
                AppointmentsTodayCount = db.Appointments
                    .Where(a => a.StudentId == userId && DbFunctions.TruncateTime(a.AppointmentDate) == DateTime.Today)
                    .Count(),
                //NewMessagesCount = db.Messages.Where(n => n.IsRead == false).Count(),
                NewAnnouncementsCount = newAnnouncementsCount


            };
            return View(model);
        }

        public ActionResult RequestAppointment()
        {
            var studentId = User.Identity.GetUserId();
            var student = db.Students.Find(studentId);
            GuidanceSession guidanceSession = db.GuidanceSessions.
                Where(g => g.ClassId == student.ClassId).
                FirstOrDefault();

            Appointment appointment = new Appointment
            {
                StudentId = studentId,
                Student = db.Students.Find(studentId),
                AppointmentDate = guidanceSession.Day,
                AppointmentNotes = "",
                GuidanceSession = guidanceSession,
                Room = guidanceSession.Room,
            };
            return View(appointment);
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RequestAppointment(Appointment model)
        {
            var studentId = User.Identity.GetUserId();
            var student = db.Students.Find(studentId);
            GuidanceSession guidanceSession = db.GuidanceSessions.Where(g => g.ClassId == student.ClassId).FirstOrDefault();

            var appointment = new Appointment { };

            //no ViewBag message by default
            ViewBag.Message = "";

            string stringDate = guidanceSession.Day.ToString("dd/MM/yyyy");
            string stringTime = guidanceSession.Time.ToString();

            //make datetime string from date and time
            stringDate = stringDate + " " + stringTime;

            //try to transform string to datetime
            try
            {
                appointment.AppointmentDate = DateTime.Parse(stringDate);
            }
            //if not successfull return alert
            catch
            {
                ViewBag.Message = "incorrect input for date";
            }


            appointment.AppointmentNotes = model.AppointmentNotes;            
            appointment.StudentId = studentId;
            appointment.Student = db.Students.Where(c => c.Id == model.StudentId).FirstOrDefault();
            appointment.AppointmentStatus = AppointmentStatus.Requested;
            appointment.GuidanceTeacherId = student.Class.GuidanceTeacherId;
            appointment.GuidanceTeacher = db.GuidanceTeachers.Find(appointment.GuidanceTeacherId);
            appointment.GuidanceSession = db.GuidanceSessions.Where(g => g.ClassId == student.ClassId).FirstOrDefault();
            appointment.Room = guidanceSession.Room;



            //if viewbag has any error message => return them to view
            if (ViewBag.Message != "")
            {
                return View(appointment);
            }
            else
            {
                //add Appointment to db
                db.Appointments.Add(appointment);
                db.SaveChanges();
                TempData["Success"] = "Appointment created successfully.";
                //redirect to issue page
                return RedirectToAction("StudentDash", "Student");
            }
        }
    }
}