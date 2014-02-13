using Disco.BI.Extensions;
using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Jobs.JobLists;
using Disco.Models.UI.Job;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Jobs.JobLists;
using Disco.Services.Jobs.JobQueues;
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

        public virtual ActionResult Index()
        {
            var m = new Models.Job.IndexModel();

            if (Authorization.Has(Claims.Job.Lists.MyJobs))
                m.MyJobs = ManagedJobList.MyJobsTable(Authorization);

            if (Authorization.Has(Claims.Job.Lists.StaleJobs))
            {
                var staleThreshold = DateTime.Today.AddMinutes(Database.DiscoConfiguration.JobPreferences.StaleJobMinutesThreshold * -1);
                m.StaleJobs = ManagedJobList.OpenJobsTable(q => q.Where(j => j.LastActivityDate < staleThreshold).OrderBy(j => j.LastActivityDate));
                m.StaleJobs.ShowLastActivityDate = true;
                m.StaleJobs.ShowDates = false;
            }
            if (Authorization.Has(Claims.Job.ShowDailyChart))
                m.DailyOpenedClosedStatistics = Disco.BI.JobBI.Statistics.DailyOpenedClosed.Data(Database, true);

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobIndexModel>(this.ControllerContext, m);

            return View(m);
        }
        #endregion

        #region Lists
        [DiscoAuthorize(Claims.Job.Lists.JobQueueLists)]
        public virtual ActionResult Queue(int id)
        {
            var queueToken = JobQueueService.GetQueue(id);

            if (queueToken == null)
                throw new ArgumentException("Invalid Job Queue Id", "id");

            var m = new Models.Job.ListModel() { Title = string.Format("Queue: {0}", queueToken.JobQueue.Name) };
            m.JobTable = ManagedJobList.OpenJobsTable(q => q.Where(j => j.ActiveJobQueues.Any(jqj => jqj.QueueId == queueToken.JobQueue.Id)));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        [DiscoAuthorize(Claims.Job.Lists.AllOpen)]
        public virtual ActionResult AllOpen()
        {
            var m = new Models.Job.ListModel() { Title = "All Open Jobs" };
            m.JobTable = ManagedJobList.OpenJobsTable(q => q.OrderBy(j => j.JobId));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        [DiscoAuthorize(Claims.Job.Lists.AwaitingTechnicianAction)]
        public virtual ActionResult AwaitingTechnicianAction()
        {
            var m = new Models.Job.ListModel() { Title = "Jobs Awaiting Technician Action" };

            m.JobTable = ManagedJobList.OpenJobsTable(ManagedJobList.AwaitingTechnicianActionFilter);

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        [DiscoAuthorize(Claims.Job.Lists.DevicesReadyForReturn)]
        public virtual ActionResult DevicesReadyForReturn()
        {
            var m = new Models.Job.ListModel() { Title = "Jobs with Devices Ready for Return" };

            m.JobTable = ManagedJobList.OpenJobsTable(q => q.Where(j => !j.WaitingForUserAction.HasValue
                && j.DeviceHeld != null && j.DeviceReturnedDate == null && j.DeviceReadyForReturn != null &&
                ((!j.JobMetaNonWarranty_AccountingChargeRequiredDate.HasValue && !j.JobMetaNonWarranty_AccountingChargeAddedDate.HasValue) || j.JobMetaNonWarranty_AccountingChargePaidDate.HasValue))
                .OrderBy(j => j.JobId));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        [DiscoAuthorize(Claims.Job.Lists.DevicesAwaitingRepair)]
        public virtual ActionResult DevicesAwaitingRepair()
        {
            var m = new Models.Job.ListModel() { Title = "Jobs with Devices Awaiting Repair" };

            m.JobTable = ManagedJobList.OpenJobsTable(q => q.Where(j =>
                (j.JobMetaNonWarranty_RepairerLoggedDate != null && j.JobMetaNonWarranty_RepairerCompletedDate == null) ||
                (j.JobMetaWarranty_ExternalLoggedDate != null && j.JobMetaWarranty_ExternalCompletedDate == null)
                ).OrderBy(j => j.JobId));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        #region "Finance Lists"
        [DiscoAuthorize(Claims.Job.Lists.AwaitingFinance)]
        public virtual ActionResult AwaitingFinance()
        {
            var m = new Models.Job.ListModel() { Title = "Jobs Awaiting Finance" };
            m.JobTable = ManagedJobList.OpenJobsTable(q => q.Where(j =>
                (j.JobTypeId == JobType.JobTypeIds.HNWar && (j.JobMetaNonWarranty_IsInsuranceClaim.Value && !j.JobMetaInsurance_ClaimFormSentDate.HasValue)) ||
                (j.JobTypeId == JobType.JobTypeIds.HNWar && (j.JobMetaNonWarranty_AccountingChargeRequiredDate.HasValue && (!j.JobMetaNonWarranty_AccountingChargeAddedDate.HasValue && !j.JobMetaNonWarranty_AccountingChargePaidDate.HasValue))) ||
                (j.JobTypeId == JobType.JobTypeIds.HNWar && (!j.JobMetaNonWarranty_AccountingChargeAddedDate.HasValue || !j.JobMetaNonWarranty_AccountingChargePaidDate.HasValue)) ||
                (j.JobTypeId == JobType.JobTypeIds.UMgmt && Job.UserManagementFlags.Infringement_BreachFinancialAgreement == (j.Flags & Job.UserManagementFlags.Infringement_BreachFinancialAgreement))
                ));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        [DiscoAuthorizeAll(Claims.Job.Lists.AwaitingFinance, Claims.Job.Lists.AwaitingFinanceCharge)]
        public virtual ActionResult AwaitingFinanceCharge()
        {
            var m = new Models.Job.ListModel() { Title = "Jobs Awaiting Finance - Accounting Charge" };
            m.JobTable = ManagedJobList.OpenJobsTable(q => q.Where(j =>
                j.JobTypeId == JobType.JobTypeIds.HNWar && (j.JobMetaNonWarranty_AccountingChargeRequiredDate.HasValue && (!j.JobMetaNonWarranty_AccountingChargeAddedDate.HasValue && !j.JobMetaNonWarranty_AccountingChargePaidDate.HasValue))
                ).OrderBy(j => j.JobId));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        [DiscoAuthorizeAll(Claims.Job.Lists.AwaitingFinance, Claims.Job.Lists.AwaitingFinancePayment)]
        public virtual ActionResult AwaitingFinancePayment()
        {
            var m = new Models.Job.ListModel() { Title = "Jobs Awaiting Finance - Accounting Payment" };
            m.JobTable = ManagedJobList.OpenJobsTable(q => q.Where(j =>
                j.JobTypeId == JobType.JobTypeIds.HNWar && ((j.JobMetaNonWarranty_AccountingChargeRequiredDate.HasValue || j.JobMetaNonWarranty_AccountingChargeAddedDate.HasValue) && !j.JobMetaNonWarranty_AccountingChargePaidDate.HasValue)
                ).OrderBy(j => j.JobId));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        [DiscoAuthorizeAll(Claims.Job.Lists.AwaitingFinance, Claims.Job.Lists.AwaitingFinanceInsuranceProcessing)]
        public virtual ActionResult AwaitingFinanceInsuranceProcessing()
        {
            var m = new Models.Job.ListModel() { Title = "Jobs Awaiting Finance - Insurance Processing" };
            m.JobTable = ManagedJobList.OpenJobsTable(q => q.Where(j =>
                j.JobTypeId == JobType.JobTypeIds.HNWar && (j.JobMetaNonWarranty_IsInsuranceClaim.Value && !j.JobMetaInsurance_ClaimFormSentDate.HasValue)
                ).OrderBy(j => j.JobId));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        [DiscoAuthorizeAll(Claims.Job.Lists.AwaitingFinance, Claims.Job.Lists.AwaitingFinanceAgreementBreach)]
        public virtual ActionResult AwaitingFinanceAgreementBreach()
        {
            var m = new Models.Job.ListModel() { Title = "Jobs Awaiting Finance - Agreement Breach" };
            m.JobTable = ManagedJobList.OpenJobsTable(q => q.Where(j =>
                j.JobTypeId == JobType.JobTypeIds.UMgmt && Job.UserManagementFlags.Infringement_BreachFinancialAgreement == (j.Flags & Job.UserManagementFlags.Infringement_BreachFinancialAgreement)
                ).OrderBy(j => j.JobId));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }
        #endregion

        [DiscoAuthorize(Claims.Job.Lists.AwaitingUserAction)]
        public virtual ActionResult AwaitingUserAction()
        {
            var m = new Models.Job.ListModel() { Title = "Jobs Awaiting User Action" };
            m.JobTable = ManagedJobList.OpenJobsTable(q => q.Where(j =>
                j.WaitingForUserAction.HasValue ||
                (j.JobMetaNonWarranty_AccountingChargeAddedDate != null && j.JobMetaNonWarranty_AccountingChargePaidDate == null) ||
                (j.JobMetaNonWarranty_AccountingChargeRequiredDate != null && j.JobMetaNonWarranty_AccountingChargePaidDate == null)
                ).OrderBy(j => j.JobId));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        [DiscoAuthorize(Claims.Job.Lists.RecentlyClosed)]
        public virtual ActionResult RecentlyClosed()
        {
            var m = new Models.Job.ListModel() { Title = "Recently Closed Jobs" };
            m.JobTable = new JobTableModel() { ShowStatus = true };

            var dateTimeNow = DateTime.Now;
            var closedThreshold = dateTimeNow.AddDays(-2);
            if (dateTimeNow.DayOfWeek == DayOfWeek.Monday)
                closedThreshold = closedThreshold.AddDays(-2);
            if (dateTimeNow.DayOfWeek == DayOfWeek.Tuesday)
                closedThreshold = closedThreshold.AddDays(-1);
            m.JobTable.Fill(Database, Disco.Services.Searching.Search.BuildJobTableModel(Database).Where(j => j.ClosedDate > closedThreshold).OrderBy(j => j.Id), true);

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        [DiscoAuthorize(Claims.Job.Lists.Locations)]
        public virtual ActionResult Locations()
        {
            var m = new Models.Job.ListModel() { Title = "Held Device Locations" };

            m.JobTable = ManagedJobList.OpenJobsTable(q => q.Where(j => j.DeviceHeld.HasValue && !j.DeviceReturnedDate.HasValue).OrderBy(j => j.DeviceHeldLocation));
            m.JobTable.ShowLocation = true;
            m.JobTable.ShowTechnician = false;
            m.JobTable.ShowType = false;

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        [DiscoAuthorize(Claims.Job.Lists.LongRunningJobs)]
        public virtual ActionResult LongRunning()
        {
            var m = new Models.Job.ListModel() { Title = "Long Running Jobs" };

            var longRunningThreshold = DateTime.Today.AddDays(Database.DiscoConfiguration.JobPreferences.LongRunningJobDaysThreshold * -1);
            m.JobTable = ManagedJobList.OpenJobsTable(q => q.Where(j => j.OpenedDate < longRunningThreshold).OrderBy(j => j.JobId));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        [DiscoAuthorize(Claims.Job.Lists.StaleJobs)]
        public virtual ActionResult Stale()
        {
            var m = new Models.Job.ListModel() { Title = "Stale Jobs" };

            var staleThreshold = DateTime.Today.AddMinutes(Database.DiscoConfiguration.JobPreferences.StaleJobMinutesThreshold * -1);
            m.JobTable = ManagedJobList.OpenJobsTable(q => q.Where(j => j.LastActivityDate < staleThreshold).OrderBy(j => j.LastActivityDate));
            m.JobTable.ShowLastActivityDate = true;
            m.JobTable.ShowDates = false;

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

            if ((!m.Job.ClosedDate.HasValue && m.Job.OpenedDate < DateTime.Today.AddDays(Database.DiscoConfiguration.JobPreferences.LongRunningJobDaysThreshold * -1)) ||
                (m.Job.ClosedDate.HasValue && m.Job.OpenedDate.AddDays(Database.DiscoConfiguration.JobPreferences.LongRunningJobDaysThreshold) < m.Job.ClosedDate.Value))
            {
                m.LongRunning = m.Job.ClosedDate.HasValue ? m.Job.ClosedDate.Value - m.Job.OpenedDate : DateTime.Now - m.Job.OpenedDate;
            }

            if (Authorization.Has(Claims.Job.Actions.UpdateSubTypes))
                m.UpdatableJobSubTypes = m.Job.JobType.JobSubTypes.OrderBy(jst => jst.Description).ToList();

            if (Authorization.Has(Claims.Job.Actions.GenerateDocuments))
                m.AvailableDocumentTemplates = m.Job.AvailableDocumentTemplates(Database, UserService.CurrentUser, DateTime.Now);

            // Available Job Queues
            IEnumerable<JobQueueToken> jobQueues = null;
            if (Authorization.Has(Claims.Job.Actions.AddAnyQueues))
                jobQueues = JobQueueService.GetQueues();
            else if (Authorization.Has(Claims.Job.Actions.AddOwnQueues))
                jobQueues = JobQueueService.UsersQueues(CurrentUser);
            m.AvailableQueues = jobQueues == null ? null : jobQueues.Select(qt => qt.JobQueue).Where(q => m.Job.CanAddQueue(q)).ToList();

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
            m.UpdateModel(Database, Authorization);

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobCreateModel>(this.ControllerContext, m);

            return View(m);
        }
        [HttpPost, DiscoAuthorize(Claims.Job.Actions.Create)]
        public virtual ActionResult Create(Models.Job.CreateModel m)
        {
            m.UpdateModel(Database, Authorization);

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
                    if (Authorization.Has(Claims.Job.Actions.Close) && m.QuickLog.HasValue && m.QuickLog.Value && m.QuickLogTaskTimeMinutes.HasValue && m.QuickLogTaskTimeMinutes.Value > 0)
                    {
                        // Quick Log
                        // Set Opened Date in the past
                        j.OpenedDate = DateTime.Now.AddMinutes(-1 * m.QuickLogTaskTimeMinutes.Value);
                        // Close Job
                        j.OnCloseNormally(currentUser);
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
                redirectModel.RedirectDelay = TimeSpan.FromSeconds(2);
                if (m.QuickLog.HasValue && m.QuickLog.Value && !string.IsNullOrWhiteSpace(m.SourceUrl))
                    redirectModel.RedirectLink = m.SourceUrl;
                else if (!Authorization.Has(Claims.Job.Show))
                    if (!string.IsNullOrWhiteSpace(m.SourceUrl))
                        redirectModel.RedirectLink = m.SourceUrl;
                    else
                        redirectModel.RedirectLink = Url.Action(MVC.Job.Index());
                else
                {
                    redirectModel.RedirectLink = Url.Action(MVC.Job.Show(j.Id));
                    redirectModel.RedirectDelay = null;
                }

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
