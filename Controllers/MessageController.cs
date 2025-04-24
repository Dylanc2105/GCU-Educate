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
                .Where(c => c.UserOneId == userId || c.UserTwoId == userId)
                .Include(c => c.UserOne)
                .Include(c => c.UserTwo)
                .Include(c => c.Messages)
                .OrderByDescending(c => c.LastUpdated)
                .ToListAsync();

            var result = new List<object>();

            foreach (var c in conversations)
            {
                var otherUser = c.UserOneId == userId ? c.UserTwo : c.UserOne;
                var role = (await userManager.GetRolesAsync(otherUser.Id)).FirstOrDefault();

                result.Add(new
                {
                    Id = c.Id,
                    Name = $"{otherUser.FirstName} {otherUser.LastName}",
                    Role = role,
                    IsPinned = c.UserOneId == userId ? c.IsPinnedByUserOne : c.IsPinnedByUserTwo,
                    LastMessage = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault()?.Content,
                    UnreadCount = c.Messages.Count(m => m.ReceiverId == userId && !m.IsRead),
                    UpdatedAt = c.LastUpdated
                });
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public async Task<ActionResult> GetMessages(int conversationId)
        {
            var userId = User.Identity.GetUserId();

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

            var result = messages.Select(m => new
            {
                m.Id,
                m.SenderId,
                SenderName = $"{m.Sender.FirstName} {m.Sender.LastName}",
                m.Content,
                SentAt = m.SentAt.ToString("o"),
                IsMine = m.SenderId == userId
            });

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
        public async Task<ActionResult> StartConversation(string userId)
        {
            var myId = User.Identity.GetUserId();

            var convo = await db.Conversations
                .FirstOrDefaultAsync(c =>
                    (c.UserOneId == myId && c.UserTwoId == userId) ||
                    (c.UserOneId == userId && c.UserTwoId == myId));

            if (convo == null)
            {
                convo = new Conversation
                {
                    UserOneId = myId,
                    UserTwoId = userId,
                    LastUpdated = DateTime.UtcNow
                };
                db.Conversations.Add(convo);
                await db.SaveChangesAsync();
            }

            var otherUser = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            return Json(new { conversationId = convo.Id, name = $"{otherUser.FirstName} {otherUser.LastName}" });
        }
    }
}