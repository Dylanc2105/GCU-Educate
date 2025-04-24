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
                AppointmentsTodayCount = db.Appointments
                    .Where(a => a.StudentId == userId && DbFunctions.TruncateTime(a.AppointmentDate) == DateTime.Today)
                    .Count(),
                NewAnnouncementsCount = newAnnouncementsCount,
                NewMessagesCount = unreadMessagesCount // ✅ Add this to the view model
            };

            return View(model);
        }
    }
}

    