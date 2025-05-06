using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models.ViewModels
{
    public class AppointmentViewModel
    {
        public int AppointmentId { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string AppointmentNotes { get; set; }
        public int GuidanceSessionId { get; set; }
        public string Room { get; set; }
        public AppointmentStatus AppointmentStatus { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public List<TimeSlotViewModel> AvailableTimeSlots { get; set; }
        public int? IssueId { get; set; }
        public string IssueTitle { get; set; }
        public string IssueDescription { get; set; }
    }

    public class TimeSlotViewModel
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }

        public string DisplayText => $"{StartTime.ToString(@"hh\:mm")} - {EndTime.ToString(@"hh\:mm")}";
    }
}