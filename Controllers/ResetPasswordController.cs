
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GuidanceTracker.Controllers
{
    public class ResetPasswordController : Controller
    {
        // GET: PasswordReset
        public ActionResult ResetCodeVerification()
        {
            return View("ResetCodeVerification");
        }

        [HttpPost]
        public ActionResult VerifyCode(string resetCode)
        {
            if (resetCode == "1234") // CHANGE THIS THIS IS A TEMP CODE FOR TESTING
            {
                TempData["CodeVerified"] = true;
                return RedirectToAction("ResetPassword");
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid code. Please try again.";
                return View("Index");
            }
        }

        public ActionResult ResetPassword()
        {
            if (TempData["CodeVerified"] == null || !(bool)TempData["CodeVerified"])
            {
                return RedirectToAction("Index");
            }

            ViewBag.Email = "email@email.com";
            return View();
        }

        [HttpPost]
        public ActionResult SubmitNewPassword(string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.ErrorMessage = "Please fill in both fields.";
                return View("ResetPassword");
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.ErrorMessage = "Passwords do not match.";
                return View("ResetPassword");
            }

            // Validate that the password contains at least one uppercase letter, one number, and one special character.
            bool hasUppercase = newPassword.Any(char.IsUpper);
            bool hasDigit = newPassword.Any(char.IsDigit);
            bool hasSpecialChar = newPassword.Any(ch => !char.IsLetterOrDigit(ch));

            if (!hasUppercase || !hasDigit || !hasSpecialChar)
            {
                ViewBag.ErrorMessage = "Password must contain at least one uppercase letter, one number, and one special character.";
                return View("ResetPassword");
            }

            // If the validations pass, proceed with resetting the password.
            TempData["SuccessMessage"] = "Password successfully reset!";
            return RedirectToAction("Login");
        }


        public ActionResult Login()
        {
            return RedirectToAction("Index", "Login");
        }

    }

}
