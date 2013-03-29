using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dependencies;
using NHibernate;
using ReadingTool.Common;
using ReadingTool.Entities;
using ReadingTool.Services;
using ReadingTool.Repository;

namespace ReadingTool.Api.Handler
{
    public class BasicAuthenticationMessageHandler : DelegatingHandler
    {
        private readonly IUserService _userService;
        public const string BasicScheme = "Basic";
        public const string ChallengeAuthenticationHeaderName = "WWW-Authenticate";
        public const char AuthorizationHeaderSeparator = ':';

        public BasicAuthenticationMessageHandler()
            : this(GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IUserService)) as IUserService)
        {

        }

        public BasicAuthenticationMessageHandler(IUserService userService)
        {
            _userService = userService;
        }

        protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            var authHeader = request.Headers.Authorization;
            if(authHeader == null)
            {
                return CreateUnauthorizedResponse();
            }
            if(authHeader.Scheme != BasicScheme)
            {
                return CreateUnauthorizedResponse();
            }

            var encodedCredentials = authHeader.Parameter;
            var credentialBytes = Convert.FromBase64String(encodedCredentials);
            var credentials = Encoding.ASCII.GetString(credentialBytes);
            var credentialParts = credentials.Split(AuthorizationHeaderSeparator);
            if(credentialParts.Length != 2)
            {
                return CreateUnauthorizedResponse();
            }
            var username = credentialParts[0].Trim();
            var password = credentialParts[1].Trim();

            var user = _userService.ValidateUser(username, password);

            if(user == null)
            {
                return CreateUnauthorizedResponse();
            }
            else
            {
                var identity = new GenericIdentity(user.UserId.ToString(), BasicScheme);
                identity.AddClaim(new Claim(ClaimTypes.Sid, user.UserId.ToString()));
                identity.AddClaim(new Claim(ClaimTypes.Email, user.EmailAddress));
                identity.AddClaim(new Claim(ClaimTypes.GivenName, user.Username));

                var principal = new GenericPrincipal(identity, new string[] { });
                Thread.CurrentPrincipal = principal;
                if(HttpContext.Current != null)
                {
                    HttpContext.Current.User = principal;
                }
            }

            return base.SendAsync(request, cancellationToken);
        }

        private Task<HttpResponseMessage> CreateUnauthorizedResponse()
        {
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            response.Headers.Add(ChallengeAuthenticationHeaderName, BasicScheme);
            var taskCompletionSource = new TaskCompletionSource<HttpResponseMessage>();
            taskCompletionSource.SetResult(response);
            return taskCompletionSource.Task;
        }
    }
}