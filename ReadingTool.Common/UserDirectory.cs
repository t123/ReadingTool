using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingTool.Common
{
    public class UserDirectory
    {
        public static string GetDirectory(Guid userId)
        {
            string id = userId.ToString();
            string bucket = id.Substring(id.Length - 2, 2);
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Users", bucket, id);
            return path;
        }
    }
}
