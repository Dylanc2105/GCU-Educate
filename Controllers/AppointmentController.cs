using GuidanceTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
                AppointmentNotes = "",
                GuidanceSession = guidanceSession,
                Room = guidanceSession.Room,

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

            }

            var student = db.Students.Find(model.StudentId);
            GuidanceSession guidanceSession = db.GuidanceSessions.Where(g => g.ClassId == student.ClassId).FirstOrDefault();

            appointment.AppointmentNotes = model.AppointmentNotes;
            appointment.StudentId = model.StudentId;
            appointment.Student = db.Students.Where(c => c.Id == model.StudentId).FirstOrDefault();
            appointment.AppointmentStatus = model.AppointmentStatus;
            appointment.GuidanceTeacherId = User.Identity.GetUserId();
            appointment.GuidanceTeacher = db.GuidanceTeachers.Find(appointment.GuidanceTeacherId);
            appointment.Room = model.Room;

            appointment.GuidanceSession = db.GuidanceSessions.Where(g => g.ClassId == student.ClassId).FirstOrDefault();



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


        public ActionResult CreateAppointmentWithIssue(int issueId)
        {
            var issue = db.Issues.
                Where(i => i.IssueId == issueId).
                Include(i => i.Student).FirstOrDefault();

            GuidanceSession guidanceSession = db.GuidanceSessions.Where(g => g.ClassId == issue.Student.ClassId).FirstOrDefault();

            //assign time, guidance session  before sending appointment to view
            var appointment = new Appointment
            {
                StudentId = issue.StudentId,
                Student = db.Students.Find(issue.StudentId),
                AppointmentDate = DateTime.Now,
                AppointmentNotes = "",
                GuidanceSession = guidanceSession,
                Room = guidanceSession.Room,
                IssueId = issueId,
                Issue = issue
            };

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAppointmentWithIssue(Appointment model, string stringDate, string stringTime)
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

            }

            var student = db.Students.Find(model.StudentId);
            GuidanceSession guidanceSession = db.GuidanceSessions.Where(g => g.ClassId == student.ClassId).FirstOrDefault();

            appointment.AppointmentNotes = model.AppointmentNotes;
            appointment.StudentId = model.StudentId;
            appointment.Student = student;
            appointment.Student = db.Students.Where(c => c.Id == model.StudentId).FirstOrDefault();
            appointment.AppointmentStatus = model.AppointmentStatus;
            appointment.GuidanceTeacherId = User.Identity.GetUserId();
            appointment.GuidanceTeacher = db.GuidanceTeachers.Find(appointment.GuidanceTeacherId);
            appointment.Room = model.Room;
            appointment.IssueId = model.IssueId;
            appointment.Issue = db.Issues.Find(appointment.IssueId); ;
            appointment.GuidanceSession = db.GuidanceSessions.Where(g => g.ClassId == student.ClassId).FirstOrDefault();



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

        public ActionResult AppointmentsToBeApproved()
        {
            //gett all appointments with students and their class data
            var appointments = db.Appointments
                .Where(a => a.AppointmentStatus == AppointmentStatus.Requested)
                .Include(a => a.Student)
                .Include(a => a.Student.Class)
                .ToList();

            return View(appointments);
        }



        public ActionResult ApproveAppointment(int id)
        {
            var appointment = db.Appointments.Find(id);

            //if appointment found in db correctly : update status
            if (appointment != null)
            {
                appointment.AppointmentStatus = AppointmentStatus.Scheduled;
                db.SaveChanges();
            }
            else
            {
                return HttpNotFound();
            }
            return RedirectToAction("AppointmentsToBeApproved");
        }





        public ActionResult CancelAppointment(int id)
        {
            var appointment = db.Appointments.Find(id);

            //if appointment found in db correctly : update status
            if (appointment != null)
            {
                appointment.AppointmentStatus = AppointmentStatus.Cancelled;
                db.SaveChanges();
            }
            else
            {
                return HttpNotFound();
            }
            return RedirectToAction("AppointmentsToBeApproved");
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

                //make sure that date or time been changed
                if (DateTime.Parse(stringDate) != appointment.AppointmentDate)
                {
                    //try to transform string to datetime
                    try
                    {
                        appointment.AppointmentDate = DateTime.Parse(stringDate);
                        appointment.AppointmentStatus = AppointmentStatus.Rescheduled;
                    }
                    //if not successfull return alert
                    catch
                    {
                        ViewBag.Message = "incorrect input for date";
                    }
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
        public ActionResult AppointmentsForSessionPartial(int id)
        {
            var appointments = db.Appointments.
                Where(a => a.GuidanceSessionId == id).
                Include(a => a.Student).
                Include(a => a.GuidanceTeacher).
                ToList();

            var session = db.GuidanceSessions.Find(id);

            ViewBag.Class = session.Class.ClassName;
            ViewBag.GSTime = session.Time;
            ViewBag.Room = session.Room;


            return PartialView("_AppointmentsForSession", appointments);
        }
    }
}