#region License
// LanguageDirection.cs is part of ReadingTool.Common
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
// Copyright (C) 2013 Travis Watt
#endregion

using System.ComponentModel;

namespace ReadingTool.Common
{
    public enum LanguageDirection
    {
        [Description("Left To Right")]
        LTR = 1,

        [Description("Right To Left")]
        RTL = 2
    }

    public enum ModalBehaviour
    {
        [Description("Left click")]
        LeftClick = 0,

        //Rollover = 2, //This is unusable

        [Description("Control left click")]
        CtrlLeftClick = 3,

        [Description("Shift left click")]
        ShiftLeftClick = 4,

        [Description("Middle click")]
        MiddleClick = 5,

        [Description("Right click")]
        RightClick = 6
    }
}
