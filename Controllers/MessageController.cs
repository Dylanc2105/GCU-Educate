using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using GuidanceTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Collections.Generic;
using GuidanceTracker.Services;

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
                .OrderByDescending(c => c.LastUpdated)
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

            // Assuming _notificationService is injected in your controller

            // Send notification
            new NotificationService().NotifyNewMessage(receiverId);


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

            // Determine the receiver
            var receiver = convo.UserOneId == userId ? convo.UserTwo : convo.UserOne;

            // Prevent linking issues in student conversations
            var userManager = new UserManager<User>(new UserStore<User>(db));
            var role = (await userManager.GetRolesAsync(receiver.Id)).FirstOrDefault();
            if (role == "Student")
                return new HttpStatusCodeResult(403, "Cannot link issues in student conversations.");

            // Get the issue with student details
            var issue = await db.Issues
                .Include(i => i.Student)
                .FirstOrDefaultAsync(i => i.IssueId == issueId);

            if (issue == null)
                return HttpNotFound("Issue not found.");

            // Mini issue card HTML
            var html = $@"
<div class='mini-issue-card' style='background:#fff;border-radius:8px;box-shadow:0 1.5px 6px #bda9da27;padding:14px 17px 13px 17px;margin:8px 0 3px 0;border-left:5px solid #7F3D98;max-width:380px;'>
    <div class='mini-issue-title' style='font-size:1.12em;font-weight:700;color:#7F3D98;margin-bottom:2px;'>{issue.IssueTitle}</div>
    <div class='mini-issue-meta' style='font-size:0.93em;color:#7F3D98;margin-bottom:4px;'>
        <span class='mini-issue-date' style='margin-right:15px;'><b>Date:</b> {issue.CreatedAt:dd MMM yyyy}</span>
        <span class='mini-issue-status'><b>Status:</b> {issue.IssueStatus}</span>
    </div>
    <div style='display:flex;gap:7px;'>
        <a href='/Issue/ViewIssue/{issue.IssueId}' target='_blank' class='mini-issue-btn' style='background:#7F3D98;color:#fff;padding:5px 16px;border-radius:7px;text-decoration:none;font-weight:700;font-size:0.96em;'>View in Student Hub</a>
    </div>
</div>";

            // Create and save the message
            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = userId,
                ReceiverId = receiver.Id,
                Content = html,
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
            try
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

                // Prevent linking meetings in student conversations
                if (role == "Student")
                    return new HttpStatusCodeResult(403, "Cannot link meetings in student conversations.");

                var meeting = await db.Appointments
                    .Include(a => a.Student)
                    .Include(a => a.GuidanceTeacher)
                    .Include(a => a.GuidanceSession)
                    .FirstOrDefaultAsync(m => m.AppointmentId == meetingId);

                if (meeting == null)
                    return HttpNotFound("Meeting not found.");

                var html = $@"
<div class='mini-meeting-card' style='background:#fff;border-radius:8px;box-shadow:0 1.5px 6px #bda9da27;padding:14px 17px 13px 17px;margin:8px 0 3px 0;border-left:5px solid #B61A25;max-width:410px;'>
    <div class='mini-meeting-title' style='font-size:1.12em;font-weight:700;color:#B61A25;margin-bottom:2px;'>
        Meeting with {meeting.Student?.FirstName} {meeting.Student?.LastName}
    </div>
    <div class='mini-meeting-meta' style='font-size:0.93em;color:#B61A25;margin-bottom:2px;'>
        <span style='margin-right:15px;'><b>Date:</b> {meeting.AppointmentDate:dddd, dd MMM yyyy}</span>
        <span><b>Status:</b> {meeting.AppointmentStatus}</span>
    </div>
    <div class='mini-meeting-meta' style='font-size:0.93em;color:#B61A25;margin-bottom:6px;'>
        <span style='margin-right:15px;'><b>Time:</b> {meeting.StartTime:hh\\:mm} - {meeting.EndTime:hh\\:mm}</span>
        <span><b>Room:</b> {meeting.Room ?? "N/A"}</span>
    </div>
    <div class='mini-meeting-notes' style='font-size:1em;color:#444;margin-bottom:7px;word-break:break-word;'>
        {(string.IsNullOrWhiteSpace(meeting.AppointmentNotes) ? "<i>No notes</i>" : meeting.AppointmentNotes)}
    </div>
    <div style='display:flex;gap:7px;'>
        <a href='/Meeting/ViewMeeting?id={meeting.AppointmentId}' target='_blank' class='mini-issue-btn' style='background:#B61A25;color:#fff;padding:5px 16px;border-radius:7px;text-decoration:none;font-weight:700;font-size:0.96em;'>View Meeting Details</a>
    </div>
</div>";

                var message = new Message
                {
                    ConversationId = conversationId,
                    SenderId = userId,
                    ReceiverId = receiver.Id,
                    Content = html,
                    SentAt = DateTime.UtcNow,
                    IsRead = false
                };

                convo.LastUpdated = DateTime.UtcNow;
                db.Messages.Add(message);
                await db.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Log the error and return message for debug
                return Json(new { success = false, error = ex.Message, stack = ex.StackTrace });
            }
        }


        [HttpPost]
        public async Task<ActionResult> StartConversation(string userId)
        {
            var myId = User.Identity.GetUserId();
            if (userId == myId)
                return Json(new { success = false, error = "You can't chat with yourself." });

            // Try find existing conversation (order does not matter)
            var convo = await db.Conversations
                .FirstOrDefaultAsync(c =>
                    (c.UserOneId == myId && c.UserTwoId == userId) ||
                    (c.UserOneId == userId && c.UserTwoId == myId)
                );

            // Create new conversation if doesn't exist
            if (convo == null)
            {
                convo = new Conversation
                {
                    UserOneId = myId,
                    UserTwoId = userId,
                    LastUpdated = DateTime.UtcNow,
                    IsPinnedByUserOne = false,
                    IsPinnedByUserTwo = false,
                    IsArchivedByUserOne = false,
                    IsArchivedByUserTwo = false
                };
                db.Conversations.Add(convo);
                await db.SaveChangesAsync();
            }

            // Get the name of the other user for displaying in the chat header
            var otherUser = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var otherName = otherUser != null
                ? (otherUser.FirstName + " " + otherUser.LastName).Trim()
                : "Unknown User";

            return Json(new { success = true, conversationId = convo.Id, name = otherName });
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

            convo.ArchivedAt = DateTime.UtcNow; // <-- THIS LINE IS IMPORTANT!
            await db.SaveChangesAsync();
            return Json(new { success = true });
        }

        public async Task<ActionResult> GetArchivedConversations()
        {
            var userId = User.Identity.GetUserId();

            var archived = await db.Conversations
                .Include(c => c.UserOne)
                .Include(c => c.UserTwo)
                .Include(c => c.Messages)
                .Where(c =>
                    (c.UserOneId == userId && c.IsArchivedByUserOne) ||
                    (c.UserTwoId == userId && c.IsArchivedByUserTwo))
                .OrderByDescending(c => c.ArchivedAt ?? c.LastUpdated)
                .ToListAsync();

            var result = archived.Select(convo => {
                var otherUser = convo.UserOneId == userId ? convo.UserTwo : convo.UserOne;
                var lastMsg = convo.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
                return new
                {
                    Id = convo.Id,
                    Name = $"{otherUser.FirstName} {otherUser.LastName}",
                    LastMessage = lastMsg != null ? StripHtml(lastMsg.Content).Truncate(40) : "",
                    ArchivedAt = convo.ArchivedAt ?? convo.LastUpdated
                };
            }).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetArchivedMessages(int conversationId)
        {
            var userId = User.Identity.GetUserId();

            var convo = await db.Conversations
                .Include(c => c.UserOne)
                .Include(c => c.UserTwo)
                .Include(c => c.Messages.Select(m => m.Sender))
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            // Only allow user to see archived conversations they own
            if (convo == null)
                return HttpNotFound("Conversation not found.");

            bool isArchived =
                (convo.UserOneId == userId && convo.IsArchivedByUserOne)
                || (convo.UserTwoId == userId && convo.IsArchivedByUserTwo);

            if (!isArchived)
                return new HttpStatusCodeResult(403, "Not archived for current user.");

            var otherUser = convo.UserOneId == userId ? convo.UserTwo : convo.UserOne;

            var messages = convo.Messages
                .Where(m => m.ConversationId == conversationId && (m.SenderId == userId || m.ReceiverId == userId))
                .OrderBy(m => m.SentAt)
                .ToList();

            var result = new
            {
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




        // GET: /Meeting/GetUserMeetings
        public ActionResult GetUserMeetings()
        {
            var userId = User.Identity.GetUserId();

            // Only show meetings for the user (adapt if roles differ)
            var meetings = db.Appointments
                .Include(a => a.Student)
                .Where(a => a.StudentId == userId || a.GuidanceTeacherId == userId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.StartTime)
                .Select(a => new
                {
                    MeetingId = a.AppointmentId,
                    Title = "Meeting with " + ((a.Student != null) ? (a.Student.FirstName + " " + a.Student.LastName) : "N/A"),
                    StudentName = (a.Student != null) ? (a.Student.FirstName + " " + a.Student.LastName) : "N/A",
                    Date = a.AppointmentDate != null ? a.AppointmentDate.ToString("o") : null,
                })


                .ToList();

            return Json(meetings, JsonRequestBehavior.AllowGet);
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
