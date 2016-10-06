using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web.Mvc;

namespace Disco.Web.Extensions
{
    public class JsonDotNetValueProviderFactory : ValueProviderFactory
    {
        public override IValueProvider GetValueProvider(ControllerContext controllerContext)
        {
            if (controllerContext == null)
                throw new ArgumentNullException(nameof(controllerContext));

            if (!controllerContext.HttpContext.Request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
                return null;

            if (controllerContext.HttpContext.Request.InputStream.Length == 0)
                return null;

            var jsonSerializer = new JsonSerializer();
            jsonSerializer.Converters.Add(new ExpandoObjectConverter());

            using (var streamReader = new StreamReader(controllerContext.HttpContext.Request.InputStream, Encoding.UTF8, true, 0x400, true))
            {
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    var bodyObject = jsonSerializer.Deserialize<ExpandoObject>(jsonReader);
                    return new DictionaryValueProvider<object>(bodyObject, CultureInfo.CurrentCulture);
                }
            }
        }
    }
}
