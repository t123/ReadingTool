#region License
// MyAccountController.cs is part of ReadingTool
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using AutoMapper;
using Ionic.Zip;
using MongoDB.Bson;
using MvcContrib;
using Newtonsoft.Json;
using ReadingTool.Attributes;
using ReadingTool.Binders;
using ReadingTool.Common;
using ReadingTool.Common.Helpers;
using ReadingTool.Entities;
using ReadingTool.Entities.Identity;
using ReadingTool.Entities.LWT;
using ReadingTool.Extensions;
using ReadingTool.Filters;
using ReadingTool.Models.Create.Import;
using ReadingTool.Models.Create.LWT;
using ReadingTool.Models.Create.User;
using ReadingTool.Services;

namespace ReadingTool.Controllers
{
    [CustomAuthorize]
    public class MyAccountController : BaseController
    {
        private readonly IUserService _userService;

        public MyAccountController()
        {

        }

        public MyAccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [AutoMap(typeof(User), typeof(ProfileModel))]
        public ActionResult Index()
        {
            var user = _userService.FindOne(UserId);
            if(user == null)
            {
                return this.RedirectToAction<RegistrationController>(x => x.SignIn()).Error("Please sign in");
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(ProfileModel model)
        {
            var user = _userService.FindOneByUsername((HttpContext.User.Identity as UserIdentity).Name);

            if(!string.IsNullOrEmpty(model.UnencryptedPassword))
            {
                if(string.IsNullOrEmpty(model.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "Please fill in your current password");
                }
                else
                {
                    SecurityManager securityManager = new SecurityManager(_userService);
                    if(securityManager.Authenticate(user.Username, model.CurrentPassword) == null)
                    {
                        ModelState.AddModelError("CurrentPassword", "The password you entered is incorrect");
                    }
                }
            }

            SystemLanguage systemLanguage = null;
            if(!string.IsNullOrEmpty(model.NativeLanguage))
            {
                systemLanguage = (DependencyResolver.Current.GetService<ISystemLanguageService>()).FindByName(model.NativeLanguage);

                if(systemLanguage == null)
                {
                    ModelState.AddModelError("NativeLanguage", "This language could not be found");
                }
            }

            if(ModelState.IsValid)
            {
                UpdateModel(user, new[] { "EmailAddress", "DisplayName", "UnencryptedPassword", "ReceiveMessages", "ShareWords" });
                user.NativeLanguageId = systemLanguage == null ? (ObjectId?)null : systemLanguage.SystemLanguageId;

                _userService.Save(user);

                var cookie = SecurityManager.CreateAuthenticationTicket(user);
                if(cookie != null)
                {
                    Response.Cookies.Add(cookie);
                }

                return this.RedirectToAction(x => x.Index()).Success("Profile updated");
            }

            return View(model).Error(Common.Messages.FormValidationError);
        }

        [HttpGet]
        [AutoMap(typeof(MediaControl), typeof(MediaControlModel))]
        public ActionResult MediaControls()
        {
            var user = _userService.FindOne(UserId);
            return View(user.MediaControl ?? new MediaControl());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MediaControls(MediaControlModel model)
        {
            if(ModelState.IsValid)
            {
                var user = _userService.FindOne(UserId);
                var mc = Mapper.Map<MediaControlModel, MediaControl>(model);
                user.MediaControl = mc;
                _userService.Save(user);

                return this.RedirectToAction(x => x.MediaControls()).Success("Media controls saved");
            }

            return View(model).Error(Common.Messages.FormValidationError);
        }

        [HttpGet]
        [AutoMap(typeof(Style), typeof(StyleModel))]
        public ActionResult Styles()
        {
            var user = _userService.FindOne(UserId);
            return View(user.Style ?? new Style());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Styles(StyleModel model)
        {
            if(ModelState.IsValid)
            {
                var user = _userService.FindOne(UserId);
                var styles = Mapper.Map<StyleModel, Style>(model);
                user.Style = styles;
                _userService.Save(user);

                return this.RedirectToAction(x => x.Styles()).Success("Style layout saved");
            }

            return View(model).Error(Common.Messages.FormValidationError);
        }

        [HttpGet]
        [AutoMap(typeof(User), typeof(PublicProfileModel))]
        public ActionResult PublicProfile()
        {
            var user = _userService.FindOne(UserId);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult PublicProfile(PublicProfileModel model)
        {
            if(ModelState.IsValid)
            {
                var user = _userService.FindOne(UserId);
                var profile = Mapper.Map<PublicProfileModel, PublicProfile>(model);
                user.PublicProfile = profile;

                _userService.Save(user);

                return this.RedirectToAction(x => x.PublicProfile()).Success("Public profile saved");
            }

            return View(model).Error(Common.Messages.FormValidationError);
        }

        [HttpGet]
        public ActionResult Account()
        {
            var user = _userService.FindOne(UserId);
            if(user == null)
            {
                return this.RedirectToAction<RegistrationController>(x => x.SignIn()).Error("Please sign in");
            }

            return View();
        }

        [HttpGet]
        public ActionResult Delete()
        {
            return this.RedirectToAction(x => x.Account());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(DeleteAccountModel model)
        {
            if(ModelState.IsValid)
            {
                var user = _userService.FindOne(UserId);

                if(PasswordHelper.Verify(model.Password, user.Password))
                {
                    _userService.DeleteData(UserId, false);
                    return this.RedirectToAction(x => x.Index()).Success("All your data has been deleted");
                }

                return this.RedirectToAction(x => x.Account()).Error("Please check your password");
            }

            return this.RedirectToAction(x => x.Account()).Error("Your data was not deleted");
        }

        [HttpGet]
        public ActionResult DeleteAccount()
        {
            return this.RedirectToAction(x => x.Account());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAccount(DeleteAccountModel model)
        {
            if(ModelState.IsValid)
            {
                var user = _userService.FindOne(UserId);

                if(PasswordHelper.Verify(model.Password, user.Password))
                {
                    _userService.DeleteData(UserId, true);
                    FormsAuthentication.SignOut();
                    return this.RedirectToAction<HomeController>(x => x.Index()).Success("Your account has been deleted");
                }

                return this.RedirectToAction(x => x.Account()).Error("Please check your password");
            }

            return this.RedirectToAction(x => x.Account()).Error("Your account was not deleted");
        }

        [HttpGet]
        public ActionResult ImportExport()
        {
            ViewBag.AllowImports = SystemSettings.Instance.Values.UserData.AllowAccountImport;
            ViewBag.VerifyHash = SystemSettings.Instance.Values.UserData.VerifyHash;

            return View(new ImportDataUploadModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Export()
        {
            try
            {
                var wordService = DependencyResolver.Current.GetService<IWordService>();
                var languageService = DependencyResolver.Current.GetService<ILanguageService>();
                var sytemLanguageService = DependencyResolver.Current.GetService<ISystemLanguageService>();
                var itemService = DependencyResolver.Current.GetService<IItemService>();

                var user = _userService.FindOne(UserId);
                var languages = languageService.FindAllForOwner();
                var slanguages = languages.Select(l => sytemLanguageService.FindOne(l.SystemLanguageId)).ToList();
                var words = wordService.FindAllForOwner();
                var items = itemService.FindAllForOwner();

                //Move to a data contract
                user.Password = string.Empty;
                foreach(var item in items) item.TokenisedText = null;

                var merged = new ImportExportDataModel()
                {
                    User = user,
                    SystemLanguages = slanguages,
                    Languages = languages,
                    Items = items,
                    Words = words
                };

                var settings = new JsonSerializerSettings();
                settings.ContractResolver = new MongoIdJsonContractResolver();
                string json = JsonConvert.SerializeObject(merged, Formatting.Indented, settings);

                var sharedSecret = SystemSettings.Instance.Values.Security.SharedSecret;
                string hash = null;
                if(!string.IsNullOrEmpty(sharedSecret))
                {
                    string hash1 = PasswordHelper.CalculateSHA1(json);
                    string hash2 = PasswordHelper.CalculateSHA1(string.Format("{0}{1}", hash1, sharedSecret));
                    hash = PasswordHelper.CalculateSHA1(hash2);
                }

                MemoryStream ms = new MemoryStream();
                using(ZipFile zip = new ZipFile())
                {
                    zip.AddEntry("account.json", json, Encoding.UTF8);

                    if(!string.IsNullOrEmpty(hash))
                    {
                        zip.AddEntry("donotdelete.txt", hash);
                    }

                    zip.Save(ms);
                }

                ms.Seek(0, SeekOrigin.Begin);
                return File(ms, "application/zip", "userdata.zip");
            }
            catch(Exception e)
            {
                return this.RedirectToAction(x => x.ImportExport()).Error(e.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(ImportDataUploadModel model)
        {
            if(!SystemSettings.Instance.Values.UserData.AllowAccountImport)
            {
                return this.RedirectToAction(x => x.ImportExport()).Error("Imports are not available on this site");
            }

            if(ModelState.IsValid)
            {
                try
                {
                    ImportExportDataModel data;

                    using(var zip = ZipFile.Read(model.File.InputStream))
                    {
                        var accountData = zip["account.json"];

                        if(accountData == null)
                        {
                            throw new FileNotFoundException("The account.json file was not in the ZIP archive");
                        }

                        string jsonString = null;
                        using(var sr = new StreamReader(accountData.OpenReader()))
                        {
                            jsonString = sr.ReadToEnd();
                        }

                        if(SystemSettings.Instance.Values.UserData.VerifyHash)
                        {
                            var hashData = zip["donotdelete.txt"];

                            if(hashData == null)
                            {
                                throw new FileNotFoundException("Could not find hash data to verify account");
                            }

                            string originalHash = null;
                            using(var sr = new StreamReader(hashData.OpenReader()))
                            {
                                originalHash = sr.ReadToEnd();
                            }

                            string hash1 = PasswordHelper.CalculateSHA1(jsonString);
                            string hash2 = PasswordHelper.CalculateSHA1(string.Format("{0}{1}", hash1, SystemSettings.Instance.Values.Security.SharedSecret));
                            string hash3 = PasswordHelper.CalculateSHA1(hash2);

                            if(!originalHash.Equals(hash3, StringComparison.InvariantCultureIgnoreCase))
                            {
                                throw new NotSupportedException("Could not verify hash");
                            }
                        }

                        var settings = new JsonSerializerSettings();
                        settings.ContractResolver = new MongoIdJsonContractResolver();
                        data = JsonConvert.DeserializeObject<ImportExportDataModel>(jsonString, settings);
                    }

                    //TODO decide how to handle data imports, JSON serializer needs writing
                    //--> ids that are no longer unique, manipulated
                    //--> reassign all ids?

                    var wordService = DependencyResolver.Current.GetService<IWordService>();
                    var languageService = DependencyResolver.Current.GetService<ILanguageService>();
                    var systemLanguageService = DependencyResolver.Current.GetService<ISystemLanguageService>();
                    var itemService = DependencyResolver.Current.GetService<IItemService>();

                    Dictionary<ObjectId, ObjectId> assigned = new Dictionary<ObjectId, ObjectId>();

                    foreach(var language in data.Languages)
                    {
                        language.Owner = UserId;
                        ObjectId newId = ObjectId.GenerateNewId();
                        assigned[language.LanguageId] = newId;
                        language.LanguageId = newId;

                        var sl = systemLanguageService.FindOne(language.SystemLanguageId);
                        if(sl == null) language.SystemLanguageId = systemLanguageService.FindByCode(SystemLanguage.NotYetSetCode).SystemLanguageId;

                        languageService.Save(language);
                    }

                    foreach(var item in data.Items)
                    {
                        item.Owner = UserId;
                        ObjectId newId = ObjectId.GenerateNewId();
                        assigned[item.ItemId] = newId;
                        item.ItemId = newId;
                        item.LanguageId = assigned[item.LanguageId];

                        itemService.Save(item);
                    }

                    foreach(var word in data.Words)
                    {
                        word.Owner = UserId;
                        ObjectId newId = ObjectId.GenerateNewId();
                        assigned[word.WordId] = newId;
                        word.WordId = newId;
                        word.LanguageId = assigned[word.LanguageId];
                        //word.ItemId = assigned.ContainsKey(word.ItemId) ? assigned[word.ItemId] : ObjectId.Empty;

                        wordService.Save(word);
                    }

                    return this.RedirectToAction(x => x.ImportExport()).Success("Success! Your data was imported.");
                }
                catch(Exception e)
                {
                    return this.RedirectToAction(x => x.ImportExport()).Error(e.Message);
                }
            }

            return this.RedirectToAction(x => x.ImportExport()).Error(model.File == null || model.File.InputStream == null || model.File.ContentLength == 0 ? "Please choose a file" : "Something went wrong :(");
        }

        [HttpGet]
        public ActionResult LwtImport()
        {
            return View();
        }

        [HttpGet]
        public ActionResult LwtImportJson()
        {
            return View(new LwtJsonModel());
        }

        [HttpPost]
        public ActionResult LwtImportJson(LwtJsonModel model)
        {
            if(ModelState.IsValid)
            {
                var lwt = Mapper.Map<LwtJsonModel, Lwt>(model);

                try
                {
                    if(ZipFile.IsZipFile(model.File.InputStream, true))
                    {
                        model.File.InputStream.Position = 0;
                        using(var zip = ZipFile.Read(model.File.InputStream))
                        {
                            var data = zip[0];

                            if(data == null)
                            {
                                throw new FileNotFoundException();
                            }

                            using(var sr = new StreamReader(data.OpenReader()))
                            {
                                lwt.JsonData = sr.ReadToEnd();
                            }
                        }
                    }
                    else
                    {
                        using(var sr = new StreamReader(model.File.InputStream, Encoding.UTF8))
                        {
                            lwt.JsonData = sr.ReadToEnd();
                        }
                    }
                }
                catch
                {
                    using(var sr = new StreamReader(model.File.InputStream, Encoding.UTF8))
                    {
                        lwt.JsonData = sr.ReadToEnd();
                    }
                }
                if(!string.IsNullOrEmpty(lwt.JsonData))
                {
                    var lwtImportService = DependencyResolver.Current.GetService<ILwtImportService>();
                    lwtImportService.ImportDataJson(lwt);

                    if(lwt.TestMode)
                    {
                        return View(model).Success("The test was successful, all your data was recognised.");
                    }
                    else
                    {
                        return this.RedirectToAction(x => x.LwtImportJson()).Success("Import was succesful");
                    }
                }
            }

            return View(model).Error("Please check the errors below");
        }

        [HttpGet]
        public ActionResult LwtImportSql()
        {
            return View(new LwtModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LwtImportSql(LwtModel model)
        {
            if(ModelState.IsValid)
            {
                var lwtImportService = DependencyResolver.Current.GetService<ILwtImportService>();
                if(model.Ping)
                {
                    var result = lwtImportService.PingServer(Mapper.Map<LwtModel, Lwt>(model));
                    ModelState.AddModelError("Ping", result ? "Connection Successful" : "Could not connect");
                }
                else
                {
                    lwtImportService.ImportData(Mapper.Map<LwtModel, Lwt>(model));

                    if(model.TestMode)
                    {
                        return View(model).Success("The test was successful, all your data was recognised.");
                    }
                    else
                    {
                        return this.RedirectToAction(x => x.LwtImportSql()).Success("Import was succesful");
                    }
                }
            }

            return View(model).Error("Please check the errors below");
        }
    }
}
