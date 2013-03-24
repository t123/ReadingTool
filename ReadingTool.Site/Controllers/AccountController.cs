using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using ReadingTool.Entities;
using ReadingTool.Services;
using ReadingTool.Site.Attributes;
using ReadingTool.Site.Models.Account;

namespace ReadingTool.Site.Controllers.Home
{
    [Authorize]
    [NeedsPersistence]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        public ActionResult Index()
        {
            User user = null;
            try
            {
                user = _userService.Repository.FindOne(x => x.UserId == Guid.Parse(HttpContext.User.Identity.Name));

                if(user == null)
                {
                    throw new Exception("Invalid user");
                }
            }
            catch(Exception e)
            {
                _logger.Error(e);
                return RedirectToAction("SignOut", "Home");
            }

            return View(new AccountModel { User = Mapper.Map<User, AccountModel.UserModel>(user) });
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Update([Bind(Prefix = "User")]AccountModel.UserModel model)
        {
            if(!ModelState.IsValid)
            {
                return View("Index", new AccountModel() { User = model });
            }

            var user = _userService.Repository.FindOne(x => x.UserId == Guid.Parse(HttpContext.User.Identity.Name));
            user.DisplayName = model.DisplayName;
            user.EmailAddress = model.EmailAddress;
            _userService.Repository.Save(user);

            return RedirectToAction("Index");
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult ChangePassword([Bind(Prefix = "Password")]AccountModel.PasswordModel model)
        {
            var user = _userService.ValidateUser(Guid.Parse(HttpContext.User.Identity.Name), model.OldPassword);

            if(user == null)
            {
                ModelState.AddModelError("Password.OldPassword", "Your password is incorrect");
            }

            if(!ModelState.IsValid)
            {
                return View("Index", new AccountModel()
                    {
                        Password = model,
                        User = Mapper.Map<User, AccountModel.UserModel>(_userService.Repository.FindOne(x => x.UserId == Guid.Parse(HttpContext.User.Identity.Name)))
                    });
            }

            _userService.UpdatePassword(user, model.NewPassword);
            return RedirectToAction("Index");
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Delete()
        {
            _userService.Repository.Delete(_userService.Repository.FindOne(x => x.UserId == Guid.Parse(HttpContext.User.Identity.Name)));
            return RedirectToAction("SignOut", "Home");
        }
    }
}
