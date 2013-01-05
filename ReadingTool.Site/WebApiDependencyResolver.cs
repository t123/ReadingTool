using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ninject;

namespace ReadingTool.Site
{
    public class WebApiDependencyResolver : System.Web.Http.Dependencies.IDependencyResolver
    {
        private readonly IKernel _kernel;

        public WebApiDependencyResolver(IKernel kernel)
        {
            _kernel = kernel;
        }
        public System.Web.Http.Dependencies.IDependencyScope BeginScope()
        {
            return this;
        }

        public object GetService(Type serviceType)
        {
            return _kernel.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                return _kernel.GetAll(serviceType);
            }
            catch(Exception)
            {
                return new List<object>();
            }
        }

        public void Dispose()
        {
        }
    }
}