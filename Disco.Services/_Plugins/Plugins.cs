using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Disco.Data.Repository;
using System.IO;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using RazorGenerator.Mvc;
using System.Web.WebPages;

namespace Disco.Services.Plugins
{
    public static class Plugins
    {
        private static Dictionary<string, PluginDefinition> _LoadedPlugins;
        internal static Dictionary<Type, string> CategoryDisplayNames;

        private static object _PluginLock = new object();

        public static string PluginPath { get; private set; }

        public static PluginDefinition GetPlugin(string PluginId, Type CategoryType = null)
        {
            if (_LoadedPlugins == null)
                throw new InvalidOperationException("Plugins have not been initialized");

            PluginDefinition def;
            if (_LoadedPlugins.TryGetValue(PluginId, out def))
            {
                if (CategoryType == null)
                    return def;
                else
                {
                    if (CategoryType.IsAssignableFrom(def.PluginCategoryType))
                        return def;
                    else
                        throw new InvalidCategoryTypeException(CategoryType, PluginId);
                }
            }
            else
            {
                throw new UnknownPluginException(PluginId);
            }
        }
        public static List<PluginDefinition> GetPlugins(Type CategoryType)
        {
            if (_LoadedPlugins == null)
                throw new InvalidOperationException("Plugins have not been initialized");

            return _LoadedPlugins.Values.Where(p => p.PluginCategoryType.IsAssignableFrom(CategoryType)).OrderBy(p => p.Name).ToList();
        }
        public static List<PluginDefinition> GetPlugins()
        {
            if (_LoadedPlugins == null)
                throw new InvalidOperationException("Plugins have not been initialized");

            return _LoadedPlugins.Values.ToList();
        }

        public static string PluginCategoryDisplayName(Type CategoryType)
        {
            if (CategoryType == null)
                throw new ArgumentNullException("CategoryType");

            string displayName;
            if (CategoryDisplayNames.TryGetValue(CategoryType, out displayName))
                return displayName;
            else
                throw new InvalidOperationException(string.Format("Unknown Plugin Category Type: [{0}]", CategoryType.Name));
        }

        #region Initalizing

        public static void InitalizePlugins(DiscoDataContext dbContext)
        {
            if (_LoadedPlugins == null)
            {
                lock (_PluginLock)
                {
                    if (_LoadedPlugins == null)
                    {
                        Dictionary<string, PluginDefinition> loadedPlugins = new Dictionary<string, PluginDefinition>();

                        PluginPath = dbContext.DiscoConfiguration.PluginsLocation;

                        AppDomain appDomain = AppDomain.CurrentDomain;

                        // Subscribe to Assembly Resolving
                        appDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                        // Load Internal (Default?) Plugins
                        IEnumerable<Assembly> discoAssemblies = (from a in appDomain.GetAssemblies()
                                                                 where !a.GlobalAssemblyCache && !a.IsDynamic &&
                                                                    a.FullName.StartsWith("Disco.", StringComparison.InvariantCultureIgnoreCase)
                                                                 select a);
                        foreach (var discoAssembly in discoAssemblies)
                        {
                            List<PluginDefinition> assemblyPluginDefinitions = InitializePluginAssembly(dbContext, discoAssembly, false);
                            foreach (PluginDefinition definition in assemblyPluginDefinitions)
                                loadedPlugins[definition.Id] = definition;
                        }

                        // Load External (DataStore) Plugins
                        DirectoryInfo pluginDirectoryRoot = new DirectoryInfo(PluginPath);
                        if (pluginDirectoryRoot.Exists)
                        {
                            foreach (DirectoryInfo pluginDirectory in pluginDirectoryRoot.EnumerateDirectories())
                            {
                                string pluginAssemblyFilename = Path.Combine(pluginDirectory.FullName, string.Format("{0}.dll", pluginDirectory.Name));
                                if (File.Exists(pluginAssemblyFilename))
                                {
                                    Assembly pluginAssembly = null;
                                    try
                                    {
                                        pluginAssembly = Assembly.LoadFile(pluginAssemblyFilename);

                                        if (pluginAssembly != null)
                                        {
                                            PluginsLog.LogInitializingPluginAssembly(pluginAssembly);
                                            List<PluginDefinition> assemblyPluginDefinitions = InitializePluginAssembly(dbContext, pluginAssembly, true);
                                            foreach (PluginDefinition definition in assemblyPluginDefinitions)
                                                loadedPlugins[definition.Id] = definition;
                                        }
                                    }
                                    catch (Exception ex) { PluginsLog.LogInitializeException(pluginAssemblyFilename, ex); }
                                }
                            }
                        }

                        // Determine Category Information
                        CategoryDisplayNames = InitializeCategoryDetails(loadedPlugins.Values);


                        _LoadedPlugins = loadedPlugins;
                    }
                }
            }
        }

        private static Dictionary<Type, string> InitializeCategoryDetails(IEnumerable<PluginDefinition> plugins)
        {
            Dictionary<Type, string> categoryDisplayNames = new Dictionary<Type, string>();
            foreach (var pluginDefinition in plugins)
            {
                if (!categoryDisplayNames.ContainsKey(pluginDefinition.PluginCategoryType))
                {
                    string displayName = null;

                    var displayAttributes = pluginDefinition.PluginCategoryType.GetCustomAttributes(typeof(PluginCategoryAttribute), true);
                    if (displayAttributes != null && displayAttributes.Length > 0)
                        displayName = ((PluginCategoryAttribute)(displayAttributes[0])).DisplayName;

                    if (string.IsNullOrWhiteSpace(displayName))
                        displayName = pluginDefinition.PluginCategoryType.Name;

                    categoryDisplayNames[pluginDefinition.PluginCategoryType] = displayName;
                }
            }
            return categoryDisplayNames;
        }

        private static List<PluginDefinition> InitializePluginAssembly(DiscoDataContext dbContext, Assembly PluginAssembly, bool ResolveReferences)
        {
            List<PluginDefinition> pluginDefinitions = new List<PluginDefinition>();

            var pluginTypes = (from type in PluginAssembly.GetTypes()
                               where typeof(Plugin).IsAssignableFrom(type) && !type.IsAbstract
                               select type).ToList();

            if (pluginTypes.Count > 0)
            {
                Dictionary<string, string> referencedAssemblies = null;

                string hostDirectory = Path.GetDirectoryName(PluginAssembly.Location);

                if (ResolveReferences)
                    referencedAssemblies = ImportReferencedAssemblies(PluginAssembly, hostDirectory);

                foreach (Type t in pluginTypes)
                {
                    var p = InitializePlugin(dbContext, t, hostDirectory, referencedAssemblies);
                    if (p != null)
                        pluginDefinitions.Add(p);
                }

            }

            return pluginDefinitions;
        }

        private static Dictionary<string, string> ImportReferencedAssemblies(Assembly PluginAssembly, string HostDirectory)
        {
            Dictionary<string, string> referencedAssemblies = new Dictionary<string, string>();
            foreach (string referenceFilename in Directory.EnumerateFiles(HostDirectory, "*.dll", SearchOption.TopDirectoryOnly))
            {
                if (!referenceFilename.Equals(PluginAssembly.Location, StringComparison.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        Assembly pluginRefAssembly = Assembly.ReflectionOnlyLoadFrom(referenceFilename);
                        referencedAssemblies[pluginRefAssembly.FullName] = referenceFilename;
                    }
                    catch (Exception) { } // Ignore Load Exceptions
                }
            }
            return referencedAssemblies;
        }

        private static PluginDefinition InitializePlugin(DiscoDataContext dbContext, Type PluginType, string HostDirectory, Dictionary<string, string> ReferencedAssemblies = null)
        {
            if (!typeof(Plugin).IsAssignableFrom(PluginType))
                throw new ArgumentException(string.Format("Plugins [{0}] does not inherit from [IDiscoPlugin]", PluginType.Name));

            using (Plugin instance = (Plugin)Activator.CreateInstance(PluginType))
            {
                try
                {
                    PluginDefinition definition = new PluginDefinition(instance, HostDirectory, ReferencedAssemblies);

                    PluginsLog.LogInitializingPlugin(definition);

                    instance.Initalize(dbContext);

                    return definition;
                }
                catch (Exception ex)
                {
                    PluginsLog.LogInitializeException(PluginType.Assembly.Location, ex);
                    return null;
                }
            }
        }

        #endregion

        #region Plugin Referenced Assemblies Resolving

        public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.RequestingAssembly.Location.StartsWith(PluginPath, StringComparison.InvariantCultureIgnoreCase) && _LoadedPlugins != null)
            {
                // Try best guess first
                PluginDefinition requestingPlugin = _LoadedPlugins.Values.Where(p => p.PluginType.Assembly == args.RequestingAssembly).FirstOrDefault();
                if (requestingPlugin != null)
                {
                    Assembly loadedAssembly = CurrentDomain_AssemblyResolve_ByPlugin(requestingPlugin, args);
                    if (loadedAssembly != null)
                        return loadedAssembly;
                }

                // Try all Plugin References
                foreach (var pluginDef in _LoadedPlugins.Values)
                {
                    Assembly loadedAssembly = CurrentDomain_AssemblyResolve_ByPlugin(pluginDef, args);
                    if (loadedAssembly != null)
                        return loadedAssembly;
                }
            }
            return null;
        }
        private static Assembly CurrentDomain_AssemblyResolve_ByPlugin(PluginDefinition PluginDefinition, ResolveEventArgs args)
        {
            if (PluginDefinition.PluginReferenceAssemblies != null)
            {
                string assemblyPath;
                if (PluginDefinition.PluginReferenceAssemblies.TryGetValue(args.Name, out assemblyPath))
                {
                    try
                    {
                        Assembly loadedAssembly = Assembly.LoadFile(assemblyPath);

                        PluginsLog.LogPluginReferenceAssemblyLoaded(args.Name, assemblyPath, args.RequestingAssembly.FullName);

                        return loadedAssembly;
                    }
                    catch (Exception ex)
                    {
                        PluginsLog.LogPluginException(string.Format("Resolving Plugin Reference Assembly: '{0}' [{1}]; Requested by: '{2}' [{3}]; Disco.Plugins.DiscoPlugins.CurrentDomain_AssemblyResolve()", args.Name, assemblyPath, args.RequestingAssembly.FullName, args.RequestingAssembly.Location), ex);
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
