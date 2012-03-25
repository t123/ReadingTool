#region License
// EnumHelper.cs is part of ReadingTool.Common
// 
// ReadingTool.Common is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Common is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Common. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System.ComponentModel;
using System.Reflection;

namespace ReadingTool.Common.Helpers
{
    /// <summary>
    /// http://stackoverflow.com/questions/773303/splitting-camelcase
    /// </summary>
    public class EnumHelper
    {
        public static string GetDescription(System.Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes.Length > 0)
                return attributes[0].Description;

            return value.ToString();
        }

        public static string GetAlternateName(System.Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            AlternateNameAttribute[] attributes = (AlternateNameAttribute[])fi.GetCustomAttributes(typeof(AlternateNameAttribute), false);

            if (attributes.Length > 0)
                return attributes[0].Name;

            return value.ToString();
        }

        
    }
}
