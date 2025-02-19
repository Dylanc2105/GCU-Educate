using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GuidanceTracker.Controllers
{
    public class LecturerController : Controller
    {
        [Authorize(Roles = "Lecturer")]
        // GET: Lecturer
        public ActionResult LecturerDash()
        {
            return View("LecturerDash");
        }
    }
}