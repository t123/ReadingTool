#region License
// Xor.cs is part of ReadingTool.API
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

using System;

namespace ReadingTool.API.Areas.V1.Common
{
    public static class Xor
    {
        public static string Transform(string crypt, string password)
        {
            int iInIndex = 0;
            int iKeyIndex = 0;
            string xOR = string.Empty;

            if((crypt.Length == 0) | (password.Length == 0))
                return xOR;
            for(iInIndex = 0; iInIndex <= (crypt.Length - 1); iInIndex++)
            {
                iKeyIndex++;
                xOR += Convert.ToChar(Convert.ToInt32(Convert.ToChar(crypt.Substring(iInIndex, 1)))
                       ^ Convert.ToInt32(Convert.ToChar(password.Substring(iKeyIndex - 1, 1))));

                if(iKeyIndex == password.Length)
                    iKeyIndex = 0;

            }

            return xOR;
        }
    }

}