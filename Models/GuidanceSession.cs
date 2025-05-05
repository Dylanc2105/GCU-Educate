using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models
{
    public class GuidanceSession
    {
        public int GuidanceSessionId { get; set; }
        public string Room { get; set; }

        // The session start time
        public TimeSpan Time { get; set; }

        // The session date
        public DateTime Day { get; set; }

        // Session duration is one hour (60 minutes)
        [NotMapped]
        public TimeSpan Duration => TimeSpan.FromHours(1);

        // End time calculated from start time plus duration
        [NotMapped]
        public TimeSpan EndTime => Time.Add(Duration);

        // Each appointment is 10 minutes, so we can have 6 slots per hour
        [NotMapped]
        public int MaxAppointmentSlots => 6;

        // Navigation properties
        [Required]
        public int ClassId { get; set; }

        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; }

        // Get all possible time slots for this session (each 10 minutes)
        [NotMapped]
        public List<TimeSpan> AllTimeSlots
        {
            get
            {
                var slots = new List<TimeSpan>();
                for (int i = 0; i < MaxAppointmentSlots; i++)
                {
                    slots.Add(Time.Add(TimeSpan.FromMinutes(i * 10)));
                }
                return slots;
            }
        }

        // Method to check if a specific time slot is available
        public bool IsTimeSlotAvailable(TimeSpan timeSlot)
        {
            if (Appointments == null)
                return true;

            // Check if this timeSlot falls within the session time range
            if (timeSlot < Time || timeSlot >= EndTime)
                return false;

            // Check if there's already an appointment at this time slot
            return !Appointments.Any(a => a.StartTime == timeSlot &&
                a.AppointmentStatus != AppointmentStatus.Cancelled);
        }

        // Get all available time slots (not booked)
        public List<TimeSpan> GetAvailableTimeSlots()
        {
            return AllTimeSlots.Where(IsTimeSlotAvailable).ToList();
        }
    }
}