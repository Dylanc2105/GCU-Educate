using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GuidanceTracker.Controllers
{
    
    public class TicketsController : Controller
    {
        
        // GET: Ticket
        public ActionResult Index()
        {
            return View();
        }
    }
}