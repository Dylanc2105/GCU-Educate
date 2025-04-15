using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models.ViewModels
{
	public class StudentDashViewModel
	{
        public string FirstName { get; set; }
        public int AppointmentsTodayCount { get; set; }
        public int NewMessagesCount { get; set; }
        public int NewAnnouncementsCount { get; set; }
    }
}