﻿using Disco.Data.Repository;
using Disco.Services.Authorization;
using Disco.Services.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Disco.Services.Plugins
{
    public class PluginManifest
    {
        [JsonProperty]
        public string Id { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string Author { get; set; }
        [JsonProperty]
        public string Url { get; set; }
        [JsonProperty]
        public Version Version { get; set; }
        [JsonProperty]
        public Version HostVersionMin { get; set; }
        [JsonProperty]
        public Version HostVersionMax { get; set; }
        [JsonProperty]
        internal string AssemblyPath { get; set; }
        [JsonProperty]
        private string TypeName { get; set; }
        [JsonProperty]
        private string ConfigurationHandlerTypeName { get; set; }
        [JsonProperty]
        private string WebHandlerTypeName { get; set; }

        [JsonProperty]
        internal Dictionary<string, string> AssemblyReferences { get; set; }

        [JsonProperty]
        public List<PluginFeatureManifest> Features { get; private set; }

        [JsonIgnore]
        internal Assembly PluginAssembly { get; private set; }
        [JsonIgnore]
        internal Type Type { get; private set; }
        [JsonIgnore]
        private Type ConfigurationHandlerType { get; set; }
        [JsonIgnore]
        private Type WebHandlerType { get; set; }
        [JsonIgnore]
        private DiscoAuthorizeBaseAttribute[] WebHandlerAuthorizers { get; set; }

        [JsonIgnore]
        public string PluginLocation { get; private set; }
        [JsonIgnore]
        public string StorageLocation { get; private set; }

        [JsonIgnore]
        public string VersionFormatted
        {
            get
            {
                var v = Version;
                return $"{v.Major}.{v.Minor}.{v.Build:0000}.{v.Revision:0000}";
            }
        }

        [JsonIgnore]
        private bool environmentInitalized { get; set; }

        private static Dictionary<string, Tuple<string, DateTime>> WebResourceHashes = new Dictionary<string, Tuple<string, DateTime>>();

        public List<PluginFeatureManifest> GetFeatures(Type FeatureCategoryType)
        {
            return Features.Where(fm => fm.CategoryType.IsAssignableFrom(FeatureCategoryType)).ToList();
        }
        public PluginFeatureManifest GetFeature(string PluginFeatureId)
        {
            return Features.Where(fm => fm.Id == PluginFeatureId).FirstOrDefault();
        }

        public Plugin CreateInstance()
        {
            var i = (Plugin)Activator.CreateInstance(Type);
            i.Manifest = this;
            return i;
        }

        /// <summary>
        /// Deserializes a Json Manifest
        /// </summary>
        /// <param name="FilePath">Path to the Json Manifest file</param>
        /// <returns></returns>
        public static PluginManifest FromPluginManifestFile(string FilePath)
        {
            using (Stream manifestStream = File.OpenRead(FilePath))
            {
                PluginManifest manifest = FromPluginManifestFile(manifestStream, Path.GetDirectoryName(FilePath));
                return manifest;
            }
        }

        /// <summary>
        /// Deserializes a Json Manifest
        /// </summary>
        /// <param name="FileStream">Stream containing the encoded Json Manifest File</param>
        /// <param name="PluginLocation">PluginLocation to be set in the manifest</param>
        /// <returns></returns>
        public static PluginManifest FromPluginManifestFile(Stream FileStream, string PluginLocation = null)
        {
            string manifestString;
            using (StreamReader manifestStreamReader = new StreamReader(FileStream))
            {
                manifestString = manifestStreamReader.ReadToEnd();
            }

            var manifest = JsonConvert.DeserializeObject<PluginManifest>(manifestString, new VersionConverter());

            manifest.PluginLocation = PluginLocation;

            return manifest;
        }

        private static Lazy<IReadOnlyCollection<string>> pluginExcludedAssemblies = new Lazy<IReadOnlyCollection<string>>(() =>
        {
            return new List<string>()
            {
                "C5",
                "ClosedXML",
                "Common.Logging",
                "Disco.BI",
                "Disco.Data",
                "Disco.Models",
                "Disco.Services",
                "Disco.Web",
                "Disco.Web.Extensions",
                "DocumentFormat.OpenXml",
                "EntityFramework",
                "itextsharp",
                "LumenWorks.Framework.IO",
                "MarkdownSharp",
                "Microsoft.AspNet.SignalR.Core",
                "Microsoft.AspNet.SignalR.SystemWeb",
                "Microsoft.AspNetCore.Connections.Abstractions",
                "Microsoft.AspNetCore.Http.Connections.Client",
                "Microsoft.AspNetCore.Http.Connections.Common",
                "Microsoft.AspNetCore.SignalR.Client.Core",
                "Microsoft.AspNetCore.SignalR.Client",
                "Microsoft.AspNetCore.SignalR.Common",
                "Microsoft.AspNetCore.SignalR.Protocols.Json",
                "Microsoft.Bcl.AsyncInterfaces",
                "Microsoft.Bcl.TimeProvider",
                "Microsoft.Extensions.DependencyInjection.Abstractions",
                "Microsoft.Extensions.DependencyInjection",
                "Microsoft.Extensions.Features",
                "Microsoft.Extensions.Logging.Abstractions",
                "Microsoft.Extensions.Logging",
                "Microsoft.Extensions.Options",
                "Microsoft.Extensions.Primitives",
                "Microsoft.Owin",
                "Microsoft.Owin.Host.SystemWeb",
                "Microsoft.Owin.Security",
                "Microsoft.Web.Infrastructure",
                "Newtonsoft.Json.Bson",
                "Newtonsoft.Json",
                "Owin",
                "PdfiumViewer",
                "PdfSharp",
                "PListNet",
                "Quartz",
                "RazorGenerator.Mvc",
                "Renci.SshNet",
                "Spring.Core",
                "System.Buffers",
                "System.Data.SqlServerCe",
                "System.Data.SqlServerCe.Entity",
                "System.Diagnostics.DiagnosticSource",
                "System.IO.Pipelines",
                "System.Memory",
                "System.Net.Http",
                "System.Net.Http.Extensions",
                "System.Net.Http.Formatting",
                "System.Net.Http.Primitives",
                "System.Net.ServerSentEvents",
                "System.Numerics.Vectors",
                "System.Reactive.Core",
                "System.Reactive.Interfaces",
                "System.Reactive.Linq",
                "System.Reactive.PlatformServices",
                "System.Runtime.CompilerServices.Unsafe",
                "System.Runtime.InteropServices.RuntimeInformation",
                "System.Text.Encodings.Web",
                "System.Text.Json",
                "System.Threading.Channels",
                "System.Threading.Tasks.Extensions",
                "System.ValueTuple",
                "System.Web.Helpers",
                "System.Web.Http",
                "System.Web.Http.WebHost",
                "System.Web.Mvc",
                "System.Web.Razor",
                "System.Web.WebPages.Deployment",
                "System.Web.WebPages",
                "System.Web.WebPages.Razor",
                "T4MVCExtensions",
                "WebActivatorEx",
                "ZXingNet",
            };
        });
        public static IReadOnlyCollection<string> PluginExcludedAssemblies
        {
            get
            {
                return pluginExcludedAssemblies.Value;
            }
        }

        /// <summary>
        /// Uses reflection to build a Plugin Manifest
        /// </summary>
        /// <param name="pluginAssembly">Assembly containing a plugin</param>
        /// <returns>A plugin manifest for the first encountered plugin within the assembly</returns>
        public static PluginManifest FromPluginAssembly(Assembly assembly)
        {
            // Determine Plugin Properties
            var pluginType = (from type in assembly.GetTypes()
                              where typeof(Plugin).IsAssignableFrom(type) && !type.IsAbstract
                              select type).FirstOrDefault();

            if (pluginType == null)
                throw new ArgumentException("No Plugin was found in this Assembly", "pluginAssembly");

            var assemblyName = assembly.GetName();

            var pluginAttributes = pluginType.GetCustomAttribute<PluginAttribute>(false);

            if (pluginAttributes == null)
                throw new ArgumentException($"Plugin found [{pluginType.Name}], but no PluginAttribute found", "pluginAssembly");

            var pluginId = pluginAttributes.Id;
            var pluginName = pluginAttributes.Name;
            var pluginAuthor = pluginAttributes.Author;
            var pluginUrl = pluginAttributes.Url;

            var pluginHostVersionMin = pluginAttributes.HostVersionMin == null ? null : Version.Parse(pluginAttributes.HostVersionMin);
            var pluginHostVersionMax = pluginAttributes.HostVersionMax == null ? null : Version.Parse(pluginAttributes.HostVersionMax);

            var pluginVersion = assemblyName.Version;
            var pluginAssemblyPath = Path.GetFileName(assembly.Location);
            var pluginTypeName = pluginType.FullName;
            var pluginLocation = Path.GetDirectoryName(assembly.Location);

            // Find Configuration Handler
            var pluginConfigurationHandlerType = (from type in assembly.GetTypes()
                                                  where typeof(PluginConfigurationHandler).IsAssignableFrom(type) && !type.IsAbstract
                                                  select type).FirstOrDefault();
            if (pluginConfigurationHandlerType == null)
                throw new ArgumentException("A Plugin was found, but no Configuration Handler was found in this Assembly - this is required", "pluginAssembly");

            // Find Web Handler
            var pluginWebHandlerType = (from type in assembly.GetTypes()
                                        where typeof(PluginWebHandler).IsAssignableFrom(type) && !type.IsAbstract
                                        select type).FirstOrDefault();

            Dictionary<string, string> pluginAssemblyReferences = new Dictionary<string, string>();

            foreach (string referenceFilename in Directory.EnumerateFiles(pluginLocation, "*.dll", SearchOption.TopDirectoryOnly))
            {
                if (!referenceFilename.Equals(assembly.Location, StringComparison.OrdinalIgnoreCase))
                {
                    // Ignore Excluded Assemblies
                    if (!PluginExcludedAssemblies.Contains(Path.GetFileNameWithoutExtension(referenceFilename)))
                    {
                        try
                        {
                            Assembly pluginRefAssembly = Assembly.ReflectionOnlyLoadFrom(referenceFilename);
                            pluginAssemblyReferences[pluginRefAssembly.FullName] = referenceFilename.Substring(pluginLocation.Length + 1);
                        }
                        catch (Exception) { } // Ignore Load Exceptions
                    }
                }
            }

            PluginManifest pluginManifest = new PluginManifest()
            {
                Id = pluginId,
                Name = pluginName,
                Author = pluginAuthor,
                Version = pluginVersion,
                Url = pluginUrl,
                HostVersionMin = pluginHostVersionMin,
                HostVersionMax = pluginHostVersionMax,
                AssemblyPath = pluginAssemblyPath,
                TypeName = pluginTypeName,
                AssemblyReferences = pluginAssemblyReferences,
                PluginAssembly = assembly,
                Type = pluginType,
                PluginLocation = pluginLocation,
                ConfigurationHandlerType = pluginConfigurationHandlerType,
                ConfigurationHandlerTypeName = pluginConfigurationHandlerType.FullName,
                WebHandlerType = pluginWebHandlerType,
                WebHandlerTypeName = (pluginWebHandlerType == null ? null : pluginWebHandlerType.FullName)
            };

            pluginManifest.Features = (from type in assembly.GetTypes()
                                       where typeof(PluginFeature).IsAssignableFrom(type) && !type.IsAbstract
                                       select PluginFeatureManifest.FromPluginFeatureType(type, pluginManifest)).ToList();

            return pluginManifest;
        }

        public string ToManifestFile()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented, new VersionConverter());
        }
        private bool InitializePluginEnvironment(DiscoDataContext Database)
        {
            if (!environmentInitalized)
            {

                var assemblyFullPath = Path.Combine(PluginLocation, AssemblyPath);

                if (!File.Exists(assemblyFullPath))
                    throw new FileNotFoundException($"Plugin Assembly [{Id}] not found at: {assemblyFullPath}", assemblyFullPath);

                if (PluginAssembly == null)
                    PluginAssembly = Assembly.LoadFile(assemblyFullPath);

                if (PluginAssembly == null)
                    throw new InvalidOperationException($"Unable to load Plugin Assembly [{Id}] at: {assemblyFullPath}");

                PluginsLog.LogInitializingPluginAssembly(PluginAssembly);

                // Check Manifest/Assembly Versions Match
                if (Version != PluginAssembly.GetName().Version)
                    throw new InvalidOperationException($"The plugin [{Id}] manifest version [{Version}] doesn't match the plugin assembly [{assemblyFullPath} : {PluginAssembly.GetName().Version}]");

                if (Type == null)
                    Type = PluginAssembly.GetType(TypeName, true, true);

                if (ConfigurationHandlerType == null)
                    ConfigurationHandlerType = PluginAssembly.GetType(ConfigurationHandlerTypeName, true, true);

                if (!string.IsNullOrEmpty(WebHandlerTypeName) && WebHandlerType == null)
                    WebHandlerType = PluginAssembly.GetType(WebHandlerTypeName, true, true);

                // Update non-static values
                StorageLocation = Path.Combine(Database.DiscoConfiguration.PluginStorageLocation, Id);

                environmentInitalized = true;
            }

            return true;
        }
        internal bool AfterPluginUpdate(DiscoDataContext Database, PluginManifest PreviousManifest)
        {
            // Initialize Plugin
            InitializePluginEnvironment(Database);

            using (var pluginInstance = CreateInstance())
            {
                pluginInstance.AfterUpdate(Database, PreviousManifest);
            }

            return true;
        }
        internal bool UninstallPlugin(DiscoDataContext Database, bool UninstallData, ScheduledTaskStatus Status)
        {
            // Initialize Plugin
            InitializePluginEnvironment(Database);

            using (var pluginInstance = CreateInstance())
            {
                pluginInstance.Uninstall(Database, UninstallData, Status);
            }

            return true;
        }
        internal bool InstallPlugin(DiscoDataContext Database, ScheduledTaskStatus Status)
        {
            // Initialize Plugin
            InitializePluginEnvironment(Database);

            using (var pluginInstance = CreateInstance())
            {
                pluginInstance.Install(Database, Status);
            }

            return true;
        }
        internal bool InitializePlugin(DiscoDataContext Database)
        {
            // Initialize Plugin
            InitializePluginEnvironment(Database);

            // Initialize Plugin
            using (var pluginInstance = CreateInstance())
            {
                pluginInstance.Initialize(Database);
            }
            PluginsLog.LogInitializedPlugin(this);

            // Initialize Plugin Features
            if (Features != null)
            {
                foreach (var feature in Features)
                {
                    feature.Initialize(Database, this);
                }
            }
            else
            {
                Features = new List<PluginFeatureManifest>();
            }

            return true;
        }

        public PluginConfigurationHandler CreateConfigurationHandler()
        {
            // Configuration Handler is Required
            if (ConfigurationHandlerType == null)
                throw new ArgumentNullException("ConfigurationType");
            if (!typeof(PluginConfigurationHandler).IsAssignableFrom(ConfigurationHandlerType))
                throw new ArgumentException("The Plugin ConfigurationHandlerType must inherit Disco.Services.Plugins.PluginConfigurationHandler", "ConfigurationHandlerType");

            var handler = (PluginConfigurationHandler)Activator.CreateInstance(ConfigurationHandlerType);

            handler.Manifest = this;

            return handler;
        }
        [JsonIgnore]
        public string ConfigurationUrl
        {
            get
            {
                return $"/Config/Plugins/{HttpUtility.UrlEncode(Id)}";
            }
        }
        [JsonIgnore]
        public bool HasWebHandler
        {
            get
            {
                return WebHandlerType != null;
            }
        }
        public PluginWebHandler CreateWebHandler(Controller HostController)
        {
            // Web Handler is Not Required
            if (WebHandlerType == null)
                return null;

            if (!typeof(PluginWebHandler).IsAssignableFrom(WebHandlerType))
                throw new ArgumentException("The Plugin WebHandlerType must inherit Disco.Services.Plugins.PluginWebHandler", "WebHandlerType");

            // Determine WebHandler Authorize Attributes
            if (WebHandlerAuthorizers == null)
            {
                WebHandlerAuthorizers = WebHandlerType.GetCustomAttributes<DiscoAuthorizeBaseAttribute>(true).ToArray();
            }
            if (WebHandlerAuthorizers.Length > 0)
            {
                var attributeDenied = WebHandlerAuthorizers.FirstOrDefault(a => !a.IsAuthorized(HostController.HttpContext));
                if (attributeDenied != null)
                    throw new AccessDeniedException(attributeDenied.HandleUnauthorizedMessage(), $"[Plugin]::{Id}::[Handler]");
            }

            var handler = (PluginWebHandler)Activator.CreateInstance(WebHandlerType);

            handler.Manifest = this;
            handler.HostController = HostController;
            handler.Url = HostController.Url;

            return handler;
        }
        [JsonIgnore]
        public string WebHandlerUrl
        {
            get
            {
                return $"/Plugin/{HttpUtility.UrlEncode(Id)}";
            }
        }
        public string WebActionUrl(string Action)
        {
            if (!HasWebHandler)
                throw new NotSupportedException("This plugin doesn't have a web handler");

            var url = UrlHelper.GenerateUrl("Plugin", null, null,
                new RouteValueDictionary(new Dictionary<string, object>() { { "PluginId", Id }, { "PluginAction", Action } }),
                RouteTable.Routes, HttpContext.Current.Request.RequestContext, false);

            return url;
        }

        public Tuple<string, string> WebResourcePath(string Resource)
        {
            if (string.IsNullOrWhiteSpace(Resource))
                throw new ArgumentNullException("Resource");

            if (Resource.Contains(".."))
                throw new ArgumentException("Resource Paths cannot navigate to the parent", "Resource");

            var resourcePath = Path.Combine(PluginLocation, "WebResources", Resource.Replace(@"/", @"\"));

            Tuple<string, DateTime> resourceHash;
            string resourceKey = $"{Name}://{Resource}";
            if (WebResourceHashes.TryGetValue(resourceKey, out resourceHash))
            {
#if DEBUG
                var fileDateCheck = System.IO.File.GetLastWriteTime(resourcePath);
                if (fileDateCheck == resourceHash.Item2)
#endif
                    return new Tuple<string, string>(resourcePath, resourceHash.Item1);
            }

            if (!File.Exists(resourcePath))
                throw new FileNotFoundException($"Resource [{Resource}] not found", resourcePath);

            var fileDate = System.IO.File.GetLastWriteTime(resourcePath);
            var fileBytes = System.IO.File.ReadAllBytes(resourcePath);
            if (fileBytes.Length > 0)
            {
                using (SHA256 sha = SHA256.Create())
                {
                    byte[] hash = sha.ComputeHash(fileBytes);
                    resourceHash = new Tuple<string, DateTime>(HttpServerUtility.UrlTokenEncode(hash), fileDate);
                }
            }
            WebResourceHashes[resourceKey] = resourceHash;

            return new Tuple<string, string>(resourcePath, resourceHash.Item1);
        }
        public string WebResourceUrl(string Resource)
        {
            var resourcePath = WebResourcePath(Resource);

            var url = UrlHelper.GenerateUrl("Plugin_Resources", null, null,
                new RouteValueDictionary(new Dictionary<string, object>() { { "PluginId", Id }, { "res", Resource } }),
                RouteTable.Routes, HttpContext.Current.Request.RequestContext, false);

            url += $"?v={resourcePath.Item2}";

            return url;
        }

        public void LogException(Exception PluginException)
        {
            PluginsLog.LogPluginException(ToString(), PluginException);
        }
        public void LogWarning(string Message)
        {
            LogWarning(Message, null);
            PluginsLog.LogPluginWarning(this, Message);
        }
        public void LogWarning(string MessageFormat, params object[] Args)
        {
            PluginsLog.LogPluginWarning(this, string.Format(MessageFormat, Args), Args);
        }
        public void LogMessage(string Message)
        {
            PluginsLog.LogPluginMessage(this, Message);
        }
        public void LogMessage(string MessageFormat, params object[] Args)
        {
            PluginsLog.LogPluginMessage(this, string.Format(MessageFormat, Args), Args);
        }

        public override string ToString()
        {
            return $"{Name} [{Id} v{VersionFormatted}]";
        }
    }
}
