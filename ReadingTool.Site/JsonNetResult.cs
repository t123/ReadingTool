using System;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ReadingTool.Site
{
    /// <summary>
    /// <see cref="http://stackoverflow.com/questions/7109967/using-json-net-as-default-json-serializer-in-asp-net-mvc-3-is-it-possible"/>
    /// </summary>
    public class JsonNetResult : JsonResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            if(context == null)
                throw new ArgumentNullException("context");

            var response = context.HttpContext.Response;

            response.ContentType = !string.IsNullOrEmpty(ContentType) ? ContentType : "application/json";

            if(ContentEncoding != null)
                response.ContentEncoding = ContentEncoding;

            if(Data == null)
                return;

            var serializedObject = JsonConvert.SerializeObject(
                Data,
                Formatting.None,
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            response.Write(serializedObject);
        }
    }
}