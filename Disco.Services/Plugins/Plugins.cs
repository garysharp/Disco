using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Disco.Data.Repository;
using System.IO.Compression;

namespace Disco.Services.Plugins
{
    public static class Plugins
    {
        private static Dictionary<Assembly, PluginManifest> _PluginAssemblyManifests;
        private static Dictionary<string, PluginManifest> _PluginManifests;
        internal static Dictionary<Type, string> FeatureCategoryDisplayNames;

        private static object _PluginLock = new object();
        public static string PluginPath { get; private set; }

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

        public static void InitalizePlugins(DiscoDataContext dbContext)
        {
            if (_PluginManifests == null)
            {
                lock (_PluginLock)
                {
                    if (_PluginManifests == null)
                    {
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

                                            pluginManifest.InitializePlugin(dbContext);

                                            loadedPlugins[pluginManifest.Id] = pluginManifest;
                                        }
                                    }
                                    catch (Exception ex) { PluginsLog.LogInitializeException(pluginManifestFilename, ex); }
                                }
                            }
                        }

                        _PluginManifests = loadedPlugins;

                        ReinitializePluginEnvironment();

                        // Install Plugins - TEMPORARY? Workaround until UI in place? Or Useful for 'built-in' plugins?
                        if (pluginDirectoryRoot.Exists)
                        {
                            foreach (FileInfo pluginPackageFile in pluginDirectoryRoot.EnumerateFiles("*.discoPlugin", SearchOption.TopDirectoryOnly))
                            {
                                // Install Plugin
                                InstallPlugin(dbContext, pluginPackageFile.FullName);
                                // Delete Package File
                                pluginPackageFile.Delete();
                            }
                        }
                    }
                }
            }
        }

        private static void ReinitializePluginEnvironment()
        {
            FeatureCategoryDisplayNames = InitializeFeatureCategoryDetails(_PluginManifests.Values);
            _PluginAssemblyManifests = _PluginManifests.Values.ToDictionary(p => p.PluginAssembly, p => p);
        }

        public static void InstallPlugin(DiscoDataContext dbContext, String PackageFilePath)
        {
            using (var packageStream = File.OpenRead(PackageFilePath))
            {
                InstallPlugin(dbContext, packageStream);
            }
        }

        public static void InstallPlugin(DiscoDataContext dbContext, Stream PluginPackage)
        {
            if (_PluginManifests == null)
                throw new InvalidOperationException("Plugins have not been initialized");

            using (MemoryStream packageStream = new MemoryStream())
            {
                PluginPackage.CopyTo(packageStream);
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

                    lock (_PluginLock)
                    {
                        if (_PluginManifests == null)
                            throw new InvalidOperationException("Plugins have not been initialized");

                        // Ensure not already installed
                        if (_PluginManifests.ContainsKey(packageManifest.Id))
                            throw new InvalidOperationException(string.Format("The '{0} [{1}]' Plugin is already installed, please uninstall any existing versions before trying again", packageManifest.Name, packageManifest.Id));

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

                        // Initialize Plugin
                        packageManifest.InitializePlugin(dbContext);

                        // Add Plugin Manifest to Environment
                        _PluginManifests[packageManifest.Id] = packageManifest;

                        // Reinitialize Plugin Environment
                        ReinitializePluginEnvironment();
                    }

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
