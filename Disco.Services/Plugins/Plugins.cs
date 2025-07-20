using Disco.Data.Repository;
using Disco.Models.Services.Interop.DiscoServices;
using Disco.Services.Interop.DiscoServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;

namespace Disco.Services.Plugins
{
    public static class Plugins
    {
        private static Dictionary<Assembly, PluginManifest> _PluginAssemblyManifests;
        private static Dictionary<string, PluginManifest> _PluginManifests;
        private static Dictionary<string, string> _PluginAssemblyReferences;
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
                    throw new InvalidOperationException($"The '{Manifest.Name} [{Manifest.Id}]' Plugin is already installed, please uninstall any existing versions before trying again");

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
        public static bool TryGetPlugin(string PluginId, Type ContainsCategoryType, out PluginManifest PluginManifest)
        {
            PluginManifest = null;

            if (_PluginManifests == null)
                return false;

            PluginManifest manifest;
            if (_PluginManifests.TryGetValue(PluginId, out manifest))
            {
                if (ContainsCategoryType == null)
                {
                    PluginManifest = manifest;
                    return true;
                }
                else
                {
                    foreach (var featureManifest in manifest.Features)
                    {
                        if (ContainsCategoryType.IsAssignableFrom(featureManifest.CategoryType))
                        {
                            PluginManifest = manifest;
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        public static PluginManifest GetPlugin(string PluginId)
        {
            return GetPlugin(PluginId, null);
        }
        public static bool TryGetPlugin(string PluginId, out PluginManifest PluginManifest)
        {
            return TryGetPlugin(PluginId, null, out PluginManifest);
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
        public static bool TryGetPlugin(Assembly PluginAssembly, out PluginManifest PluginManifest)
        {
            PluginManifest = null;

            if (_PluginAssemblyManifests == null)
                return false;

            PluginManifest manifest;
            if (_PluginAssemblyManifests.TryGetValue(PluginAssembly, out manifest))
            {
                PluginManifest = manifest;
                return true;
            }
            else
            {
                return false;
            }
        }
        public static List<PluginManifest> GetPlugins()
        {
            if (_PluginManifests == null)
                throw new InvalidOperationException("Plugins have not been initialized");

            return _PluginManifests.Values.ToList();
        }

        public static bool PluginFeatureInstalled(string PluginFeatureId)
        {
            if (_PluginManifests == null)
                throw new InvalidOperationException("Plugins have not been initialized");

            return _PluginManifests.Values.SelectMany(pm => pm.Features).Where(fm => fm.Id == PluginFeatureId).Count() > 0;
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
        public static bool TryGetPluginFeature(string PluginFeatureId, Type CategoryType, out PluginFeatureManifest PluginFeatureManifest)
        {
            if (_PluginManifests == null)
            {
                PluginFeatureManifest = null;
                return false;
            }

            var featureManifest = _PluginManifests.Values
                .SelectMany(pm => pm.Features)
                .Where(fm => fm.Id == PluginFeatureId)
                .FirstOrDefault();

            if (featureManifest == null)
            {
                PluginFeatureManifest = null;
                return false;
            }

            if (CategoryType == null)
            {
                PluginFeatureManifest = featureManifest;
                return true;
            }
            else
            {
                if (CategoryType.IsAssignableFrom(featureManifest.CategoryType))
                {
                    PluginFeatureManifest = featureManifest;
                    return true;
                }
                else
                {
                    PluginFeatureManifest = null;
                    return false;
                }
            }
        }
        public static PluginFeatureManifest GetPluginFeature(string PluginFeatureId)
        {
            return GetPluginFeature(PluginFeatureId, null);
        }
        public static bool TryGetPluginFeature(string PluginFeatureId, out PluginFeatureManifest PluginFeatureManifest)
        {
            return TryGetPluginFeature(PluginFeatureId, null, out PluginFeatureManifest);
        }
        public static List<PluginFeatureManifest> GetPluginFeatures(Type FeatureCategoryType)
        {
            if (_PluginManifests == null)
                throw new InvalidOperationException("Plugins have not been initialized");

            return _PluginManifests.Values.SelectMany(pm => pm.Features).Where(fm => FeatureCategoryType.IsAssignableFrom(fm.CategoryType)).OrderBy(fm => fm.PluginManifest.Name).ToList();
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
                throw new InvalidOperationException($"Unknown Plugin Feature Category Type: [{FeatureCategoryType.Name}]");
        }

        public static void InitalizePlugins(DiscoDataContext Database)
        {
            if (_PluginManifests == null)
            {
                lock (_PluginLock)
                {
                    if (_PluginManifests == null)
                    {
                        Version hostVersion = typeof(Plugins).Assembly.GetName().Version;
                        var compatibilityData = new Lazy<PluginLibraryIncompatibility>(() => PluginLibrary.LoadManifest(Database).LoadIncompatibilityData());
                        Dictionary<string, PluginManifest> loadedPlugins = new Dictionary<string, PluginManifest>();

                        PluginPath = Database.DiscoConfiguration.PluginsLocation;

                        AppDomain appDomain = AppDomain.CurrentDomain;

                        // Subscribe to Assembly Resolving
                        _PluginAssemblyReferences = new Dictionary<string, string>();
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
                                                throw new InvalidOperationException($"The plugin [{pluginManifest.Id}] is already initialized");

                                            // Check for Update
                                            string updatePackagePath = Path.Combine(pluginDirectoryRoot.FullName, $"{pluginManifest.Id}.discoPlugin");
                                            if (File.Exists(updatePackagePath))
                                            {
                                                // Update Plugin
                                                pluginManifest = UpdatePlugin(Database, pluginManifest, updatePackagePath, compatibilityData.Value);
                                            }

                                            if (pluginManifest != null)
                                            {
                                                // Check Version Compatibility
                                                var pluginIncompatible = compatibilityData.Value.IncompatiblePlugins.FirstOrDefault(i => i.PluginId.Equals(pluginManifest.Id, StringComparison.OrdinalIgnoreCase) && pluginManifest.Version == i.Version);
                                                if (pluginIncompatible != null)
                                                    throw new InvalidOperationException($"The plugin [{pluginManifest.Id} v{pluginManifest.VersionFormatted}] is not compatible: {pluginIncompatible.Reason}");

                                                if (pluginManifest.HostVersionMin != null && pluginManifest.HostVersionMin > hostVersion)
                                                    throw new InvalidOperationException($"The plugin [{pluginManifest.Id} v{pluginManifest.VersionFormatted}] does not support this version of Disco ICT (Requires v{pluginManifest.HostVersionMin.ToString()} or greater)");
                                                if (pluginManifest.HostVersionMax != null && pluginManifest.HostVersionMax < hostVersion)
                                                    throw new InvalidOperationException($"The plugin [{pluginManifest.Id} v{pluginManifest.VersionFormatted}] does not support this version of Disco ICT (Support expired as of v{pluginManifest.HostVersionMax.ToString()})");

                                                RegisterPluginAssemblyReferences(pluginManifest);

                                                pluginManifest.InitializePlugin(Database);
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
                                        string pluginStorageLocation = Path.Combine(Database.DiscoConfiguration.PluginStorageLocation, uninstallManifest.Id);

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

        public static PluginManifest UpdatePlugin(DiscoDataContext Database, PluginManifest ExistingManifest, String UpdatePluginPackageFilePath, PluginLibraryIncompatibility PluginLibraryIncompatibility = null)
        {
            PluginManifest updatedManifest;

            using (var packageStream = File.OpenRead(UpdatePluginPackageFilePath))
            {
                updatedManifest = UpdatePlugin(Database, ExistingManifest, packageStream, PluginLibraryIncompatibility);
            }

            // Remove Update after processing
            File.Delete(UpdatePluginPackageFilePath);

            return updatedManifest;
        }

        public static PluginManifest UpdatePlugin(DiscoDataContext Database, PluginManifest ExistingManifest, Stream UpdatePluginPackage, PluginLibraryIncompatibility PluginLibraryIncompatibility = null)
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

                    if (ExistingManifest.Version > packageManifest.Version)
                    {
                        throw new InvalidDataException("A newer version of this plugin is already installed");
                    }

                    // Check Compatibility
                    if (PluginLibraryIncompatibility == null)
                        PluginLibraryIncompatibility = PluginLibrary.LoadManifest(Database).LoadIncompatibilityData();
                    var pluginIncompatibility = PluginLibraryIncompatibility.IncompatiblePlugins.FirstOrDefault(i => i.PluginId.Equals(packageManifest.Id, StringComparison.OrdinalIgnoreCase) && packageManifest.Version == i.Version);
                    if (pluginIncompatibility != null)
                        throw new InvalidOperationException($"The plugin [{packageManifest.Id} v{packageManifest.VersionFormatted}] is not compatible: {pluginIncompatibility.Reason}");

                    string packagePath = Path.Combine(Database.DiscoConfiguration.PluginsLocation, packageManifest.Id);

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
                    packageManifest.AfterPluginUpdate(Database, ExistingManifest);

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
        internal static void RestartApp(TimeSpan delay)
        {
            lock (_restartTimerLock)
            {
                if (_restartTimer != null)
                {
                    _restartTimer.Dispose();
                }

                if (delay == TimeSpan.Zero)
                    HttpRuntime.UnloadAppDomain();
                else
                {
                    _restartTimer = new Timer((state) =>
                    {
                        HttpRuntime.UnloadAppDomain();
                    }, null, (int)delay.TotalMilliseconds, Timeout.Infinite);
                }
            }
        }
        #endregion

        #region Plugin Referenced Assemblies Resolving

        public static void RegisterPluginAssemblyReferences(PluginManifest manifest)
        {
            if (manifest.AssemblyReferences != null)
            {
                foreach (var reference in manifest.AssemblyReferences)
                    if (!_PluginAssemblyReferences.ContainsKey(reference.Key))
                        _PluginAssemblyReferences.Add(reference.Key, Path.Combine(manifest.PluginLocation, reference.Value));
            }
        }

        public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.RequestingAssembly != null && args.RequestingAssembly.Location.StartsWith(PluginPath, StringComparison.OrdinalIgnoreCase) && _PluginAssemblyReferences != null)
            {
                if (_PluginAssemblyReferences.TryGetValue(args.Name, out var assemblyPath))
                {
                    try
                    {
                        Assembly loadedAssembly = Assembly.LoadFile(assemblyPath);

                        PluginsLog.LogPluginReferenceAssemblyLoaded(args.Name, assemblyPath, args.RequestingAssembly.FullName);

                        return loadedAssembly;
                    }
                    catch (Exception ex)
                    {
                        PluginsLog.LogPluginException($"Resolving Plugin Reference Assembly: '{args.Name}' [{assemblyPath}]; Requested by: '{args.RequestingAssembly.FullName}' [{args.RequestingAssembly.Location}]; Disco.Plugins.DiscoPlugins.CurrentDomain_AssemblyResolve()", ex);
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
