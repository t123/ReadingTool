#region License
// UserController.cs is part of ReadingTool.API
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

using ReadingTool.API.Areas.V1.Common;
using ReadingTool.API.Areas.V1.Models;
using ReadingTool.API.Common;
using ReadingTool.Common.Helpers;
using ReadingTool.Entities;
using ReadingTool.Services;

namespace ReadingTool.API.Areas.V1.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public UserController(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        public JsonNetResult Authenticate(string username, string password)
        {
            var user = _userService.FindOneByUsername(username);
            AuthorisationResponse model;

            if(user == null || !PasswordHelper.Verify(password, user.Password))
            {
                model = new AuthorisationResponse { StatusCode = StatusCode.BadCredentials, StatusMessage = "Invalid username or password" };
            }
            else
            {
                var token = _tokenService.Save(new Token() { UserId = user.UserId });
                model = new AuthorisationResponse { StatusCode = StatusCode.Ok, Token = token.TokenId };
            }

            return new JsonNetResult() { Data = model };
        }

        public JsonNetResult AuthenticationFailed()
        {
            var model = new BaseResponseModel { StatusCode = StatusCode.BadCredentials, StatusMessage = "Invalid token" };
            return new JsonNetResult() { Data = model };
        }

        public JsonNetResult NoCompression()
        {
            var model = new BaseResponseModel { StatusCode = StatusCode.ClientError, StatusMessage = "Client must support compression (send Accept-Encoding in header)" };
            return new JsonNetResult() { Data = model };
        }

        public JsonNetResult PostOnly()
        {
            var model = new BaseResponseModel { StatusCode = StatusCode.ClientError, StatusMessage = "HTTP post only" };
            return new JsonNetResult() { Data = model };
        }

        public JsonNetResult LimitExceeded()
        {
            var model = new BaseResponseModel { StatusCode = StatusCode.LimitExceeded, StatusMessage = "API limit exceeded" };
            return new JsonNetResult() { Data = model };
        }
    }
}
