using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;

namespace GuidanceTracker.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class User : IdentityUser
    {
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        [Display(Name = "Post Code")]
        public string Postcode { get; set; }
        [Display(Name = "Joined")]
        public DateTime RegistredAt { get; set; }

        // For resetting password
        public string ResetCode { get; set; } // Stores the 8-digit reset code
        public DateTime? ResetCodeExpiry { get; set; } // Expiration time of reset code

        // Messaging relationships
        public virtual ICollection<Message> SentMessages { get; set; }
        public virtual ICollection<Message> ReceivedMessages { get; set; }

        public virtual ICollection<Conversation> ConversationsAsUserOne { get; set; }
        public virtual ICollection<Conversation> ConversationsAsUserTwo { get; set; }

        public DateTime? LastSeen { get; set; } = DateTime.UtcNow;
        public bool AppearOffline { get; set; } = false;



        private ApplicationUserManager userManager;


        [NotMapped]
        public string CurrentRole
        {
            get
            {
                if (userManager == null)
                {
                    userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                }
                return userManager.GetRoles(Id).Single();
            }
        }


        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    //public class ApplicationDbContext : IdentityDbContext<User>
    //{
    //    public ApplicationDbContext()
    //        : base("DefaultConnection", throwIfV1Schema: false)
    //    {
    //    }

    //    public static ApplicationDbContext Create()
    //    {
    //        return new ApplicationDbContext();
    //    }
    //}
}