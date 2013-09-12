using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.BI;
using Disco.BI.Extensions;
using Disco.Data.Repository;
using Disco.Models.Repository;
using System.Data.Objects.SqlClient;
using Disco.Web.Extensions;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Models.UI.Device;
using Disco.Services.Plugins;


namespace Disco.Web.Controllers
{
    public partial class DeviceController : dbAdminController
    {
        #region Index
        public virtual ActionResult Index()
        {
            Models.Device.IndexModel m = new Models.Device.IndexModel();

            // UI Extensions
            UIExtensions.ExecuteExtensions<DeviceIndexModel>(this.ControllerContext, m);

            return View();
        }
        #endregion

        #region Add Offline
        public virtual ActionResult AddOffline()
        {
            var m = new Models.Device.AddOfflineModel()
            {
                DefaultDeviceProfileId = dbContext.DiscoConfiguration.DeviceProfiles.DefaultAddDeviceOfflineDeviceProfileId
            };
            m.DeviceBatches = dbContext.DeviceBatches.ToList();
            m.DeviceProfiles = dbContext.DeviceProfiles.ToList();
            if (m.DefaultDeviceProfileId == 0)
            {
                m.DeviceProfiles.Insert(0, new DeviceProfile() { Id = 0, Name = "Select a Device Profile" });
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<DeviceAddOfflineModel>(this.ControllerContext, m);

            return View(m);
        }
        [HttpPost]
        public virtual ActionResult AddOffline(Models.Device.AddOfflineModel m)
        {
            // Trim Serial Number & Error Check
            m.Device.SerialNumber = m.Device.SerialNumber.Trim();
            if (string.IsNullOrEmpty(m.Device.SerialNumber))
            {
                ModelState.AddModelError("Device.SerialNumber", "The Serial Number is Required");
            }
            else
            {
                // Ensure Existing Device Doesn't Exist
                if (!string.IsNullOrEmpty(m.Device.SerialNumber) && dbContext.Devices.Count(d => d.SerialNumber == m.Device.SerialNumber) > 0)
                    ModelState.AddModelError("Device.SerialNumber", "A Device what this Serial Number already exists");
            }


            if (ModelState.IsValid)
            {
                var d = m.Device.AddOffline(dbContext);
                dbContext.SaveChanges();
                return RedirectToAction(MVC.Device.Show(d.SerialNumber));
            }
            return AddOffline();
        }
        #endregion

        #region Import/Export
        [HttpGet]
        public virtual ActionResult ImportExport()
        {
            Models.Device.ImportModel m = new Models.Device.ImportModel();

            m.DeviceModels = dbContext.DeviceModels.ToList();
            m.DeviceProfiles = dbContext.DeviceProfiles.ToList();
            m.DeviceBatches = dbContext.DeviceBatches.ToList();

            // UI Extensions
            UIExtensions.ExecuteExtensions<DeviceImportModel>(this.ControllerContext, m);

            return View(m);
        }
        [HttpGet]
        public virtual ActionResult ImportReview(string ImportParseTaskId)
        {
            if (string.IsNullOrWhiteSpace(ImportParseTaskId))
                throw new ArgumentNullException("ImportParseTaskId");

            var session = Disco.BI.DeviceBI.Importing.Import.GetSession(ImportParseTaskId);

            if (session == null)
                throw new ArgumentException("The Import Parse Task Id is invalid or the session timed out (60 minutes), try importing again", "ImportParseTaskId");

            Models.Device.ImportReviewModel m = Models.Device.ImportReviewModel.FromImportDeviceSession(session);

            // UI Extensions
            UIExtensions.ExecuteExtensions<DeviceImportReviewModel>(this.ControllerContext, m);

            return View(m);
        }
        #endregion

        #region Show
        public virtual ActionResult Show(string id)
        {
            var m = new Models.Device.ShowModel();

            dbContext.Configuration.LazyLoadingEnabled = true;

            m.Device = dbContext.Devices.Include("DeviceDetails")
                .Where(d => d.SerialNumber == id)
                .FirstOrDefault();

            if (m.Device == null)
                throw new ArgumentException(string.Format("Unknown Device: [{0}]", id), "id");

            // Removed 2012-07-03 G#
            // Deferred to Ajax call - improve load performance
            // Update Device LastNetworkLogonDate
            //if (m.Device.UpdateLastNetworkLogonDate())
            //    dbContext.SaveChanges();

            // No Necessary - Yet...
            //if (!string.IsNullOrWhiteSpace(m.Device.ComputerName))
            //{
            //    var adMachineAccount = BI.Interop.ActiveDirectory.ActiveDirectory.GetMachineAccount(m.Device.ComputerName);
            //    if (adMachineAccount != null)
            //    {
            //        m.OrganisationUnit = adMachineAccount.ParentDistinguishedName;
            //    }
            //}

            m.DeviceProfiles = dbContext.DeviceProfiles.ToList();

            m.DeviceBatches = dbContext.DeviceBatches.ToList();

            m.Jobs = new Disco.Models.BI.Job.JobTableModel()
            {
                ShowStatus = true,
                ShowDevice = false,
                IsSmallTable = false,
                HideClosedJobs = true,
                EnablePaging = false
            };
            m.Jobs.Fill(dbContext, BI.JobBI.Searching.BuildJobTableModel(dbContext).Where(j => j.DeviceSerialNumber == m.Device.SerialNumber).OrderByDescending(j => j.Id));

            m.Certificates = dbContext.DeviceCertificates.Where(c => c.DeviceSerialNumber == m.Device.SerialNumber).ToList();

            //m.AttachmentTypes = dbContext.AttachmentTypes.Where(at => at.Scope == AttachmentType.AttachmentTypeScopes.Device).ToList();
            m.DocumentTemplates = m.Device.AvailableDocumentTemplates(dbContext, DiscoApplication.CurrentUser, DateTime.Now);

            m.DeviceProfileDefaultOrganisationAddress = m.Device.DeviceProfile.DefaultOrganisationAddressDetails(dbContext);


            PluginFeatureManifest deviceProfileCertificateProvider;
            if (Disco.Services.Plugins.Plugins.TryGetPluginFeature(m.Device.DeviceProfile.CertificateProviderId, out deviceProfileCertificateProvider))
                m.DeviceProfileCertificateProvider = deviceProfileCertificateProvider;

            // UI Extensions
            UIExtensions.ExecuteExtensions<DeviceShowModel>(this.ControllerContext, m);

            return View(m);
        }
        #endregion
    }
}
