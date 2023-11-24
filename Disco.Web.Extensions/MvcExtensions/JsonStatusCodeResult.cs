using Disco.Web.Extensions.MvcExtensions;
using System.Web.Mvc;

namespace Disco.Web.Extensions.MvcExtensions
{
    public class JsonStatusCodeResult : JsonResult
    {
        public int StatusCode { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.TrySkipIisCustomErrors = true;
            context.HttpContext.Response.StatusCode = StatusCode;
            base.ExecuteResult(context);
        }
    }
}

namespace Disco.Web
{
    public static class JsonStatusCodeResultExtensions
    {
        public static JsonStatusCodeResult JsonStatusCode(this Controller controller, int statusCode, object data)
        {
            return new JsonStatusCodeResult { StatusCode = statusCode, Data = data };
        }
    }
}
