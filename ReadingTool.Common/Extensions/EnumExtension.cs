using System;
using System.ComponentModel;
using System.Reflection;

namespace ReadingTool.Common.Extensions
{
    public static class EnumExtension
    {
        public static string ToDescription(this Enum e)
        {
            try
            {
                FieldInfo fi = e.GetType().GetField(e.ToString());
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if(attributes.Length > 0)
                {
                    string description = attributes[0].Description;
                    return description;
                }

                return e.ToString();
            }
            catch
            {
                return e.ToString();
            }
        }
    }
}
