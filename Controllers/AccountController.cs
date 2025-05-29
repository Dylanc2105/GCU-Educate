using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using GuidanceTracker.Models;
using System.Web.Security;
//using Microsoft.AspNetCore.Identity.UI.Services;

namespace GuidanceTracker.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public ActionResult Report()
        {
            return View();
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);

            switch (result)
            {
                case SignInStatus.Success:
                    var user = await UserManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        var roles = await UserManager.GetRolesAsync(user.Id);
                        if (roles.Contains("Student"))
                        {
                            return RedirectToAction("StudentDash", "Student");
                        }
                        else if (roles.Contains("Lecturer"))
                        {
                            return RedirectToAction("LecturerDash", "Lecturer");
                        }
                        else if (roles.Contains("GuidanceTeacher"))
                        {
                            return RedirectToAction("GuidanceTeacherDash", "GuidanceTeacher");
                        }
                        else if (roles.Contains("CurriculumHead"))
                        {
                            return RedirectToAction("CurriculumHeadDash", "CurriculumHead");
                        }
                    }
                    return RedirectToLocal(returnUrl);

                case SignInStatus.LockedOut:
                    return View("Lockout");

                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });

                case SignInStatus.Failure:
                default:
                    var existingUser = await UserManager.FindByEmailAsync(model.Email);

                    if (existingUser == null)
                    {
                        ModelState.AddModelError("", "Incorrect login details. Please try again.");
                        System.Diagnostics.Debug.WriteLine("⚠️ Incorrect login details. Please try again.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Incorrect login details. Please try again.");
                        System.Diagnostics.Debug.WriteLine("❌ Incorrect login details. Please try again. " + model.Email);
                    }

                    return View(model); // Ensure error is displayed on the page
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Street = model.Street,
                    City = model.City,
                    //data & time of registration

                    RegistredAt = DateTime.UtcNow,
                };
             
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);
                    
                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Find user by email
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return View("ForgotPasswordConfirmation");
            }

            // Generate a random 8-digit reset code
            var random = new Random();
            string resetCode = random.Next(10000000, 99999999).ToString();

            // Store the reset code and expiry time in the database
            user.ResetCode = resetCode;
            user.ResetCodeExpiry = DateTime.UtcNow.AddMinutes(10);  // Code expires in 10 mins
            //await UserManager.UpdateAsync(user);

            var result = await UserManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to save reset code. Please try again.");
                return View(model);
            }

            // Create reset email
            string emailBody = $"Your password reset code is: {resetCode}. It expires in 10 minutes.";
            var message = new IdentityMessage
            {
                Destination = user.Email,
                Subject = "Reset Password Code",
                Body = emailBody
            };

            await UserManager.EmailService.SendAsync(message);

            return View("ForgotPasswordConfirmation");
        }

        [HttpPost]
        public ActionResult SetAppearOffline(bool appearOffline)
        {
            var userId = User.Identity.GetUserId();
            using (var db = new GuidanceTrackerDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Id == userId);
                if (user == null) return HttpNotFound();

                user.AppearOffline = appearOffline;
                db.SaveChanges();

                return Json(new { success = true });
            }
        }



        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(string resetCode, string userEmail)
        {
            if (string.IsNullOrEmpty(resetCode) || string.IsNullOrEmpty(userEmail))
            {
                ModelState.AddModelError("", "Reset code and email are required.");
                return View("ResetCodeVerification"); // Ensure this view exists
            }

            var user = await UserManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View("ResetCodeVerification");
            }

            if (string.IsNullOrEmpty(user.ResetCode) || user.ResetCodeExpiry == null || user.ResetCodeExpiry < DateTime.UtcNow)
            {
                ModelState.AddModelError("", "Invalid or expired reset code. Please request a new one.");
                return View("ResetCodeVerification");
            }

            if (user.ResetCode != resetCode)
            {
                ModelState.AddModelError("", "Incorrect reset code. Please try again.");
                return View("ResetCodeVerification");
            }

            // Reset code is valid, allow user to proceed
            TempData["CodeVerified"] = true;
            TempData["UserEmail"] = userEmail;
            return RedirectToAction("ResetPassword");
        }


        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordWithCodeViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Attempt to find user by email
            var user = await UserManager.FindByEmailAsync(model.Email.Trim());
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email.");
                return View(model);
            }

            // Validate the reset code: 
            // - Ensure the code exists
            // - Ensure it is not expired
            // - Ensure it matches the provided input
            if (string.IsNullOrEmpty(user.ResetCode) ||
                user.ResetCodeExpiry == null ||
                user.ResetCodeExpiry < DateTime.UtcNow ||
                !string.Equals(user.ResetCode.Trim(), model.ResetCode.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Invalid or expired reset code detected!");
                ModelState.AddModelError("", "Invalid or expired reset code.");
                return View(model);
            }

            // Remove the old password
            var removePasswordResult = await UserManager.RemovePasswordAsync(user.Id);
            if (!removePasswordResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to reset password.");
                return View(model);
            }

            // Set the new password
            var addPasswordResult = await UserManager.AddPasswordAsync(user.Id, model.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to set new password.");
                return View(model);
            }

            // Clear reset code after successful password reset
            user.ResetCode = null;
            user.ResetCodeExpiry = null;
            await UserManager.UpdateAsync(user);

            return RedirectToAction("Login", "Account");
        }




        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new User { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            var userId = User.Identity.GetUserId();
            using (var db = new GuidanceTrackerDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Id == userId);
                if (user != null)
                {
                    user.LastSeen = null;
                    db.SaveChanges();
                }
            }

            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }


        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }
        public ActionResult GetAvailableChatUsers()
        {
            var currentUserId = User.Identity.GetUserId();
            var currentUser = new GuidanceTrackerDbContext().Users.Find(currentUserId);
            var db = new GuidanceTrackerDbContext();

            var users = db.Users
                .Where(u => u.Id != currentUserId)
                .ToList()
                .Where(u => CanMessage(currentUser, u)) // Apply access rules
                .Select(u => new
                {
                    u.Id,
                    Name = u.FirstName + " " + u.LastName,
                    Role = db.Roles.FirstOrDefault(r => r.Users.Any(x => x.UserId == u.Id))?.Name ?? "Unknown"
                })
                .OrderBy(u => u.Name)
                .ToList();

            return Json(users, JsonRequestBehavior.AllowGet);
        }

        private bool CanMessage(User current, User target)
        {
            bool isStudent = current is Student;
            bool isLecturer = current is Lecturer;
            bool isGuidance = current is GuidanceTeacher;

            bool targetIsStudent = target is Student;
            bool targetIsLecturer = target is Lecturer;
            bool targetIsGuidance = target is GuidanceTeacher;

            if (isStudent)
                return targetIsGuidance && ((Student)current).GuidanceTeacherId == target.Id;

            if (isLecturer)
                return targetIsLecturer || targetIsGuidance;

            if (isGuidance)
                return targetIsStudent || targetIsLecturer || targetIsGuidance;

            return false;
        }



        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}