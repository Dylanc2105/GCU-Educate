using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using GuidanceTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Collections.Generic;

namespace GuidanceTracker.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();

        public ActionResult Index()
        {
            return View();
        }

        // Returns all conversations with roles list for filter
        public async Task<ActionResult> Inbox()
        {
            var userId = User.Identity.GetUserId();
            var userManager = new UserManager<User>(new UserStore<User>(db));

            var conversations = await db.Conversations
                .Where(c => (c.UserOneId == userId && !c.IsArchivedByUserOne) || (c.UserTwoId == userId && !c.IsArchivedByUserTwo))
                .Include(c => c.UserOne)
                .Include(c => c.UserTwo)
                .Include(c => c.Messages)
                .OrderByDescending(c => c.UserOneId == userId ? (c.IsPinnedByUserOne ? 1 : 0) : (c.IsPinnedByUserTwo ? 1 : 0))
                .ThenByDescending(c => c.LastUpdated)
                .ToListAsync();

            var result = new List<object>();
            var uniqueRoles = new HashSet<string>();

            foreach (var convo in conversations)
            {
                var otherUser = convo.UserOneId == userId ? convo.UserTwo : convo.UserOne;
                var isOnline = !otherUser.AppearOffline
                               && otherUser.LastSeen.HasValue
                               && (DateTime.UtcNow - otherUser.LastSeen.Value).TotalMinutes <= 3;

                var role = (await userManager.GetRolesAsync(otherUser.Id)).FirstOrDefault() ?? "Unknown";
                uniqueRoles.Add(role);

                var lastMsg = convo.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();

                result.Add(new
                {
                    Id = convo.Id,
                    Name = $"{otherUser.FirstName} {otherUser.LastName}",
                    Role = role,
                    IsPinned = convo.UserOneId == userId ? convo.IsPinnedByUserOne : convo.IsPinnedByUserTwo,
                    LastMessage = GetMessagePreview(lastMsg),
                    UnreadCount = convo.Messages.Count(m => m.ReceiverId == userId && !m.IsRead),
                    UpdatedAt = convo.LastUpdated,
                    IsOnline = isOnline
                });
            }

            return Json(new { conversations = result, roles = uniqueRoles.ToList() }, JsonRequestBehavior.AllowGet);
        }

        // Get messages for a conversation with user status
        public async Task<ActionResult> GetMessages(int conversationId)
        {
            var userId = User.Identity.GetUserId();

            var convo = await db.Conversations
                .Include(c => c.UserOne)
                .Include(c => c.UserTwo)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (convo == null)
                return HttpNotFound("Conversation not found.");

            var otherUser = convo.UserOneId == userId ? convo.UserTwo : convo.UserOne;

            var isOnline = !otherUser.AppearOffline
                && otherUser.LastSeen.HasValue
                && (DateTime.UtcNow - otherUser.LastSeen.Value).TotalMinutes <= 3;

            var userManager = new UserManager<User>(new UserStore<User>(db));
            var otherRole = (await userManager.GetRolesAsync(otherUser.Id)).FirstOrDefault() ?? "Unknown";

            var messages = await db.Messages
                .Where(m => m.ConversationId == conversationId &&
                            (m.SenderId == userId || m.ReceiverId == userId))
                .Include(m => m.Sender)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            var unread = messages.Where(m => m.ReceiverId == userId && !m.IsRead).ToList();
            if (unread.Any())
            {
                foreach (var msg in unread)
                    msg.IsRead = true;
                await db.SaveChangesAsync();
            }

            var result = new
            {
                Role = otherRole,
                IsOnline = isOnline,
                Messages = messages.Select(m => new
                {
                    m.Id,
                    m.SenderId,
                    SenderName = $"{m.Sender.FirstName} {m.Sender.LastName}",
                    m.Content,
                    SentAt = m.SentAt.ToString("o"),
                    IsMine = m.SenderId == userId
                })
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> SendMessage(int conversationId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return Json(new { success = false, error = "Message content cannot be empty." });

            var userId = User.Identity.GetUserId();
            var convo = await db.Conversations.FindAsync(conversationId);

            if (convo == null)
                return HttpNotFound("Conversation not found.");

            var receiverId = (convo.UserOneId == userId) ? convo.UserTwoId : convo.UserOneId;

            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = userId,
                ReceiverId = receiverId,
                Content = content,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            convo.LastUpdated = DateTime.UtcNow;
            db.Messages.Add(message);
            await db.SaveChangesAsync();

            return Json(new { success = true });
        }

        // Get users allowed to start conversations with current user
        public async Task<ActionResult> GetAvailableUsers()
        {
            var userId = User.Identity.GetUserId();
            var userManager = new UserManager<User>(new UserStore<User>(db));
            var currentRoles = await userManager.GetRolesAsync(userId);
            var currentRole = currentRoles.FirstOrDefault();

            var allUsers = await db.Users.Where(u => u.Id != userId).ToListAsync();
            var result = new List<object>();

            foreach (var user in allUsers)
            {
                var roles = await userManager.GetRolesAsync(user.Id);
                var role = roles.FirstOrDefault() ?? "Unknown";

                bool isAllowed =
                    (currentRole == "Lecturer" && (role == "Lecturer" || role == "GuidanceTeacher" || role == "Student")) ||
                    (currentRole == "GuidanceTeacher" && (role == "Lecturer" || role == "GuidanceTeacher" || role == "Student")) ||
                    (currentRole == "Student" && (role == "Lecturer" || (role == "GuidanceTeacher" && userManager.IsInRole(user.Id, "GuidanceTeacher"))));

                if (isAllowed)
                {
                    result.Add(new
                    {
                        user.Id,
                        Name = $"{user.FirstName} {user.LastName}",
                        Role = role
                    });
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> SendIssueCardMessage(int conversationId, int issueId)
        {
            var userId = User.Identity.GetUserId();
            var convo = await db.Conversations
                .Include(c => c.UserOne)
                .Include(c => c.UserTwo)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (convo == null)
                return HttpNotFound("Conversation not found.");

            var receiver = convo.UserOneId == userId ? convo.UserTwo : convo.UserOne;
            var userManager = new UserManager<User>(new UserStore<User>(db));
            var role = (await userManager.GetRolesAsync(receiver.Id)).FirstOrDefault();

            if (role == "Student")
                return new HttpStatusCodeResult(403, "Cannot link issues in student conversations.");

            var issue = await db.Issues.Include(i => i.Student).FirstOrDefaultAsync(i => i.IssueId == issueId);

            if (issue == null)
                return HttpNotFound("Issue not found.");

            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = userId,
                ReceiverId = receiver.Id,
                Content = $@"<div class='mini-issue-card'>
                                <div class='mini-issue-header'><strong>{issue.IssueTitle}</strong><span class='mini-date'>{issue.CreatedAt.ToShortDateString()}</span></div>
                                <div class='mini-status'><strong>Status:</strong> {issue.IssueStatus}</div>
                                <div class='mini-description'><strong>For:</strong> {issue.Student.FirstName} {issue.Student.LastName}</div>
                                <div class='mini-actions'><a href='/Issue/ViewIssue?id={issue.IssueId}' class='mini-btn' target='_blank'>View Details</a></div>
                             </div>",
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            convo.LastUpdated = DateTime.UtcNow;
            db.Messages.Add(message);
            await db.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<ActionResult> SendMeetingCardMessage(int conversationId, int meetingId)
        {
            var userId = User.Identity.GetUserId();
            var convo = await db.Conversations
                .Include(c => c.UserOne)
                .Include(c => c.UserTwo)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (convo == null)
                return HttpNotFound("Conversation not found.");

            var receiver = convo.UserOneId == userId ? convo.UserTwo : convo.UserOne;
            var userManager = new UserManager<User>(new UserStore<User>(db));
            var role = (await userManager.GetRolesAsync(receiver.Id)).FirstOrDefault();

            if (role == "Student")
                return new HttpStatusCodeResult(403, "Cannot link meetings in student conversations.");

            var meeting = await db.Appointments
                .Include(a => a.Student)
                .FirstOrDefaultAsync(m => m.AppointmentId == meetingId);

            if (meeting == null)
                return HttpNotFound("Meeting not found.");

            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = userId,
                ReceiverId = receiver.Id,
                Content = $@"<div class='mini-meeting-card'>
                                <div class='mini-meeting-header'>
                                    <strong>{meeting.Student.FirstName} {meeting.Student.LastName}</strong>
                                    <span class='mini-date'>{meeting.AppointmentDate.ToShortDateString()}</span>
                                </div>
                                <div class='mini-description'>Meeting Details here</div>
                                <div class='mini-actions'>
                                    <a href='/Meeting/ViewMeeting?id={meeting.AppointmentId}' class='mini-btn' target='_blank'>View Details</a>
                                </div>
                             </div>",
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            convo.LastUpdated = DateTime.UtcNow;
            db.Messages.Add(message);
            await db.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<ActionResult> ArchiveConversation(int conversationId)
        {
            var userId = User.Identity.GetUserId();
            var convo = await db.Conversations.FirstOrDefaultAsync(c => c.Id == conversationId);

            if (convo == null)
                return HttpNotFound("Conversation not found.");

            if (convo.UserOneId == userId)
                convo.IsArchivedByUserOne = true;
            else if (convo.UserTwoId == userId)
                convo.IsArchivedByUserTwo = true;
            else
                return new HttpStatusCodeResult(403, "You are not part of this conversation.");

            await db.SaveChangesAsync();
            return Json(new { success = true });
        }

        // Helpers
        private string GetMessagePreview(Message msg)
        {
            if (msg == null) return null;
            if (msg.Content != null && msg.Content.Contains("mini-issue-card")) return "🔗 Linked an issue";
            if (msg.Content != null && msg.Content.Contains("mini-meeting-card")) return "📅 Linked a meeting";
            return StripHtml(msg.Content).Truncate(40);
        }

        private string StripHtml(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input ?? "", "<.*?>", string.Empty);
        }
    }
}

public static class StringExtensions
{
    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
    }
}
