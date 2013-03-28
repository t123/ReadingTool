using System;
using System.Collections.Generic;
using System.Linq;
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