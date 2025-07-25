using Disco.Models.Repository;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Plugins;
using Disco.Services.Plugins.Features.RepairProvider;
using Disco.Services.Plugins.Features.WarrantyProvider;
using Disco.Services.Web;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class DeviceModelController : AuthorizedDatabaseController
    {

        const string pDescription = "description";
        const string pManufacturer = "manufacturer";
        const string pModel = "model";
        const string pDefaultPurchaseDate = "defaultpurchasedate";
        const string pDefaultWarrantyProvider = "defaultwarrantyprovider";
        const string pDefaultRepairProvider = "defaultrepairprovider";

        [DiscoAuthorize(Claims.Config.DeviceModel.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Update(int id, string key, string value = null, bool redirect = false)
        {
            Authorization.Require(Claims.Config.DeviceModel.Configure);

            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("key");
                var deviceModel = Database.DeviceModels.Find(id);
                if (deviceModel != null)
                {
                    switch (key.ToLower())
                    {
                        case pDescription:
                            UpdateDescription(deviceModel, value);
                            break;
                        case pManufacturer:
                            UpdateManufacturer(deviceModel, value);
                            break;
                        case pModel:
                            UpdateModel(deviceModel, value);
                            break;
                        case pDefaultPurchaseDate:
                            UpdateDefaultPurchaseDate(deviceModel, value);
                            break;
                        case pDefaultWarrantyProvider:
                            UpdateDefaultWarrantyProvider(deviceModel, value);
                            break;
                        case pDefaultRepairProvider:
                            UpdateDefaultRepairProvider(deviceModel, value);
                            break;
                        default:
                            throw new Exception("Invalid Update Key");
                    }
                }
                else
                {
                    return BadRequest("Invalid Device Model Number");
                }
                if (redirect)
                    return RedirectToAction(MVC.Config.DeviceModel.Index(deviceModel.Id));
                else
                    return Ok();
            }
            catch (Exception ex)
            {
                if (redirect)
                    throw;
                else
                    return BadRequest(ex.Message);
            }
        }

        #region Update Shortcut Methods

        [DiscoAuthorize(Claims.Config.DeviceModel.Configure)]
        public virtual ActionResult UpdateDescription(int id, string Description = null, bool redirect = false)
        {
            return Update(id, pDescription, Description, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceModel.Configure)]
        public virtual ActionResult UpdateManufacturer(int id, string manufacturer = null, bool redirect = false)
        {
            return Update(id, pManufacturer, manufacturer, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceModel.Configure)]
        public virtual ActionResult UpdateModel(int id, string model = null, bool redirect = false)
        {
            return Update(id, pModel, model, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceModel.Configure)]
        public virtual ActionResult UpdateDefaultPurchaseDate(int id, string DefaultPurchaseDate = null, bool redirect = false)
        {
            return Update(id, pDefaultPurchaseDate, DefaultPurchaseDate, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceModel.Configure)]
        public virtual ActionResult UpdateDefaultWarrantyProvider(int id, string DefaultWarrantyProvider = null, bool redirect = false)
        {
            return Update(id, pDefaultWarrantyProvider, DefaultWarrantyProvider, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceModel.Configure)]
        public virtual ActionResult UpdateDefaultRepairProvider(int id, string DefaultRepairProvider = null, bool redirect = false)
        {
            return Update(id, pDefaultRepairProvider, DefaultRepairProvider, redirect);
        }

        #endregion

        #region Update Properties
        private void UpdateDescription(DeviceModel deviceModel, string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                deviceModel.Description = null;
            else
                deviceModel.Description = description;
            Database.SaveChanges();
        }
        private void UpdateManufacturer(DeviceModel deviceModel, string manufacturer)
        {
            if (!deviceModel.IsCustomModel())
                throw new InvalidCastException("Cannot update Manufacturer for a non-custom device model.");

            if (string.IsNullOrWhiteSpace(manufacturer))
                deviceModel.Manufacturer = null;
            else
                deviceModel.Manufacturer = manufacturer;
            Database.SaveChanges();
        }
        private void UpdateModel(DeviceModel deviceModel, string model)
        {
            if (!deviceModel.IsCustomModel())
                throw new InvalidCastException("Cannot update Model for a non-custom device model.");

            if (string.IsNullOrWhiteSpace(model))
                deviceModel.Model = null;
            else
                deviceModel.Model = model;
            Database.SaveChanges();
        }
        private void UpdateDefaultPurchaseDate(DeviceModel deviceModel, string defaultPurchaseDate)
        {
            if (string.IsNullOrEmpty(defaultPurchaseDate))
            {
                deviceModel.DefaultPurchaseDate = null;
            }
            else
            {
                if (DateTime.TryParse(defaultPurchaseDate, out var d))
                {
                    deviceModel.DefaultPurchaseDate = d;
                }
                else
                {
                    throw new Exception("Invalid Date Format");
                }
            }
            Database.SaveChanges();
        }
        private void UpdateDefaultWarrantyProvider(DeviceModel deviceModel, string defaultWarrantyProvider)
        {
            if (string.IsNullOrEmpty(defaultWarrantyProvider))
            {
                deviceModel.DefaultWarrantyProvider = null;
            }
            else
            {
                // Validate
                var WarrantyProvider = Plugins.GetPluginFeature(defaultWarrantyProvider, typeof(WarrantyProviderFeature));
                deviceModel.DefaultWarrantyProvider = WarrantyProvider.Id;
            }
            Database.SaveChanges();
        }
        private void UpdateDefaultRepairProvider(DeviceModel deviceModel, string defaultRepairProvider)
        {
            if (string.IsNullOrEmpty(defaultRepairProvider))
            {
                deviceModel.DefaultRepairProvider = null;
            }
            else
            {
                // Validate
                var RepairProvider = Plugins.GetPluginFeature(defaultRepairProvider, typeof(RepairProviderFeature));
                deviceModel.DefaultRepairProvider = RepairProvider.Id;
            }
            Database.SaveChanges();
        }
        #endregion

        #region ModelImage
        [OutputCache(Duration = 31536000, Location = System.Web.UI.OutputCacheLocation.Any, VaryByParam = "*")]
        public virtual ActionResult Image(int? id, string v = null)
        {
            if (id.HasValue)
            {
                var m = Database.DeviceModels.Find(id.Value);
                if (m != null)
                {
                    // Try From DataStore

                    var deviceModelImage = m.Image();
                    if (deviceModelImage != null)
                        return File(deviceModelImage, "image/png");

                    // DataStore Failed - Use Generic Images
                    if (m.ModelType != null)
                    {
                        var modelTypePath = Server.MapPath($"~/ClientSource/Style/Images/DeviceTypes/{m.ModelType}.png");
                        if (System.IO.File.Exists(modelTypePath))
                        {
                            return File(modelTypePath, "image/png");
                        }
                    }
                }
            }
            return File(Links.ClientSource.Style.Images.DeviceTypes.Unknown_png, "image/png");
        }

        [DiscoAuthorize(Claims.Config.DeviceModel.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Image(int id, bool redirect, HttpPostedFileBase Image)
        {
            if (Image != null && Image.ContentLength > 0)
            {
                var dm = Database.DeviceModels.Find(id);
                if (dm != null)
                {
                    if (dm.ImageImport(Image.InputStream))
                    {
                        Database.SaveChanges();
                        if (redirect)
                            return RedirectToAction(MVC.Config.DeviceModel.Index(dm.Id));
                        else
                            return Ok();
                    }
                    else
                    {
                        if (redirect)
                            return RedirectToAction(MVC.Config.DeviceModel.Index(dm.Id));
                        else
                            return BadRequest("Invalid Image Format");
                    }
                }
                if (redirect)
                    return RedirectToAction(MVC.Config.DeviceModel.Index());
                else
                    return BadRequest("Invalid Device Model Number");
            }
            if (redirect)
                return RedirectToAction(MVC.Config.DeviceModel.Index());
            else
                return BadRequest("No Image Supplied");
        }
        #endregion

        #region Actions

        [DiscoAuthorize(Claims.Config.DeviceModel.Delete)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Delete(int id, bool? redirect = false)
        {
            try
            {
                var dm = Database.DeviceModels.Find(id);
                if (dm != null)
                {
                    dm.Delete(Database);
                    Database.SaveChanges();
                    if (redirect.HasValue && redirect.Value)
                        return RedirectToAction(MVC.Config.DeviceModel.Index(null));
                    else
                        return Ok();
                }
                throw new Exception("Invalid Device Model Number");
            }
            catch (Exception ex)
            {
                if (redirect.HasValue && redirect.Value)
                    throw;
                else
                    return BadRequest(ex.Message);
            }
        }

        #endregion

        #region Device Model Components

        [DiscoAuthorize(Claims.Config.DeviceModel.Show)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Component(int id)
        {
            var dc = Database.DeviceComponents.Include(c => c.JobSubTypes).Where(i => i.Id == id).FirstOrDefault();
            if (dc == null)
                return BadRequest("Invalid Device Component Id");

            return Json(Models.DeviceModel.ComponentModel.FromDeviceComponent(dc));
        }

        [DiscoAuthorize(Claims.Config.DeviceModel.ConfigureComponents)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult ComponentAdd(int? id, string description, string cost)
        {
            DeviceModel dm = null;
            if (id.HasValue)
            {
                dm = Database.DeviceModels.Find(id.Value);
                if (dm == null)
                    return BadRequest("Invalid Device Model Id");
            }

            if (string.IsNullOrEmpty(description))
                description = "?";
            if (!string.IsNullOrEmpty(cost) && cost.Contains("$"))
                cost = cost.Substring(cost.IndexOf("$") + 1);
            decimal.TryParse(cost, out var costValue);

            var dc = new DeviceComponent()
            {
                Description = description,
                Cost = costValue
            };
            if (dm != null)
            {
                dc.DeviceModelId = dm.Id;
            }
            dc.JobSubTypes = new List<JobSubType>();

            Database.DeviceComponents.Add(dc);
            Database.SaveChanges();

            return Json(Models.DeviceModel.ComponentModel.FromDeviceComponent(dc));
        }

        [DiscoAuthorize(Claims.Config.DeviceModel.ConfigureComponents)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult ComponentUpdateJobSubTypes(int id, List<string> jobSubTypes)
        {
            var dc = Database.DeviceComponents.Include(c => c.JobSubTypes).Where(i => i.Id == id).FirstOrDefault();
            if (dc == null)
                return BadRequest("Invalid Device Component Id");

            dc.JobSubTypes.Clear();

            if (jobSubTypes != null)
            {
                var jsts = Database.JobSubTypes.Where(jst => jobSubTypes.Contains(jst.JobTypeId + "_" + jst.Id));
                foreach (var jst in jsts)
                {
                    dc.JobSubTypes.Add(jst);
                }
            }

            Database.SaveChanges();

            return Json(Models.DeviceModel.ComponentModel.FromDeviceComponent(dc));
        }

        [DiscoAuthorize(Claims.Config.DeviceModel.ConfigureComponents)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult ComponentUpdate(int id, string description, string cost)
        {
            var dc = Database.DeviceComponents.Include(c => c.JobSubTypes).Where(i => i.Id == id).FirstOrDefault();
            if (dc == null)
                return BadRequest("Invalid Device Component Id");

            if (string.IsNullOrEmpty(description))
                description = "?";
            if (!string.IsNullOrEmpty(cost) && cost.Contains("$"))
                cost = cost.Substring(cost.IndexOf("$") + 1);
            decimal.TryParse(cost, out var costValue);

            dc.Description = description;
            dc.Cost = costValue;

            Database.SaveChanges();

            return Json(Models.DeviceModel.ComponentModel.FromDeviceComponent(dc));
        }

        [DiscoAuthorize(Claims.Config.DeviceModel.ConfigureComponents)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult ComponentRemove(int id)
        {
            var dc = Database.DeviceComponents.Include(c => c.JobSubTypes).Where(c => c.Id == id).FirstOrDefault();
            if (dc == null)
                return BadRequest("Invalid Device Component Id");

            dc.JobSubTypes.Clear();
            Database.DeviceComponents.Remove(dc);
            Database.SaveChanges();
            return Ok();
        }
        #endregion

        #region Index
        [DiscoAuthorizeAny(Claims.Config.DeviceModel.Show, Claims.Config.Enrolment.ShowStatus)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult Index()
        {
            var deviceModels = Database.DeviceModels.AsEnumerable().Select(dm => Models.DeviceModel._DeviceModel.FromDeviceModel(dm)).ToList();
            return Json(deviceModels);
        }
        #endregion

    }
}
