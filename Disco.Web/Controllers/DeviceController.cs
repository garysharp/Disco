using Disco.BI.Extensions;
using Disco.Models.Repository;
using Disco.Models.Services.Devices.Exporting;
using Disco.Models.Services.Jobs.JobLists;
using Disco.Models.UI.Device;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Plugins;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Services.Users;
using Disco.Services.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;


namespace Disco.Web.Controllers
{
    public partial class DeviceController : AuthorizedDatabaseController
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
        [DiscoAuthorize(Claims.Device.Actions.EnrolDevices)]
        public virtual ActionResult AddOffline()
        {
            var m = new Models.Device.AddOfflineModel()
            {
                DefaultDeviceProfileId = Database.DiscoConfiguration.DeviceProfiles.DefaultAddDeviceOfflineDeviceProfileId
            };

            if (Authorization.Has(Claims.Device.Properties.DeviceBatch))
                m.DeviceBatches = Database.DeviceBatches.ToList();

            if (Authorization.Has(Claims.Device.Properties.DeviceProfile))
            {
                m.DeviceProfiles = Database.DeviceProfiles.ToList();
                if (m.DefaultDeviceProfileId == 0)
                {
                    m.DeviceProfiles.Insert(0, new DeviceProfile() { Id = 0, Name = "Select a Device Profile" });
                }
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<DeviceAddOfflineModel>(this.ControllerContext, m);

            return View(m);
        }
        [DiscoAuthorize(Claims.Device.Actions.EnrolDevices), HttpPost]
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
                if (!string.IsNullOrEmpty(m.Device.SerialNumber) && Database.Devices.Count(d => d.SerialNumber == m.Device.SerialNumber) > 0)
                    ModelState.AddModelError("Device.SerialNumber", "A Device what this Serial Number already exists");
            }

            if (ModelState.IsValid)
            {
                var d = m.Device.AddOffline(Database);
                Database.SaveChanges();
                return RedirectToAction(MVC.Device.Show(d.SerialNumber));
            }
            return AddOffline();
        }
        #endregion

        #region Export

        [DiscoAuthorizeAny(Claims.Device.Actions.Export), HttpGet]
        public virtual ActionResult Export(string DownloadId, DeviceExportTypes? ExportType, int? ExportTypeTargetId)
        {
            var m = new Models.Device.ExportModel()
            {
                Options = Database.DiscoConfiguration.Devices.LastExportOptions,
                DeviceBatches = Database.DeviceBatches.OrderBy(db => db.Name).Select(db => new { Key = db.Id, Value = db.Name }).ToList().Select(i => new KeyValuePair<int, string>(i.Key, i.Value)),
                DeviceModels = Database.DeviceModels.OrderBy(dm => dm.Description).Select(dm => new { Key = dm.Id, Value = dm.Description }).ToList().Select(i => new KeyValuePair<int, string>(i.Key, i.Value)),
                DeviceProfiles = Database.DeviceProfiles.OrderBy(dp => dp.Name).Select(dp => new { Key = dp.Id, Value = dp.Name }).ToList().Select(i => new KeyValuePair<int, string>(i.Key, i.Value))
            };

            if (!string.IsNullOrWhiteSpace(DownloadId))
                m.DownloadExportSessionId = DownloadId;

            if (ExportType.HasValue && ExportTypeTargetId.HasValue)
            {
                m.Options.ExportType = ExportType.Value;
                m.Options.ExportTypeTargetId = ExportTypeTargetId.Value;
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<DeviceExportModel>(this.ControllerContext, m);

            return View(m);
        }

        #endregion

        #region Import/Export
        [DiscoAuthorizeAny(Claims.Device.Actions.Import, Claims.Device.Actions.Export), HttpGet]
        public virtual ActionResult ImportExport()
        {
            Models.Device.ImportModel m = new Models.Device.ImportModel();

            if (Authorization.Has(Claims.Device.Actions.Import))
            {
                m.DeviceModels = Database.DeviceModels.ToList();
                m.DeviceProfiles = Database.DeviceProfiles.ToList();
                m.DeviceBatches = Database.DeviceBatches.ToList();
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<DeviceImportModel>(this.ControllerContext, m);

            return View(m);
        }
        [DiscoAuthorize(Claims.Device.Actions.Import), HttpGet]
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
        [DiscoAuthorize(Claims.Device.Show)]
        public virtual ActionResult Show(string id)
        {
            var m = new Models.Device.ShowModel();

            Database.Configuration.LazyLoadingEnabled = true;

            m.Device = Database.Devices
                .Include("DeviceModel").Include("DeviceDetails").Include("DeviceUserAssignments.AssignedUser").Include("DeviceAttachments")
                .FirstOrDefault(d => d.SerialNumber == id);

            if (m.Device == null)
                throw new ArgumentException(string.Format("Unknown Device: [{0}]", id), "id");

            // No Necessary - Yet...
            //if (!string.IsNullOrWhiteSpace(m.Device.ComputerName))
            //{
            //    var adMachineAccount = BI.Interop.ActiveDirectory.ActiveDirectory.GetMachineAccount(m.Device.ComputerName);
            //    if (adMachineAccount != null)
            //    {
            //        m.OrganisationUnit = adMachineAccount.ParentDistinguishedName;
            //    }
            //}

            if (Authorization.Has(Claims.Device.Properties.DeviceProfile))
                m.DeviceProfiles = Database.DeviceProfiles.ToList();

            if (Authorization.Has(Claims.Device.Properties.DeviceBatch))
                m.DeviceBatches = Database.DeviceBatches.ToList();

            if (Authorization.Has(Claims.Device.ShowJobs))
            {
                m.Jobs = new JobTableModel()
                {
                    ShowStatus = true,
                    ShowDevice = false,
                    IsSmallTable = false,
                    HideClosedJobs = true,
                    EnablePaging = false
                };
                m.Jobs.Fill(Database, Disco.Services.Searching.Search.BuildJobTableModel(Database).Where(j => j.DeviceSerialNumber == m.Device.SerialNumber).OrderByDescending(j => j.Id), true);
            }

            if (Authorization.Has(Claims.Device.ShowCertificates))
                m.Certificates = Database.DeviceCertificates.Where(c => c.DeviceSerialNumber == m.Device.SerialNumber).ToList();

            if (Authorization.Has(Claims.Device.Actions.GenerateDocuments))
                m.DocumentTemplates = m.Device.AvailableDocumentTemplates(Database, UserService.CurrentUser, DateTime.Now);

            m.DeviceProfileDefaultOrganisationAddress = m.Device.DeviceProfile.DefaultOrganisationAddressDetails(Database);

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
