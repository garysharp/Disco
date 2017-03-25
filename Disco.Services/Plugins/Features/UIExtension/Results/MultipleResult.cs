using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Disco.Services.Plugins.Features.UIExtension.Results
{
    public class MultipleResult : UIExtensionResult
    {
        private IEnumerable<UIExtensionResult> results;

        public MultipleResult(PluginFeatureManifest Source, params UIExtensionResult[] Results) : base(Source)
        {
            if (Results == null || Results.Length == 0)
                throw new ArgumentException("At least one result is required", "Results");

            results = Results;
        }

        public override void ExecuteResult<T>(WebViewPage<T> page)
        {
            foreach (var result in results)
            {
                result.ExecuteResult(page);
                page.WriteLiteral("\n");
            }
        }
    }
}
