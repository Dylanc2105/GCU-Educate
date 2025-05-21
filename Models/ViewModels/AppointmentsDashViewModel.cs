using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models.ViewModels
{
    public class AppointmentsDashViewModel
    {
        public int AppointmentsTodayCount { get; set; }
        public int AppointmentsToBeApprovedCount { get; set; }
        public int AppointmentsForWeekCount { get; set; }

    }
}