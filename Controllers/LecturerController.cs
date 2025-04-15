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
        public ActionResult LecturerDash()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Lecturers.Find(userId);
            var today = DateTime.Today;

            // counts the posts that don't have a row in the PostRead table for the current user
            var visiblePosts = PostVisibilityHelper.GetVisiblePosts(userId, db, User);

            var newAnnouncementsCount = visiblePosts
                .Where(p => !db.PostReads.Any(pr => pr.PostId == p.PostId || pr.UserId == userId))
                .Count();

            var model = new LecturerDashViewModel
            {
                FirstName = user.FirstName,
                ActiveIssuesCount = db.Issues.Where(i => (i.IssueStatus == IssueStatus.New && i.IssueStatus == IssueStatus.InProgress)
                && i.LecturerId == userId)
                .Count(),
                //NewMessagesCount = db.Messages.Where(n => n.IsRead == false).Count(),
                NewAnnouncementsCount = newAnnouncementsCount


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