using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security.DataProtection;
using GuidanceTracker.Models;
using Microsoft.AspNet.Identity.Owin;

namespace GuidanceTracker.Controllers
{
    public class ResetPasswordController : Controller
    {
        // GET: ResetCodeVerification
        public ActionResult ResetCodeVerification()
        {
            if (TempData["UserEmail"] != null)
            {
                ViewBag.UserEmail = TempData["UserEmail"];
                TempData.Keep("UserEmail");
            }
            return View("ResetCodeVerification");
        }

        // POST: Verify the reset code and store the user email in TempData.
        [HttpPost]
        public ActionResult VerifyCode(string resetCode, string userEmail)
        {
            // Db contex to access users
            using (var context = new GuidanceTrackerDbContext())
            {
                var user = context.Users.FirstOrDefault(u => u.Email == userEmail);

                // Check if the user exists
                if (user == null)
                {
                    ViewBag.ErrorMessage = "User not found.";
                    return View("ResetCodeVerification");
                }

                // Validate the reset code: ensure its not empty, hasnt expired, and matches the stored code
                if (string.IsNullOrEmpty(user.ResetCode) ||
                    user.ResetCodeExpiry == null || 
                    user.ResetCodeExpiry < DateTime.UtcNow ||
                    !string.Equals(user.ResetCode.Trim(), resetCode.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    ViewBag.ErrorMessage = "Invalid or expired reset code.";
                    return View("ResetCodeVerification");
                }

                // If reset code is valid, store confirmation in TempData and proceed to reset password page
                TempData["CodeVerified"] = true;
                TempData["UserEmail"] = userEmail;
                return RedirectToAction("ResetPassword");
            }

        }

        // GET: ResetPassword page. Only accessible after code verification.
        public ActionResult ResetPassword()
        {
            if (TempData["CodeVerified"] == null || !(bool)TempData["CodeVerified"])
            {
                return RedirectToAction("ResetCodeVerification");
            }

            TempData.Keep("CodeVerified");
            TempData.Keep("UserEmail");
            ViewBag.Email = TempData["UserEmail"];
            return View("ResetPassword");
        }

        // POST: Process the new password submission.
        [HttpPost]
        public ActionResult SubmitNewPassword(string newPassword, string confirmPassword)
        {
            // Basic field validations.
            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.ErrorMessage = "Please fill in both fields.";
                ViewBag.Email = TempData["UserEmail"];
                TempData.Keep("UserEmail");
                return View("ResetPassword");
            }
            if (newPassword != confirmPassword)
            {
                ViewBag.ErrorMessage = "Passwords do not match.";
                ViewBag.Email = TempData["UserEmail"];
                TempData.Keep("UserEmail");
                return View("ResetPassword");
            }
            if (newPassword.Length < 6)
            {
                ViewBag.ErrorMessage = "Password must be at least 6 characters long.";
                ViewBag.Email = TempData["UserEmail"];
                TempData.Keep("UserEmail");
                return View("ResetPassword");
            }
            if (!newPassword.Any(char.IsUpper))
            {
                ViewBag.ErrorMessage = "Password must contain at least one uppercase letter.";
                ViewBag.Email = TempData["UserEmail"];
                TempData.Keep("UserEmail");
                return View("ResetPassword");
            }
            if (!newPassword.Any(char.IsDigit))
            {
                ViewBag.ErrorMessage = "Password must contain at least one digit.";
                ViewBag.Email = TempData["UserEmail"];
                TempData.Keep("UserEmail");
                return View("ResetPassword");
            }
            if (!newPassword.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                ViewBag.ErrorMessage = "Password must contain at least one special character.";
                ViewBag.Email = TempData["UserEmail"];
                TempData.Keep("UserEmail");
                return View("ResetPassword");
            }

            string userEmail = TempData["UserEmail"]?.ToString();
            if (string.IsNullOrEmpty(userEmail))
            {
                ViewBag.ErrorMessage = "User email not found.";
                return View("ResetPassword");
            }

            // Update the user's password.
            using (var context = new GuidanceTrackerDbContext())
            {
                var userManager = new UserManager<User>(new UserStore<User>(context));

                // Configure the token provider for password reset.
                var dataProtectionProvider = new DpapiDataProtectionProvider("MyAppName");
                userManager.UserTokenProvider = new DataProtectorTokenProvider<User>(
                    dataProtectionProvider.Create("ResetPassword"));

                // Find the user by email.
                var user = userManager.FindByEmail(userEmail);
                if (user == null)
                {
                    ViewBag.ErrorMessage = "User not found.";
                    return View("ResetPassword");
                }

                // Generate a password reset token and reset the password.
                var token = userManager.GeneratePasswordResetToken(user.Id);
                var result = userManager.ResetPassword(user.Id, token, newPassword);

                if (result.Succeeded)
                {
                    TempData["PasswordResetConfirmed"] = true;
                    return RedirectToAction("ResetPasswordConfirmation");
                }
                else
                {
                    ViewBag.ErrorMessage = "An error occurred while resetting your password: " +
                                             string.Join(", ", result.Errors);
                    ViewBag.Email = userEmail;
                    TempData.Keep("UserEmail");
                    return View("ResetPassword");
                }
            }
        }

        // GET: ResetPasswordConfirmation page.
        public ActionResult ResetPasswordConfirmation()
        {
            if (TempData["PasswordResetConfirmed"] == null || !(bool)TempData["PasswordResetConfirmed"])
            {
                return RedirectToAction("ResetCodeVerification");
            }
            ViewBag.Email = TempData["UserEmail"];
            return View("ResetPasswordConfirmation");
        }
    }
}
