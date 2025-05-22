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

        // Show all notifications, gruoped by day
        [Authorize]
        public ActionResult All()
        {
            var userId = User.Identity.GetUserId();

            var notifications = db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToList();

            var grouped = notifications
                .GroupBy(n => n.CreatedAt.Date)
                .ToDictionary(
                    g => g.Key == DateTime.Today ? "Today" :
                         g.Key == DateTime.Today.AddDays(-1) ? "Yesterday" :
                         g.Key.ToString("dd MMM yyyy"),
                    g => g.ToList()
                );

            return View("AllNotifications", grouped);
        }




        // Count No. of read issues
        [HttpGet]
        [Authorize]
        public JsonResult GetUnreadCount()
        {
            var userId = User.Identity.GetUserId();

            var unreadCount = db.Notifications
                .Count(n => n.UserId == userId && !n.IsRead);

            return Json(new { count = unreadCount }, JsonRequestBehavior.AllowGet);
        }




        [HttpGet]
        public ActionResult GetRecentNotifications()
        {
            // identify user
            var userId = User.Identity.GetUserId();



            // order notifications for user by the newest 5 into a list
            var notifications = db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
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

            //return new HttpStatusCodeResult(HttpStatusCode.OK); // Silent success response
            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize]
        public ActionResult MarkAllAsRead()
        {
            var userId = User.Identity.GetUserId();

            var unread = db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToList();

            foreach (var n in unread)
            {
                n.IsRead = true;
            }

            db.SaveChanges();

            return RedirectToAction("All");
        }

    }
}