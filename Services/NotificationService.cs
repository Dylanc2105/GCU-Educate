using GuidanceTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GuidanceTracker.Services
{
    public class NotificationService
    {
        private readonly GuidanceTrackerDbContext _db;

        public NotificationService()
        {
            _db = new GuidanceTrackerDbContext();
        }

        /// Sends a notification to all users — used for global announcements.
        public void CreateNotificationForAllUsers(string message, string redirectUrl, NotificationType type)
        {
            var users = _db.Users.ToList();

            foreach (var user in users)
            {
                _db.Notifications.Add(new Notification
                {
                    UserId = user.Id,
                    Message = message,
                    RedirectUrl = redirectUrl,
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    Type = type
                });
            }

            _db.SaveChanges();
        }

        /// Sends a single notification to one user.
        public void CreateNotificationForUser(string userId, string message, string redirectUrl, NotificationType type)
        {
            _db.Notifications.Add(new Notification
            {
                UserId = userId,
                Message = message,
                RedirectUrl = redirectUrl,
                CreatedAt = DateTime.Now,
                IsRead = false,
                Type = type
            });

            _db.SaveChanges();
        }

        /// Fetches a list of the most recent X notifications for the user.
        public List<Notification> GetRecentNotifications(string userId, int count = 5)
        {
            return _db.Notifications
                      .Where(n => n.UserId == userId)
                      .OrderByDescending(n => n.CreatedAt)
                      .Take(count)
                      .ToList();
        }

        /// Marks a specific notification as read, ensuring the user owns it.
        public bool MarkAsRead(int notificationId, string userId)
        {
            var notification = _db.Notifications.Find(notificationId);

            if (notification == null || notification.UserId != userId)
                return false;

            notification.IsRead = true;
            _db.SaveChanges();
            return true;
        }

        /// Updates or creates a single "new messages" notification for the user.
        public void NotifyNewMessage(string receiverId)
        {
            var existing = _db.Notifications
                .FirstOrDefault(n => n.UserId == receiverId &&
                                     n.Type == NotificationType.Message &&
                                     !n.IsRead);

            if (existing != null)
            {
                // Refresh the timestamp to make it rise in the list
                existing.Message = "You have new messages";
                existing.CreatedAt = DateTime.Now;
            }
            else
            {
                _db.Notifications.Add(new Notification
                {
                    UserId = receiverId,
                    Type = NotificationType.Message,
                    Message = "You have new messages",
                    RedirectUrl = "/Message/Index",
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });
            }

            _db.SaveChanges();
        }

        /// Notifies each guidance teacher only once per class that a lecturer
        /// has raised one or more issues for students in that class.
        public void NotifyGuidanceForClassIssues(List<string> studentIds, string raisedByUserId)
        {
            var user = _db.Users.Find(raisedByUserId);
            if (user == null) return;

            var fullName = $"{user.FirstName} {user.LastName}";

            // Group affected students by class and their guidance teacher
            var grouped = _db.Students
                .Where(s => studentIds.Contains(s.Id) && s.GuidanceTeacherId != null)
                .Select(s => new
                {
                    s.Class.ClassId,
                    ClassName = s.Class.ClassName,
                    s.GuidanceTeacherId
                })
                .Distinct()
                .ToList();

            // Notify once per (class + guidance teacher) pair
            foreach (var g in grouped)
            {
                _db.Notifications.Add(new Notification
                {
                    UserId = g.GuidanceTeacherId,
                    Type = NotificationType.Issue,
                    Message = $"{fullName} raised a new issue for Class {g.ClassName}",
                    RedirectUrl = "/Issue/Index",
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });
            }

            _db.SaveChanges();
        }


        /// Notifies all relevant users when a comment is posted on an issue (except the commenter).
        public void NotifyNewComment(Issue issue, string commenterId)
        {
            var usersToNotify = new[] { issue.StudentId, issue.GuidanceTeacherId, issue.LecturerId }
                                .Where(id => !string.IsNullOrEmpty(id) && id != commenterId)
                                .Distinct();

            foreach (var userId in usersToNotify)
            {
                CreateNotificationForUser(
                    userId,
                    $"New comment on Issue #{issue.IssueId}",
                    $"/Issue/ViewIssue/{issue.IssueId}",
                    NotificationType.Issue
                );
            }
        }

        /// Notifies the assigned guidance teacher when a student requests a new appointment.
        public void NotifyAppointmentRequested(Appointment appointment)
        {
            if (appointment == null || string.IsNullOrEmpty(appointment.GuidanceTeacherId))
                return;

            var student = _db.Students.Find(appointment.StudentId);
            var studentName = student != null
                ? $"{student.FirstName} {student.LastName}"
                : "A student";

            // Notify guidance teacher
            CreateNotificationForUser(
                appointment.GuidanceTeacherId,
                $"New appointment request from {studentName}",
                "/Appointment/AppointmentsToBeApproved",
                NotificationType.Appointment
            );
        }

    }
}