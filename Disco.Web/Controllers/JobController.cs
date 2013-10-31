using Disco.BI.Extensions;
using Disco.BI.JobBI;
using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.UI.Job;
using Disco.Services.Authorization;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Services.Plugins.Features.WarrantyProvider;
using Disco.Services.Users;
using Disco.Services.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Controllers
{
    public partial class JobController : AuthorizedDatabaseController
    {

        #region Index


        #region Managed Job Lists
        private static object jobListCreationLock = new object();

        private static ManagedJobList jobList_OpenJobs;
        private static ManagedJobList jobList_LongRunning;

        internal static ManagedJobList ReInitializeLongRunningJobList(DiscoDataContext Database)
        {
            if (jobList_LongRunning == null)
                return InitializeLongRunningJobList(Database);
            else
            {
                var longRunningThreshold = DateTime.Today.AddDays(Database.DiscoConfiguration.JobPreferences.LongRunningJobDaysThreshold * -1);

                return jobList_LongRunning.ReInitialize(Database, q => q.Where(j => j.ClosedDate == null && j.OpenedDate < longRunningThreshold));
            }
        }
        internal static ManagedJobList InitializeLongRunningJobList(DiscoDataContext Database)
        {
            if (jobList_LongRunning == null)
            {
                lock (jobListCreationLock)
                {
                    if (jobList_LongRunning == null)
                    {
                        var longRunningThreshold = DateTime.Today.AddDays(Database.DiscoConfiguration.JobPreferences.LongRunningJobDaysThreshold * -1);

                        jobList_LongRunning = new ManagedJobList()
                        {
                            Name = "Long Running Jobs",
                            FilterFunction = q => q.Where(j => j.ClosedDate == null && j.OpenedDate < longRunningThreshold),
                            SortFunction = q => q.OrderBy(j => j.Id),
                            ShowStatus = true
                        }.Initialize(Database);
                    }
                }
            }
            return jobList_LongRunning;
        }
        internal static ManagedJobList InitializeOpenJobList(DiscoDataContext Database)
        {
            if (jobList_OpenJobs == null)
            {
                lock (jobListCreationLock)
                {
                    if (jobList_OpenJobs == null)
                    {
                        jobList_OpenJobs = new ManagedJobList()
                        {
                            Name = "Open Jobs Awaiting Technician Action",
                            FilterFunction = q => q.Where(j => j.ClosedDate == null && !j.WaitingForUserAction.HasValue
                                && !(j.JobTypeId == JobType.JobTypeIds.HNWar && (j.JobMetaNonWarranty.RepairerLoggedDate.HasValue && j.JobMetaNonWarranty.IsInsuranceClaim && !j.JobMetaInsurance.ClaimFormSentDate.HasValue))
                                && !(j.JobTypeId == JobType.JobTypeIds.HNWar && (j.JobMetaNonWarranty.RepairerLoggedDate.HasValue && !j.JobMetaNonWarranty.RepairerCompletedDate.HasValue))
                                && !(j.JobTypeId == JobType.JobTypeIds.HNWar && (j.JobMetaNonWarranty.RepairerLoggedDate.HasValue && j.JobMetaNonWarranty.AccountingChargeAddedDate.HasValue && !j.JobMetaNonWarranty.AccountingChargePaidDate.HasValue))
                                && !(j.JobTypeId == JobType.JobTypeIds.HWar && (j.JobMetaWarranty.ExternalLoggedDate.HasValue && !j.JobMetaWarranty.ExternalCompletedDate.HasValue))
                                && (j.DeviceHeld == null || j.DeviceReturnedDate != null || j.DeviceReadyForReturn == null)),
                            SortFunction = q => q.OrderBy(j => j.Id),
                            ShowStatus = true
                        }.Initialize(Database);
                    }
                }
            }
            return jobList_OpenJobs;
        }
        #endregion

        public virtual ActionResult Index()
        {
            var m = new Models.Job.IndexModel();

            InitializeOpenJobList(Database);
            InitializeLongRunningJobList(Database);

            if (Authorization.Has(Claims.Job.Lists.AwaitingTechnicianAction))
                m.OpenJobs = jobList_OpenJobs;
            if (Authorization.Has(Claims.Job.Lists.LongRunningJobs))
                m.LongRunningJobs = jobList_LongRunning;
            if (Authorization.Has(Claims.Job.ShowDailyChart))
                m.DailyOpenedClosedStatistics = Disco.BI.JobBI.Statistics.DailyOpenedClosed.Data(Database, true);

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobIndexModel>(this.ControllerContext, m);

            return View(m);
        }
        #endregion

        #region Lists
        [DiscoAuthorize(Claims.Job.Lists.AllOpen)]
        public virtual ActionResult AllOpen()
        {
            Database.Configuration.LazyLoadingEnabled = true;
            var m = new Models.Job.ListModel() { Title = "All Open Jobs" };
            m.JobTable = new Disco.Models.BI.Job.JobTableModel() { ShowStatus = true };
            m.JobTable.Fill(Database, BI.JobBI.Searching.BuildJobTableModel(Database).Where(j => j.ClosedDate == null).OrderBy(j => j.Id));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        [DiscoAuthorize(Claims.Job.Lists.DevicesReadyForReturn)]
        public virtual ActionResult DevicesReadyForReturn()
        {
            Database.Configuration.LazyLoadingEnabled = true;
            var m = new Models.Job.ListModel() { Title = "Jobs with Devices Ready for Return" };
            m.JobTable = new Disco.Models.BI.Job.JobTableModel() { ShowStatus = true };
            m.JobTable.Fill(Database, BI.JobBI.Searching.BuildJobTableModel(Database).Where(j => !j.WaitingForUserAction.HasValue
                && j.DeviceHeld != null && j.DeviceReturnedDate == null && j.DeviceReadyForReturn != null &&
                ((!j.JobMetaNonWarranty.AccountingChargeRequiredDate.HasValue && !j.JobMetaNonWarranty.AccountingChargeAddedDate.HasValue) || j.JobMetaNonWarranty.AccountingChargePaidDate.HasValue)
                && j.ClosedDate == null).OrderBy(j => j.Id));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        [DiscoAuthorize(Claims.Job.Lists.DevicesAwaitingRepair)]
        public virtual ActionResult DevicesAwaitingRepair()
        {
            Database.Configuration.LazyLoadingEnabled = true;
            var m = new Models.Job.ListModel() { Title = "Jobs with Devices Awaiting Repair" };
            m.JobTable = new Disco.Models.BI.Job.JobTableModel() { ShowStatus = true };
            m.JobTable.Fill(Database, BI.JobBI.Searching.BuildJobTableModel(Database).Where(j => j.ClosedDate == null &&
                (
                (j.JobMetaNonWarranty.RepairerLoggedDate != null && j.JobMetaNonWarranty.RepairerCompletedDate == null) ||
                (j.JobMetaWarranty.ExternalLoggedDate != null && j.JobMetaWarranty.ExternalCompletedDate == null)
                )).OrderBy(j => j.Id));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        #region "Finance Lists"
        [DiscoAuthorize(Claims.Job.Lists.AwaitingFinance)]
        public virtual ActionResult AwaitingFinance()
        {
            Database.Configuration.LazyLoadingEnabled = true;
            var m = new Models.Job.ListModel() { Title = "Jobs Awaiting Finance" };
            m.JobTable = new Disco.Models.BI.Job.JobTableModel() { ShowStatus = true };
            m.JobTable.Fill(Database, BI.JobBI.Searching.BuildJobTableModel(Database).Where(j => j.ClosedDate == null &&
                (
                (j.JobTypeId == JobType.JobTypeIds.HNWar && (j.JobMetaNonWarranty.IsInsuranceClaim && !j.JobMetaInsurance.ClaimFormSentDate.HasValue)) ||
                (j.JobTypeId == JobType.JobTypeIds.HNWar && (j.JobMetaNonWarranty.AccountingChargeRequiredDate.HasValue && (!j.JobMetaNonWarranty.AccountingChargeAddedDate.HasValue && !j.JobMetaNonWarranty.AccountingChargePaidDate.HasValue))) ||
                (j.JobTypeId == JobType.JobTypeIds.HNWar && (!j.JobMetaNonWarranty.AccountingChargeAddedDate.HasValue || !j.JobMetaNonWarranty.AccountingChargePaidDate.HasValue)) ||
                (j.JobTypeId == JobType.JobTypeIds.UMgmt && Job.UserManagementFlags.Infringement_BreachFinancialAgreement == (j.Flags & Job.UserManagementFlags.Infringement_BreachFinancialAgreement))
                )));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        [DiscoAuthorizeAll(Claims.Job.Lists.AwaitingFinance, Claims.Job.Lists.AwaitingFinanceCharge)]
        public virtual ActionResult AwaitingFinanceCharge()
        {
            Database.Configuration.LazyLoadingEnabled = true;
            var m = new Models.Job.ListModel() { Title = "Jobs Awaiting Finance - Accounting Charge" };
            m.JobTable = new Disco.Models.BI.Job.JobTableModel() { ShowStatus = true };
            m.JobTable.Fill(Database, BI.JobBI.Searching.BuildJobTableModel(Database).Where(j => j.ClosedDate == null &&
                (j.JobTypeId == JobType.JobTypeIds.HNWar && (j.JobMetaNonWarranty.AccountingChargeRequiredDate.HasValue && (!j.JobMetaNonWarranty.AccountingChargeAddedDate.HasValue && !j.JobMetaNonWarranty.AccountingChargePaidDate.HasValue)))
                ).OrderBy(j => j.Id));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        [DiscoAuthorizeAll(Claims.Job.Lists.AwaitingFinance, Claims.Job.Lists.AwaitingFinancePayment)]
        public virtual ActionResult AwaitingFinancePayment()
        {
            Database.Configuration.LazyLoadingEnabled = true;
            var m = new Models.Job.ListModel() { Title = "Jobs Awaiting Finance - Accounting Payment" };
            m.JobTable = new Disco.Models.BI.Job.JobTableModel() { ShowStatus = true };
            m.JobTable.Fill(Database, BI.JobBI.Searching.BuildJobTableModel(Database).Where(j => j.ClosedDate == null &&
                (j.JobTypeId == JobType.JobTypeIds.HNWar && (!j.JobMetaNonWarranty.AccountingChargeAddedDate.HasValue || !j.JobMetaNonWarranty.AccountingChargePaidDate.HasValue))
                ).OrderBy(j => j.Id));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        [DiscoAuthorizeAll(Claims.Job.Lists.AwaitingFinance, Claims.Job.Lists.AwaitingFinanceInsuranceProcessing)]
        public virtual ActionResult AwaitingFinanceInsuranceProcessing()
        {
            Database.Configuration.LazyLoadingEnabled = true;
            var m = new Models.Job.ListModel() { Title = "Jobs Awaiting Finance - Insurance Processing" };
            m.JobTable = new Disco.Models.BI.Job.JobTableModel() { ShowStatus = true };
            m.JobTable.Fill(Database, BI.JobBI.Searching.BuildJobTableModel(Database).Where(j => j.ClosedDate == null &&
                (j.JobTypeId == JobType.JobTypeIds.HNWar && (j.JobMetaNonWarranty.IsInsuranceClaim && !j.JobMetaInsurance.ClaimFormSentDate.HasValue))
                ).OrderBy(j => j.Id));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        [DiscoAuthorizeAll(Claims.Job.Lists.AwaitingFinance, Claims.Job.Lists.AwaitingFinanceAgreementBreach)]
        public virtual ActionResult AwaitingFinanceAgreementBreach()
        {
            Database.Configuration.LazyLoadingEnabled = true;
            var m = new Models.Job.ListModel() { Title = "Jobs Awaiting Finance - Agreement Breach" };
            m.JobTable = new Disco.Models.BI.Job.JobTableModel() { ShowStatus = true };
            m.JobTable.Fill(Database, BI.JobBI.Searching.BuildJobTableModel(Database).Where(j => j.ClosedDate == null &&
                (j.JobTypeId == JobType.JobTypeIds.UMgmt && Job.UserManagementFlags.Infringement_BreachFinancialAgreement == (j.Flags & Job.UserManagementFlags.Infringement_BreachFinancialAgreement))
                ).OrderBy(j => j.Id));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }
        #endregion

        [DiscoAuthorize(Claims.Job.Lists.AwaitingUserAction)]
        public virtual ActionResult AwaitingUserAction()
        {
            Database.Configuration.LazyLoadingEnabled = true;
            var m = new Models.Job.ListModel() { Title = "Jobs Awaiting User Action" };
            m.JobTable = new Disco.Models.BI.Job.JobTableModel() { ShowStatus = true };
            m.JobTable.Fill(Database, BI.JobBI.Searching.BuildJobTableModel(Database).Where(j => (j.WaitingForUserAction.HasValue || (j.JobMetaNonWarranty.AccountingChargeAddedDate != null && j.JobMetaNonWarranty.AccountingChargePaidDate == null))
                                                                          && j.ClosedDate == null).OrderBy(j => j.Id));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        [DiscoAuthorize(Claims.Job.Lists.RecentlyClosed)]
        public virtual ActionResult RecentlyClosed()
        {
            Database.Configuration.LazyLoadingEnabled = true;
            var m = new Models.Job.ListModel() { Title = "Recently Closed Jobs" };
            m.JobTable = new Disco.Models.BI.Job.JobTableModel() { ShowStatus = true };

            var dateTimeNow = DateTime.Now;
            var closedThreshold = dateTimeNow.AddDays(-2);
            if (dateTimeNow.DayOfWeek == DayOfWeek.Monday)
                closedThreshold = closedThreshold.AddDays(-2);
            if (dateTimeNow.DayOfWeek == DayOfWeek.Tuesday)
                closedThreshold = closedThreshold.AddDays(-1);
            m.JobTable.Fill(Database, BI.JobBI.Searching.BuildJobTableModel(Database).Where(j => j.ClosedDate > closedThreshold).OrderBy(j => j.Id));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        [DiscoAuthorize(Claims.Job.Lists.Locations)]
        public virtual ActionResult Locations()
        {
            Database.Configuration.LazyLoadingEnabled = true;
            var m = new Models.Job.ListModel() { Title = "Held Device Locations" };
            m.JobTable = new Disco.Models.BI.Job.JobTableModel() { ShowStatus = true, ShowLocation = true, ShowTechnician = false, ShowType = false };
            m.JobTable.Fill(Database, BI.JobBI.Searching.BuildJobTableModel(Database).Where(j => j.ClosedDate == null && j.DeviceHeld.HasValue && !j.DeviceReturnedDate.HasValue).OrderBy(j => j.DeviceHeldLocation));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        #endregion

        #region Show
        [DiscoAuthorize(Claims.Job.Show)]
        public virtual ActionResult Show(int? id)
        {
            if (!id.HasValue)
                return RedirectToAction(MVC.Job.Index());

            Database.Configuration.LazyLoadingEnabled = true;

            var m = new Models.Job.ShowModel();

            m.Job = (from j in Database.Jobs.Include("Device.DeviceModel").Include("Device.DeviceBatch").Include("DeviceHeldTechUser").Include("DeviceReadyForReturnTechUser").Include("DeviceReturnedTechUser")
                         .Include("OpenedTechUser").Include("ClosedTechUser").Include("JobType").Include("JobSubTypes").Include("User").Include("JobLogs.TechUser")
                     where (j.Id == id.Value)
                     select j).FirstOrDefault();

            if (m.Job == null)
                throw new ArgumentException(string.Format("Unknown Job: [{0}]", id), "id");

            // Validate Authorization
            switch (m.Job.JobTypeId)
            {
                case JobType.JobTypeIds.HMisc:
                    Authorization.Require(Claims.Job.Types.ShowHMisc);
                    break;
                case JobType.JobTypeIds.HNWar:
                    Authorization.Require(Claims.Job.Types.ShowHNWar);
                    break;
                case JobType.JobTypeIds.HWar:
                    Authorization.Require(Claims.Job.Types.ShowHWar);
                    break;
                case JobType.JobTypeIds.SApp:
                    Authorization.Require(Claims.Job.Types.ShowSApp);
                    break;
                case JobType.JobTypeIds.SImg:
                    Authorization.Require(Claims.Job.Types.ShowSImg);
                    break;
                case JobType.JobTypeIds.SOS:
                    Authorization.Require(Claims.Job.Types.ShowSOS);
                    break;
                case JobType.JobTypeIds.UMgmt:
                    Authorization.Require(Claims.Job.Types.ShowUMgmt);
                    break;
                default:
                    throw new InvalidOperationException("Unknown JobType");
            }

            m.IsLongRunning =
                (!m.Job.ClosedDate.HasValue && m.Job.OpenedDate < DateTime.Today.AddDays(Database.DiscoConfiguration.JobPreferences.LongRunningJobDaysThreshold * -1)) ||
                (m.Job.ClosedDate.HasValue && m.Job.OpenedDate.AddDays(Database.DiscoConfiguration.JobPreferences.LongRunningJobDaysThreshold) < m.Job.ClosedDate.Value);

            if (Authorization.Has(Claims.Job.Actions.UpdateSubTypes))
                m.UpdatableJobSubTypes = m.Job.JobType.JobSubTypes.OrderBy(jst => jst.Description).ToList();

            if (Authorization.Has(Claims.Job.Actions.GenerateDocuments))
                m.AvailableDocumentTemplates = m.Job.AvailableDocumentTemplates(Database, UserService.CurrentUser, DateTime.Now);

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobShowModel>(this.ControllerContext, m);

            return View(m);
        }
        #endregion

        #region Create
        [DiscoAuthorize(Claims.Job.Actions.Create)]
        public virtual ActionResult Create(string DeviceSerialNumber, string UserId)
        {
            var m = new Models.Job.CreateModel()
            {
                DeviceSerialNumber = DeviceSerialNumber,
                UserId = UserId
            };
            m.UpdateModel(Database);

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobCreateModel>(this.ControllerContext, m);

            return View(m);
        }
        [HttpPost, DiscoAuthorize(Claims.Job.Actions.Create)]
        public virtual ActionResult Create(Models.Job.CreateModel m)
        {
            m.UpdateModel(Database);

            if (!ModelState.IsValid)
            {
                // UI Extensions
                UIExtensions.ExecuteExtensions<JobCreateModel>(this.ControllerContext, m);

                return View(m);
            }
            else
            {
                // Create New Job
                var currentUser = Database.Users.Find(UserService.CurrentUserId);
                var j = BI.JobBI.Utilities.Create(Database, m.Device, m.User, m.GetJobType, m.GetJobSubTypes, currentUser);

                if (m.DeviceHeld.Value)
                {
                    j.OnDeviceHeld(currentUser);
                    m.QuickLog = false;
                }
                else
                {
                    if (m.QuickLog.HasValue && m.QuickLog.Value && m.QuickLogTaskTimeMinutes.HasValue && m.QuickLogTaskTimeMinutes.Value > 0)
                    {
                        // Quick Log
                        // Set Opened Date in the past
                        j.OpenedDate = DateTime.Now.AddMinutes(-1 * m.QuickLogTaskTimeMinutes.Value);
                        // Close Job
                        j.OnClose(currentUser);
                    }
                    else
                    {
                        m.QuickLog = false;
                    }
                }

                // Add Comments
                if (!string.IsNullOrWhiteSpace(m.Comments))
                {
                    var jl = new Disco.Models.Repository.JobLog()
                    {
                        Job = j,
                        TechUser = currentUser,
                        Timestamp = DateTime.Now,
                        Comments = m.Comments
                    };
                    Database.JobLogs.Add(jl);
                }

                Database.SaveChanges();

                // Return Dialog Redirect
                var redirectModel = new Models.Job.CreateRedirectModel();
                if (m.QuickLog.HasValue && m.QuickLog.Value && !string.IsNullOrWhiteSpace(m.QuickLogDestinationUrl))
                    redirectModel.RedirectLink = m.QuickLogDestinationUrl;
                else
                    redirectModel.RedirectLink = Url.Action(MVC.Job.Show(j.Id));

                return View(Views.Create_Redirect, redirectModel);
            }
        }
        #endregion

        #region Log Warranty
        [DiscoAuthorize(Claims.Job.Actions.LogWarranty)]
        public virtual ActionResult LogWarranty(int id, string WarrantyProviderId, int? OrganisationAddressId)
        {
            var m = new Models.Job.LogWarrantyModel() { JobId = id, WarrantyProviderId = WarrantyProviderId, OrganisationAddressId = OrganisationAddressId };
            m.UpdateModel(Database, false);
            m.FaultDescription = m.Job.GenerateFaultDescription(Database);

            if (m.WarrantyProvider != null)
            {
                using (var wp = m.WarrantyProvider.CreateInstance<WarrantyProviderFeature>())
                {
                    if (wp.SubmitJobViewType != null)
                    {
                        m.WarrantyProviderSubmitJobViewType = wp.SubmitJobViewType;
                        m.WarrantyProviderSubmitJobModel = wp.SubmitJobViewModel(Database, this, m.Job, m.OrganisationAddress, m.TechUser);
                    }
                }
            }

            return View(m);
        }
        [HttpPost, DiscoAuthorize(Claims.Job.Actions.LogWarranty)]
        public virtual ActionResult LogWarranty(Models.Job.LogWarrantyModel m, FormCollection form)
        {
            m.UpdateModel(Database, true);

            if (ModelState.IsValid)
            {
                switch (m.WarrantyAction)
                {
                    case "Disclose":
                        using (var p = m.WarrantyProvider.CreateInstance<WarrantyProviderFeature>())
                        {
                            Dictionary<string, string> warrantyProviderProperties;
                            try
                            {
                                warrantyProviderProperties = p.SubmitJobParseProperties(Database, form, this, m.Job, m.OrganisationAddress, m.TechUser, m.FaultDescription);
                            }
                            catch (Exception ex)
                            {
                                m.Error = ex;
                                return View(Views.LogWarrantyError, m);
                            }
                            if (!ModelState.IsValid)
                                return View(Views.LogWarranty, m);

                            if (warrantyProviderProperties != null)
                            {
                                m.WarrantyProviderPropertiesJson = JsonConvert.SerializeObject(warrantyProviderProperties);
                            }
                            m.DiscloseProperties = p.SubmitJobDiscloseInfo(Database, m.Job, m.OrganisationAddress, m.TechUser, m.FaultDescription, warrantyProviderProperties);
                            return View(Views.LogWarrantyDisclose, m);
                        }
                    case "Submit":
                        try
                        {
                            m.Job.OnLogWarranty(Database, m.FaultDescription, m.WarrantyProvider, m.OrganisationAddress, m.TechUser, m.WarrantyProviderProperties());
                            Database.SaveChanges();
                            return RedirectToAction(MVC.Job.Show(m.JobId));
                        }
                        catch (Exception ex)
                        {
                            m.Error = ex;
                            return View(Views.LogWarrantyError, m);
                            throw;
                        }
                    default:
                        return RedirectToAction(MVC.Job.Show(m.JobId));
                }

            }
            else
            {
                return View(Views.LogWarranty, m);
            }
        }

        [DiscoAuthorize(Claims.Job.Properties.WarrantyProperties.ProviderDetails)]
        public virtual ActionResult WarrantyProviderJobDetails(int id)
        {
            Models.Job.WarrantyProviderJobDetailsModel model = new Models.Job.WarrantyProviderJobDetailsModel();

            Job job = Database.Jobs.Include("Device.DeviceModel").Include("JobMetaWarranty").Include("JobSubTypes").Where(j => j.Id == id).FirstOrDefault();
            if (job != null)
            {
                if (job.JobMetaWarranty != null && !string.IsNullOrEmpty(job.JobMetaWarranty.ExternalName))
                {
                    var providerDef = WarrantyProviderFeature.FindPluginFeature(job.JobMetaWarranty.ExternalName);

                    if (providerDef != null)
                    {
                        using (WarrantyProviderFeature providerInstance = providerDef.CreateInstance<WarrantyProviderFeature>())
                        {
                            if (providerInstance.JobDetailsSupported)
                            {
                                try
                                {
                                    object providerModel = providerInstance.JobDetailsViewModel(Database, this, job);

                                    model.JobDetailsSupported = true;
                                    model.ViewType = providerInstance.JobDetailsViewType;
                                    model.ViewModel = providerModel;
                                    return View(model);
                                }
                                catch (Exception ex)
                                {
                                    model.JobDetailsSupported = false;
                                    model.JobDetailsException = ex;
                                    return View(model);
                                }
                            }
                            else
                            {
                                model.JobDetailsSupported = false;
                                model.JobDetailsNotSupportedMessage = string.Format("Plugin '{0} ({1})' (Warranty Provider for '{2}') doesn't support Job Details", providerInstance.Manifest.Name, providerInstance.Manifest.Id, providerInstance.WarrantyProviderId);
                                return View(model);
                            }
                        }
                    }

                    model.JobDetailsSupported = false;
                    model.JobDetailsNotSupportedMessage = string.Format("Warranty Provider '{0}' is not integrated with Disco", job.JobMetaWarranty.ExternalName);
                    return View(model);
                }
                else
                {
                    model.JobDetailsSupported = false;
                    model.JobDetailsNotSupportedMessage = "Job not in the correct state";
                    return View(model);
                }
            }
            else
            {
                return HttpNotFound("Invalid Job Id");
            }
        }
        #endregion

    }
}
