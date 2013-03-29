#region License
// GravatarHelperExtension.cs is part of ReadingTool.Site
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
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;

namespace ReadingTool.Site.Helpers
{
    public static class GravatarHelperExtension
    {
        public static HtmlString Gravatar(this HtmlHelper helper, string emailAddress, string displayName = "", string classes = "")
        {
            TagBuilder img = new TagBuilder("img");
            img.AddCssClass(classes);

            if(string.IsNullOrWhiteSpace(emailAddress))
            {
                img.Attributes.Add("src", "http://www.gravatar.com/avatar/?s=80&d=mm");
                img.Attributes.Add("height", "80");
                img.Attributes.Add("width", "80");
                img.Attributes.Add("title", "Get your gravatar");
                img.Attributes.Add("alt", "gravatar");
            }
            else
            {
                System.Text.ASCIIEncoding encoder = new System.Text.ASCIIEncoding();
                byte[] combined = encoder.GetBytes(emailAddress);
                MD5CryptoServiceProvider cryptoTransformMD5 = new MD5CryptoServiceProvider();
                string hashValue = BitConverter.ToString(cryptoTransformMD5.ComputeHash(combined)).Replace("-", "");

                img.Attributes.Add("src", string.Format("https://www.gravatar.com/avatar/{0}?s=80&d=mm", hashValue));
                img.Attributes.Add("height", "80");
                img.Attributes.Add("width", "80");
                img.Attributes.Add("title", "your gravatar");
                img.Attributes.Add("alt", "gravatar");
            }

            return new MvcHtmlString(img.ToString());
        }
    }
}