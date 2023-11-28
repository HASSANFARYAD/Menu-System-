using HealthGuage.HelpingClasses;
using HealthGuage.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HealthGuage.Repositories;
using HealthGuage.Filters;
using LoginFinal.Filters;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Template.HelpingClasses;
using Newtonsoft.Json;
using Template.Repositories;
using DocumentFormat.OpenXml.Office2016.Drawing.Command;
using DocumentFormat.OpenXml.Drawing;
using Template.Models;
using DocumentFormat.OpenXml.InkML;

namespace HealthGuage.Controllers
{
    [ExceptionFilter]
    public class AuthController : Controller
    {
        private readonly IUserRepo userRepo;
        private readonly IContentFileRepo contentFileRepo;
        private readonly GeneralPurpose gp;
        private readonly ProjectVariables projectVariables;

        public AuthController(IUserRepo _userRepo, IHttpContextAccessor haccess, 
                                IOptions<ProjectVariables> options, IContentFileRepo _contentFileRepo)
        {
            userRepo = _userRepo;
            contentFileRepo = _contentFileRepo;
            gp = new GeneralPurpose(haccess);
            projectVariables = options.Value;
        }

        private async Task<bool> IsUserValidate()
        {
            int userId = Convert.ToInt32(gp.GetUserClaims().Id);
            if (userId != null)
            {
                bool isAutheticate = await userRepo.IsUserValidate(userId);
                if (!isAutheticate)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public async Task<IActionResult> Login(string msg = "", string color = "black")
        {
            UserDto? loggedinUser = gp.GetUserClaims();

            if (loggedinUser != null)
            {
                if(loggedinUser.Role == 1)
                    return RedirectToAction("Index", "Admin");
                else
                    return RedirectToAction("Index", "Home");
            }

            ViewBag.Message = msg;
            ViewBag.Color = color;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PostLogin(string Email, string Password)
        {
            User? user = await userRepo.GetUserByLogin(Email);

            if (user == null)
            {
                return RedirectToAction("Login", new { msg = "Incorrect Email/Password!", color = "red" });
            }

            string passwrd = StringCipher.DecryptString(user.Password);

            if (!passwrd.Equals(Password))
            {
                return RedirectToAction("Login", new { msg = "Incorrect Email/Password!", color = "red" });
            }

            await gp.SetUserClaims(user);

            if (user.Role == 1)
                return RedirectToAction("Index", "Admin");
            else
                return RedirectToAction("Index", "Home");
        }


        #region Forgot Password

        public IActionResult ForgotPassword(string msg = "", string color = "black")
        {
            ViewBag.Color = color;
            ViewBag.Message = msg;

            return View();
            //return RedirectToAction("Login", "Auth");
        }

        public async Task<IActionResult> PostForgotPassword(string Email = "")
        {
            User? u = await userRepo.GetUserByEmail(Email);

            if (u != null)
            {
                string BaseUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}" + "/";

                MailSender mailSender = new MailSender(projectVariables);
                bool checkMail = await mailSender.SendForgotPasswordEmailAsync(u.Email, StringCipher.Base64Encode(u.Id.ToString()), BaseUrl);

                if (checkMail == true)
                {
                    return RedirectToAction("ForgotPassword", "Auth", new { msg = "Please check your mails' inbox/spam", color = "green" });
                }
                else
                {
                    return RedirectToAction("ForgotPassword", "Auth", new { msg = "Mail sending fail!", color = "red" });
                }
            }
            else
            {
                return RedirectToAction("ForgotPassword", "Auth", new { msg = "Email does not belong to our record!!", color = "red" });
            }
        }


        public IActionResult ResetPassword(string encId = "", string t = "", string msg = "", string color = "black")
        {
            DateTime PassDate = new DateTime(Convert.ToInt64(t)).Date;
            DateTime CurrentDate = GeneralPurpose.DateTimeNow().Date;

            if (CurrentDate != PassDate)
            {
                return RedirectToAction("Login", "Auth", new { msg = "Link expired, Please try again!", color = "red" });
            }


            ViewBag.Time = t;
            ViewBag.EncId = encId;
            ViewBag.Message = msg;
            ViewBag.Color = color;

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> PostResetPassword(string encId = "", string t = "", string NewPassword = "", string ConfirmPassword = "")
        {
            if (NewPassword != ConfirmPassword)
            {
                return RedirectToAction("ResetPassword", "Auth", new { encId = encId, t = t, msg = "Password and confirm password did not match", color = "red" });
            }

            int id = Convert.ToInt32(StringCipher.Base64Decode(encId));
            User user = await userRepo.GetUserById(id);
            user.Password = StringCipher.EncryptString(NewPassword);

            if (await userRepo.UpdateUser(user))
            {
                return RedirectToAction("Login", "Auth", new { msg = "Password reset successful, Try login", color = "green" });
            }
            else
            {
                return RedirectToAction("ResetPassword", "Auth", new { encId = encId, t = t, msg = "Something' wrong!", color = "red" });
            }
        }

        #endregion


        #region Signup
        public IActionResult Register(string msg = "", string color = "black")
        {
            ViewBag.Message = msg;
            ViewBag.Color = color;

            //return View();
            return RedirectToAction("Login", "Auth");
        }

        [HttpPost]
        public async Task<IActionResult> PostRegister(User _user, string _confirmPassword = "")
        {
            if (_user.Password != _confirmPassword)
            {
                return RedirectToAction("Register", "Auth", new { msg = "Password and confirm password didn't match", color = "red" });
            }

            bool checkEmail = await userRepo.ValidateEmail(_user.Email);
            if (checkEmail == false)
            {
                return RedirectToAction("Register", "Auth", new { msg = "Email already exists. Try sign in!", color = "red" });
            }

            User u = new User()
            {
                Name = _user.Name.Trim(),
                ProfilePicture = _user.ProfilePicture,
                PhoneNumber = _user.PhoneNumber,
                Email = _user.Email.Trim(),
                Password = StringCipher.EncryptString(_user.Password),
                Role = 2,
                IsActive = 1,
                CreatedAt = GeneralPurpose.DateTimeNow()
            };

            if (await userRepo.AddUser(u))
            {
                return RedirectToAction("Login", "Auth", new { msg = "Account created successfully, Please login", color = "green" });
            }
            else
            {
                return RedirectToAction("Register", "Auth", new { msg = "Somethings' wrong", color = "red" });
            }
        }

        #endregion


        #region Manage Profile
        [ValidationFilter(Roles = new int[] { 1, 2 })]
        public async Task<IActionResult> UpdateProfile(string msg = "", string color = "black")
        {
            bool chkUserValidate = await IsUserValidate();
            if (!chkUserValidate)
            {
                return RedirectToAction("LogOut", "Auth", new { msg = "Something' wrong", color = "red" });
            }

            int userId = Convert.ToInt32(gp.GetUserClaims().Id);

            User? u = await userRepo.GetUserById(userId);

            ViewBag.User = u;
            ViewBag.Message = msg;
            ViewBag.Color = color;

            return View();
        }

        [ValidationFilter(Roles = new int[] { 1, 2 })]
        public async Task<IActionResult> PostUpdateProfile(User _user)
        {
            bool checkEmail = await userRepo.ValidateEmail(_user.Email, _user.Id);

            if (checkEmail == false)
            {
                return RedirectToAction("UpdateProfile", "Auth", new { msg = "Email used by someone else, Please try another", color = "red" });
            }

            User u = await userRepo.GetUserById(_user.Id);
            u.Name = _user.Name.Trim();
            u.PhoneNumber = _user.PhoneNumber;
            u.Email = _user.Email.Trim();

            if (_user.File != null)
            {
                if (_user.File.Length != 0)
                {
                    string FileExt = System.IO.Path.GetExtension(_user.File.FileName);

                    if (FileExt.ToLower().Equals(".jpg") || FileExt.ToLower().Equals(".png") || FileExt.ToLower().Equals(".jpeg"))
                    {
                        string updatedProfile = await GeneralPurpose.UploadProfilePicture(_user.File, u.ProfilePicture, "", _user.Id.ToString());
                        u.ProfilePicture = string.IsNullOrEmpty(updatedProfile) ? u.ProfilePicture : updatedProfile;
                    }
                    else
                    {
                        return RedirectToAction("UpdateProfile", "Auth", new { msg = "File must be type of jpg/png/jpeg", color = "red" });
                    }
                }
                else
                {
                    return RedirectToAction("UpdateProfile", "Auth", new { msg = "Corrupt file. Select other one", color = "red" });
                }

            }

            if (await userRepo.UpdateUser(u))
            {
                await gp.SetUserClaims(u);
                return RedirectToAction("UpdateProfile", "Auth", new { msg = "Profile updated successfully!", color = "green" });
            }
            else
            {
                return RedirectToAction("UpdateProfile", "Auth", new { msg = "Somthings' Wrong!", color = "red" });
            }
        }

        [ValidationFilter(Roles = new int[] { 1, 2 })]
        public async Task<IActionResult> UpdatePassword(string msg = "", string color = "black")
        {
            bool chkUserValidate = await IsUserValidate();
            if (!chkUserValidate)
            {
                return RedirectToAction("LogOut", "Auth", new { msg = "Something' wrong", color = "red" });
            }

            ViewBag.Message = msg;
            ViewBag.Color = color;

            return View();
        }

        [ValidationFilter(Roles = new int[] { 1, 2 })]
        public async Task<IActionResult> PostUpdatePassword(string oldPassword = "", string newPassword = "", string confirmPassword = "")
        {
            if (newPassword != confirmPassword)
            {
                return RedirectToAction("UpdatePassword", "Auth", new { msg = "New password and Confirm password did not match!", color = "red" });
            }

            int userId = Convert.ToInt32(gp.GetUserClaims().Id);
            User? u = await userRepo.GetUserById(userId);

            if (StringCipher.DecryptString(u.Password) != oldPassword)
            {
                return RedirectToAction("UpdatePassword", "Auth", new { msg = "Old password did not match!", color = "red" });
            }

            u.Password = StringCipher.EncryptString(newPassword);

            if (await userRepo.UpdateUser(u))
            {
                return RedirectToAction("UpdatePassword", "Auth", new { msg = "Password updated successfully!", color = "green" });
            }
            else
            {
                return RedirectToAction("UpdatePassword", "Auth", new { msg = "Somthings' wrong!", color = "red" });
            }
        }

        #endregion

        public async Task<IActionResult> LogOut(string msg = "", string color = "")
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth", new { msg = msg, color = color });
        }

    }
}
