using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Optimization;
using System.IO;

namespace Disco.Web.Extensions.MvcExtensions.Bundles
{
    public class JsJoin : IBundleTransform
    {
        internal static string JsContentType;
        static JsJoin()
        {
            JsContentType = "text/javascript";
        }

        public void Process(BundleContext context, BundleResponse response)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (response == null)
                throw new ArgumentNullException("response");

            // Not Needed - the new Transforms pipeline has already loaded the content
            //if (!context.EnableInstrumentation && string.IsNullOrEmpty(response.Content))
            //{
            //    try
            //    {
            //        StringBuilder bundleContent = new StringBuilder();
            //        foreach (FileInfo file in response.Files)
            //        {
            //            string fileContent = File.ReadAllText(file.FullName);

            //            bundleContent.AppendLine(fileContent);
            //        }

            //        response.Content = bundleContent.ToString();
            //    }
            //    catch (Exception ex)
            //    {
            //        GenerateErrorResponse(response, new string[] { ex.GetType().Name, ex.Message, ex.StackTrace });
            //    }
            //}

            response.ContentType = JsContentType;
        }

        internal static void GenerateErrorResponse(BundleResponse bundle, ICollection<string> errors)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("/* ");
            builder.Append("Bundle creation failed [JsJoin].").Append("\r\n");
            foreach (string str in errors)
            {
                builder.Append(str).Append("\r\n");
            }
            builder.Append(" */\r\n");
            bundle.Content = builder.ToString();
        }

    }
}
