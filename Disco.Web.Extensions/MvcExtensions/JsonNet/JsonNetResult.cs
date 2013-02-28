using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace Disco.Web.Extensions
{
    public class JsonNetResult : JsonResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            var response = context.HttpContext.Response;

            response.ContentType = !string.IsNullOrEmpty(ContentType) ? ContentType : "application/json";

            if (ContentEncoding != null)
                response.ContentEncoding = ContentEncoding;

            if (Data == null)
                return;

            var serializedObject = JsonConvert.SerializeObject(Data, Formatting.Indented);

            response.Write(serializedObject);
        }
    }

    public static class JsonNetExtensions
    {
        public static JsonNetResult JsonNet(this Controller controller, object Data, JsonRequestBehavior JsonRequestBehavior)
        {
            return JsonNet(controller, Data, null, null, JsonRequestBehavior);
        }
        public static JsonNetResult JsonNet(this Controller controller, object Data, string ContentType, JsonRequestBehavior JsonRequestBehavior)
        {
            return JsonNet(controller, Data, ContentType, null, JsonRequestBehavior);
        }
        public static JsonNetResult JsonNet(this Controller controller, object Data, Encoding ContentEncoding, JsonRequestBehavior JsonRequestBehavior)
        {
            return JsonNet(controller, Data, null, ContentEncoding, JsonRequestBehavior);
        }
        public static JsonNetResult JsonNet(this Controller controller, object Data, string ContentType, Encoding ContentEncoding, JsonRequestBehavior JsonRequestBehavior)
        {
            return new JsonNetResult()
            {
                Data = Data,
                ContentType = ContentType,
                ContentEncoding = ContentEncoding,
                JsonRequestBehavior = JsonRequestBehavior
            };
        }
    }
}
