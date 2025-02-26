using Disco.Services.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace Disco.Services.Plugins
{
    public abstract class PluginWebHandlerController : PluginWebHandler
    {
        public ControllerContext ControllerContext => HostController.ControllerContext;

        public void OnAuthorization(AuthorizationContext authorizationContext, string actionName)
        {
            var actionDescriptor = (PluginActionDescriptor)authorizationContext.ActionDescriptor;

            var authorizationFilters = actionDescriptor.GetAuthorizationFilters;
            foreach (var filter in authorizationFilters)
            {
                filter.OnAuthorization(authorizationContext);
                if (authorizationContext.Result != null)
                    return;
            }
        }

        public override ActionResult ExecuteAction(string actionName)
        {
            var handlerType = GetType();
            var methodDescriptor = FindControllerMethod(ControllerContext, Manifest, handlerType, actionName);

            if (methodDescriptor == null)
                return HttpNotFound("No such plugin method");

            var authorizationContext = new AuthorizationContext(ControllerContext, methodDescriptor);
            OnAuthorization(authorizationContext, actionName);
            if (authorizationContext.Result != null)
                return authorizationContext.Result;

            return methodDescriptor.Execute(this, ControllerContext);
        }

        private static PluginActionDescriptor FindControllerMethod(ControllerContext context, PluginManifest manifest, Type handler, string actionName)
        {
            var descriptors = GetWebHandler(manifest, handler);
            return (PluginActionDescriptor)descriptors.FindAction(context, actionName);
        }

        #region Method Cache
        private static readonly Dictionary<Type, PluginControllerDescription> pluginControllerCache = new Dictionary<Type, PluginControllerDescription>();
        private static PluginControllerDescription GetWebHandler(PluginManifest manifest, Type handler)
        {
            if (!pluginControllerCache.TryGetValue(handler, out var result))
            {
                // Cache Miss
                result = new PluginControllerDescription(manifest, handler);
                pluginControllerCache[handler] = result;
            }

            return result;
        }

        private class PluginParameterDescriptor : ParameterDescriptor
        {
            private static readonly object[] emptyObjectArray = new object[0];

            private readonly PluginActionDescriptor actionDescriptor;
            public override ActionDescriptor ActionDescriptor => actionDescriptor;
            public override string ParameterName { get; }
            public override Type ParameterType { get; }
            public override ParameterBindingInfo BindingInfo { get; }
            public override object DefaultValue { get; }

            public PluginParameterDescriptor(PluginActionDescriptor actionDescriptor, string parameterName, Type parameterType, IModelBinder modelBinder, object defaultValue)
            {
                this.actionDescriptor = actionDescriptor;
                ParameterName = parameterName;
                ParameterType = parameterType;
                DefaultValue = defaultValue;
                BindingInfo = new PluginParameterBindingInfo(modelBinder);
            }

            public object BindParameter(ControllerContext controllerContext)
            {
                IValueProvider valueProvider = controllerContext.Controller.ValueProvider;

                ModelBindingContext bindingContext = new ModelBindingContext()
                {
                    FallbackToEmptyPrefix = true,
                    ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, ParameterType),
                    ModelName = ParameterName,
                    ModelState = controllerContext.Controller.ViewData.ModelState,
                    PropertyFilter = (p) => true,
                    ValueProvider = valueProvider
                };

                var parameterValue = BindingInfo.Binder.BindModel(controllerContext.Controller.ControllerContext, bindingContext);

                if (parameterValue == null)
                    parameterValue = DefaultValue;

                return parameterValue;
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                return emptyObjectArray;
            }
            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return emptyObjectArray;
            }
            public override bool IsDefined(Type attributeType, bool inherit)
            {
                return false;
            }

            private class PluginParameterBindingInfo : ParameterBindingInfo
            {
                private static readonly string[] emptyStringArray = new string[0];
                public override IModelBinder Binder { get; }
                public override ICollection<string> Exclude { get; } = emptyStringArray;
                public override ICollection<string> Include { get; } = emptyStringArray;

                public PluginParameterBindingInfo(IModelBinder modelBinder)
                {
                    Binder = modelBinder;
                }
            }
        }

        private class PluginActionDescriptor : ActionDescriptor
        {
            private readonly PluginControllerDescription controllerDescription;
            private readonly MethodInfo methodInfo;
            private readonly IAuthorizationFilter[] authorizationFilters;
            private readonly ActionMethodSelectorAttribute methodSelector;
            private readonly PluginParameterDescriptor[] parameterDescriptors;
            public override string UniqueId { get; }
            public override string ActionName { get; }
            public override ControllerDescriptor ControllerDescriptor => controllerDescription;

            public PluginActionDescriptor(PluginControllerDescription controllerDescription, string methodName, MethodInfo methodInfo)
            {
                this.controllerDescription = controllerDescription;
                this.methodInfo = methodInfo;
                authorizationFilters = DiscoverAuthorizationFilters();
                methodSelector = DiscoverMethodSelector();
                parameterDescriptors = DiscoverParameters();

                switch (methodSelector)
                {
                    case HttpPostAttribute _:
                        methodName += ":POST";
                        break;
                    case HttpGetAttribute _:
                        methodName += ":GET";
                        break;
                    case HttpPutAttribute _:
                        methodName += ":PUT";
                        break;
                    case HttpDeleteAttribute _:
                        methodName += ":DELETE";
                        break;
                }

                ActionName = methodName;
                UniqueId = $"{ControllerDescriptor.UniqueId}_{methodName}";
            }

            private IAuthorizationFilter[] DiscoverAuthorizationFilters()
            {
                var filters = methodInfo.GetCustomAttributes<FilterAttribute>(true).OfType<IAuthorizationFilter>().ToList();

                foreach (var authorizer in filters.OfType<DiscoAuthorizeBaseAttribute>())
                    authorizer.AuthorizeResource = string.Format("[Plugin]::{0}::{1}", controllerDescription.Manifest.Id, methodInfo.Name);

                return filters.ToArray();
            }

            private ActionMethodSelectorAttribute DiscoverMethodSelector()
            {
                return methodInfo.GetCustomAttributes<ActionMethodSelectorAttribute>(true).FirstOrDefault();
            }

            private PluginParameterDescriptor[] DiscoverParameters()
            {
                var methodParams = methodInfo.GetParameters();
                var result = new List<PluginParameterDescriptor>(methodParams.Length);

                for (int i = 0; i < methodParams.Length; i++)
                {
                    var methodParam = methodParams[i];

                    IModelBinder modelBinder = ModelBinders.Binders.GetBinder(methodParam.ParameterType);

                    var defaultValue = (object)null;
                    if (methodParam.DefaultValue != DBNull.Value)
                        defaultValue = methodParam.DefaultValue;

                    result.Add(new PluginParameterDescriptor(this, methodParam.Name, methodParam.ParameterType, modelBinder, defaultValue));
                }

                return result.ToArray();
            }

            public override object Execute(ControllerContext controllerContext, IDictionary<string, object> parameters)
            {
                throw new NotSupportedException();
            }

            public ActionResult Execute(PluginWebHandlerController pluginController, ControllerContext controllerContext)
            {
                if (methodSelector != null && !methodSelector.IsValidForRequest(controllerContext, methodInfo))
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

                var methodParameters = BuildMethodParameters(methodInfo, controllerContext.Controller);

                return (ActionResult)methodInfo.Invoke(pluginController, methodParameters);
            }

            private static object[] BuildMethodParameters(MethodInfo methodInfo, ControllerBase HostController)
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
                }

                return result;
            }

            private object[] BuildMethodParameters(ControllerContext controllerContext)
            {
                var parameters = new object[parameterDescriptors.Length];
                for (int i = 0; i < parameterDescriptors.Length; i++)
                {
                    parameters[i] = parameterDescriptors[i].BindParameter(controllerContext);
                }
                return parameters;
            }

            public override ParameterDescriptor[] GetParameters()
            {
                return parameterDescriptors;
            }

            public IEnumerable<IAuthorizationFilter> GetAuthorizationFilters => authorizationFilters;

            public override object[] GetCustomAttributes(bool inherit)
            {
                return authorizationFilters;
            }
            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                var result = new List<IAuthorizationFilter>(authorizationFilters.Length);
                foreach (var filter in authorizationFilters)
                {
                    if (attributeType.IsAssignableFrom(filter.GetType()))
                        result.Add(filter);
                }
                return result.ToArray();
            }
            public override bool IsDefined(Type attributeType, bool inherit)
            {
                foreach (var filter in authorizationFilters)
                {
                    if (attributeType.IsAssignableFrom(filter.GetType()))
                    {
                        return true;
                    }
                }
                return false;
            }

            public override IEnumerable<FilterAttribute> GetFilterAttributes(bool useCache)
            {
                return authorizationFilters.OfType<FilterAttribute>();
            }
        }

        private class PluginControllerDescription : ControllerDescriptor
        {
            private static readonly object[] emptyObjectArray = new object[0];
            private readonly Dictionary<string, PluginActionDescriptor> actions;
            public override string ControllerName { get; }

            public PluginManifest Manifest { get; }
            public override Type ControllerType { get; }
            public override string UniqueId { get; }

            public PluginControllerDescription(PluginManifest manifest, Type controllerType)
            {
                Manifest = manifest;
                ControllerType = controllerType;
                actions = DiscoverActions();
                ControllerName = controllerType.Name;
                UniqueId = $"DiscoPlugin_{manifest.Id}_{controllerType.Name}";
            }

            private Dictionary<string, PluginActionDescriptor> DiscoverActions()
            {
                var actions = new Dictionary<string, PluginActionDescriptor>(StringComparer.OrdinalIgnoreCase);
                var methods = Array.FindAll(ControllerType.GetMethods(BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly), mi => { return !mi.IsSpecialName && typeof(ActionResult).IsAssignableFrom(mi.ReturnType); });
                foreach (var method in methods)
                {
                    var descriptor = new PluginActionDescriptor(this, method.Name, method);
                    actions.Add(descriptor.ActionName, descriptor);
                }
                return actions;
            }

            public override ActionDescriptor FindAction(ControllerContext controllerContext, string actionName)
            {
                var method = controllerContext.HttpContext.Request.HttpMethod.ToUpperInvariant();

                if (actions.TryGetValue($"{actionName}:{method}", out var action))
                    return action;
                if (actions.TryGetValue(actionName, out action))
                    return action;
                else
                    return null;
            }

            public override ActionDescriptor[] GetCanonicalActions()
            {
                return actions.Values.ToArray();
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                return emptyObjectArray;
            }
            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return emptyObjectArray;
            }
            public override IEnumerable<FilterAttribute> GetFilterAttributes(bool useCache)
            {
                return Enumerable.Empty<FilterAttribute>();
            }
            public override bool IsDefined(Type attributeType, bool inherit)
            {
                return false;
            }
        }
        #endregion
    }
}
