using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using AutoMapper;
using Newtonsoft.Json;
using ReadingTool.Core;
using ReadingTool.Core.Formatters;
using ReadingTool.Entities;
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
        private readonly ILanguageService _languageService;
        private readonly ITermService _termService;
        private readonly ITextService _textService;
        private readonly ISystemLanguageService _systemLanguageService;

        public MyAccountController(
            IUserService userService,
            IAuthenticationService authenticationService,
            ILanguageService languageService,
            ITermService termService,
            ITextService textService,
            ISystemLanguageService systemLanguageService
            )
        {
            _userService = userService;
            _authenticationService = authenticationService;
            _languageService = languageService;
            _termService = termService;
            _textService = textService;
            _systemLanguageService = systemLanguageService;
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

            this.FlashError(Constants.Messages.FORM_FAIL);

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

        [HttpGet]
        public ActionResult Delete()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(DeleteAccountModel model)
        {
            var user = _userService.Find(this.CurrentUserId());
            if(!string.IsNullOrEmpty(model.Password) && _userService.VerifyPassword(model.Password, user.Password))
            {
                _userService.Delete(user);
                this.FlashSuccess("Your account and all your data has been deleted.");
                FormsAuthentication.SignOut();

                return RedirectToAction("", "~~Home");
            }

            this.FlashError(Constants.Messages.FORM_FAIL);

            return View();
        }

        [HttpGet]
        public ActionResult Controls()
        {
            var user = _userService.Find(this.CurrentUserId());
            var mapped = Mapper.Map<KeyBindings, KeyBindingsModel>(user.KeyBindings ?? new KeyBindings());
            return View(mapped);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Controls(KeyBindingsModel model)
        {
            var user = _userService.Find(this.CurrentUserId());

            if(ModelState.IsValid)
            {
                var kb = user.KeyBindings ?? new KeyBindings();
                kb = Mapper.Map<KeyBindingsModel, KeyBindings>(model);

                user.KeyBindings = kb;
                _userService.Save(user);

                this.FlashSuccess(Constants.Messages.FORM_UPDATE, DescriptionFormatter.GetDescription(model));
                return RedirectToAction("Controls");
            }

            this.FlashError(Constants.Messages.FORM_FAIL);
            return View(model);
        }

        [HttpGet]
        public ActionResult ImportExport()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ImportExport(string export)
        {
            switch(export)
            {
                case "export your account data":
                    var user = _userService.Find(this.CurrentUserId());
                    var languages = _languageService.FindAll().ToList();
                    user.Password = string.Empty;

                    var exportModel = new ExportModel()
                    {
                        User = user,
                        Languages = languages,
                        Terms = _termService.FindAll().ToList(),
                        Texts = _textService.FindAll().ToList(),
                    };

                    foreach(var l in languages.Where(x => x.SystemLanguageId != null))
                    {
                        var sl = _systemLanguageService.Find(l.SystemLanguageId.Value);
                        if(sl != null)
                        {
                            exportModel.SystemLanguages.Add(sl);
                        }
                    }

                    var jsonString = JsonConvert.SerializeObject(exportModel);

                    //TODO zip this up
                    return File(Encoding.UTF8.GetBytes(jsonString), "application/json", "account.json");
            }

            this.FlashError(Constants.Messages.FORM_FAIL);
            return RedirectToAction("ImportExport");
        }

        private void UpdateUser(Entities.User user)
        {
            var cookie = _authenticationService.CreateAuthenticationTicket(user);
            Response.Cookies.Add(cookie);
        }
    }
}
