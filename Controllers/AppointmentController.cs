using GuidanceTracker.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GuidanceTracker.Controllers
{
    [Authorize(Roles = "GuidanceTeacher")]
    public class AppointmentController : Controller
    {
        private readonly GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();
        // GET: Appointment


        public ActionResult Create(int ticketId)
        {
            var appointment = new Appointment
            {
                TicketId = ticketId,
                Ticket = db.Tickets.Where(c => c.TicketId == ticketId).FirstOrDefault(),               
                Time = DateTime.Now,
                Room = "",
                AppointmentComment = ""
            };
            return View(appointment);
                

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Appointment model)
        {
            var appointment = new Appointment
            {
                Time = model.Time,
                Room = model.Room,
                AppointmentComment = model.AppointmentComment,
                TicketId=model.TicketId,

            };
            // var appointment = db.Appointments.Where(c => c.AppointmentId == model.AppointmentId).FirstOrDefault();
            //{

            //    appointment.Time = model.Time;
            //    appointment.AppointmentComment = model.AppointmentComment;
            //    appointment.Room = model.Room;
            //};
            db.Appointments.Add(appointment);
            db.SaveChanges();

            TempData["Success"] = "Appointment created successfully.";
            return RedirectToAction("ViewIssue", "Issue", new { id = model.TicketId });
        }



        public ActionResult Edit(int id)
        {
            var appointment = db.Appointments.Where(c => c.AppointmentId == id).FirstOrDefault();
            return View(appointment);
           
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Appointment model)
        {
            var appointment = db.Appointments.Where(c => c.AppointmentId == model.AppointmentId).FirstOrDefault();
            {
                //appointment.TicketId = model.TicketId;
                appointment.Time = model.Time;
                appointment.Room = model.Room;
                appointment.AppointmentComment = model.AppointmentComment;
            };
            db.SaveChanges();

            TempData["Success"] = "Appointment edited successfully.";
            return RedirectToAction("ViewIssue", "Issue", new { id = appointment.TicketId });
        }
    }
}