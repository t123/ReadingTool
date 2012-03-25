using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ReadingTool.Common.Attributes
{
    public class AltRegularExpressionAttribute : RegularExpressionAttribute
    {
        public AltRegularExpressionAttribute(string pattern)
            : base(pattern)
        {
        }

        /// <summary>
        /// Mono validates the empty string as false, even if it's not required
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(value == null || string.IsNullOrEmpty(value.ToString()))
                return ValidationResult.Success;

            return base.IsValid(value, validationContext);
        }
    }
}