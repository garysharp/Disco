using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using Microsoft.AspNet.SignalR;
using Owin;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Disco.BI.Interop.SignalRHandlers
{
    /// <summary>
    /// Required for SignalR 1.1.0 NTLM support in Firefox & Safari
    /// Returns 401 (Unauthorized) instead of 403 (Forbidden) when an unauthenticated request is processed
    /// 
    /// TODO: Remove this workaround when implementing SignalR 2.x
    /// 
    /// Thanks to David Fowler (@davidfowl)
    /// </summary>
    public static class SignalRAuthenticationWorkaround
    {
        public static void AddMiddleware(IAppBuilder app)
        {
            Func<AppFunc, AppFunc> convert403To401 = Convert403To401;

            app.Use(convert403To401);
        }

        private static AppFunc Convert403To401(AppFunc next)
        {
            return env =>
            {
                // Execute the SignalR pipeline
                Task task = next(env);

                // Get the status code
                int statusCode = 0;
                if (env.ContainsKey("owin.ResponseStatusCode"))
                {
                    statusCode = (int)env["owin.ResponseStatusCode"];
                }

                // If its 403 then convert it to 401 (we shouldn't do 
                // this if it's a cross domain request since it doesn't make sense)
                if (statusCode == 403)
                {
                    env["owin.ResponseStatusCode"] = 401;
                }

                // Return the original task
                return task;
            };
        }
    }
}
