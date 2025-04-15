using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models.ViewModels
{
	public class GuidanceDashViewModel
	{
		public string FirstName { get; set; }
		public int NewIssuesCount { get; set; }
		public int NewNotificationsCount { get; set; }

        public int AppointmentsTodayCount { get; set; }
		public int NewAnnouncementsCount { get; set; }
		// also need smth for metrics here.


    }
}