using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Ionic.Zip;
using Ionic.Zlib;
using ReadingTool.Entities;
using ReadingTool.Services;
using ReadingTool.Site.Attributes;
using ReadingTool.Site.Helpers;
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

        private long UserId
        {
            get { return long.Parse(HttpContext.User.Identity.Name); }
        }

        public ActionResult Index()
        {
            User user = null;
            try
            {
                user = _userService.Repository.FindOne(x => x.UserId == UserId);

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

            var user = _userService.Repository.FindOne(x => x.UserId == UserId);
            user.DisplayName = model.DisplayName;
            user.EmailAddress = model.EmailAddress;
            _userService.Repository.Save(user);

            this.FlashSuccess("Your account has been updated.");
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
                        User = Mapper.Map<User, AccountModel.UserModel>(_userService.Repository.FindOne(x => x.UserId == UserId))
                    });
            }

            this.FlashSuccess("Your password has been updated.");
            _userService.UpdatePassword(user, model.NewPassword);
            return RedirectToAction("Index");
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Delete([Bind(Prefix = "Delete")]AccountModel.DeleteModel model)
        {
            var user = _userService.ValidateUser(Guid.Parse(HttpContext.User.Identity.Name), model.Password);

            if(user == null)
            {
                ModelState.AddModelError("Delete.Password", "Your password is incorrect");
            }

            if(!ModelState.IsValid)
            {
                return View("Index", new AccountModel()
                {
                    User = Mapper.Map<User, AccountModel.UserModel>(_userService.Repository.FindOne(x => x.UserId == UserId))
                });
            }

            this.FlashSuccess("Your account has been deleted.");
            _userService.Repository.Delete(_userService.Repository.FindOne(x => x.UserId == UserId));
            return RedirectToAction("SignOut", "Home");
        }

        public FileStreamResult ExportAccount()
        {
            var user = _userService.Repository.FindOne(UserId);

            if(user == null)
            {
                throw new NoNullAllowedException();
            }

            var model = new
                {
                    Languages = user.Languages.Select(x => new
                        {
                            Name = x.Name,
                            Code = x.Code,
                            Dictionaries = x.Dictionaries.Select(y => new
                                {
                                    Name = y.Name,
                                    Encoding = y.Encoding,
                                    WindowName = y.WindowName,
                                    Url = y.Url,
                                    Sentence = y.Sentence,
                                    AutoOpen = y.AutoOpen
                                })
                        }),
                    Texts = user.Texts.Select(x => new
                        {
                            TextId = x.TextId,
                            Title = x.Title,
                            CollectionName = x.CollectionName,
                            CollectionNo = x.CollectionNo,
                            Language1 = x.Language1.Name,
                            Language2 = x.Language2 == null ? "" : x.Language2.Name,
                            Created = x.Created,
                            Modified = x.Modified,
                            LastRead = x.LastRead,
                            L1Text = x.L1Text,
                            L2Text = x.L2Text
                        }),
                    Term = user.Terms.Select(x => new
                        {
                            State = x.State,
                            Phrase = x.Phrase,
                            BasePhrase = x.BasePhrase,
                            Sentence = x.Sentence,
                            Definition = x.Definition,
                            Box = x.Box,
                            NextReview = x.NextReview,
                            Text = x.Text == null ? (long?)null : x.Text.TextId,
                            Language = x.Language.Name,
                            Created = x.Created,
                            Modified = x.Modified,
                            Tags = x.Tags.Select(y => y.TagTerm),
                            Length = x.Length
                        })
                };

            string data = ServiceStack.Text.JsonSerializer.SerializeToString(model);

            MemoryStream ms = new MemoryStream();
            using(ZipFile zip = new ZipFile())
            {
                zip.CompressionMethod = CompressionMethod.Deflate;
                zip.CompressionLevel = CompressionLevel.BestCompression;
                zip.AddEntry("account.json", data, Encoding.UTF8);
                zip.Save(ms);
            }

            ms.Seek(0, SeekOrigin.Begin);
            return File(ms, "application/zip", "account.zip");
        }
    }
}
