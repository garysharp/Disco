using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Disco.Data.Repository;
using System.IO.Compression;
using Disco.Models.BI.Interop.Community;
using System.Web;
using Newtonsoft.Json;
using System.Threading;

namespace Disco.Services.Plugins
{
    public static class Plugins
    {
        private static Dictionary<Assembly, PluginManifest> _PluginAssemblyManifests;
        private static Dictionary<string, PluginManifest> _PluginManifests;
        internal static Dictionary<Type, string> FeatureCategoryDisplayNames;

        internal static object _PluginLock = new object();
        public static string PluginPath { get; private set; }

        public static bool PluginsLoaded
        {
            get
            {
                return (_PluginManifests != null);
            }
        }

        internal static void AddPlugin(PluginManifest Manifest)
        {
            lock (_PluginLock)
            {
                if (_PluginManifests.ContainsKey(Manifest.Id))
                    throw new InvalidOperationException(string.Format("The '{0} [{1}]' Plugin is already installed, please uninstall any existing versions before trying again", Manifest.Name, Manifest.Id));

                // Add Plugin Manifest to Environment
                _PluginManifests[Manifest.Id] = Manifest;

                // Reinitialize Plugin Host Environment
                Plugins.ReinitializePluginHostEnvironment();
            }
        }

        public static bool PluginInstalled(string PluginId)
        {
            if (_PluginManifests == null)
                throw new InvalidOperationException("Plugins have not been initialized");

            PluginManifest manifest;
            return _PluginManifests.TryGetValue(PluginId, out manifest);
        }

        public static PluginManifest GetPlugin(string PluginId, Type ContainsCategoryType)
        {
            if (_PluginManifests == null)
                throw new InvalidOperationException("Plugins have not been initialized");

            PluginManifest manifest;
            if (_PluginManifests.TryGetValue(PluginId, out manifest))
            {
                if (ContainsCategoryType == null)
                    return manifest;
                else
                {
                    foreach (var featureManifest in manifest.Features)
                    {
                        if (ContainsCategoryType.IsAssignableFrom(featureManifest.CategoryType))
                            return manifest;
                    }

                    throw new InvalidFeatureCategoryTypeException(ContainsCategoryType, PluginId);
                }
            }
            else
            {
                throw new UnknownPluginException(PluginId);
            }
        }
        public static PluginManifest GetPlugin(string PluginId)
        {
            return GetPlugin(PluginId, null);
        }
        public static PluginManifest GetPlugin(Assembly PluginAssembly)
        {
            if (_PluginAssemblyManifests == null)
                throw new InvalidOperationException("Plugins have not been initialized");

            PluginManifest manifest;
            if (_PluginAssemblyManifests.TryGetValue(PluginAssembly, out manifest))
            {
                return manifest;
            }
            else
            {
                throw new UnknownPluginException(PluginAssembly.FullName);
            }
        }
        public static List<PluginManifest> GetPlugins()
        {
            if (_PluginManifests == null)
                throw new InvalidOperationException("Plugins have not been initialized");

            return _PluginManifests.Values.ToList();
        }

        public static PluginFeatureManifest GetPluginFeature(string PluginFeatureId, Type CategoryType)
        {
            if (_PluginManifests == null)
                throw new InvalidOperationException("Plugins have not been initialized");

            var featureManifest = _PluginManifests.Values.SelectMany(pm => pm.Features).Where(fm => fm.Id == PluginFeatureId).FirstOrDefault();

            if (featureManifest == null)
                throw new UnknownPluginException(PluginFeatureId, "Unknown Feature");

            if (CategoryType == null)
                return featureManifest;
            else
                if (CategoryType.IsAssignableFrom(featureManifest.CategoryType))
                    return featureManifest;
                else
                    throw new InvalidFeatureCategoryTypeException(CategoryType, PluginFeatureId);
        }
        public static PluginFeatureManifest GetPluginFeature(string PluginFeatureId)
        {
            return GetPluginFeature(PluginFeatureId, null);
        }
        public static List<PluginFeatureManifest> GetPluginFeatures(Type FeatureCategoryType)
        {
            if (_PluginManifests == null)
                throw new InvalidOperationException("Plugins have not been initialized");

            return _PluginManifests.Values.SelectMany(pm => pm.Features).Where(fm => fm.CategoryType.IsAssignableFrom(FeatureCategoryType)).OrderBy(fm => fm.PluginManifest.Name).ToList();
        }
        public static List<PluginFeatureManifest> GetPluginFeatures()
        {
            if (_PluginManifests == null)
                throw new InvalidOperationException("Plugins have not been initialized");

            return _PluginManifests.Values.SelectMany(pm => pm.Features).ToList();
        }


        public static string PluginFeatureCategoryDisplayName(Type FeatureCategoryType)
        {
            if (FeatureCategoryType == null)
                throw new ArgumentNullException("FeatureType");

            string displayName;
            if (FeatureCategoryDisplayNames.TryGetValue(FeatureCategoryType, out displayName))
                return displayName;
            else
                throw new InvalidOperationException(string.Format("Unknown Plugin Feature Category Type: [{0}]", FeatureCategoryType.Name));
        }

        public static string CatalogueFile(DiscoDataContext dbContext)
        {
            return Path.Combine(dbContext.DiscoConfiguration.PluginPackagesLocation, "Catalogue.json");
        }
        public static string CompatibilityFile(DiscoDataContext dbContext)
        {
            return Path.Combine(dbContext.DiscoConfiguration.PluginPackagesLocation, "Compatibility.json");
        }

        public static PluginLibraryUpdateResponse LoadCatalogue(DiscoDataContext dbContext)
        {
            var catalogueFile = CatalogueFile(dbContext);

            if (!File.Exists(catalogueFile))
                return null;

            return JsonConvert.DeserializeObject<PluginLibraryUpdateResponse>(File.ReadAllText(catalogueFile));
        }

        public static PluginLibraryCompatibilityResponse LoadCompatibilityData(DiscoDataContext dbContext)
        {
            var pluginAssembly = typeof(Plugins).Assembly;
            Version hostVersion = pluginAssembly.GetName().Version;
            PluginLibraryCompatibilityResponse Data = null;
            var localCompatFile = Path.Combine(Path.GetDirectoryName(pluginAssembly.Location), "ReleasePluginCompatibility.json");
            var serverCompatFile = CompatibilityFile(dbContext);

            if (File.Exists(localCompatFile))
            {
                Data = JsonConvert.DeserializeObject<PluginLibraryCompatibilityResponse>(File.ReadAllText(localCompatFile));
                Data.HostVersion = hostVersion.ToString(4);
            }
            if (File.Exists(serverCompatFile))
            {
                var serverData = JsonConvert.DeserializeObject<PluginLibraryCompatibilityResponse>(File.ReadAllText(serverCompatFile));
                if (Version.Parse(serverData.HostVersion) == hostVersion)
                {
                    if (Data == null)
                    {
                        // No Local Compatibility File
                        Data = serverData;
                    }
                    else
                    {
                        // Join Compatibility Files
                        var localItems = Data.Plugins;
                        var localItemVersions = localItems.ToDictionary(i => i, i => Version.Parse(i.Version));
                        var joinedItems = localItems.ToList();
                        Data.ResponseTimestamp = serverData.ResponseTimestamp;
                        foreach (var serverItem in serverData.Plugins)
                        {
                            var serverItemVersion = Version.Parse(serverItem.Version);
                            var localItem = localItems.FirstOrDefault(i => i.Id.Equals(serverItem.Id, StringComparison.InvariantCultureIgnoreCase) && serverItemVersion == localItemVersions[i]);
                            if (localItem != null)
                                joinedItems.Remove(localItem);

                            joinedItems.Add(serverItem);
                        }
                        Data.Plugins = joinedItems;
                    }
                }
            }
            if (Data == null)
            {
                Data = new PluginLibraryCompatibilityResponse()
                {
                    HostVersion = hostVersion.ToString(4),
                    Plugins = new List<PluginLibraryCompatibilityItem>(),
                    ResponseTimestamp = new DateTime(2011, 7, 1)
                };
            }

            return Data;
        }

        public static void InitalizePlugins(DiscoDataContext dbContext)
        {
            if (_PluginManifests == null)
            {
                lock (_PluginLock)
                {
                    if (_PluginManifests == null)
                    {
                        Version hostVersion = typeof(Plugins).Assembly.GetName().Version;
                        var compatibilityData = new Lazy<PluginLibraryCompatibilityResponse>(() => LoadCompatibilityData(dbContext));
                        Dictionary<string, PluginManifest> loadedPlugins = new Dictionary<string, PluginManifest>();

                        PluginPath = dbContext.DiscoConfiguration.PluginsLocation;

                        AppDomain appDomain = AppDomain.CurrentDomain;

                        // Subscribe to Assembly Resolving
                        appDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                        DirectoryInfo pluginDirectoryRoot = new DirectoryInfo(PluginPath);
                        if (pluginDirectoryRoot.Exists)
                        {
                            foreach (DirectoryInfo pluginDirectory in pluginDirectoryRoot.EnumerateDirectories())
                            {
                                string pluginManifestFilename = Path.Combine(pluginDirectory.FullName, "manifest.json");
                                if (File.Exists(pluginManifestFilename))
                                {
                                    PluginManifest pluginManifest = null;
                                    try
                                    {
                                        pluginManifest = PluginManifest.FromPluginManifestFile(pluginManifestFilename);

                                        if (pluginManifest != null)
                                        {
                                            if (loadedPlugins.ContainsKey(pluginManifest.Id))
                                                throw new InvalidOperationException(string.Format("The plugin [{0}] is already initialized", pluginManifest.Id));

                                            // Check for Update
                                            string updatePackagePath = Path.Combine(pluginDirectoryRoot.FullName, string.Format("{0}.discoPlugin", pluginManifest.Id));
                                            if (File.Exists(updatePackagePath))
                                            {
                                                // Update Plugin
                                                pluginManifest = UpdatePlugin(dbContext, pluginManifest, updatePackagePath, compatibilityData.Value);
                                            }

                                            if (pluginManifest != null)
                                            {
                                                // Check Version Compatibility
                                                var pluginCompatibility = compatibilityData.Value.Plugins.FirstOrDefault(i => i.Id.Equals(pluginManifest.Id, StringComparison.InvariantCultureIgnoreCase) && pluginManifest.Version == Version.Parse(i.Version));
                                                if (pluginCompatibility != null && !pluginCompatibility.Compatible)
                                                    throw new InvalidOperationException(string.Format("The plugin [{0} v{1}] is not compatible: {2}", pluginManifest.Id, pluginManifest.VersionFormatted, pluginCompatibility.Reason));

                                                if (pluginManifest.HostVersionMin != null && pluginManifest.HostVersionMin > hostVersion)
                                                    throw new InvalidOperationException(string.Format("The plugin [{0} v{1}] does not support this version of Disco (Requires v{2} or greater)", pluginManifest.Id, pluginManifest.VersionFormatted, pluginManifest.HostVersionMin.ToString()));
                                                if (pluginManifest.HostVersionMax != null && pluginManifest.HostVersionMax < hostVersion)
                                                    throw new InvalidOperationException(string.Format("The plugin [{0} v{1}] does not support this version of Disco (Support expired as of v{2})", pluginManifest.Id, pluginManifest.VersionFormatted, pluginManifest.HostVersionMax.ToString()));

                                                pluginManifest.InitializePlugin(dbContext);
                                                loadedPlugins[pluginManifest.Id] = pluginManifest;
                                            }
                                        }
                                    }
                                    catch (Exception ex) { PluginsLog.LogInitializeException(pluginManifestFilename, ex); }
                                }
                                else
                                {
                                    string pluginManifestUninstallFilename = Path.Combine(pluginDirectory.FullName, "manifest.uninstall.json");
                                    if (File.Exists(pluginManifestUninstallFilename))
                                    {
                                        PluginManifest uninstallManifest = PluginManifest.FromPluginManifestFile(pluginManifestUninstallFilename);

                                        // Remove All Files
                                        DateTime removeRetryTime = DateTime.Now.AddSeconds(60);
                                        while (true)
                                        {
                                            UnauthorizedAccessException lastAccessException;
                                            try
                                            {
                                                pluginDirectory.Delete(true);
                                                break;
                                            }
                                            catch (UnauthorizedAccessException ex) { lastAccessException = ex; }
                                            if (removeRetryTime < DateTime.Now)
                                                throw lastAccessException;
                                            System.Threading.Thread.Sleep(2000);
                                        }

                                        // Check for Data Removal
                                        bool DataUninstalled = false;
                                        string pluginStorageLocation = Path.Combine(dbContext.DiscoConfiguration.PluginStorageLocation, uninstallManifest.Id);

                                        string pluginManifestUninstallDataFilename = Path.Combine(pluginStorageLocation, "manifest.uninstall.json");
                                        if (File.Exists(pluginManifestUninstallDataFilename))
                                        {
                                            DataUninstalled = true;
                                            Directory.Delete(pluginStorageLocation, true);
                                        }

                                        PluginsLog.LogUninstalled(uninstallManifest, DataUninstalled);
                                    }
                                }
                            }
                        }

                        _PluginManifests = loadedPlugins;

                        ReinitializePluginHostEnvironment();
                    }
                }
            }
        }

        private static void ReinitializePluginHostEnvironment()
        {
            FeatureCategoryDisplayNames = InitializeFeatureCategoryDetails(_PluginManifests.Values);
            _PluginAssemblyManifests = _PluginManifests.Values.ToDictionary(p => p.PluginAssembly, p => p);
        }

        public static PluginManifest UpdatePlugin(DiscoDataContext dbContext, PluginManifest ExistingManifest, String UpdatePluginPackageFilePath, PluginLibraryCompatibilityResponse CompatibilityData = null)
        {
            PluginManifest updatedManifest;

            using (var packageStream = File.OpenRead(UpdatePluginPackageFilePath))
            {
                updatedManifest = UpdatePlugin(dbContext, ExistingManifest, packageStream, CompatibilityData);
            }

            // Remove Update after processing
            File.Delete(UpdatePluginPackageFilePath);

            return updatedManifest;
        }

        public static PluginManifest UpdatePlugin(DiscoDataContext dbContext, PluginManifest ExistingManifest, Stream UpdatePluginPackage, PluginLibraryCompatibilityResponse CompatibilityData = null)
        {
            using (MemoryStream packageStream = new MemoryStream())
            {
                if (UpdatePluginPackage.Position != 0)
                    UpdatePluginPackage.Position = 0;

                UpdatePluginPackage.CopyTo(packageStream);
                packageStream.Position = 0;

                using (ZipArchive packageArchive = new ZipArchive(packageStream, ZipArchiveMode.Read, false))
                {

                    ZipArchiveEntry packageManifestEntry = packageArchive.GetEntry("manifest.json");
                    if (packageManifestEntry == null)
                        throw new InvalidDataException("The plugin package does not contain the 'manifest.json' entry");

                    PluginManifest packageManifest;

                    using (Stream packageManifestStream = packageManifestEntry.Open())
                    {
                        packageManifest = PluginManifest.FromPluginManifestFile(packageManifestStream);
                    }

                    if (ExistingManifest.Version == packageManifest.Version)
                    {
                        // Skip Update if already installed
                        PluginsLog.LogInitializeWarning(string.Format("This plugin [{0}] version [{1}] is already installed, skipping Update", ExistingManifest.Id, ExistingManifest.Version));
                        return ExistingManifest;
                    }
                    if (ExistingManifest.Version > packageManifest.Version)
                    {
                        throw new InvalidDataException("A newer version of this plugin is already installed");
                    }

                    // Check Compatibility
                    if (CompatibilityData == null)
                        CompatibilityData = LoadCompatibilityData(dbContext);
                    var pluginCompatibility = CompatibilityData.Plugins.FirstOrDefault(i => i.Id.Equals(packageManifest.Id, StringComparison.InvariantCultureIgnoreCase) && packageManifest.Version == Version.Parse(i.Version));
                    if (pluginCompatibility != null && !pluginCompatibility.Compatible)
                        throw new InvalidOperationException(string.Format("The plugin [{0} v{1}] is not compatible: {2}", packageManifest.Id, packageManifest.VersionFormatted, pluginCompatibility.Reason));

                    string packagePath = Path.Combine(dbContext.DiscoConfiguration.PluginsLocation, packageManifest.Id);

                    // Force Delete of Existing Folder
                    if (Directory.Exists(packagePath))
                        Directory.Delete(packagePath, true);

                    Directory.CreateDirectory(packagePath);

                    // Extract Package Contents
                    foreach (var packageEntry in packageArchive.Entries)
                    {
                        // Determine Extraction Path
                        var packageEntryTarget = Path.Combine(packagePath, packageEntry.FullName);

                        // Create Sub Directories
                        Directory.CreateDirectory(Path.GetDirectoryName(packageEntryTarget));

                        using (var packageEntryStream = packageEntry.Open())
                        {
                            using (var packageTargetStream = File.Open(packageEntryTarget, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                packageEntryStream.CopyTo(packageTargetStream);
                            }
                        }
                    }

                    // Reload Manifest
                    packageManifest = PluginManifest.FromPluginManifestFile(Path.Combine(packagePath, "manifest.json"));

                    // Trigger AfterPluginUpdate
                    packageManifest.AfterPluginUpdate(dbContext, ExistingManifest);

                    PluginsLog.LogAfterUpdate(ExistingManifest, packageManifest);

                    // Return Updated Manifest
                    return packageManifest;
                }
            }
        }

        private static Dictionary<Type, string> InitializeFeatureCategoryDetails(IEnumerable<PluginManifest> pluginManifests)
        {
            Dictionary<Type, string> categoryDisplayNames = new Dictionary<Type, string>();

            // Always add 'Other'
            var otherFeatureType = typeof(Features.Other.OtherFeature);
            categoryDisplayNames.Add(otherFeatureType, ((PluginFeatureCategoryAttribute)otherFeatureType.GetCustomAttributes(typeof(PluginFeatureCategoryAttribute), false).FirstOrDefault()).DisplayName);

            foreach (var pluginManifest in pluginManifests)
            {
                foreach (var featureManifest in pluginManifest.Features)
                {
                    if (!categoryDisplayNames.ContainsKey(featureManifest.CategoryType))
                    {
                        string displayName = null;

                        var displayAttributes = featureManifest.CategoryType.GetCustomAttributes(typeof(PluginFeatureCategoryAttribute), true);
                        if (displayAttributes != null && displayAttributes.Length > 0)
                            displayName = ((PluginFeatureCategoryAttribute)(displayAttributes[0])).DisplayName;

                        if (string.IsNullOrWhiteSpace(displayName))
                            displayName = featureManifest.CategoryType.Name;

                        categoryDisplayNames[featureManifest.CategoryType] = displayName;
                    }
                }
            }
            return categoryDisplayNames;
        }

        #region Restart App
        private static object _restartTimerLock = new object();
        private static Timer _restartTimer;
        internal static void RestartApp(int DelayMilliseconds)
        {
            lock (_restartTimerLock)
            {
                if (_restartTimer != null)
                {
                    _restartTimer.Dispose();
                }

                _restartTimer = new Timer((state) =>
                {
                    HttpRuntime.UnloadAppDomain();
                    //AppDomain.Unload(AppDomain.CurrentDomain);
                }, null, DelayMilliseconds, Timeout.Infinite);
            }
        }
        #endregion

        #region Plugin Referenced Assemblies Resolving

        public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.RequestingAssembly.Location.StartsWith(PluginPath, StringComparison.InvariantCultureIgnoreCase) && _PluginManifests != null)
            {
                // Try best guess first
                PluginManifest requestingPlugin = _PluginManifests.Values.Where(p => p.Type.Assembly == args.RequestingAssembly).FirstOrDefault();
                if (requestingPlugin != null)
                {
                    Assembly loadedAssembly = CurrentDomain_AssemblyResolve_ByPlugin(requestingPlugin, args);
                    if (loadedAssembly != null)
                        return loadedAssembly;
                }

                // Try all Plugin References
                foreach (var pluginDef in _PluginManifests.Values)
                {
                    Assembly loadedAssembly = CurrentDomain_AssemblyResolve_ByPlugin(pluginDef, args);
                    if (loadedAssembly != null)
                        return loadedAssembly;
                }
            }
            return null;
        }
        private static Assembly CurrentDomain_AssemblyResolve_ByPlugin(PluginManifest pluginManifest, ResolveEventArgs args)
        {
            if (pluginManifest.AssemblyReferences != null)
            {
                string assemblyPath;
                if (pluginManifest.AssemblyReferences.TryGetValue(args.Name, out assemblyPath))
                {
                    var resolvedAssemblyPath = Path.Combine(pluginManifest.PluginLocation, assemblyPath);

                    try
                    {
                        Assembly loadedAssembly = Assembly.LoadFile(resolvedAssemblyPath);

                        PluginsLog.LogPluginReferenceAssemblyLoaded(args.Name, resolvedAssemblyPath, args.RequestingAssembly.FullName);

                        return loadedAssembly;
                    }
                    catch (Exception ex)
                    {
                        PluginsLog.LogPluginException(string.Format("Resolving Plugin Reference Assembly: '{0}' [{1}]; Requested by: '{2}' [{3}]; Disco.Plugins.DiscoPlugins.CurrentDomain_AssemblyResolve()", args.Name, resolvedAssemblyPath, args.RequestingAssembly.FullName, args.RequestingAssembly.Location), ex);
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
