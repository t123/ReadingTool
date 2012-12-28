using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReadingTool.Core.Formatters
{
    public class DescriptionFormatter
    {
        public static string GetDescription<T>() where T : class
        {
            try
            {
                var t = typeof (T);
                DescriptionAttribute[] attributes = (DescriptionAttribute[])t.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if(attributes.Length > 0)
                {
                    string description = attributes[0].Description;
                    return description;
                }

                return t.Name.CamelCaseToWords();
            }
            catch
            {
                return "(unknown)";
            }
        }

        public static string GetDescription(object o)
        {
            if(o == null)
            {
                return "(unknown)";
            }

            try
            {
                DescriptionAttribute[] attributes = (DescriptionAttribute[])o.GetType().GetCustomAttributes(typeof(DescriptionAttribute), false);

                if(attributes.Length > 0)
                {
                    string description = attributes[0].Description;
                    return description;
                }

                return o.GetType().Name.CamelCaseToWords();
            }
            catch
            {
                return o.ToString();
            }
        }

        //public static string GetDescription(Type t)
        //{
        //    if(t == null)
        //    {
        //        return "(unknown)";
        //    }

        //    try
        //    {
        //        var o = Activator.CreateInstance(t);

        //        FieldInfo fi = o.GetType().GetField(o.ToString());
        //        DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

        //        if(attributes.Length > 0)
        //        {
        //            string description = attributes[0].Description;
        //            return description;
        //        }

        //        return o.ToString().CamelCaseToWords();
        //    }
        //    catch
        //    {
        //        return t.Name.CamelCaseToWords();
        //    }
        //}
    }
}
