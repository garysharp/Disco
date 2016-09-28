using Disco.Models.ClientServices;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Reflection;

namespace Disco.Client.Extensions
{
    public static class ClientServicesExtensions
    {
#if DEBUG
        public const string ServicePathAuthenticatedTemplate = "http://WS-GSHARP:57252/Services/Client/Authenticated/{0}";
        public const string ServicePathUnauthenticatedTemplate = "http://WS-GSHARP:57252/Services/Client/Unauthenticated/{0}";
#else
        public const string ServicePathAuthenticatedTemplate = "http://DISCO:9292/Services/Client/Authenticated/{0}";
        public const string ServicePathUnauthenticatedTemplate = "http://DISCO:9292/Services/Client/Unauthenticated/{0}";
#endif

        public static ResponseType Post<ResponseType>(this ServiceBase<ResponseType> Service, bool Authenticated)
        {
            ResponseType serviceResponse;
            string serviceUrl;

            if (Authenticated)
                serviceUrl = string.Format(ServicePathAuthenticatedTemplate, Service.Feature);
            else
                serviceUrl = string.Format(ServicePathUnauthenticatedTemplate, Service.Feature);

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(serviceUrl);
            request.UserAgent = $"Disco-Client/{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}";
            request.ContentType = "application/json";
            request.Method = WebRequestMethods.Http.Post;
            request.UseDefaultCredentials = true;
            request.Timeout = 300000; // 5 Minutes

            var jsonSerializer = new JsonSerializer();

            using (var requestWriter = new StreamWriter(request.GetRequestStream()))
            {
                using (var jsonWriter = new JsonTextWriter(requestWriter))
                {
                    jsonSerializer.Serialize(jsonWriter, Service);
                }
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (var responseReader = new StreamReader(response.GetResponseStream()))
                {
                    using (var jsonReader = new JsonTextReader(responseReader))
                    {
                        serviceResponse = jsonSerializer.Deserialize<ResponseType>(jsonReader);
                    }
                }
            }

            return serviceResponse;
        }

    }
}
