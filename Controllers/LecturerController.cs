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
    public class LecturerController : Controller
    {

        private GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();

        [Authorize(Roles = "Lecturer")]
        // GET: Lecturer
        [Authorize(Roles = "Lecturer")]
        public ActionResult LecturerDash()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Lecturers.Find(userId);

            // Count unread announcements
            var visiblePosts = PostVisibilityHelper.GetVisiblePosts(userId, db, User);
            var newAnnouncementsCount = visiblePosts
                .Where(p => !db.PostReads.Any(pr => pr.PostId == p.PostId && pr.UserId == userId))
                .Count();

            // Count active issues
            var activeIssuesCount = db.Issues
                .Where(i => (i.IssueStatus == IssueStatus.New || i.IssueStatus == IssueStatus.InProgress)
                && i.LecturerId == userId)
                .Count();

            //  Count unread messages
            var unreadMessagesCount = db.Messages
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .Count();

            var model = new LecturerDashViewModel
            {
                FirstName = user.FirstName,
                ActiveIssuesCount = activeIssuesCount,
                NewAnnouncementsCount = newAnnouncementsCount,
                NewMessagesCount = unreadMessagesCount 

            };

            return View(model);
        }


        public ActionResult ViewAllStudents()
        {
            var students = db.Students.OrderBy(s => s.RegistredAt).ToList();

            return View(students);
        }
    }
}