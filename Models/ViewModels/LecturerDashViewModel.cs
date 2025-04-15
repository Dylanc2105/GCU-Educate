using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models.ViewModels
{
	public class LecturerDashViewModel
	{
        public string FirstName { get; set; }
        public int NewProgressIssuesCount { get; set; }
        public int NewNotificationsCount { get; set; }
        public int NewAnnouncementsCount { get; set; }
    }
}