#region License
// BaseResponse.cs is part of ReadingTool.API
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

namespace ReadingTool.API.Areas.V1.Models
{
    public class BaseResponseModel
    {
        public StatusCode StatusCode { get; set; }
        public string StatusMessage { get; set; }

        public BaseResponseModel()
        {
            StatusCode = StatusCode.Ok;
            StatusMessage = string.Empty;
        }
    }

    public class AuthorisationResponse : BaseResponseModel
    {
        public string Token { get; set; }
    }

    public class CountResponse : BaseResponseModel
    {
        public int Count { get; set; }
        public int PerPage { get; set; }

        public CountResponse()
        {
            PerPage = 250;
        }
    }

    public class LanguageResponse : BaseResponseModel
    {
        public LanguageModel[] Languages { get; set; }
    }

    public class ItemResponse : BaseResponseModel
    {
        public ItemModel[] Items { get; set; }
    }

    public class WordResponse : BaseResponseModel
    {
        public WordModel[] Words { get; set; }
    }

    public class GroupResponse : BaseResponseModel
    {
        public GroupModel[] Groups { get; set; }
    }

    public class GroupItemsResponse : BaseResponseModel
    {
        public GroupItemModel[] Items { get; set; }
    }

    public class ErrorResponse : BaseResponseModel
    {
    }
}