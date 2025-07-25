using System.Web;
using System.Web.Helpers;

namespace Disco.Web
{
    public static class AntiForgeryExtensions
    {
        public static string GetToken(HttpContextBase context)
        {
            var previousCookieToken = default(string);

            var previousCookie = context.Request.Cookies[AntiForgeryConfig.CookieName];
            if (previousCookie != null && !string.IsNullOrEmpty(previousCookie.Value))
                previousCookieToken = previousCookie.Value;

            AntiForgery.GetTokens(previousCookieToken, out var cookieToken, out var formToken);

            if (cookieToken != null)
            {
                var httpCookie = new HttpCookie(AntiForgeryConfig.CookieName, cookieToken);
                httpCookie.HttpOnly = true;
                if (AntiForgeryConfig.RequireSsl)
                    httpCookie.Secure = true;

                context.Response.Cookies.Set(httpCookie);
            }
            return formToken;
        }
    }
}
