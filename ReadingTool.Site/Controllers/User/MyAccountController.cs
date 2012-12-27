using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ReadingTool.Core;
using ReadingTool.Services;
using ReadingTool.Site.Helpers;
using ReadingTool.Site.Models.User;

namespace ReadingTool.Site.Controllers.User
{
    [Authorize(Roles = Constants.Roles.WEB)]
    public class MyAccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authenticationService;

        public MyAccountController(IUserService userService, IAuthenticationService authenticationService)
        {
            _userService = userService;
            _authenticationService = authenticationService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var user = _userService.Find(this.CurrentUserId());

            if(user == null)
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("SignIn", "~~Account");
            }

            var tuple = new Tuple<ProfileViewModel, PasswordChangeViewModel>
                (
                new ProfileViewModel()
                    {
                        Created = user.Created,
                        DisplayName = user.DisplayName,
                        EmailAddress = user.EmailAddress,
                        Id = user.Id,
                        Username = user.Username,
                        Theme = user.Theme
                    },
                new PasswordChangeViewModel()
                    {
                        ConfirmNewPassword = string.Empty,
                        CurrentPassword = string.Empty,
                        NewPassword = string.Empty
                    }
                );

            return View(tuple);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(
            [Bind(Prefix = "Item1")]ProfileViewModel profile,
            [Bind(Prefix = "Item2")]PasswordChangeViewModel passwordChange)
        {
            var user = _userService.Find(this.CurrentUserId());

            if(user == null)
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("SignIn", "~~Account");
            }

            bool changePassword = false;
            if(
                !string.IsNullOrEmpty(passwordChange.NewPassword) ||
                !string.IsNullOrEmpty(passwordChange.CurrentPassword) ||
                !string.IsNullOrEmpty(passwordChange.ConfirmNewPassword)
                )
            {
                changePassword = true;

                if(string.IsNullOrEmpty(passwordChange.NewPassword))
                {
                    ModelState.AddModelError("Item2.NewPassword", "Please enter your new password");
                }
                else if(string.IsNullOrEmpty(passwordChange.CurrentPassword))
                {
                    ModelState.AddModelError("Item2.CurrentPassword", "Please enter your current password");
                }
                else if(string.IsNullOrEmpty(passwordChange.CurrentPassword))
                {
                    ModelState.AddModelError("Item2.ConfirmNewPassword", "Please confirm your new password");
                }
                else if(!passwordChange.NewPassword.Equals(passwordChange.ConfirmNewPassword))
                {
                    ModelState.AddModelError("Item2.ConfirmNewPassword", "Your passwords do not match");
                }
                else if(!_userService.VerifyPassword(passwordChange.CurrentPassword, user.Password))
                {
                    ModelState.AddModelError("Item2.CurrentPassword", "Your password is incorrect");
                }
            }

            if(ModelState.IsValid)
            {
                string originalTheme = (user.Theme ?? "");
                user.DisplayName = profile.DisplayName;
                user.Username = profile.Username;
                user.EmailAddress = profile.EmailAddress;
                user.Theme = profile.Theme;
                _userService.Save(user, changePassword ? passwordChange.NewPassword : "");

                this.FlashSuccess("Your profile has been saved");

                if(!originalTheme.Equals(profile.Theme, StringComparison.InvariantCultureIgnoreCase))
                {
                    UpdateUser(user);
                }

                return RedirectToAction("Index");
            }

            var tuple = new Tuple<ProfileViewModel, PasswordChangeViewModel>
                (
                new ProfileViewModel()
                {
                    Created = user.Created,
                    DisplayName = profile.DisplayName,
                    EmailAddress = user.EmailAddress,
                    Id = user.Id,
                    Username = user.Username
                },
                new PasswordChangeViewModel()
                {
                    ConfirmNewPassword = string.Empty,
                    CurrentPassword = string.Empty,
                    NewPassword = string.Empty
                }
                );

            return View(tuple);
        }

        private void UpdateUser(Entities.User user)
        {
            var cookie = _authenticationService.CreateAuthenticationTicket(user);
            Response.Cookies.Add(cookie);
        }
    }
}
