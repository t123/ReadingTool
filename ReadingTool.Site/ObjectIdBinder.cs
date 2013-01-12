using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using MongoDB.Bson;

namespace ReadingTool.Site
{
    public class ObjectIdBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if(result.AttemptedValue == null)
            {
                return null;
            }

            ObjectId test;
            if(ObjectId.TryParse(result.AttemptedValue, out test))
            {
                return test;
            }

            return null;
        }
    }
}
