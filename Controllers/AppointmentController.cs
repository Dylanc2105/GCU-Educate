using GuidanceTracker.Models;
using GuidanceTracker.Models.ViewModels;
using GuidanceTracker.Services;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GuidanceTracker.Controllers
{
    [Authorize(Roles = "GuidanceTeacher")]
    public class AppointmentController : Controller
    {
        private readonly GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();

        public List<Appointment> GetAppointments(string studentId)
        {
            var appointments = db.Appointments.Where(s => s.StudentId == studentId).ToList();
            return appointments;
        }

        // GET: Create appointment 
        public ActionResult Create(string studentId)
        {
            var student = db.Students.Find(studentId);
            if (student == null)
            {
                return HttpNotFound("Student not found");
            }

            GuidanceSession guidanceSession = db.GuidanceSessions
                .Include(g => g.Appointments)
                .Where(g => g.ClassId == student.ClassId)
                .FirstOrDefault();

            if (guidanceSession == null)
            {
                return HttpNotFound("No guidance session found for this student's class");
            }


            // Build view model with ONLY available time slots
            var viewModel = new AppointmentViewModel
            {
                StudentId = studentId,
                StudentName = $"{student.FirstName} {student.LastName}",
                AppointmentDate = guidanceSession.Day,
                GuidanceSessionId = guidanceSession.GuidanceSessionId,
                Room = guidanceSession.Room,
                AppointmentStatus = AppointmentStatus.Requested,
                // Only get available time slots
                AvailableTimeSlots = GetOnlyAvailableTimeSlots(guidanceSession)
            };

            return View(viewModel);
        }

        [Authorize(Roles = "GuidanceTeacher, Lecturer, Student")]
        [HttpGet]
        public JsonResult GetUserMeetings()
        {
            var userId = User.Identity.GetUserId();

            var meetings = db.Appointments
                .Include(m => m.Student)
                .Where(m => m.StudentId == userId || m.GuidanceTeacherId == userId)
                .OrderByDescending(m => m.StartTime)
                .ToList()
                .Select(m => new
                {
                    MeetingId = m.AppointmentId,
                    Title = "Meeting with " + (m.Student != null ? m.Student.FirstName + " " + m.Student.LastName : ""),
                    StudentName = m.Student != null ? m.Student.FirstName + " " + m.Student.LastName : "N/A",
                    Date = m.AppointmentDate.ToString("o")
                })
                .ToList();

            return Json(meetings, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AppointmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload ONLY available time slots
                var session = db.GuidanceSessions
                    .Include(g => g.Appointments)
                    .FirstOrDefault(g => g.GuidanceSessionId == model.GuidanceSessionId);

                model.AvailableTimeSlots = GetOnlyAvailableTimeSlots(session);
                return View(model);
            }

            var student = db.Students.Find(model.StudentId);
            if (student == null)
            {
                return HttpNotFound("Student not found");
            }

            var guidanceSession = db.GuidanceSessions.Find(model.GuidanceSessionId);
            if (guidanceSession == null)
            {
                return HttpNotFound("Guidance session not found");
            }

            // Verify the time slot is still available (in case of concurrent submissions)
            guidanceSession = db.GuidanceSessions
                .Include(g => g.Appointments)
                .FirstOrDefault(g => g.GuidanceSessionId == model.GuidanceSessionId);

            if (!guidanceSession.IsTimeSlotAvailable(model.StartTime))
            {
                ModelState.AddModelError("StartTime", "This time slot is no longer available");
                model.AvailableTimeSlots = GetOnlyAvailableTimeSlots(guidanceSession);
                return View(model);
            }

            // Create the appointment
            var appointment = new Appointment
            {
                StudentId = model.StudentId,
                GuidanceSessionId = model.GuidanceSessionId,
                AppointmentDate = model.AppointmentDate,
                StartTime = model.StartTime,
                AppointmentNotes = model.AppointmentNotes,
                AppointmentStatus = model.AppointmentStatus,
                GuidanceTeacherId = User.Identity.GetUserId(),
                Room = model.Room,
                IssueId = model.IssueId
            };

            db.Appointments.Add(appointment);
            db.SaveChanges();

            TempData["Success"] = "Appointment created successfully.";
            return RedirectToAction("Index", "Issue");
        }

        // GET: Create appointment with issue
        public ActionResult CreateAppointmentWithIssue(int issueId)
        {
            var issue = db.Issues
                .Where(i => i.IssueId == issueId)
                .Include(i => i.Student)
                .FirstOrDefault();

            if (issue == null)
            {
                return HttpNotFound("Issue not found");
            }

            var student = issue.Student;

            GuidanceSession guidanceSession = db.GuidanceSessions
                .Include(g => g.Appointments)
                .Where(g => g.ClassId == student.ClassId)
                .FirstOrDefault();

            if (guidanceSession == null)
            {
                return HttpNotFound("No guidance session found for this student's class");
            }

            // Build view model with ONLY available time slots
            var viewModel = new AppointmentViewModel
            {
                StudentId = student.Id,
                StudentName = $"{student.FirstName} {student.LastName}",
                AppointmentDate = guidanceSession.Day,
                GuidanceSessionId = guidanceSession.GuidanceSessionId,
                Room = guidanceSession.Room,
                AppointmentStatus = AppointmentStatus.Requested,
                IssueId = issueId,
                IssueDescription = issue.IssueDescription,
                // Only get available time slots
                AvailableTimeSlots = GetOnlyAvailableTimeSlots(guidanceSession)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAppointmentWithIssue(AppointmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload ONLY available time slots
                var session = db.GuidanceSessions
                    .Include(g => g.Appointments)
                    .FirstOrDefault(g => g.GuidanceSessionId == model.GuidanceSessionId);

                model.AvailableTimeSlots = GetOnlyAvailableTimeSlots(session);
                return View(model);
            }

            var student = db.Students.Find(model.StudentId);
            if (student == null)
            {
                return HttpNotFound("Student not found");
            }

            var guidanceSession = db.GuidanceSessions.Find(model.GuidanceSessionId);
            if (guidanceSession == null)
            {
                return HttpNotFound("Guidance session not found");
            }

            var issue = db.Issues.Find(model.IssueId);
            if (issue == null)
            {
                return HttpNotFound("Issue not found");
            }

            // Verify the time slot is still available (in case of concurrent submissions)
            guidanceSession = db.GuidanceSessions
                .Include(g => g.Appointments)
                .FirstOrDefault(g => g.GuidanceSessionId == model.GuidanceSessionId);

            if (!guidanceSession.IsTimeSlotAvailable(model.StartTime))
            {
                ModelState.AddModelError("StartTime", "This time slot is no longer available");
                model.AvailableTimeSlots = GetOnlyAvailableTimeSlots(guidanceSession);
                return View(model);
            }

            // Create the appointment
            var appointment = new Appointment
            {
                StudentId = model.StudentId,
                GuidanceSessionId = model.GuidanceSessionId,
                AppointmentDate = model.AppointmentDate,
                StartTime = model.StartTime,
                AppointmentNotes = model.AppointmentNotes,
                AppointmentStatus = model.AppointmentStatus,
                GuidanceTeacherId = User.Identity.GetUserId(),
                Room = model.Room,
                IssueId = model.IssueId
            };

            db.Appointments.Add(appointment);
            db.SaveChanges();

            TempData["Success"] = "Appointment created successfully.";
            return RedirectToAction("Index", "Issue");
        }

        public ActionResult AppointmentsToBeApproved()
        {
            //get all appointments with students and their class data
            var appointments = db.Appointments
                .Where(a => a.AppointmentStatus == AppointmentStatus.Requested)
                .Include(a => a.Student)
                .Include(a => a.Student.Class)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToList();

            return View(appointments);
        }

        public async Task<ActionResult> ApproveAppointment(int id)
        {
            var appointment = db.Appointments.Find(id);

            //if appointment found in db correctly : update status
            if (appointment != null)
            {
                appointment.AppointmentStatus = AppointmentStatus.Scheduled;
                db.SaveChanges();

                var emailService = new AppEmailService();
                await emailService.SendAsync(
                    appointment.Student.Email,
                    "Appointment Confirmed",
                    $"<p>Hello {appointment.Student.FirstName},</p>" +
                    $"<p>Your appointment has been confirmed for <strong>{appointment.AppointmentDate:dddd, dd MMM yyyy h:mm tt}</strong> in Room {appointment.Room}.</p>" +
                    "<p>Please arrive a few minutes early.</p>"
                );

            }
            else
            {
                return HttpNotFound();
            }
            TempData["Success"] = "Appointment confirmed.";
            return RedirectToAction("AppointmentsToBeApproved");
        }

        public async Task<ActionResult> CancelAppointment(int id)
        {
            var appointment = db.Appointments.Find(id);

            //if appointment found in db correctly : update status
            if (appointment != null)
            {
                appointment.AppointmentStatus = AppointmentStatus.Cancelled;
                db.SaveChanges();

                var emailService = new AppEmailService();
                await emailService.SendAsync(
                    appointment.Student.Email,
                    "Appointment Cancelled",
                    $"<p>Hello {appointment.Student.FirstName},</p>" +
                    $"<p>Your scheduled appointment on <strong>{appointment.AppointmentDate:dddd, dd MMM yyyy h:mm tt}</strong> in Room {appointment.Room} has been <span style='color:red;'>cancelled</span>.</p>" +
                    "<p>If you have any questions, please contact your guidance teacher.</p>"
                );

            }
            else
            {
                return HttpNotFound();
            }

            TempData["Success"] = "Appointment cancelled.";
            return RedirectToAction("AppointmentsToBeApproved");
        }

        public ActionResult Edit(int id)
        {
            // Get appointment from db with related data
            var appointment = db.Appointments
                .Include(a => a.GuidanceSession)
                .Include(a => a.Student)
                .FirstOrDefault(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return HttpNotFound();
            }

            string description = "";

            if (appointment.IssueId != null)
            {
                description = appointment.Issue?.IssueDescription ?? "";
            }

            // Load guidance session with appointments to check availability
            var guidanceSession = db.GuidanceSessions
                .Include(g => g.Appointments)
                .FirstOrDefault(g => g.GuidanceSessionId == appointment.GuidanceSessionId);

            // Create view model
            var viewModel = new AppointmentViewModel
            {
                AppointmentId = appointment.AppointmentId,
                StudentId = appointment.StudentId,
                StudentName = $"{appointment.Student.FirstName} {appointment.Student.LastName}",
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime,
                GuidanceSessionId = appointment.GuidanceSessionId,
                Room = appointment.Room,
                AppointmentNotes = appointment.AppointmentNotes,
                AppointmentStatus = appointment.AppointmentStatus,
                IssueId = appointment.IssueId,
                IssueDescription = description,
                // Get only available time slots plus the current appointment's time slot
                AvailableTimeSlots = GetOnlyAvailableTimeSlotsForEdit(guidanceSession, appointment.AppointmentId)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(AppointmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload ONLY available time slots
                var guidanceSession = db.GuidanceSessions
                    .Include(g => g.Appointments)
                    .FirstOrDefault(g => g.GuidanceSessionId == model.GuidanceSessionId);

                model.AvailableTimeSlots = GetOnlyAvailableTimeSlotsForEdit(guidanceSession, model.AppointmentId);
                return View(model);
            }

            var appointment = db.Appointments.Find(model.AppointmentId);
            if (appointment == null)
            {
                return HttpNotFound();
            }

            // Check if time slot has changed
            bool timeChanged = appointment.StartTime != model.StartTime;

            if (timeChanged)
            {
                // Verify the time slot is still available
                var guidanceSession = db.GuidanceSessions
                    .Include(g => g.Appointments)
                    .FirstOrDefault(g => g.GuidanceSessionId == model.GuidanceSessionId);

                if (!guidanceSession.IsTimeSlotAvailable(model.StartTime))
                {
                    ModelState.AddModelError("StartTime", "This time slot is no longer available");
                    model.AvailableTimeSlots = GetOnlyAvailableTimeSlotsForEdit(guidanceSession, model.AppointmentId);
                    return View(model);
                }

                // Update time and status if time changed
                appointment.StartTime = model.StartTime;
                appointment.AppointmentStatus = AppointmentStatus.Rescheduled;
            }

            // Update other fields
            appointment.AppointmentNotes = model.AppointmentNotes;
            appointment.Room = model.Room;

            db.SaveChanges();

            // Send reschedule email
            var student = db.Students.Find(appointment.StudentId);
            var emailService = new AppEmailService();
            var formattedDate = appointment.AppointmentDate.ToString("dddd, dd MMM yyyy");
            var formattedTime = appointment.StartTime.ToString(@"hh\:mm"); // TimeSpan needs `\:` escaping

            await emailService.SendAsync(
                student.Email,
                "Appointment Rescheduled",
                $"<p>Hello {student.FirstName},</p>" +
                $"<p>Your appointment has been <strong>rescheduled</strong> to " +
                $"<strong>{formattedDate} at {formattedTime}</strong> in Room {appointment.Room}.</p>" +
                $"<p>Please take note of the new time and arrive promptly.</p>"
            );


            TempData["Success"] = "Appointment updated.";
            return RedirectToAction("AppointmentsToBeApproved", "Appointment");
        }

        public ActionResult AppointmentsForSessionPartial(int id)
        {
            var appointments = db.Appointments
                .Where(a => a.GuidanceSessionId == id)
                .Include(a => a.Student)
                .Include(a => a.GuidanceTeacher)
                .OrderBy(a => a.StartTime)
                .ToList();

            var session = db.GuidanceSessions.Find(id);

            ViewBag.Class = session.Class.ClassName;
            ViewBag.GSTime = session.Time;
            ViewBag.Room = session.Room;

            return PartialView("_AppointmentsForSession", appointments);
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

        // Helper method to get ONLY available time slots for the edit view,
        // also including the slot that belongs to the current appointment
        private List<TimeSlotViewModel> GetOnlyAvailableTimeSlotsForEdit(GuidanceSession session, int appointmentId)
        {
            if (session == null)
                return new List<TimeSlotViewModel>();

            var timeSlots = new List<TimeSlotViewModel>();

            // Get the current appointment to include its time slot
            var currentAppointment = db.Appointments.Find(appointmentId);
            var currentTimeSlot = currentAppointment?.StartTime;

            foreach (var slot in session.AllTimeSlots)
            {
                // Check if slot belongs to the current appointment
                bool isCurrentSlot = currentTimeSlot.HasValue && slot == currentTimeSlot.Value;

                // For editing, we check availability excluding the current appointment
                bool isAvailable = !session.Appointments
                    .Where(a => a.AppointmentId != appointmentId)
                    .Any(a => a.StartTime == slot && a.AppointmentStatus != AppointmentStatus.Cancelled);

                // Add to the list if it's available OR it's the current appointment's slot
                if (isAvailable || isCurrentSlot)
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

        // Existing form methods below...
        public ActionResult AppointmentForm(int id)
        {
            var meeting = db.Appointments.
                         Where(a => a.AppointmentId == id).
                         Include(a => a.Student).
                         FirstOrDefault();

            if (meeting == null)
            {
                return HttpNotFound();
            }

            AppointmentFeedbackForm form = new AppointmentFeedbackForm
            {
                GuidanceTeacherId = meeting.GuidanceTeacherId,
                StudentId = meeting.StudentId,
                Student = meeting.Student,
                AppointmentId = meeting.AppointmentId,
                Appointment = meeting,
                IssueId = meeting.IssueId.Value,
                Issue = meeting.Issue,
            };
            return View(form);
        }

        [HttpPost]
        public ActionResult AppointmentForm(AppointmentFeedbackForm model)
        {
            try
            {
                var userId = User.Identity.GetUserId();

                AppointmentFeedbackForm form = new AppointmentFeedbackForm { };

                form.StudentOpinion = model.StudentOpinion;
                form.StudentActions = model.StudentActions;
                form.OtherActions = model.OtherActions;
                form.GuidanceTeacherNotes = model.GuidanceTeacherNotes;
                form.GuidanceTeacherId = userId;
                form.GuidanceTeacher = db.GuidanceTeachers.Find(userId);
                form.StudentId = model.StudentId;
                form.Student = db.Students.Find(model.StudentId);
                form.AppointmentId = model.AppointmentId;
                form.Appointment = db.Appointments.Find(model.AppointmentId);
                form.IssueId = model.IssueId;
                form.Issue = db.Issues.Find(model.IssueId);
                db.AppointmentFeedbackForms.Add(form);
                db.SaveChanges();
                TempData["Message"] = "Feedback form submitted successfully!"; // Use TempData
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Submission of feedback form failed: " + ex.Message; // Store Error
            }
            return RedirectToAction("GuidanceTeacherDash", "GuidanceTeacher");
        }

        public ActionResult ProgressMeetingForm(int id)
        {
            var meeting = db.Appointments.
                Where(a => a.AppointmentId == id).
                Include(a => a.Student).
                FirstOrDefault();

            ProgressMeetingFeedbackForm form = new ProgressMeetingFeedbackForm
            {
                GuidanceTeacherId = meeting.GuidanceTeacherId,
                StudentId = meeting.StudentId,
                Student = meeting.Student,
                AppointmentId = meeting.AppointmentId,
                Appointment = meeting
            };
            return View(form);
        }

        [HttpPost]
        public ActionResult ProgressMeetingForm(ProgressMeetingFeedbackForm model)
        {
            try
            {
                var userId = User.Identity.GetUserId();

                ProgressMeetingFeedbackForm form = new ProgressMeetingFeedbackForm { };

                form.CourseFeedback = model.CourseFeedback;
                form.CourseFeedbackComments = model.CourseFeedbackComments;
                form.Strengths = model.Strengths;
                form.ToBeImproved = model.ToBeImproved;
                form.Goals = model.Goals;
                form.GuidanceTeacherNotes = model.GuidanceTeacherNotes;
                form.GuidanceTeacherId = userId;
                form.GuidanceTeacher = db.GuidanceTeachers.Find(userId);
                form.StudentId = model.StudentId;
                form.Student = db.Students.Find(model.StudentId);
                form.AppointmentId = model.AppointmentId;
                form.Appointment = db.Appointments.Find(model.AppointmentId);
                db.ProgressMeetingFeedbackForms.Add(form);
                db.SaveChanges();
                TempData["Message"] = "Feedback form submitted successfully!"; // Use TempData
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Submission of feedback form failed: " + ex.Message; // Store Error
            }
            return RedirectToAction("GuidanceTeacherDash", "GuidanceTeacher");
        }



        // GET: Display the student selection view
        public ActionResult SelectStudentForAppointment()
        {
            var userId = User.Identity.GetUserId();

            // Get classes associated with this guidance teacher
            var guidanceTeacher = db.GuidanceTeachers
                .Include(gt => gt.Classes)
                .FirstOrDefault(gt => gt.Id == userId);


            var model = new StudentIssueSelectViewModel
            {
                Classes = guidanceTeacher.Classes.ToList(),
                Students = new List<Student>() // Empty until class is selected
            };

            return View(model);
        }

        // AJAX: Get Students by Class ID
        public ActionResult GetStudentsByClass(int classId)
        {
            var students = db.Students
                .Where(s => s.ClassId == classId)
                .Select(s => new
                {
                    s.Id,
                    FullName = s.FirstName + " " + s.LastName
                })
                .ToList();

            return Json(students, JsonRequestBehavior.AllowGet);
        }

        // AJAX: Get Issues by Student ID
        public ActionResult GetStudentIssues(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return Json(new { error = "Student ID is required" }, JsonRequestBehavior.AllowGet);
            }

            var student = db.Students.Find(studentId);
            if (student == null)
            {
                return Json(new { error = "Student not found" }, JsonRequestBehavior.AllowGet);
            }

            // Get active issues for this student
            // First get the basic data from database
            var issuesQuery = db.Issues
                .Where(i => i.StudentId == studentId && i.IssueStatus != IssueStatus.Archived)
                .OrderByDescending(i => i.CreatedAt)
                .ToList(); // Execute the query and bring data into memory

            // Then perform the ToString() operations in memory
            var issues = issuesQuery.Select(i => new
            {
                IssueId = i.IssueId,
                IssueTitle = i.IssueTitle.ToString(), // Now safe to call ToString()
                IssueDescription = i.IssueDescription ?? "No description available",
                IssueStatus = i.IssueStatus.ToString(),
                CreatedAt = i.CreatedAt.ToString("dd/MM/yyyy")
            }).ToList();

            return Json(new
            {
                success = true,
                studentName = student.FirstName + " " + student.LastName,
                studentId = student.Id,
                issues = issues
            }, JsonRequestBehavior.AllowGet);
        }
    }
}