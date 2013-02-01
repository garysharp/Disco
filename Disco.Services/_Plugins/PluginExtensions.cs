using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Data.Repository;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web;

namespace Disco.Services.Plugins
{
    public static class PluginExtensions
    {
        public static string PluginStorage(this Plugin plugin, DiscoDataContext dbContext)
        {
            string pluginStorageLocationRoot = dbContext.DiscoConfiguration.PluginStorageLocation;

            string storageLocation = Path.Combine(pluginStorageLocationRoot, plugin.Id);

            if (!Directory.Exists(storageLocation))
                Directory.CreateDirectory(storageLocation);

            return storageLocation;
        }

        public static string PluginCategoryDisplayName(this Plugin plugin)
        {
            if (plugin == null)
                throw new ArgumentNullException("plugin");

            return Plugins.CategoryDisplayNames[plugin.PluginCategoryType];
        }

        #region Model Binding from Controller
        public static bool TryUpdateModel<TModel>(this Controller controller, TModel model) where TModel : class
        {
            return controller.TryUpdateModel<TModel>(model, null, controller.ValueProvider);
        }
        public static bool TryUpdateModel<TModel>(this Controller controller, TModel model, IValueProvider valueProvider) where TModel : class
        {
            return controller.TryUpdateModel<TModel>(model, null, valueProvider);
        }
        public static bool TryUpdateModel<TModel>(this Controller controller, TModel model, string prefix) where TModel : class
        {
            return controller.TryUpdateModel<TModel>(model, prefix, controller.ValueProvider);
        }
        public static bool TryUpdateModel<TModel>(this Controller controller, TModel model, string prefix, IValueProvider valueProvider) where TModel : class
        {
            if (model == null)
                throw new ArgumentNullException("model");
            if (valueProvider == null)
                throw new ArgumentNullException("valueProvider");

            Predicate<string> predicate = propertyName => true;
            IModelBinder binder = ModelBinders.Binders.GetBinder(typeof(TModel));

            ModelBindingContext context2 = new ModelBindingContext
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, typeof(TModel)),
                ModelName = prefix,
                ModelState = controller.ModelState,
                PropertyFilter = predicate,
                ValueProvider = valueProvider
            };

            ModelBindingContext bindingContext = context2;

            binder.BindModel(controller.ControllerContext, bindingContext);

            return controller.ModelState.IsValid;
        }
        #endregion
    }
}
