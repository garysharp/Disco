using Disco.Models.Repository;
using Disco.Models.Services.Devices.Exporting;
using Disco.Models.Services.Jobs.JobLists;
using Disco.Models.UI.Device;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Devices.Exporting;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Plugins.Features.DetailsProvider;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Services.Users;
using Disco.Services.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
            else if (m.Device.SerialNumber.Contains("/") || m.Device.SerialNumber.Contains(@"\"))
            {
                ModelState.AddModelError("Device.SerialNumber", @"The Serial Number cannot contain '/' or '\' characters");
            }
            else
            {
                // Ensure Existing Device Doesn't Exist
                if (!string.IsNullOrEmpty(m.Device.SerialNumber) && Database.Devices.Count(d => d.SerialNumber == m.Device.SerialNumber) > 0)
                    ModelState.AddModelError("Device.SerialNumber", "A Device what this Serial Number already exists");
            }
            if (string.IsNullOrWhiteSpace(m.Device.DeviceDomainId))
                m.Device.DeviceDomainId = null;
            if (m.Device.DeviceDomainId != null)
            {
                try
                {
                    m.Device.DeviceDomainId = ActiveDirectory.ParseDomainAccountId(m.Device.DeviceDomainId);
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("Device.DeviceDomainId", ex.Message);
                }
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
                DeviceBatches = new KeyValuePair<int, string>[] { new KeyValuePair<int, string>(0, "<No Associated Batch>") }.Concat(Database.DeviceBatches.OrderBy(db => db.Name).Select(db => new { Key = db.Id, Value = db.Name }).ToList().Select(i => new KeyValuePair<int, string>(i.Key, i.Value))),
                DeviceModels = Database.DeviceModels.OrderBy(dm => dm.Description).Select(dm => new { Key = dm.Id, Value = dm.Description }).ToList().Select(i => new KeyValuePair<int, string>(i.Key, i.Value)),
                DeviceProfiles = Database.DeviceProfiles.OrderBy(dp => dp.Name).Select(dp => new { Key = dp.Id, Value = dp.Name }).ToList().Select(i => new KeyValuePair<int, string>(i.Key, i.Value))
            };

            if (!string.IsNullOrWhiteSpace(DownloadId))
            {
                string key = string.Format(Areas.API.Controllers.DeviceController.ExportSessionCacheKey, DownloadId);
                var context = HttpRuntime.Cache.Get(key) as DeviceExportTaskContext;

                if (context != null)
                {
                    m.ExportSessionResult = context.Result;
                    m.ExportSessionId = DownloadId;
                }
            }

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

        #region Import
        [DiscoAuthorize(Claims.Device.Actions.Import), HttpGet]
        public virtual ActionResult Import(string Id)
        {
            var m = new Models.Device.ImportModel()
            {
                DeviceModels = Database.DeviceModels.ToList(),
                DeviceProfiles = Database.DeviceProfiles.ToList(),
                DeviceBatches = Database.DeviceBatches.ToList()
            };

            if (!string.IsNullOrWhiteSpace(Id))
                m.CompletedImportSessionContext = Areas.API.Controllers.DeviceController.Import_RetrieveContext(Id, Remove: true);

            // UI Extensions
            UIExtensions.ExecuteExtensions<DeviceImportModel>(this.ControllerContext, m);

            return View(m);
        }

        [DiscoAuthorize(Claims.Device.Actions.Import), HttpGet]
        public virtual ActionResult ImportHeaders(string Id)
        {
            if (string.IsNullOrWhiteSpace(Id))
                throw new ArgumentNullException("Id");

            var context = Areas.API.Controllers.DeviceController.Import_RetrieveContext(Id);

            if (context == null)
                throw new ArgumentException("The Import Session Id is invalid or the session timed out (60 minutes), try importing again", "Id");

            var m = new Models.Device.ImportHeadersModel()
            {
                Context = context
            };

            // UI Extensions
            UIExtensions.ExecuteExtensions<DeviceImportHeadersModel>(this.ControllerContext, m);

            return View(m);
        }

        [DiscoAuthorize(Claims.Device.Actions.Import), HttpGet]
        public virtual ActionResult ImportReview(string Id)
        {
            if (string.IsNullOrWhiteSpace(Id))
                throw new ArgumentNullException("Id");

            var context = Areas.API.Controllers.DeviceController.Import_RetrieveContext(Id);

            if (context == null)
                throw new ArgumentException("The Import Session Id is invalid or the session timed out (60 minutes), try importing again", "Id");

            var m = new Models.Device.ImportReviewModel()
            {
                Context = context,
                StatisticErrorRecords = context.Records.Count(r => r.HasError),
                StatisticNewRecords = context.Records.Count(r => r.RecordAction == System.Data.EntityState.Added),
                StatisticModifiedRecords = context.Records.Count(r => r.RecordAction == System.Data.EntityState.Modified),
                StatisticUnmodifiedRecords = context.Records.Count(r => r.RecordAction == System.Data.EntityState.Unchanged)
            };

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
                .Include("DeviceModel")
                .Include("DeviceProfile")
                .Include("DeviceBatch")
                .Include("DeviceDetails")
                .Include("DeviceUserAssignments.AssignedUser.UserFlagAssignments")
                .Include("AssignedUser.UserFlagAssignments")
                .Include("AssignedUser.UserDetails")
                .Include("DeviceCertificates")
                .Include("DeviceAttachments.TechUser")
                .Include("DeviceAttachments.DocumentTemplate")
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
            {
                m.DocumentTemplates = m.Device.AvailableDocumentTemplates(Database, UserService.CurrentUser, DateTime.Now);
                m.DocumentTemplatePackages = m.Device.AvailableDocumentTemplatePackages(Database, UserService.CurrentUser);
            }

            m.DeviceProfileDefaultOrganisationAddress = m.Device.DeviceProfile.DefaultOrganisationAddressDetails(Database);

            if (m.Device.DeviceProfile.CertificateProviders != null)
            {
                m.DeviceProfileCertificateProviders = m.Device.DeviceProfile.GetCertificateProviders().ToList();
            }
            if (m.Device.DeviceProfile.WirelessProfileProviders != null)
            {
                m.DeviceProfileWirelessProfileProviders = m.Device.DeviceProfile.GetWirelessProfileProviders().ToList();
            }

            if (Authorization.Has(Claims.User.ShowDetails))
            {
                // Populate Custom Details
                m.PopulateDetails(Database);
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<DeviceShowModel>(this.ControllerContext, m);

            return View(m);
        }
        #endregion
    }
}
