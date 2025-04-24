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
        public int UnreadMessagesCount { get; set; }
        public int AppointmentsTodayCount { get; set; }
		public int NewAnnouncementsCount { get; set; }
		public int NewFeedbackCount { get; set; }
		public int AppointmentsToBeApprovedCount { get; set; }
        public int GuidanceSessionsForWeekCount { get; set; }


    }
}