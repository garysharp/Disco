using Spring.Core.TypeResolution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Disco.Services.Plugins.Features.ExpressionExtensionProvider
{
    [PluginFeatureCategory(DisplayName = "Expression Extension")]
    public abstract class ExpressionExtensionProviderFeature : PluginFeature
    {
        private static readonly Dictionary<string, ExpressionExtensionRegistration> registrations = new Dictionary<string, ExpressionExtensionRegistration>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Assembly> pluginAssemblies = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

        public void RegisterExpressionExtension(string alias, Type extensionType)
        {
            if (!extensionType.IsClass || !extensionType.IsAbstract || !extensionType.IsSealed)
                throw new ArgumentException("Expression Extension Types must be a static class", nameof(extensionType));

            lock (registrations)
            {
                registrations.Add(alias, new ExpressionExtensionRegistration(Manifest, alias, extensionType));
                var assemblyName = extensionType.Assembly.FullName.Substring(0, extensionType.Assembly.FullName.IndexOf(','));
                pluginAssemblies[assemblyName] = extensionType.Assembly;
                TypeRegistry.RegisterType(alias, extensionType);
            }
        }

        public static List<ExpressionExtensionRegistration> GetExpressionExtensionRegistrations()
        {
            lock (registrations)
            {
                return registrations.Values.ToList();
            }
        }

        public static bool TryGetExtensionAssembly(string assemblyName, out Assembly assembly)
            => pluginAssemblies.TryGetValue(assemblyName, out assembly);
    }
}
