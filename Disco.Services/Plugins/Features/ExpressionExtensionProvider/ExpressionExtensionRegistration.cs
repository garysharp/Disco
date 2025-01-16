using System;

namespace Disco.Services.Plugins.Features.ExpressionExtensionProvider
{
    public class ExpressionExtensionRegistration
    {
        public PluginFeatureManifest FeatureManifest { get; }
        public string Alias { get; }
        public Type ExtensionType { get; }

        public ExpressionExtensionRegistration(PluginFeatureManifest featureManifest, string alias, Type extensionType)
        {
            FeatureManifest = featureManifest;
            Alias = alias;
            ExtensionType = extensionType;
        }
    }
}
