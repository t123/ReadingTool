using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;
using ReadingTool.Common;
using ReadingTool.Entities;
using ReadingTool.Services;
using ReadingTool.Site.Attributes;
using ReadingTool.Site.Models.Home;

namespace ReadingTool.Site.Controllers.Home
{
    [NeedsPersistence]
    public class HomeController : Controller
    {
        private readonly IUserService _userService;

        public HomeController(IUserService userService)
        {
            _userService = userService;
        }

        public ActionResult Index()
        {
            if(HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Account");
            }

            return View();
        }

        private string[] GetErrors(string message = "")
        {
            IList<string> errorList = new List<string>();
            if(!string.IsNullOrEmpty(message))
            {
                errorList.Add(message);
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            foreach(var error in errors)
            {
                errorList.Add(error.ErrorMessage);
            }

            return errorList.ToArray();
        }

        [HttpGet]
        public ActionResult SignIn()
        {
            return View("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignIn([Bind(Prefix = "SignIn")]AccountModel.SignInModel model)
        {
            if(!ModelState.IsValid)
            {
                ViewBag.SignInErrors = GetErrors();
                return View("Index", new AccountModel { SignIn = model });
            }

            var user = _userService.ValidateUser(model.Username, model.Password);
            if(user == null)
            {
                ViewBag.SignInErrors = GetErrors("Either your username or password is incorrect.");
                return View("Index", new AccountModel { SignIn = model });
            }

            CreateUserCookie(user);

            return RedirectToAction("Index", "Account");
        }

        private void CreateUserCookie(User user)
        {
            var userdata = new UserIdentity.UserData()
            {
                DisplayName = user.DisplayName,
                EmailAddress = user.EmailAddress,
                Roles = user.Roles,
                Username = user.Username
            };

            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                1,
                user.UserId.ToString(),
                DateTime.Now,
                DateTime.Now.AddYears(1),
                true,
                ServiceStack.Text.TypeSerializer.SerializeToString(userdata));

            Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket)));
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Prefix = "Register")]AccountModel.RegisterModel model)
        {
            if(!ModelState.IsValid)
            {
                ViewBag.RegisterErrors = GetErrors();
                return View("Index", new AccountModel { Register = model });
            }

            if(_userService.UsernameExists(model.Username))
            {
                ViewBag.RegisterErrors = GetErrors("Sorry, this username is already taken.");
                return View("Index", new AccountModel { Register = model });
            }

            var user = _userService.CreateUser(model.Username, model.Password);
            CreateUserCookie(user);

            return RedirectToAction("Index", "Account");
        }

        public ActionResult SignOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }
    }
}
