using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ReadingTool.Api.Handler
{
    public class CorsHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string Origin = @"Origin";
            string AccessControlAllowOrigin = @"Access-Control-Allow-Origin";
            string AccessControlRequestMethod = @"Access-Control-Request-Method";
            string AccessControlAllowMethods = @"Access-Control-Allow-Methods";
            string AccessControlAllowHeaders = @"Access-Control-Allow-Headers";
            string AccessControlRequestHeaders = @"Access-Control-Request-Headers";

            bool isCorsRequest = request.Headers.Contains(Origin);
            bool isPreflightRequest = request.Method == HttpMethod.Options;
            if(isCorsRequest)
            {
                if(isPreflightRequest)
                {
                    return Task.Factory.StartNew<HttpResponseMessage>(() =>
                        {
                            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                            response.Headers.Add(AccessControlAllowOrigin, request.Headers.GetValues(Origin).First());

                            string accessControlRequestMethod = request.Headers.GetValues(AccessControlRequestMethod).FirstOrDefault();
                            if(accessControlRequestMethod != null)
                            {
                                response.Headers.Add(AccessControlAllowMethods, accessControlRequestMethod);
                            }

                            string requestedHeaders = string.Join(", ", request.Headers.GetValues(AccessControlRequestHeaders));
                            if(!string.IsNullOrEmpty(requestedHeaders))
                            {
                                response.Headers.Add(AccessControlAllowHeaders, requestedHeaders);
                            }

                            return response;
                        }, cancellationToken);
                }
                else
                {
                    return base.SendAsync(request, cancellationToken).ContinueWith<HttpResponseMessage>(t =>
                    {
                        HttpResponseMessage resp = t.Result;
                        resp.Headers.Add(AccessControlAllowOrigin, request.Headers.GetValues(Origin).First());
                        return resp;
                    });
                }
            }
            else
            {
                return base.SendAsync(request, cancellationToken);
            }
        }
    }
}