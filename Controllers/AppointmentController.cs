using GuidanceTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
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

        public List<Appointment> GetAppointments(string studentId)
        {
            var appointments = db.Appointments.Where(s => s.StudentId == studentId).ToList();
            return appointments;
        }


        public ActionResult Create(string studentId)
        {

            var student = db.Students.Find(studentId);
            GuidanceSession guidanceSession = db.GuidanceSessions.Where(g => g.ClassId == student.ClassId).FirstOrDefault();

            //assign ticket and time before sending appointment to view
            var appointment = new Appointment
            {
                StudentId = studentId,
                Student = db.Students.Find(studentId),      
                AppointmentDate = DateTime.Now,
                Room = "",
                AppointmentNotes = ""
                
            };
            return View(appointment);


        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Appointment model, string stringDate, string stringTime)
        {

            var appointment = new Appointment { };

            //no ViewBag message by default
            ViewBag.Message = "";

            //make sure that time and date are not empty
            if (stringDate == "" || stringTime == "" || stringTime == null || stringDate == null)
            {
                ViewBag.Message = "date field can not be empty";
            }

            //if not empty move on
            else
            {
                //make datetime string from date and time
                stringDate = stringDate + " " + stringTime;

                //try to transform string to datetime
                try
                {
                    appointment.AppointmentDate = DateTime.Parse(stringDate);                    
                }
                //if not successfull return alert
                catch
                {
                    ViewBag.Message = "incorrect input for date";
                }

                //if room field is empty return error
                if (model.Room == "" || model.Room == null)
                {
                    ViewBag.Message = "room field can not be empty";
                }
                //else assign room
                else
                {
                    appointment.Room = model.Room;
                }
            }
       

            appointment.AppointmentNotes = model.AppointmentNotes;
            appointment.StudentId = model.StudentId;
            appointment.Student = db.Students.Where(c => c.Id == model.StudentId).FirstOrDefault(); 
            appointment.AppointmentStatus = model.AppointmentStatus;
            appointment.GuidanceTeacherId = User.Identity.GetUserId();
            appointment.GuidanceTeacher = db.GuidanceTeachers.Find(appointment.GuidanceTeacherId);
            

            //if viewbag has any error message => return them to view
            if (ViewBag.Message != "")
            {
                return View(appointment);
            }
            else
            {
                //add Appointment to db
                db.Appointments.Add(appointment);
                db.SaveChanges();
                TempData["Success"] = "Appointment created successfully.";
                //redirect to issue page
                return RedirectToAction("Index", "Issue");
            }
        }



        public ActionResult Edit(int id)
        {
            //get appointemnt from db
            var appointment = db.Appointments.Where(c => c.AppointmentId == id).FirstOrDefault();
            return View(appointment);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Appointment model, string stringDate, string stringTime)
        {

            //no ViewBag message by default
            ViewBag.Message = "";

            //get appointment from db
            var appointment = db.Appointments.Where(c => c.AppointmentId == model.AppointmentId).FirstOrDefault();

            //make sure that time and date are not empty
            if (stringDate == "" || stringTime == "" || stringTime == null || stringDate == null)
            {
                ViewBag.Message = "date field can not be empty";
            }

            //if not empty move on
            else
            {
                //make datetime string from date and time
                stringDate = stringDate + " " + stringTime;

                //try to transform string to datetime
                try
                {
                    appointment.AppointmentDate = DateTime.Parse(stringDate);
                }
                //if not successfull return alert
                catch
                {
                    ViewBag.Message = "incorrect input for date";
                }

                //if room field is empty return error
                if (model.Room == "" || model.Room == null)
                {
                    ViewBag.Message = "room field can not be empty";
                }
                //else assign room
                else
                {
                    appointment.Room = model.Room;
                }
            }
            if (ViewBag.Message != "")
            {
                return View(appointment);
            }
            else
            { 
            appointment.AppointmentNotes = model.AppointmentNotes;
            db.SaveChanges();
            TempData["Success"] = "Appointment edited successfully.";
            return RedirectToAction("Index", "Issue");
            }
        }
    }
}