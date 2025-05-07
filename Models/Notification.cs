using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Management;

namespace GuidanceTracker.Models
{
    public class Notification
    {
        // attributes
        public int Id { get; set; }
        public string UserId { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; }
        public string RedirectUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }

    public enum NotificationType
    {
        Announcement,
        Appointment,
        Message,
        Issue
    }

}