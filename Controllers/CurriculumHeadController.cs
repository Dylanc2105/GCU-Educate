using GuidanceTracker.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GuidanceTracker.Models.ViewModels;
using static GuidanceTracker.Controllers.PostController;
using System.Data.Entity;
using System.Net;

namespace GuidanceTracker.Controllers
{
    /// <summary>
    /// Controller responsible for handling all curriculum head-related functionality and dashboard operations.
    /// This controller manages the curriculum head's view of the guidance tracking system, including
    /// viewing feedback, messages, and announcements specific to their role.
    /// </summary>
    public class CurriculumHeadController : Controller
    {
        /// <summary>
        /// Database context instance used to interact with the GuidanceTracker database.
        /// This provides access to all database entities including users, messages, posts, and feedback.
        /// </summary>
        private GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();

        /// <summary>
        /// Displays the main dashboard for curriculum heads.
        /// This action method aggregates various statistics and notifications relevant to the curriculum head,
        /// including unread feedback, messages, and announcements.
        /// </summary>
        /// <returns>
        /// A ViewResult containing the CurriculumDashViewModel with aggregated dashboard data including:
        /// - User's first name for personalised greeting
        /// - Count of new/unread announcements
        /// - Count of unread messages
        /// - Count of new feedback items awaiting review
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// 1. Retrieves the current authenticated user's information
        /// 2. Calculates unread announcements using visibility rules
        /// 3. Counts unread messages directed to the user
        /// 4. Counts unread feedback items assigned to the curriculum head
        /// All counts are performed using LINQ queries to minimise database calls
        /// </remarks>
        // GET: CurriculumHead
        public ActionResult CurriculumHeadDash()
        {
            // Get the current user's ID from the authentication context
            // This uses ASP.NET Identity to retrieve the logged-in user's unique identifier
            var userId = User.Identity.GetUserId();

            // Retrieve the CurriculumHead entity for the current user
            // This provides access to user-specific properties like FirstName
            var user = db.CurriculumHeads.Find(userId);

            // Store today's date for potential date-based filtering
            // Currently unused but available for future enhancements
            var today = DateTime.Today;

            // Get posts that are visible to the current user based on their role and permissions
            // PostVisibilityHelper encapsulates complex visibility logic including role-based access
            var visiblePosts = PostVisibilityHelper.GetVisiblePosts(userId, db, User);

            // Calculate the count of unread announcements/posts
            // An announcement is considered "new" if no PostRead record exists for this user-post combination
            var newAnnouncementsCount = visiblePosts
                .Where(p => !db.PostReads.Any(pr => pr.PostId == p.PostId && pr.UserId == userId))
                .Count();

            // COUNT UNREAD MESSAGES FOR THIS USER
            // Retrieves all messages where:
            // - The current user is the recipient
            // - The message has not been marked as read
            var unreadMessagesCount = db.Messages
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .Count();

            // Create and populate the view model with all dashboard statistics
            // This model will be passed to the view for rendering
            var model = new CurriculumDashViewModel
            {
                // User's first name for personalized dashboard greeting
                FirstName = user.FirstName,

                // Total count of announcements the user hasn't read yet
                NewAnnouncementsCount = newAnnouncementsCount,

                // Total count of unread messages in the user's inbox
                UnreadMessagesCount = unreadMessagesCount,

                // Count of feedback items that:
                // - Haven't been read by any curriculum head
                // - Are specifically assigned to this curriculum head
                NewFeedbackCount = db.SimpleFeedbacks
                    .Where(fb => fb.IsReadByCurriculumHead == false && fb.CurriculumHeadId == userId)
                    .Count(),
            };

            // Return the view with the populated model
            // The view will use this data to display the dashboard statistics
            return View(model);
        }

        public ActionResult ViewAllStudents()
        {
            var students = db.Students.OrderBy(s => s.RegistredAt).ToList();
            return View(students);
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
    }
}