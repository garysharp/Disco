using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Disco.Models.UI;
using Disco.Services.Plugins.Features.UIExtension.Results;

namespace Disco.Services.Plugins.Features.UIExtension
{
    [PluginFeatureCategory(DisplayName = "User Interface Extensions")]
    public abstract class UIExtensionFeature<UIModel> : PluginFeature where UIModel : BaseUIModel
    {
        public abstract UIExtensionResult ExecuteAction(ControllerContext context, UIModel model);

        #region ActionResults

        protected LiteralResult Literal(string Content)
        {
            return new LiteralResult(this.Manifest, Content);
        }
        protected PluginResourceScriptResult ScriptResource(string Resource)
        {
            return new PluginResourceScriptResult(this.Manifest, Resource);
        }

        #endregion

        #region Registration
        public bool Register()
        {
            return UIExtensions.UIExtensions.RegisterExtension(this);
        }
        public bool Unregister()
        {
            return UIExtensions.UIExtensions.UnregisterExtension(this);
        }
        public bool IsRegistered
        {
            get
            {
                return UIExtensions.UIExtensions.ExtensionRegistered(this);
            }
        }
        #endregion
    }
}
