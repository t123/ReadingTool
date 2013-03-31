#region License
// AccountController.cs is part of ReadingTool.Site
// 
// ReadingTool.Site is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Site is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Site. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2013 Travis Watt
#endregion

using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using AutoMapper;
using Ionic.Zip;
using Ionic.Zlib;
using ReadingTool.Common;
using ReadingTool.Entities;
using ReadingTool.Services;
using ReadingTool.Site.Attributes;
using ReadingTool.Site.Helpers;
using ReadingTool.Site.Models.Account;

namespace ReadingTool.Site.Controllers.Home
{
    [Authorize]
    [NeedsPersistence]
    public class AccountController : BaseController
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
            user.DisplayName = model.DisplayName.Trim();
            user.EmailAddress = model.EmailAddress.Trim().ToLowerInvariant();
            _userService.Repository.Save(user);

            this.FlashSuccess("Your account has been updated.");
            return RedirectToAction("Index");
        }

        public ActionResult NewApiKey()
        {
            var user = _userService.Repository.FindOne(x => x.UserId == UserId);
            user.ApiKey = _userService.CreateApiKey();
            _userService.Repository.Save(user);

            this.FlashSuccess("A new API key was created.");

            return RedirectToAction("Index");
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult ChangePassword([Bind(Prefix = "Password")]AccountModel.PasswordModel model)
        {
            var user = _userService.ValidateUser(UserId, model.OldPassword);

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
            var user = _userService.ValidateUser(UserId, model.Password);

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
            _userService.DeleteAccount();

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
                            LastRead = x.LastRead
                        }),
                    Terms = user.Terms.Select(x => new
                        {
                            State = x.State,
                            Phrase = x.Phrase,
                            BasePhrase = x.BasePhrase,
                            Sentence = x.Sentence,
                            Definition = x.Definition,
                            Box = x.Box,
                            NextReview = x.NextReview,
                            Text = x.Text == null ? (Guid?)null : x.Text.TextId,
                            Language = x.Language.Name,
                            Created = x.Created,
                            Modified = x.Modified,
                            Tags = x.Tags.Select(y => y.TagTerm),
                            Length = x.Length
                        })
                };


            MemoryStream ms = new MemoryStream();
            using(ZipFile zip = new ZipFile())
            {
                zip.CompressionMethod = CompressionMethod.Deflate;
                zip.CompressionLevel = CompressionLevel.BestCompression;

                string data = ServiceStack.Text.JsonSerializer.SerializeToString(model);
                zip.AddEntry("account.json", data, Encoding.UTF8);

                var userDirectory = UserDirectory.GetDirectory(UserId);
                if(Directory.Exists(userDirectory))
                {
                    foreach(var file in Directory.GetFiles(userDirectory))
                    {
                        var fi = new FileInfo(file);
                        zip.AddEntry("/texts/" + fi.Name, System.IO.File.ReadAllBytes(file));
                    }
                }

                zip.Save(ms);
            }

            ms.Seek(0, SeekOrigin.Begin);
            return File(ms, "application/zip", "account.zip");
        }
    }
}
