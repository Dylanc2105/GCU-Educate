using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GuidanceTracker.Controllers
{
    public class GuidanceTeacherController : Controller
    {
        [Authorize(Roles = "GuidanceTeacher")]
        // GET: GuidanceTeacher
        public ActionResult GuidanceTeacherDash()
        {
            return View("GuidanceTeacherDash");
        }
    }
}