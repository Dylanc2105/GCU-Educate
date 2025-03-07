using GuidanceTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GuidanceTracker.Controllers
{
    public class LecturerController : Controller
    {

        private GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();

        [Authorize(Roles = "Lecturer")]
        // GET: Lecturer
        public ActionResult LecturerDash()
        {
            return View("LecturerDash");
        }

        public ActionResult ViewAllStudents()
        {
            var students = db.Students.OrderBy(s => s.RegistredAt).ToList();

            return View(students);
        }
    }
}