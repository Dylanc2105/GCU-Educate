using GuidanceTracker.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GuidanceTracker.Controllers
{
    public class GuidanceSessionController : Controller
    {
        private readonly GuidanceTrackerDbContext db = new GuidanceTrackerDbContext();


        public ActionResult GuidanceSessionsForWeek()
        {
            var guidanceTeacher = User.Identity.GetUserId();

            List<GuidanceSession> sessions = db.GuidanceSessions.
                Where(g => g.Class.GuidanceTeacherId == guidanceTeacher).
                OrderBy(s=>s.Day).
                ToList();

            var sessionsByDay = sessions
                .Where(s => s.Day.DayOfWeek != DayOfWeek.Sunday && s.Day.DayOfWeek != DayOfWeek.Saturday)
                .GroupBy(s => s.Day.DayOfWeek)
                .OrderBy(g => g.Key)
                .ToList();

                return View(sessionsByDay);                     
        }
    }
}