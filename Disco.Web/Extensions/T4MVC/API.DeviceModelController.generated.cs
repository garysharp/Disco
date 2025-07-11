// <auto-generated />
// This file was generated by a T4 template.
// Don't change it directly as your change would get overwritten.  Instead, make changes
// to the .tt file (i.e. the T4 template) and save it to regenerate this file.

// Make sure the compiler doesn't complain about missing Xml comments and CLS compliance
// 0108: suppress "Foo hides inherited member Foo. Use the new keyword if hiding was intended." when a controller and its abstract parent are both processed
// 0114: suppress "Foo.BarController.Baz()' hides inherited member 'Qux.BarController.Baz()'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword." when an action (with an argument) overrides an action in a parent controller
#pragma warning disable 1591, 3008, 3009, 0108, 0114
#region T4MVC

using System;
using System.Diagnostics;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;
using System.Web.Routing;
using T4MVC;
namespace Disco.Web.Areas.API.Controllers
{
    public partial class DeviceModelController
    {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public DeviceModelController() { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected DeviceModelController(Dummy d) { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected RedirectToRouteResult RedirectToAction(ActionResult result)
        {
            var callInfo = result.GetT4MVCResult();
            return RedirectToRoute(callInfo.RouteValueDictionary);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected RedirectToRouteResult RedirectToAction(Task<ActionResult> taskResult)
        {
            return RedirectToAction(taskResult.Result);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected RedirectToRouteResult RedirectToActionPermanent(ActionResult result)
        {
            var callInfo = result.GetT4MVCResult();
            return RedirectToRoutePermanent(callInfo.RouteValueDictionary);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected RedirectToRouteResult RedirectToActionPermanent(Task<ActionResult> taskResult)
        {
            return RedirectToActionPermanent(taskResult.Result);
        }

        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult Update()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Update);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult UpdateDescription()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UpdateDescription);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult UpdateManufacturer()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UpdateManufacturer);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult UpdateModel()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UpdateModel);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult UpdateDefaultPurchaseDate()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UpdateDefaultPurchaseDate);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult UpdateDefaultWarrantyProvider()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UpdateDefaultWarrantyProvider);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult UpdateDefaultRepairProvider()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UpdateDefaultRepairProvider);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult Image()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Image);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult Delete()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Delete);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult Component()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Component);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult ComponentAdd()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ComponentAdd);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult ComponentUpdateJobSubTypes()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ComponentUpdateJobSubTypes);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult ComponentUpdate()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ComponentUpdate);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult ComponentRemove()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ComponentRemove);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public DeviceModelController Actions { get { return MVC.API.DeviceModel; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "API";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "DeviceModel";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "DeviceModel";
        [GeneratedCode("T4MVC", "2.0")]
        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass
        {
            public readonly string Update = "Update";
            public readonly string UpdateDescription = "UpdateDescription";
            public readonly string UpdateManufacturer = "UpdateManufacturer";
            public readonly string UpdateModel = "UpdateModel";
            public readonly string UpdateDefaultPurchaseDate = "UpdateDefaultPurchaseDate";
            public readonly string UpdateDefaultWarrantyProvider = "UpdateDefaultWarrantyProvider";
            public readonly string UpdateDefaultRepairProvider = "UpdateDefaultRepairProvider";
            public readonly string Image = "Image";
            public readonly string Delete = "Delete";
            public readonly string Component = "Component";
            public readonly string ComponentAdd = "ComponentAdd";
            public readonly string ComponentUpdateJobSubTypes = "ComponentUpdateJobSubTypes";
            public readonly string ComponentUpdate = "ComponentUpdate";
            public readonly string ComponentRemove = "ComponentRemove";
            public readonly string Index = "Index";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants
        {
            public const string Update = "Update";
            public const string UpdateDescription = "UpdateDescription";
            public const string UpdateManufacturer = "UpdateManufacturer";
            public const string UpdateModel = "UpdateModel";
            public const string UpdateDefaultPurchaseDate = "UpdateDefaultPurchaseDate";
            public const string UpdateDefaultWarrantyProvider = "UpdateDefaultWarrantyProvider";
            public const string UpdateDefaultRepairProvider = "UpdateDefaultRepairProvider";
            public const string Image = "Image";
            public const string Delete = "Delete";
            public const string Component = "Component";
            public const string ComponentAdd = "ComponentAdd";
            public const string ComponentUpdateJobSubTypes = "ComponentUpdateJobSubTypes";
            public const string ComponentUpdate = "ComponentUpdate";
            public const string ComponentRemove = "ComponentRemove";
            public const string Index = "Index";
        }


        static readonly ActionParamsClass_Update s_params_Update = new ActionParamsClass_Update();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Update UpdateParams { get { return s_params_Update; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Update
        {
            public readonly string id = "id";
            public readonly string key = "key";
            public readonly string value = "value";
            public readonly string redirect = "redirect";
        }
        static readonly ActionParamsClass_UpdateDescription s_params_UpdateDescription = new ActionParamsClass_UpdateDescription();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_UpdateDescription UpdateDescriptionParams { get { return s_params_UpdateDescription; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_UpdateDescription
        {
            public readonly string id = "id";
            public readonly string Description = "Description";
            public readonly string redirect = "redirect";
        }
        static readonly ActionParamsClass_UpdateManufacturer s_params_UpdateManufacturer = new ActionParamsClass_UpdateManufacturer();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_UpdateManufacturer UpdateManufacturerParams { get { return s_params_UpdateManufacturer; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_UpdateManufacturer
        {
            public readonly string id = "id";
            public readonly string manufacturer = "manufacturer";
            public readonly string redirect = "redirect";
        }
        static readonly ActionParamsClass_UpdateModel s_params_UpdateModel = new ActionParamsClass_UpdateModel();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_UpdateModel UpdateModelParams { get { return s_params_UpdateModel; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_UpdateModel
        {
            public readonly string id = "id";
            public readonly string model = "model";
            public readonly string redirect = "redirect";
        }
        static readonly ActionParamsClass_UpdateDefaultPurchaseDate s_params_UpdateDefaultPurchaseDate = new ActionParamsClass_UpdateDefaultPurchaseDate();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_UpdateDefaultPurchaseDate UpdateDefaultPurchaseDateParams { get { return s_params_UpdateDefaultPurchaseDate; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_UpdateDefaultPurchaseDate
        {
            public readonly string id = "id";
            public readonly string DefaultPurchaseDate = "DefaultPurchaseDate";
            public readonly string redirect = "redirect";
        }
        static readonly ActionParamsClass_UpdateDefaultWarrantyProvider s_params_UpdateDefaultWarrantyProvider = new ActionParamsClass_UpdateDefaultWarrantyProvider();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_UpdateDefaultWarrantyProvider UpdateDefaultWarrantyProviderParams { get { return s_params_UpdateDefaultWarrantyProvider; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_UpdateDefaultWarrantyProvider
        {
            public readonly string id = "id";
            public readonly string DefaultWarrantyProvider = "DefaultWarrantyProvider";
            public readonly string redirect = "redirect";
        }
        static readonly ActionParamsClass_UpdateDefaultRepairProvider s_params_UpdateDefaultRepairProvider = new ActionParamsClass_UpdateDefaultRepairProvider();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_UpdateDefaultRepairProvider UpdateDefaultRepairProviderParams { get { return s_params_UpdateDefaultRepairProvider; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_UpdateDefaultRepairProvider
        {
            public readonly string id = "id";
            public readonly string DefaultRepairProvider = "DefaultRepairProvider";
            public readonly string redirect = "redirect";
        }
        static readonly ActionParamsClass_Image s_params_Image = new ActionParamsClass_Image();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Image ImageParams { get { return s_params_Image; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Image
        {
            public readonly string id = "id";
            public readonly string v = "v";
            public readonly string redirect = "redirect";
            public readonly string Image = "Image";
        }
        static readonly ActionParamsClass_Delete s_params_Delete = new ActionParamsClass_Delete();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Delete DeleteParams { get { return s_params_Delete; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Delete
        {
            public readonly string id = "id";
            public readonly string redirect = "redirect";
        }
        static readonly ActionParamsClass_Component s_params_Component = new ActionParamsClass_Component();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Component ComponentParams { get { return s_params_Component; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Component
        {
            public readonly string id = "id";
        }
        static readonly ActionParamsClass_ComponentAdd s_params_ComponentAdd = new ActionParamsClass_ComponentAdd();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_ComponentAdd ComponentAddParams { get { return s_params_ComponentAdd; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_ComponentAdd
        {
            public readonly string id = "id";
            public readonly string Description = "Description";
            public readonly string Cost = "Cost";
        }
        static readonly ActionParamsClass_ComponentUpdateJobSubTypes s_params_ComponentUpdateJobSubTypes = new ActionParamsClass_ComponentUpdateJobSubTypes();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_ComponentUpdateJobSubTypes ComponentUpdateJobSubTypesParams { get { return s_params_ComponentUpdateJobSubTypes; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_ComponentUpdateJobSubTypes
        {
            public readonly string id = "id";
            public readonly string JobSubTypes = "JobSubTypes";
        }
        static readonly ActionParamsClass_ComponentUpdate s_params_ComponentUpdate = new ActionParamsClass_ComponentUpdate();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_ComponentUpdate ComponentUpdateParams { get { return s_params_ComponentUpdate; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_ComponentUpdate
        {
            public readonly string id = "id";
            public readonly string Description = "Description";
            public readonly string Cost = "Cost";
        }
        static readonly ActionParamsClass_ComponentRemove s_params_ComponentRemove = new ActionParamsClass_ComponentRemove();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_ComponentRemove ComponentRemoveParams { get { return s_params_ComponentRemove; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_ComponentRemove
        {
            public readonly string id = "id";
        }
        static readonly ViewsClass s_views = new ViewsClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ViewsClass Views { get { return s_views; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ViewsClass
        {
            static readonly _ViewNamesClass s_ViewNames = new _ViewNamesClass();
            public _ViewNamesClass ViewNames { get { return s_ViewNames; } }
            public class _ViewNamesClass
            {
            }
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public partial class T4MVC_DeviceModelController : Disco.Web.Areas.API.Controllers.DeviceModelController
    {
        public T4MVC_DeviceModelController() : base(Dummy.Instance) { }

        [NonAction]
        partial void UpdateOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int id, string key, string value, bool redirect);

        [NonAction]
        public override System.Web.Mvc.ActionResult Update(int id, string key, string value, bool redirect)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Update);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "key", key);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "value", value);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "redirect", redirect);
            UpdateOverride(callInfo, id, key, value, redirect);
            return callInfo;
        }

        [NonAction]
        partial void UpdateDescriptionOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int id, string Description, bool redirect);

        [NonAction]
        public override System.Web.Mvc.ActionResult UpdateDescription(int id, string Description, bool redirect)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UpdateDescription);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "Description", Description);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "redirect", redirect);
            UpdateDescriptionOverride(callInfo, id, Description, redirect);
            return callInfo;
        }

        [NonAction]
        partial void UpdateManufacturerOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int id, string manufacturer, bool redirect);

        [NonAction]
        public override System.Web.Mvc.ActionResult UpdateManufacturer(int id, string manufacturer, bool redirect)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UpdateManufacturer);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "manufacturer", manufacturer);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "redirect", redirect);
            UpdateManufacturerOverride(callInfo, id, manufacturer, redirect);
            return callInfo;
        }

        [NonAction]
        partial void UpdateModelOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int id, string model, bool redirect);

        [NonAction]
        public override System.Web.Mvc.ActionResult UpdateModel(int id, string model, bool redirect)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UpdateModel);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "redirect", redirect);
            UpdateModelOverride(callInfo, id, model, redirect);
            return callInfo;
        }

        [NonAction]
        partial void UpdateDefaultPurchaseDateOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int id, string DefaultPurchaseDate, bool redirect);

        [NonAction]
        public override System.Web.Mvc.ActionResult UpdateDefaultPurchaseDate(int id, string DefaultPurchaseDate, bool redirect)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UpdateDefaultPurchaseDate);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "DefaultPurchaseDate", DefaultPurchaseDate);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "redirect", redirect);
            UpdateDefaultPurchaseDateOverride(callInfo, id, DefaultPurchaseDate, redirect);
            return callInfo;
        }

        [NonAction]
        partial void UpdateDefaultWarrantyProviderOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int id, string DefaultWarrantyProvider, bool redirect);

        [NonAction]
        public override System.Web.Mvc.ActionResult UpdateDefaultWarrantyProvider(int id, string DefaultWarrantyProvider, bool redirect)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UpdateDefaultWarrantyProvider);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "DefaultWarrantyProvider", DefaultWarrantyProvider);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "redirect", redirect);
            UpdateDefaultWarrantyProviderOverride(callInfo, id, DefaultWarrantyProvider, redirect);
            return callInfo;
        }

        [NonAction]
        partial void UpdateDefaultRepairProviderOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int id, string DefaultRepairProvider, bool redirect);

        [NonAction]
        public override System.Web.Mvc.ActionResult UpdateDefaultRepairProvider(int id, string DefaultRepairProvider, bool redirect)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UpdateDefaultRepairProvider);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "DefaultRepairProvider", DefaultRepairProvider);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "redirect", redirect);
            UpdateDefaultRepairProviderOverride(callInfo, id, DefaultRepairProvider, redirect);
            return callInfo;
        }

        [NonAction]
        partial void ImageOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int? id, string v);

        [NonAction]
        public override System.Web.Mvc.ActionResult Image(int? id, string v)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Image);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "v", v);
            ImageOverride(callInfo, id, v);
            return callInfo;
        }

        [NonAction]
        partial void ImageOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int id, bool redirect, System.Web.HttpPostedFileBase Image);

        [NonAction]
        public override System.Web.Mvc.ActionResult Image(int id, bool redirect, System.Web.HttpPostedFileBase Image)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Image);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "redirect", redirect);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "Image", Image);
            ImageOverride(callInfo, id, redirect, Image);
            return callInfo;
        }

        [NonAction]
        partial void DeleteOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int id, bool? redirect);

        [NonAction]
        public override System.Web.Mvc.ActionResult Delete(int id, bool? redirect)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Delete);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "redirect", redirect);
            DeleteOverride(callInfo, id, redirect);
            return callInfo;
        }

        [NonAction]
        partial void ComponentOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int id);

        [NonAction]
        public override System.Web.Mvc.ActionResult Component(int id)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Component);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            ComponentOverride(callInfo, id);
            return callInfo;
        }

        [NonAction]
        partial void ComponentAddOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int? id, string Description, string Cost);

        [NonAction]
        public override System.Web.Mvc.ActionResult ComponentAdd(int? id, string Description, string Cost)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ComponentAdd);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "Description", Description);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "Cost", Cost);
            ComponentAddOverride(callInfo, id, Description, Cost);
            return callInfo;
        }

        [NonAction]
        partial void ComponentUpdateJobSubTypesOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int id, System.Collections.Generic.List<string> JobSubTypes);

        [NonAction]
        public override System.Web.Mvc.ActionResult ComponentUpdateJobSubTypes(int id, System.Collections.Generic.List<string> JobSubTypes)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ComponentUpdateJobSubTypes);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "JobSubTypes", JobSubTypes);
            ComponentUpdateJobSubTypesOverride(callInfo, id, JobSubTypes);
            return callInfo;
        }

        [NonAction]
        partial void ComponentUpdateOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int id, string Description, string Cost);

        [NonAction]
        public override System.Web.Mvc.ActionResult ComponentUpdate(int id, string Description, string Cost)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ComponentUpdate);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "Description", Description);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "Cost", Cost);
            ComponentUpdateOverride(callInfo, id, Description, Cost);
            return callInfo;
        }

        [NonAction]
        partial void ComponentRemoveOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int id);

        [NonAction]
        public override System.Web.Mvc.ActionResult ComponentRemove(int id)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ComponentRemove);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            ComponentRemoveOverride(callInfo, id);
            return callInfo;
        }

        [NonAction]
        partial void IndexOverride(T4MVC_System_Web_Mvc_ActionResult callInfo);

        [NonAction]
        public override System.Web.Mvc.ActionResult Index()
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Index);
            IndexOverride(callInfo);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591, 3008, 3009, 0108, 0114
