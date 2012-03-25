#region License
// HandleErrorAttribute.cs is part of ReadingTool.API
// 
// ReadingTool.API is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.API is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.API. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System.Web.Mvc;

namespace ReadingTool.API.Attributes
{
    public class CustomHandleErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            //var response = new ErrorResponse()
            //                   {
            //                       StatusCode = StatusCode.ServerError,
            //                       StatusMessage = filterContext.Exception.Message
            //                   };

            //filterContext.HttpContext.Response.StatusCode = 500;
            //filterContext.HttpContext.Response.ContentType = "application/json";

            //var serializedObject = JsonConvert.SerializeObject(
            //    response,
            //    Formatting.None,
            //    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            
            //filterContext.HttpContext.Response.Write(serializedObject);
            //filterContext.HttpContext.Response.End();
            base.OnException(filterContext);
        }
    }
}