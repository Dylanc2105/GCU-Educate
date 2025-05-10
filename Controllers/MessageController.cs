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
        public async Task<ActionResult> Inbox()
        {
            var userId = User.Identity.GetUserId();
            var userManager = new UserManager<User>(new UserStore<User>(db));

            var conversations = await db.Conversations
                .Where(c =>
                    (c.UserOneId == userId && !c.IsArchivedByUserOne) ||
                    (c.UserTwoId == userId && !c.IsArchivedByUserTwo))
                .Include(c => c.UserOne)
                .Include(c => c.UserTwo)
                .Include(c => c.Messages)
                .OrderByDescending(c => c.LastUpdated)
                .ToListAsync();

            var result = new List<object>();
            var uniqueRoles = new HashSet<string>();

            foreach (var c in conversations)
            {
                var otherUser = c.UserOneId == userId ? c.UserTwo : c.UserOne;
                var role = (await userManager.GetRolesAsync(otherUser.Id)).FirstOrDefault();
                uniqueRoles.Add(role);

                result.Add(new
                {
                    Id = c.Id,
                    Name = $"{otherUser.FirstName} {otherUser.LastName}",
                    Role = role,
                    IsPinned = c.UserOneId == userId ? c.IsPinnedByUserOne : c.IsPinnedByUserTwo,
                    LastMessage = GetMessagePreview(c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault()),
                    UnreadCount = c.Messages.Count(m => m.ReceiverId == userId && !m.IsRead),
                    UpdatedAt = c.LastUpdated
                });
            }

            return Json(new { conversations = result, roles = uniqueRoles.ToList() }, JsonRequestBehavior.AllowGet);
        }


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
            var userManager = new UserManager<User>(new UserStore<User>(db));
            var otherRole = (await userManager.GetRolesAsync(otherUser.Id)).FirstOrDefault();

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
            var userId = User.Identity.GetUserId();
            var convo = await db.Conversations.FindAsync(conversationId);

            if (convo == null)
                return HttpNotFound("Conversation not found.");

            var receiverId = convo.UserOneId == userId ? convo.UserTwoId : convo.UserOneId;

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

        public async Task<ActionResult> GetAvailableUsers()
        {
            var userId = User.Identity.GetUserId();
            var currentUser = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            var userManager = new UserManager<User>(new UserStore<User>(db));
            var currentRoles = await userManager.GetRolesAsync(userId);
            var currentRole = currentRoles.FirstOrDefault();

            var allUsers = await db.Users.Where(u => u.Id != userId).ToListAsync();
            var result = new List<object>();

            foreach (var user in allUsers)
            {
                var roles = await userManager.GetRolesAsync(user.Id);
                var role = roles.FirstOrDefault();

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
                Content = $@"<div class='mini-issue-card'><div class='mini-issue-header'><strong>{issue.IssueTitle}</strong><span class='mini-date'>{issue.CreatedAt.ToShortDateString()}</span></div><div class='mini-status'><strong>Status:</strong> {issue.IssueStatus}</div><div class='mini-description'><strong>For:</strong> {issue.Student.FirstName} {issue.Student.LastName}</div><div class='mini-actions'><a href='/Issue/ViewIssue?id={issue.IssueId}' class='mini-btn' target='_blank'>View Details</a></div></div>",
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            convo.LastUpdated = DateTime.UtcNow;
            db.Messages.Add(message);
            await db.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult ArchiveAllMessages()
        {
            var userId = User.Identity.GetUserId();
            var messages = db.Messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .ToList();

            foreach (var msg in messages)
            {
                msg.IsArchived = true;
            }

            db.SaveChanges();
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

        [HttpPost]
        public async Task<ActionResult> ArchiveAllMessagesForCurrentUser()
        {
            var userId = User.Identity.GetUserId();

            var conversations = await db.Conversations
                .Where(c => c.UserOneId == userId || c.UserTwoId == userId)
                .ToListAsync();

            foreach (var convo in conversations)
            {
                if (convo.UserOneId == userId) convo.IsArchivedByUserOne = true;
                else if (convo.UserTwoId == userId) convo.IsArchivedByUserTwo = true;
            }

            await db.SaveChangesAsync();
            return Json(new { success = true });
        }


        [HttpGet]
        public async Task<ActionResult> GetArchivedConversations()
        {
            var userId = User.Identity.GetUserId();
            var userManager = new UserManager<User>(new UserStore<User>(db));

            var archived = await db.Conversations
                .Where(c =>
                    (c.UserOneId == userId && c.IsArchivedByUserOne) ||
                    (c.UserTwoId == userId && c.IsArchivedByUserTwo))
                .Include(c => c.UserOne)
                .Include(c => c.UserTwo)
                .Include(c => c.Messages)
                .OrderByDescending(c => c.LastUpdated)
                .ToListAsync();

            var result = new List<object>();

            foreach (var c in archived)
            {
                var otherUser = c.UserOneId == userId ? c.UserTwo : c.UserOne;
                var role = (await userManager.GetRolesAsync(otherUser.Id)).FirstOrDefault();

                result.Add(new
                {
                    Id = c.Id,
                    Name = $"{otherUser.FirstName} {otherUser.LastName}",
                    Role = role,
                    LastMessage = GetMessagePreview(c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault()),
                    ArchivedAt = c.LastUpdated.ToString("o")
                });
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
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

        [HttpPost]
        public async Task<ActionResult> MarkAllAsRead()
        {
            var userId = User.Identity.GetUserId();

            var unreadMessages = await db.Messages
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }

            await db.SaveChangesAsync();

            return Json(new { success = true });
        }


        [HttpPost]
        public async Task<ActionResult> StartConversation(string userId)
        {
            var myId = User.Identity.GetUserId();

            // Look for existing conversation that is NOT archived for either user
            var convo = await db.Conversations.FirstOrDefaultAsync(c =>
                ((c.UserOneId == myId && c.UserTwoId == userId && !c.IsArchivedByUserOne && !c.IsArchivedByUserTwo) ||
                 (c.UserOneId == userId && c.UserTwoId == myId && !c.IsArchivedByUserOne && !c.IsArchivedByUserTwo))
            );

            if (convo == null)
            {
                // No unarchived convo exists — start a new one
                convo = new Conversation
                {
                    UserOneId = myId,
                    UserTwoId = userId,
                    LastUpdated = DateTime.UtcNow,
                    IsArchivedByUserOne = false,
                    IsArchivedByUserTwo = false
                };

                db.Conversations.Add(convo);
                await db.SaveChangesAsync();
            }

            var otherUser = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            return Json(new { conversationId = convo.Id, name = $"{otherUser.FirstName} {otherUser.LastName}" });
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