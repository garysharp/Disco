using Disco.Web.Extensions;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            // Remove Default Json Value Provider Factory (JavaScriptSerializer)
            var defaultJsonValueProvider = ValueProviderFactories.Factories.OfType<JsonValueProviderFactory>().FirstOrDefault();
            if (defaultJsonValueProvider != null)
            {
                ValueProviderFactories.Factories.Remove(defaultJsonValueProvider);
            }

            // Add Custom Json Value Provider Factory (Json.Net)
            ValueProviderFactories.Factories.Add(new JsonDotNetValueProviderFactory());
        }
    }
}