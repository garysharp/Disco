using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Web;
using dotless.Core;
using dotless.Core.configuration;
using System.Web.Optimization;

namespace Disco.Web.Extensions.MvcExtensions.Bundles
{
    public class LessCompile : IBundleTransform
    {
        internal static string CssContentType;
        internal static readonly ILessEngine Instance;

        static LessCompile()
        {
            CssContentType = "text/css";
            Instance = new EngineFactory(new DotlessConfiguration()
            {
                CacheEnabled = false,
                MinifyOutput = true
            }).GetEngine();
        }

        public void Process(BundleContext context, BundleResponse response)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (response == null)
                throw new ArgumentNullException("response");

            if (!context.EnableInstrumentation)
            {

                try
                {
                    StringBuilder bundleContent = new StringBuilder();
                    Uri appRootPath = new Uri(HttpContext.Current.Request.PhysicalApplicationPath);

                    var restoreEnvironmentCurrentDirectory = Environment.CurrentDirectory;

                    foreach (FileInfo file in response.Files)
                    {
                        string fileContent = File.ReadAllText(file.FullName);
                        Uri fileRootPath = new Uri(file.DirectoryName + "/");

                        // Less Compile

                        Environment.CurrentDirectory = file.DirectoryName;
                        fileContent = Instance.TransformToCss(fileContent, file.FullName);

                        // Embed Images
                        fileContent = EmbedCssImages(fileContent, fileRootPath, appRootPath);
                        bundleContent.Append(fileContent);
                    }

                    if (!Environment.CurrentDirectory.Equals(restoreEnvironmentCurrentDirectory))
                        Environment.CurrentDirectory = restoreEnvironmentCurrentDirectory;

                    response.Content = bundleContent.ToString();
                }
                catch (Exception ex)
                {
                    GenerateErrorResponse(response, new string[] { ex.GetType().Name, ex.Message, ex.StackTrace });
                }
            }
            response.ContentType = CssContentType;
        }

        private static string EmbedCssImages(string cssContent, Uri fileRootPath, Uri appRootPath)
        {
            return Regex.Replace(cssContent, "url\\((.*?)\\)", m =>
            {
                var cssFilename = m.Groups[1].Value.Trim(new char[] { '\'', '"' });
                Uri fileUri;
                if (cssFilename.StartsWith("/"))
                    fileUri = new Uri(appRootPath, cssFilename);
                else
                    fileUri = new Uri(fileRootPath, cssFilename);
                if (File.Exists(fileUri.LocalPath))
                {
                    var fileInfo = new FileInfo(fileUri.LocalPath);
                    // Ensure File is < 250kb
                    if (fileInfo.Length < 256000)
                    {
                        string contentType = null;
                        switch (fileInfo.Extension)
                        {
                            case ".png":
                                contentType = "image/png";
                                break;
                            case ".gif":
                                contentType = "image/gif";
                                break;
                            case ".jpg":
                            case ".jpeg":
                                contentType = "image/jpeg";
                                break;
                            default:
                                return m.Value;
                        }
                        StringBuilder sb = new StringBuilder();
                        sb.Append("url(data:");
                        sb.Append(contentType);
                        sb.Append(";base64,");
                        sb.Append(Convert.ToBase64String(File.ReadAllBytes(fileInfo.FullName)));
                        sb.Append(")");
                        return sb.ToString();
                    }
                    else
                    {
                        return string.Format("url(/{0})", appRootPath.MakeRelativeUri(fileUri).ToString());
                    }
                }
                else
                {
                    throw new FileNotFoundException(string.Format("Unable to embed css image, file not found: '{0}' at '{1}'", cssFilename, fileUri.AbsolutePath), cssFilename);
                }
            });
        }

        internal static void GenerateErrorResponse(BundleResponse bundle, ICollection<string> errors)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("/* ");
            builder.Append("Bundle creation failed [LessCompile].").Append("\r\n");
            foreach (string str in errors)
            {
                builder.Append(str).Append("\r\n");
            }
            builder.Append(" */\r\n");
            bundle.Content = builder.ToString();
        }

    }
}
