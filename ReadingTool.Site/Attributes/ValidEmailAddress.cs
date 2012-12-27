using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ReadingTool.Site.Attributes
{
    public class ValidEmailAddress : RegularExpressionAttribute
    {
        public ValidEmailAddress()
            : base("[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?")
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format("The {0} field is not a valid e-mail address", name);
        }

        public override bool IsValid(object value)
        {
            if(value == null)
            {
                return true;
            }

            if(string.IsNullOrWhiteSpace(value.ToString()))
            {
                return true;
            }

            return base.IsValid(value);
        }
    }
}