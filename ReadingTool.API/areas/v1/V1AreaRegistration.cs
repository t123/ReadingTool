#region License
// V1AreaRegistration.cs is part of ReadingTool.API
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
using LowercaseRoutesMVC;

namespace ReadingTool.API.Areas.V1
{
    public class V1AreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "v1";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            #region user routes
            context.MapRouteLowercase(
                "User_Authenticate",
                "v1/user/authenticate/{username}/{password}",
                new { action = "authenticate", controller = "user" }
            );

            context.MapRouteLowercase(
                "User_AuthenticationFailed",
                "v1/user/authenticationfailed",
                new { action = "authenticationfailed", controller = "user" }
            );

            context.MapRouteLowercase(
                "User_AuthenticationNoCompression",
                "v1/user/nocompression",
                new { action = "nocompression", controller = "user" }
            );

            context.MapRouteLowercase(
                "User_AuthenticationLimitExceeded",
                "v1/user/limitexceeded",
                new { action = "limitexceeded", controller = "user" }
            );

            context.MapRouteLowercase(
                "User_PostOnly",
                "v1/user/postonly",
                new { action = "postonly", controller = "user" }
            );
            #endregion

            #region language routes
            context.MapRouteLowercase(
                "Languages_List",
                "v1/languages/list",
                new { action = "list", controller = "languages" }
            );

            context.MapRouteLowercase(
                "Languages_List_ByModified",
                "v1/languages/modified/{modified}",
                new { action = "listbymodified", controller = "languages" }
            );

            context.MapRouteLowercase(
                "Languages_Single",
                "v1/languages/{languageId}",
                new { action = "single", controller = "languages" }
            );
            #endregion

            #region items routes
            context.MapRouteLowercase(
               "Items_List_ByLanguageModified",
               "v1/items/modified/{languageId}/{modified}",
               new { action = "listbylanguagemodified", controller = "items" }
           );

            context.MapRouteLowercase(
                "Items_List_ByLanguage",
                "v1/items/{languageId}/list",
                new { action = "listbylanguage", controller = "items" }
            );

            context.MapRouteLowercase(
                "Items_List_ByModified",
                "v1/items/modified/{modified}",
                new { action = "listbymodified", controller = "items" }
            );

            context.MapRouteLowercase(
                "Items_List",
                "v1/items/list",
                new { action = "list", controller = "items" }
            );

            context.MapRouteLowercase(
                "Items_Single",
                "v1/items/{itemId}",
                new { action = "single", controller = "items" }
            );
            #endregion

            #region word routes
            context.MapRouteLowercase(
                "Words_Update",
                "v1/words/update",
                new { action = "update", controller = "words" }
            );

            context.MapRouteLowercase(
                "Words_List_ByLanguageModified",
                "v1/words/modified/{languageId}/{modified}",
                new { action = "listbylanguagemodified", controller = "words" }
            );

            context.MapRouteLowercase(
                "Words_List_ByModified",
                "v1/words/modified/{modified}",
                new { action = "listbymodified", controller = "words" }
            );

            context.MapRouteLowercase(
                "Words_List_ByLanguage",
                "v1/words/{languageId}/list",
                new { action = "listbylanguage", controller = "words" }
            );

            context.MapRouteLowercase(
                "Words_List",
                "v1/words/list",
                new { action = "list", controller = "words" }
            );

            context.MapRouteLowercase(
                "Words_Single",
                "v1/words/{wordId}",
                new { action = "single", controller = "words" }
            );
            #endregion

            #region group routes
            context.MapRouteLowercase(
                "Groups_ListItemsModified",
                "v1/groups/modified/{groupId}/{modified}",
                new { action = "listitemsmodified", controller = "groups" }
            );

            context.MapRouteLowercase(
                "Groups_ListItems",
                "v1/groups/{groupId}/list",
                new { action = "listitems", controller = "groups" }
            );

            context.MapRouteLowercase(
                "Groups_List",
                "v1/groups/list",
                new { action = "list", controller = "groups" }
            );
            #endregion
        }
    }
}
