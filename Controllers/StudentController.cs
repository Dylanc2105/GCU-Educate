using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GuidanceTracker.Controllers
{
    public class StudentController : Controller
    {
        [Authorize(Roles = "Student")]
        // GET: Student
        public ActionResult StudentDash()
        {
            return View("StudentDash");
        }
    }
}