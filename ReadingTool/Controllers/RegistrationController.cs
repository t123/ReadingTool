#region License
// RegistrationController.cs is part of ReadingTool
// 
// ReadingTool is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Security;
using MvcContrib;
using ReadingTool.Common;
using ReadingTool.Common.Helpers;
using ReadingTool.Entities;
using ReadingTool.Extensions;
using ReadingTool.Models.Create.User;
using ReadingTool.Services;

namespace ReadingTool.Controllers
{
    public class RegistrationController : BaseController
    {
        private readonly IUserService _userService;

        public RegistrationController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public ActionResult SignUp()
        {
            return View();
        }

        private bool ValidateUsername(SignUpModel model)
        {
            if(_userService.FindOneByUsername(model.Username) != null)
                return false;

            if(!Regex.IsMatch(model.Username, @"[A-Za-z](?=[A-Za-z0-9_.]{3,31}$)[a-zA-Z0-9_]*\.?[a-zA-Z0-9_]*$"))
            {
                return false;
            }

            return true;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignUp(SignUpModel model)
        {
            if(!ValidateUsername(model))
            {
                ModelState.AddModelError("Username", "Please choose another username");
            }

            if(ModelState.IsValid)
            {
                User user = new User()
                                {
                                    Username = model.Username,
                                    UnencryptedPassword = model.Password
                                };

                _userService.Save(user);

                var cookie = SecurityManager.CreateAuthenticationTicket(user);
                Response.Cookies.Add(cookie);

                return this.RedirectToAction(x => x.ThankYou()).Success("Thank you!");
            }

            return View(model).Error(Messages.FormValidationError);
        }

        [HttpGet]
        public ActionResult SignIn()
        {
            ViewBag.ReturnUrl = Request.QueryString["ReturnUrl"] ?? "";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignIn(SignInModel model)
        {
            if(ModelState.IsValid)
            {
                var securityManager = new SecurityManager(_userService);
                User user = securityManager.Authenticate(model.Username, model.Password);

                if(user != null)
                {
                    var cookie = SecurityManager.CreateAuthenticationTicket(user);
                    string returnUrl = Request.Form["ReturnUrl"] ?? string.Empty;

                    if(cookie != null)
                    {
                        Response.Cookies.Add(cookie);
                    }

                    if(string.IsNullOrEmpty(returnUrl))
                    {
                        return this.RedirectToAction<MyAccountController>(x => x.Index());
                    }

                    if(
                        Url.IsLocalUrl(returnUrl) &&
                        returnUrl.Length > 1 &&
                        returnUrl.StartsWith("/") &&
                        !returnUrl.StartsWith("//") &&
                        !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }

                    return this.RedirectToAction<MyAccountController>(x => x.Index());
                }
            }

            return View(model).Error("Sorry, please check your username and password");
        }

        public ActionResult SignOut()
        {
            FormsAuthentication.SignOut();
            return this.RedirectToAction<RegistrationController>(x => x.SignedOut());
        }

        public ActionResult SignedOut()
        {
            return View();
        }

        public ActionResult Denied()
        {
            return View();
        }

        public ActionResult ThankYou()
        {
            return View(_userService.UserCount());
        }

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordModel model)
        {
            var user = _userService.FindOneByUsername(model.Username);

            if(!string.IsNullOrEmpty(model.Username))
            {
                string errorMessage = null;
                if(user == null)
                {
                    errorMessage = string.Format("User {0} does not exist", model.Username);
                }
                else if(string.IsNullOrEmpty(user.EmailAddress))
                {
                    errorMessage = string.Format("There is no email address associated with the user {0}", model.Username);
                }

                if(!string.IsNullOrEmpty(errorMessage))
                {
                    ModelState.AddModelError("Username", errorMessage);
                    return View(model).Error(errorMessage);
                }
            }

            string encrypted = PasswordHelper.HashString(PasswordHelper.CreateRandomString(10, PasswordHelper.AllowedCharacters.AlphaNumericSpecial));

            if(ModelState.IsValid)
            {
                _userService.GenerateForgottenPassword(user, encrypted);
                return this.RedirectToAction(x => x.ForgotPassword()).Success("Instructions on how to reset your password have been sent to your email address.");
            }

            return View(model).Error("Please check your username");
        }

        [HttpGet]
        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            if(ModelState.IsValid)
            {
                string result = _userService.ConfirmPasswordReset(model.Username, model.ResetKey);

                if(string.IsNullOrEmpty(result))
                {
                    _userService.ResetPassword(model.Username, model.Password);
                    return this.RedirectToAction(x => x.SignIn()).Success("Your password has been reset. Please login with your new password below.");
                }

                ModelState.AddModelError("Username", result);
            }

            return View(model).Error("Sorry, your password could not be reset");
        }
    }
}
