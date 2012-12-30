using System.Web.Mvc;
using System.Web.Routing;

namespace ReadingTool.Site.Attributes
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if(filterContext.HttpContext != null)
            {
                if(!string.IsNullOrEmpty(Roles))
                {
                    string[] requestedRoles = Roles.Split(',');
                    bool isInRole = false;
                    foreach(var role in requestedRoles)
                    {
                        if(filterContext.HttpContext.User.IsInRole(role))
                        {
                            isInRole = true;
                            break;
                        }
                    }

                    if(!isInRole)
                    {
                        filterContext.Result = new RedirectToRouteResult(
                            new RouteValueDictionary
                                {
                                    {"controller", "~~Home"},
                                    {"action", "Denied"}
                                });

                        return;
                    }
                }
            }

            base.OnAuthorization(filterContext);
        }
    }
}