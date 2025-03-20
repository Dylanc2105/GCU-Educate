using System;
using System.Linq;
using System.Web.Mvc;
using GuidanceTracker.Models;
using Microsoft.AspNet.Identity;

namespace GuidanceTracker.Controllers
{
    [Authorize(Roles = "GuidanceTeacher, Lecturer")]
    public class CommentController : Controller
    {
        private readonly GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();

        // ✅ Add a Comment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddComment(int ticketId, string Content)
        {
            if (string.IsNullOrWhiteSpace(Content))
            {
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectToAction("ViewTicket", "Issue", new { id = ticketId });
            }

            var userId = User.Identity.GetUserId();

            var comment = new Comment
            {
                IssueId = ticketId,
                UserId = userId,
                Content = Content,
                CreatedAt = DateTime.Now
            };

            db.Comments.Add(comment);
            db.SaveChanges();

            TempData["Success"] = "Comment added successfully.";
            return RedirectToAction("ViewTicket", "Issue", new { id = ticketId });
        }

        // ✅ Fetch Comments for a Ticket (AJAX)
        public JsonResult GetComments(int ticketId)
        {
            var comments = db.Comments
                .Where(c => c.IssueId == ticketId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    c.CommentId,
                    c.Content,
                    CreatedAt = c.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    UserName = c.User.FirstName + " " + c.User.LastName
                })
                .ToList();

            return Json(comments, JsonRequestBehavior.AllowGet);
        }
    }
}
