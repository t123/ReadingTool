﻿#region License
// TokenMessageHandler.cs is part of ReadingTool.Api
// 
// ReadingTool.Api is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Api is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Api. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2013 Travis Watt
#endregion

using System;
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
using ReadingTool.Services;

namespace ReadingTool.Api.Handler
{
    public class TokenMessageHandler : DelegatingHandler
    {
        private readonly IUserService _userService;
        public const string ChallengeAuthenticationHeaderName = "X-ApiKey";

        public TokenMessageHandler()
            : this(GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IUserService)) as IUserService)
        {

        }

        public TokenMessageHandler(IUserService userService)
        {
            _userService = userService;
        }

        protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            if(!request.Headers.Contains(ChallengeAuthenticationHeaderName))
            {
                return CreateUnauthorizedResponse();
            }

            var encodedCredentials = request.Headers.GetValues(ChallengeAuthenticationHeaderName).FirstOrDefault();

            if(string.IsNullOrEmpty(encodedCredentials))
            {
                return CreateUnauthorizedResponse();
            }

            var credentialBytes = Convert.FromBase64String(encodedCredentials);
            var apiKey = Encoding.ASCII.GetString(credentialBytes);

            var user = _userService.GetUserByApiKey(apiKey);

            if(user == null)
            {
                return CreateUnauthorizedResponse();
            }
            else
            {
                var identity = new GenericIdentity(user.UserId.ToString(), "ApiKey");
                identity.AddClaim(new Claim(ClaimTypes.Sid, user.UserId.ToString()));
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
            response.Headers.Add(ChallengeAuthenticationHeaderName, "ApiKey");
            var taskCompletionSource = new TaskCompletionSource<HttpResponseMessage>();
            taskCompletionSource.SetResult(response);
            return taskCompletionSource.Task;
        }
    }
}