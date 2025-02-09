using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace GuidanceTracker.Models
{
    public class GuidanceTrackerDbContext:IdentityDbContext<User>
    {


        public GuidanceTrackerDbContext():base("DefaultConnection", throwIfV1Schema: false)
        {
            Database.SetInitializer(new DatabaseInitializer());
        }

        public static GuidanceTrackerDbContext Create()
        {
            return new GuidanceTrackerDbContext();
        }
    }
}