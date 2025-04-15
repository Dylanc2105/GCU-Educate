using GuidanceTracker.Models;
using GuidanceTracker.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

            var model = new LecturerDashViewModel
            {
                FirstName = user.FirstName,
                //CurrentDateTime = DateTime.Now,
                NewProgressIssuesCount = db.Issues.Where(i => i.IssueStatus == IssueStatus.New).Count(),
                //NewNotificationsCount = db.Notifications.Where(n => n.IsRead == false).Count(),
                //NewAnnouncementsCount = db.Announcements.Where(a => a.IsRead == false).Count()


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