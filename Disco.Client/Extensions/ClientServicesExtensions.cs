using Disco.Models.ClientServices;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Reflection;

namespace Disco.Client.Extensions
{
    internal static class ClientServicesExtensions
    {
        public static ResponseType Post<ResponseType>(this ServiceBase<ResponseType> service, bool authenticated)
        {
            ResponseType serviceResponse;
            Uri serviceUrl;

            if (authenticated)
                serviceUrl = new Uri(Program.ServerUrl, $"/Services/Client/Authenticated/{service.Feature}");
            else
                serviceUrl = new Uri(Program.ServerUrl, $"/Services/Client/Unauthenticated/{service.Feature}");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serviceUrl);
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
                    jsonSerializer.Serialize(jsonWriter, service);
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
