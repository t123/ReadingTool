using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ReadingTool.Entities;
using ReadingTool.Services;
using ReadingTool.Site.Helpers;
using ReadingTool.Site.Models.Account;

namespace ReadingTool.Site.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authenticationService;

        public AccountController(IUserService userService, IAuthenticationService authenticationService)
        {
            _userService = userService;
            _authenticationService = authenticationService;
        }

        [HttpGet]
        public ActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignIn(SignInViewModel model)
        {
            if(ModelState.IsValid)
            {
                var user = _authenticationService.Authenticate(model.Username, model.Password);

                if(user != null)
                {
                    var cookie = _authenticationService.CreateAuthenticationTicket(user);

                    if(cookie != null)
                    {
                        Response.Cookies.Add(cookie);
                    }

                    return RedirectToAction("Index", "~~User.MyAccount");
                    //return RedirectToAction("Index", "MyAccount", new { __routecontext = "User" });
                }

                this.FlashError("Either your username or password is incorrect");
            }

            return View();
        }

        [HttpGet]
        public ActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignUp(SignInViewModel model)
        {
            if(ModelState.IsValid)
            {
                _userService.Create(model.Username, model.Password);

                return RedirectToAction("SignIn");
            }

            return View();
        }

        public ActionResult SignOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}
