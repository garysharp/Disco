using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Disco.Models.ClientServices;
using Newtonsoft.Json;

namespace Disco.Client.Extensions
{
    public static class ClientServicesExtensions
    {
        public const string ServicePathAuthenticatedTemplate = "http://DISCO:9292/Services/Client/Authenticated/{0}";
        public const string ServicePathUnauthenticatedTemplate = "http://DISCO:9292/Services/Client/Unauthenticated/{0}";

        public static ResponseType Post<ResponseType>(this ServiceBase<ResponseType> Service, bool Authenticated)
        {
            string jsonResponse;
            string serviceUrl;
            if (Authenticated)
                serviceUrl = string.Format(ServicePathAuthenticatedTemplate, Service.Feature);
            else
                serviceUrl = string.Format(ServicePathUnauthenticatedTemplate, Service.Feature);

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(serviceUrl);
            request.UserAgent = string.Format("Disco-Client/{0}", Assembly.GetExecutingAssembly().GetName().Version.ToString(3));
            request.ContentType = "application/json";
            request.Method = WebRequestMethods.Http.Post;
            request.UseDefaultCredentials = true;
            request.Timeout = 300000; // 5 Minutes
            string jsonRequest = JsonConvert.SerializeObject(Service);

            using (StreamWriter requestWriter = new StreamWriter(request.GetRequestStream()))
            {
                requestWriter.Write(jsonRequest);
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader responseReader = new StreamReader(response.GetResponseStream()))
                {
                    jsonResponse = responseReader.ReadToEnd();
                }
            }

            if (string.IsNullOrEmpty(jsonResponse))
                return default(ResponseType);
            else
                return JsonConvert.DeserializeObject<ResponseType>(jsonResponse);
        }

    }
}
