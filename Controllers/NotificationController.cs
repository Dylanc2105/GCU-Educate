using GuidanceTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
namespace GuidanceTracker.Controllers
{
    [Authorize(Roles = "GuidanceTeacher, Lecturer")]
    public class NotificationController : Controller
    {
        // new db context
        private GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();

        [HttpGet]
        public ActionResult GetRecentNotifications()
        {
            // identify user
            var userId = User.Identity.GetUserId();
            System.Diagnostics.Debug.WriteLine("User ID = " + userId);

            // order notifications for user by the newest 5 into a list
            var notifications = db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToList();

            return PartialView("_NotificationDropdown", notifications);
        }

        // Marks a single notification as read when clicked from the dropdown
        [HttpPost]
        [Authorize]
        public ActionResult MarkAsRead(int id)
        {
            var notification = db.Notifications.Find(id);
            var userId = User.Identity.GetUserId();

            // Ensure the notification exists and belongs to the current user
            if (notification != null && notification.UserId == userId)
            {
                notification.IsRead = true; // Update read status
                db.SaveChanges(); // Persist to database
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK); // Silent success response
        }
    }
}