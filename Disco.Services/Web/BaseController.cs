using System.Net;
using System.Web.Mvc;

namespace Disco.Services.Web
{
    [HandleError]
    public class BaseController : Controller
    {
        protected static HttpStatusCodeResult Ok(string message = null)
            => StatusCode(HttpStatusCode.OK, message);

        protected static HttpStatusCodeResult BadRequest(string message = null)
            => StatusCode(HttpStatusCode.BadRequest, message);

        protected static HttpStatusCodeResult StatusCode(HttpStatusCode statusCode, string message = null)
            => new HttpStatusCodeResult(statusCode, message);
    }
}
