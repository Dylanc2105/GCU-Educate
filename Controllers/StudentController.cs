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

            // Count unread announcements
            var visiblePosts = PostVisibilityHelper.GetVisiblePosts(userId, db, User);
            var newAnnouncementsCount = visiblePosts
                .Where(p => !db.PostReads.Any(pr => pr.PostId == p.PostId && pr.UserId == userId))
                .Count();

            // ✅ Count unread messages for student
            var unreadMessagesCount = db.Messages
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .Count();

            var model = new StudentDashViewModel
            {
                FirstName = user.FirstName,
                UpcomingAppointmentsCount = db.Appointments
                .Where(a => a.StudentId == userId && a.AppointmentDate >= DateTime.Today)
                .Count(),
                NewAnnouncementsCount = newAnnouncementsCount,
                NewMessagesCount = unreadMessagesCount // ✅ Add this to the view model
            };

            return View(model);
        }

        public ActionResult UpcomingAppoinments()
        {
            var userId = User.Identity.GetUserId();

            var appointments = db.Appointments
                .Where(a => a.StudentId == userId && a.AppointmentDate >= DateTime.Today)
                .ToList();
            return View(appointments);
        }

        public ActionResult RequestAppointment()
        {
            var studentId = User.Identity.GetUserId();
            var student = db.Students.Find(studentId);

            if (student == null)
            {
                return HttpNotFound("Student not found");
            }

            // Get the guidance session with appointments to check availability
            GuidanceSession guidanceSession = db.GuidanceSessions
                .Include(g => g.Appointments)
                .Where(g => g.ClassId == student.ClassId)
                .FirstOrDefault();

            if (guidanceSession == null)
            {
                return HttpNotFound("No guidance session found for this student's class");
            }

            // Create view model with only available time slots
            var viewModel = new AppointmentViewModel
            {
                StudentId = studentId,
                StudentName = $"{student.FirstName} {student.LastName}",
                AppointmentDate = guidanceSession.Day,
                GuidanceSessionId = guidanceSession.GuidanceSessionId,
                Room = guidanceSession.Room,
                AppointmentStatus = AppointmentStatus.Requested,
                // Only include available time slots
                AvailableTimeSlots = GetOnlyAvailableTimeSlots(guidanceSession)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RequestAppointment(AppointmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload available time slots
                var session = db.GuidanceSessions
                    .Include(g => g.Appointments)
                    .FirstOrDefault(g => g.GuidanceSessionId == model.GuidanceSessionId);

                model.AvailableTimeSlots = GetOnlyAvailableTimeSlots(session);
                return View(model);
            }

            var studentId = User.Identity.GetUserId();
            var student = db.Students.Find(studentId);

            if (student == null)
            {
                return HttpNotFound("Student not found");
            }

            var guidanceSession = db.GuidanceSessions
                .Include(g => g.Appointments)
                .FirstOrDefault(g => g.GuidanceSessionId == model.GuidanceSessionId);

            if (guidanceSession == null)
            {
                return HttpNotFound("Guidance session not found");
            }

            // Double-check that the time slot is still available (for concurrent submissions)
            if (!guidanceSession.IsTimeSlotAvailable(model.StartTime))
            {
                ModelState.AddModelError("StartTime", "This time slot is no longer available");
                model.AvailableTimeSlots = GetOnlyAvailableTimeSlots(guidanceSession);
                return View(model);
            }

            // Create the appointment
            var appointment = new Appointment
            {
                StudentId = studentId,
                GuidanceSessionId = model.GuidanceSessionId,
                AppointmentDate = model.AppointmentDate,
                StartTime = model.StartTime,
                AppointmentNotes = model.AppointmentNotes,
                AppointmentStatus = AppointmentStatus.Requested,
                GuidanceTeacherId = student.Class.GuidanceTeacherId,
                Room = model.Room
            };

            db.Appointments.Add(appointment);
            db.SaveChanges();

            TempData["Success"] = "Appointment requested successfully.";
            return RedirectToAction("StudentDash");
        }

        // Helper method to get ONLY available time slots for the view
        private List<TimeSlotViewModel> GetOnlyAvailableTimeSlots(GuidanceSession session)
        {
            if (session == null)
                return new List<TimeSlotViewModel>();

            var timeSlots = new List<TimeSlotViewModel>();

            foreach (var slot in session.AllTimeSlots)
            {
                // Check if slot is available
                bool isAvailable = session.IsTimeSlotAvailable(slot);

                // Only add available time slots to the list
                if (isAvailable)
                {
                    timeSlots.Add(new TimeSlotViewModel
                    {
                        StartTime = slot,
                        EndTime = slot.Add(TimeSpan.FromMinutes(10)),
                        IsAvailable = true
                    });
                }
            }

            return timeSlots;
        }
    }
}

    