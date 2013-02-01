using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Disco.Services.Plugins
{
    public abstract class PluginWebHandlerController : PluginWebHandler
    {

        public override ActionResult ExecuteAction(string ActionName)
        {
            var handlerType = this.GetType();
            var methodDescriptor = FindControllerMethod(handlerType, ActionName);

            if (methodDescriptor == null)
                return this.HttpNotFound("Unknown Plugin Method");

            var methodParams = BuildMethodParameters(handlerType, methodDescriptor.MethodInfo, ActionName, this.HostController);

            return (ActionResult)methodDescriptor.MethodInfo.Invoke(this, methodParams);
        }

        private static WebHandlerCachedItem FindControllerMethod(Type Handler, string ActionName)
        {
            var descriptors = CacheWebHandler(Handler);
            WebHandlerCachedItem method;
            if (descriptors.TryGetValue(ActionName.ToLower(), out method))
                return method; // Not Found
            else
                return null; // Not Found
        }
        private static object[] BuildMethodParameters(Type Handler, MethodInfo methodInfo, string ActionName, Controller HostController)
        {
            var methodParams = methodInfo.GetParameters();
            var result = new object[methodParams.Length];

            for (int i = 0; i < methodParams.Length; i++)
            {
                var methodParam = methodParams[i];

                Type parameterType = methodParam.ParameterType;
                IModelBinder modelBinder = ModelBinders.Binders.GetBinder(parameterType);
                IValueProvider valueProvider = HostController.ValueProvider;
                string parameterName = methodParam.Name;

                ModelBindingContext bindingContext = new ModelBindingContext()
                {
                    FallbackToEmptyPrefix = true,
                    ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, parameterType),
                    ModelName = parameterName,
                    ModelState = HostController.ViewData.ModelState,
                    PropertyFilter = (p) => true,
                    ValueProvider = valueProvider
                };

                var parameterValue = modelBinder.BindModel(HostController.ControllerContext, bindingContext);

                if (parameterValue == null && methodParam.HasDefaultValue)
                        parameterValue = methodParam.DefaultValue;

                result[i] = parameterValue;

                //var paramInstance = Activator.CreateInstance(methodParam.ParameterType);
                
                //IModelBinder binder = ModelBinders.Binders.GetBinder(methodParam.ParameterType);
                //ModelBindingContext bindingContext = new ModelBindingContext
                //{
                //    ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => paramInstance, methodParam.ParameterType),
                //    ModelName = methodParam.Name,
                //    ModelState = HostController.ModelState,
                //    PropertyFilter = (p) => true,
                //    ValueProvider = HostController.ValueProvider
                //};
                //binder.BindModel(HostController.ControllerContext, bindingContext);

                //if (methodParam.HasDefaultValue && paramInstance == null)
                //    paramInstance = methodParam.DefaultValue;

                //result[i] = paramInstance;
            }

            return result;
        }

        #region Method Cache
        private static Dictionary<Type, Dictionary<string, WebHandlerCachedItem>> WebHandlerCachedItems = new Dictionary<Type, Dictionary<string, WebHandlerCachedItem>>();
        private static Dictionary<string, WebHandlerCachedItem> CacheWebHandler(Type Handler)
        {
            Dictionary<string, WebHandlerCachedItem> result;

            if (!WebHandlerCachedItems.TryGetValue(Handler, out result))
            {
                // Cache Miss
                result = new Dictionary<string, WebHandlerCachedItem>();
                var methods = Array.FindAll<MethodInfo>(Handler.GetMethods(BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly), mi => { return !mi.IsSpecialName && typeof(ActionResult).IsAssignableFrom(mi.ReturnType); });
                foreach (var method in methods)
                {
                    var item = new WebHandlerCachedItem()
                    {
                        Method = method.Name,
                        MethodInfo = method
                    };
                    result.Add(item.Method.ToLower(), item);
                }
                WebHandlerCachedItems[Handler] = result;
            }

            return result;
        }
        private class WebHandlerCachedItem
        {
            public string Method { get; set; }
            public MethodInfo MethodInfo { get; set; }
        }
        #endregion
    }
}
