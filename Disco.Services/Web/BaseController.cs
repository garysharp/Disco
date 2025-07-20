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

        protected static HttpStatusCodeResult StatusCode(HttpStatusCode statusCode, string statusDescription = null)
            => new HttpStatusCodeResult(statusCode, statusDescription);

        protected static HttpNotFoundResult NotFound(string statusDescription = null)
            => new HttpNotFoundResult(statusDescription);

        protected static HttpUnauthorizedResult Unauthorized(string statusDescription = null)
            => new HttpUnauthorizedResult(statusDescription);
    }
}
