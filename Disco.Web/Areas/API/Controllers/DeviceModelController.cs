using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.BI;
using Disco.BI.Extensions;
using Disco.Models.Repository;
using Disco.Services.Plugins;
using Disco.Services.Plugins.Features.WarrantyProvider;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class DeviceModelController : dbAdminController
    {

        const string pDescription = "description";
        const string pDefaultPurchaseDate = "defaultpurchasedate";
        const string pDefaultWarrantyProvider = "defaultwarrantyprovider";

        public virtual ActionResult Update(int id, string key, string value = null, bool redirect = false)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("key");
                var deviceModel = dbContext.DeviceModels.Find(id);
                if (deviceModel != null)
                {
                    switch (key.ToLower())
                    {
                        case pDescription:
                            UpdateDescription(deviceModel, value);
                            break;
                        case pDefaultPurchaseDate:
                            UpdateDefaultPurchaseDate(deviceModel, value);
                            break;
                        case pDefaultWarrantyProvider:
                            UpdateDefaultWarrantyProvider(deviceModel, value);
                            break;
                        default:
                            throw new Exception("Invalid Update Key");
                    }
                }
                else
                {
                    return Json("Invalid Device Model Number", JsonRequestBehavior.AllowGet);
                }
                if (redirect)
                    return RedirectToAction(MVC.Config.DeviceModel.Index(deviceModel.Id));
                else
                    return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (redirect)
                    throw;
                else
                    return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        #region Update Shortcut Methods
        public virtual ActionResult UpdateDescription(int id, string Description = null, bool redirect = false)
        {
            return Update(id, pDescription, Description, redirect);
        }
        public virtual ActionResult UpdateDefaultPurchaseDate(int id, string DefaultPurchaseDate = null, bool redirect = false)
        {
            return Update(id, pDefaultPurchaseDate, DefaultPurchaseDate, redirect);
        }
        public virtual ActionResult UpdateDefaultWarrantyProvider(int id, string DefaultWarrantyProvider = null, bool redirect = false)
        {
            return Update(id, pDefaultWarrantyProvider, DefaultWarrantyProvider, redirect);
        }
        #endregion

        #region Update Properties
        private void UpdateDescription(Disco.Models.Repository.DeviceModel deviceModel, string Description)
        {
            if (string.IsNullOrWhiteSpace(Description))
                deviceModel.Description = null;
            else
                deviceModel.Description = Description;
            dbContext.SaveChanges();
        }
        private void UpdateDefaultPurchaseDate(Disco.Models.Repository.DeviceModel deviceModel, string DefaultPurchaseDate)
        {
            if (string.IsNullOrEmpty(DefaultPurchaseDate))
            {
                deviceModel.DefaultPurchaseDate = null;
            }
            else
            {
                DateTime d;
                if (DateTime.TryParse(DefaultPurchaseDate, out d))
                {
                    deviceModel.DefaultPurchaseDate = d;
                }
                else
                {
                    throw new Exception("Invalid Date Format");
                }
            }
            dbContext.SaveChanges();
        }
        private void UpdateDefaultWarrantyProvider(Disco.Models.Repository.DeviceModel deviceModel, string DefaultWarrantyProvider)
        {
            if (string.IsNullOrEmpty(DefaultWarrantyProvider))
            {
                deviceModel.DefaultWarrantyProvider = null;
            }
            else
            {
                // Validate
                var WarrantyProvider = Plugins.GetPluginFeature(DefaultWarrantyProvider, typeof(WarrantyProviderFeature));
                deviceModel.DefaultWarrantyProvider = WarrantyProvider.Id;
            }
            dbContext.SaveChanges();
        }
        #endregion

        #region ModelImage
        [OutputCache(Duration = 31536000, Location = System.Web.UI.OutputCacheLocation.Any, VaryByParam = "*")]
        public virtual ActionResult Image(int? id, string v = null)
        {
            if (id.HasValue)
            {
                var m = dbContext.DeviceModels.Find(id.Value);
                if (m != null)
                {
                    // Try From DataStore

                    var deviceModelImage = m.Image();
                    if (deviceModelImage != null)
                        return File(deviceModelImage, "image/png");
                    //if ( m.Image != null)
                    //{
                    //    return File(m.Image, "image/png");
                    //}
                    //else
                    //{
                    // DataStore Failed - Use Generic Images
                    if (m.ModelType != null)
                    {
                        var modelTypePath = Server.MapPath(string.Format("~/Content/Images/DeviceTypes/{0}.png", m.ModelType));
                        if (System.IO.File.Exists(modelTypePath))
                        {
                            return File(modelTypePath, "image/png");
                        }
                    }
                    //}
                }
            }
            return File(Links.ClientSource.Style.Images.DeviceTypes.Unknown_png, "image/png");
        }
        [HttpPost]
        public virtual ActionResult Image(int id, bool redirect, HttpPostedFileBase Image)
        {
            if (Image != null && Image.ContentLength > 0)
            {
                var dm = dbContext.DeviceModels.Find(id);
                if (dm != null)
                {
                    if (dm.ImageImport(Image.InputStream))
                    {
                        dbContext.SaveChanges();
                        if (redirect)
                            return RedirectToAction(MVC.Config.DeviceModel.Index(dm.Id));
                        else
                            return Json("OK", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        if (redirect)
                            return RedirectToAction(MVC.Config.DeviceModel.Index(dm.Id));
                        else
                            return Json("Invalid Image Format", JsonRequestBehavior.AllowGet);
                    }
                }
                if (redirect)
                    return RedirectToAction(MVC.Config.DeviceModel.Index());
                else
                    return Json("Invalid Device Model Number", JsonRequestBehavior.AllowGet);
            }
            if (redirect)
                return RedirectToAction(MVC.Config.DeviceModel.Index());
            else
                return Json("No Image Supplied", JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Actions

        public virtual ActionResult Delete(int id, Nullable<bool> redirect = false)
        {
            try
            {
                var dm = dbContext.DeviceModels.Find(id);
                if (dm != null)
                {
                    dm.Delete(dbContext);
                    dbContext.SaveChanges();
                    if (redirect.HasValue && redirect.Value)
                        return RedirectToAction(MVC.Config.DeviceModel.Index(null));
                    else
                        return Json("OK", JsonRequestBehavior.AllowGet);
                }
                throw new Exception("Invalid Device Model Number");
            }
            catch (Exception ex)
            {
                if (redirect.HasValue && redirect.Value)
                    throw;
                else
                    return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Device Model Components

        public virtual ActionResult Component(int id)
        {
            var dc = dbContext.DeviceComponents.Include("JobSubTypes").Where(i => i.Id == id).FirstOrDefault();
            if (dc != null)
            {
                return Json(new Models.DeviceModel.ComponentModel { Result = "OK", Component = Models.DeviceModel._ComponentModel.FromDeviceComponent(dc) }, JsonRequestBehavior.AllowGet);
            }
            return Json(new Models.DeviceModel.ComponentModel { Result = "Invalid Device Component Id" }, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult ComponentAdd(int? id, string Description, string Cost)
        {
            DeviceModel dm = null;
            if (id.HasValue)
            {
                dm = dbContext.DeviceModels.Find(id.Value);
                if (dm == null)
                {
                    return Json(new Models.DeviceModel.ComponentModel { Result = "Invalid Device Model Id" }, JsonRequestBehavior.AllowGet);
                }
            }

            decimal cost = 0;
            if (string.IsNullOrEmpty(Description))
                Description = "?";
            if (!string.IsNullOrEmpty(Cost) && Cost.Contains("$"))
                Cost = Cost.Substring(Cost.IndexOf("$") + 1);
            decimal.TryParse(Cost, out cost);

            var dc = new Disco.Models.Repository.DeviceComponent()
            {
                Description = Description,
                Cost = cost
            };
            if (dm != null)
            {
                dc.DeviceModelId = dm.Id;
            }
            dc.JobSubTypes = new List<JobSubType>();

            dbContext.DeviceComponents.Add(dc);
            dbContext.SaveChanges();

            return Json(new Models.DeviceModel.ComponentModel { Result = "OK", Component = Models.DeviceModel._ComponentModel.FromDeviceComponent(dc) }, JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult ComponentUpdateJobSubTypes(int id, List<string> JobSubTypes)
        {
            var dc = dbContext.DeviceComponents.Include("JobSubTypes").Where(i => i.Id == id).FirstOrDefault();
            if (dc != null)
            {
                dc.JobSubTypes.Clear();

                if (JobSubTypes != null)
                {
                    var jsts = dbContext.JobSubTypes.Where(jst => JobSubTypes.Contains(jst.JobTypeId + "_" + jst.Id));
                    foreach (var jst in jsts)
                    {
                        dc.JobSubTypes.Add(jst);
                    }
                }

                dbContext.SaveChanges();

                return Json(new Models.DeviceModel.ComponentModel { Result = "OK", Component = Models.DeviceModel._ComponentModel.FromDeviceComponent(dc) }, JsonRequestBehavior.AllowGet);
            }
            return Json(new Models.DeviceModel.ComponentModel { Result = "Invalid Device Component Id" }, JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult ComponentUpdate(int id, string Description, string Cost)
        {
            var dc = dbContext.DeviceComponents.Include("JobSubTypes").Where(i => i.Id == id).FirstOrDefault();
            if (dc != null)
            {
                decimal cost = 0;

                if (string.IsNullOrEmpty(Description))
                    Description = "?";
                if (!string.IsNullOrEmpty(Cost) && Cost.Contains("$"))
                    Cost = Cost.Substring(Cost.IndexOf("$") + 1);
                decimal.TryParse(Cost, out cost);

                dc.Description = Description;
                dc.Cost = cost;

                dbContext.SaveChanges();

                return Json(new Models.DeviceModel.ComponentModel { Result = "OK", Component = Models.DeviceModel._ComponentModel.FromDeviceComponent(dc) }, JsonRequestBehavior.AllowGet);
            }
            return Json(new Models.DeviceModel.ComponentModel { Result = "Invalid Device Component Id" }, JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult ComponentRemove(int id)
        {
            var dc = dbContext.DeviceComponents.Include("JobSubTypes").Where(c => c.Id == id).FirstOrDefault();
            if (dc != null)
            {
                dc.JobSubTypes.Clear();
                dbContext.DeviceComponents.Remove(dc);
                dbContext.SaveChanges();
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            return Json("Invalid Device Component Id", JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Index
        public virtual ActionResult Index()
        {
            var deviceModels = dbContext.DeviceModels.ToArray().Select(dm => Models.DeviceModel._DeviceModel.FromDeviceModel(dm)).ToArray();
            return Json(deviceModels, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}
