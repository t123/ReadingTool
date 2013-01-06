using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ReadingTool.Entities;
using ReadingTool.Services;

namespace ReadingTool.Site.Controllers.Api
{
    public class AuthorizationHeaderHandler : DelegatingHandler
    {
        /// <summary>
        /// <see cref="http://dotnet.dzone.com/articles/api-key-user-aspnet-web-api"/>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                IEnumerable<string> apiKeyHeaderValues = null;
                request.Headers.TryGetValues("X-ApiKey", out apiKeyHeaderValues);
                var ak = (apiKeyHeaderValues ?? new string[] { }).ToDictionary(x => "X-ApiKey", x => x).FirstOrDefault();

                ak = request.GetQueryNameValuePairs().FirstOrDefault(x => x.Key == "X-ApiKey");

                if(string.IsNullOrWhiteSpace(ak.Value))
                {
                    return base.SendAsync(request, cancellationToken);
                }

                if(!string.IsNullOrWhiteSpace(ak.Value))
                {
                    var userService = DependencyResolver.Current.GetService<IUserService>();
                    var user = userService.FindUserByApiKey(ak.Value);

                    if(user == null)
                    {
                        return base.SendAsync(request, cancellationToken);
                    }

                    IUserIdentity identity = new UserIdentity(user.Id, true, "", user.Roles, "", "", "");
                    IPrincipal userPrincipal = new UserPrincipal(identity);
                    Thread.CurrentPrincipal = userPrincipal;

                    if(HttpContext.Current != null)
                    {
                        HttpContext.Current.User = userPrincipal;
                    }
                }

                return base.SendAsync(request, cancellationToken);
            }
            catch
            {
                return base.SendAsync(request, cancellationToken);
            }
        }
    }
}