#region License
// SystemTaskResult.cs is part of ReadingTool.Entities
// 
// ReadingTool.Entities is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReadingTool.Entities is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with ReadingTool.Entities. If not, see <http://www.gnu.org/licenses/>.
// 
// Copyright (C) 2012 Travis Watt
#endregion

using System;
using System.Text;

namespace ReadingTool.Entities
{
    public class SystemTaskResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }

        public static SystemTaskResult Ok
        {
            get { return new SystemTaskResult() { Success = true }; }
        }

        public SystemTaskResult() { }

        public SystemTaskResult(bool success, string message, Exception exception)
        {
            Success = success;
            Message = message;
            Exception = exception;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}: ", Success ? "Passed" : "Failed");
            if(!string.IsNullOrEmpty(Message)) sb.Append(Message);
            if(Exception != null)
            {
                sb.AppendFormat(" ({0})", Exception.Message);
            }

            return sb.ToString();
        }
    }
}
