using Disco.Models.Repository;
using Disco.Models.Services.Jobs;
using Disco.Models.Services.Jobs.JobLists;
using Disco.Models.UI.Job;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Devices.Enrolment;
using Disco.Services.Exporting;
using Disco.Services.Jobs;
using Disco.Services.Jobs.JobLists;
using Disco.Services.Jobs.JobQueues;
using Disco.Services.Jobs.Statistics;
using Disco.Services.Logging;
using Disco.Services.Plugins.Features.DetailsProvider;
using Disco.Services.Plugins.Features.InsuranceProvider;
using Disco.Services.Plugins.Features.RepairProvider;
using Disco.Services.Plugins.Features.UIExtension;
using Disco.Services.Plugins.Features.WarrantyProvider;
using Disco.Services.Users;
using Disco.Services.Web;
using Disco.Web.Models.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
                m.DailyOpenedClosedStatistics = DailyOpenedClosed.Data(Database, true);

            if (Authorization.Has(Claims.Device.Actions.EnrolDevices))
                m.PendingEnrolments = WindowsDeviceEnrolment.GetPendingEnrolments();

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
                (j.JobTypeId == JobType.JobTypeIds.HNWar && (j.JobMetaNonWarranty_AccountingChargeRequiredDate.HasValue && !j.JobMetaNonWarranty_AccountingChargeAddedDate.HasValue)) ||
                (j.JobTypeId == JobType.JobTypeIds.HNWar && ((j.JobMetaNonWarranty_AccountingChargeRequiredDate.HasValue || j.JobMetaNonWarranty_AccountingChargeAddedDate.HasValue) && !j.JobMetaNonWarranty_AccountingChargePaidDate.HasValue)) ||
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
                j.JobTypeId == JobType.JobTypeIds.HNWar && (j.JobMetaNonWarranty_AccountingChargeRequiredDate.HasValue && !j.JobMetaNonWarranty_AccountingChargeAddedDate.HasValue)
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

            m.Job = Database.Jobs
                .Include(j => j.Device.DeviceModel)
                .Include(j => j.Device.DeviceBatch)
                .Include(j => j.Device.DeviceProfile)
                .Include(j => j.Device.DeviceDetails)
                .Include(j => j.Device.DeviceFlagAssignments)
                .Include(j => j.DeviceHeldTechUser)
                .Include(j => j.DeviceReadyForReturnTechUser)
                .Include(j => j.DeviceReturnedTechUser)
                .Include(j => j.OpenedTechUser)
                .Include(j => j.ClosedTechUser)
                .Include(j => j.JobType)
                .Include(j => j.JobSubTypes)
                .Include(j => j.User.UserFlagAssignments)
                .Include(j => j.User.UserDetails)
                .Include(j => j.JobLogs.Select(l => l.TechUser))
                .Include(j => j.JobAttachments.Select(a => a.TechUser))
                .Include(j => j.JobAttachments.Select(a => a.DocumentTemplate))
                .FirstOrDefault(j => j.Id == id.Value);

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
            {
                m.AvailableDocumentTemplates = m.Job.AvailableDocumentTemplates(Database, UserService.CurrentUser, DateTime.Now);
                m.AvailableDocumentTemplatePackages = m.Job.AvailableDocumentTemplatePackages(Database, UserService.CurrentUser);
            }

            // Available Job Queues
            IEnumerable<JobQueueToken> jobQueues = null;
            if (Authorization.Has(Claims.Job.Actions.AddAnyQueues))
                jobQueues = JobQueueService.GetQueues();
            else if (Authorization.Has(Claims.Job.Actions.AddOwnQueues))
                jobQueues = JobQueueService.UsersQueues(CurrentUser);
            m.AvailableQueues = jobQueues == null ? null : jobQueues.Select(qt => qt.JobQueue).Where(q => m.Job.CanAddQueue(q)).ToList();

            if (Authorization.Has(Claims.Job.Properties.DeviceHeldLocation))
            {
                m.LocationMode = Database.DiscoConfiguration.JobPreferences.LocationMode;
                if (m.LocationMode == LocationModes.RestrictedList)
                    m.LocationOptions = ManagedJobList.OpenJobsTable(j => j).Items.Cast<JobTableStatusItemModel>().JobLocationReferences(Database.DiscoConfiguration.JobPreferences.LocationList).ToList();
            }

            // Populate Custom Details
            if (Authorization.Has(Claims.User.ShowDetails))
            {
                m.PopulateDetails(Database);
            }

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

            m.Comments = Jobs.GenerateInitialComments(Database, m, CurrentUser, out var isTypeDynamic);
            m.RegenerateCommentsOnTypeChange = isTypeDynamic;

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobCreateModel>(this.ControllerContext, m);

            return View(m);
        }
        [HttpPost, ValidateAntiForgeryToken, DiscoAuthorize(Claims.Job.Actions.Create)]
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
                Database.Configuration.LazyLoadingEnabled = true;

                // Create New Job
                var currentUser = Database.Users.Find(UserService.CurrentUserId);

                // Try QuickLog?
                bool addAutoQueues = !(Authorization.Has(Claims.Job.Actions.Close)
                    && m.QuickLog.HasValue && m.QuickLog.Value
                    && m.QuickLogTaskTimeMinutes.HasValue && m.QuickLogTaskTimeMinutes.Value > 0);

                var j = Jobs.Create(Database, m.Device, m.User, m.GetJobType, m.GetJobSubTypes, currentUser, addAutoQueues);

                if (m.DeviceHeld.Value)
                {
                    j.OnDeviceHeld(currentUser);
                    m.QuickLog = false;
                }
                else
                {
                    if (Authorization.Has(Claims.Job.Actions.Close)
                        && m.QuickLog.HasValue && m.QuickLog.Value
                        && m.QuickLogTaskTimeMinutes.HasValue && m.QuickLogTaskTimeMinutes.Value > 0
                        && (j.JobQueues == null || j.JobQueues.All(jqj => jqj.RemovedDate.HasValue))
                        )
                    {
                        // Quick Log
                        // Set Opened Date in the past
                        j.OpenedDate = DateTime.Now.AddMinutes(-1 * m.QuickLogTaskTimeMinutes.Value);
                        // Close Job
                        j.OnCloseNormally(Database, currentUser);
                    }
                    else
                    {
                        m.QuickLog = false;
                    }
                }

                Database.SaveChanges();

                // Evaluate OnCreate Expression
                try
                {
                    var onCreateResult = j.EvaluateOnCreateExpression(Database);
                    if (!string.IsNullOrWhiteSpace(onCreateResult))
                    {
                        var jl = new JobLog()
                        {
                            Job = j,
                            TechUser = currentUser,
                            Timestamp = DateTime.Now,
                            Comments = onCreateResult
                        };
                        Database.JobLogs.Add(jl);
                    }
                }
                catch (Exception ex)
                {
                    SystemLog.LogException("Job Expression - OnCreateExpression", ex);
                }


                // Add Comments
                if (!string.IsNullOrWhiteSpace(m.Comments))
                {
                    var jl = new JobLog()
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
                var redirectModel = new Models.Job.CreateRedirectModel()
                {
                    JobId = j.Id
                };
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
            var m = new Models.Job.LogWarrantyModel()
            {
                JobId = id,
                WarrantyProviderId = WarrantyProviderId,
                OrganisationAddressId = OrganisationAddressId
            };
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
        [HttpPost, ValidateAntiForgeryToken, DiscoAuthorize(Claims.Job.Actions.LogWarranty), ValidateInput(false)]
        public virtual ActionResult LogWarranty(Models.Job.LogWarrantyModel m, FormCollection form)
        {
            m.UpdateModel(Database, true);

            if (ModelState.IsValid)
            {
                switch (m.SubmissionAction)
                {
                    case "Update":
                        var updatedModel = new Models.Job.LogWarrantyModel()
                        {
                            JobId = m.JobId,
                            WarrantyProviderId = m.WarrantyProviderId,
                            OrganisationAddressId = m.OrganisationAddressId,
                            FaultDescription = m.FaultDescription,
                            PublishAttachmentIds = m.PublishAttachmentIds,
                            PublishAttachments = m.PublishAttachments
                        };
                        updatedModel.UpdateModel(Database, false);

                        if (updatedModel.WarrantyProvider != null)
                        {
                            using (var wp = updatedModel.WarrantyProvider.CreateInstance<WarrantyProviderFeature>())
                            {
                                if (wp.SubmitJobViewType != null)
                                {
                                    updatedModel.WarrantyProviderSubmitJobViewType = wp.SubmitJobViewType;
                                    updatedModel.WarrantyProviderSubmitJobModel = wp.SubmitJobViewModel(Database, this, updatedModel.Job, updatedModel.OrganisationAddress, updatedModel.TechUser);
                                }
                            }
                        }

                        return View(updatedModel);
                    case "Manual":
                        if (string.IsNullOrWhiteSpace(m.ManualProviderName))
                        {
                            ModelState.AddModelError("ManualProviderName", "The Warranty Provider Name is required");
                            return View(Views.LogWarranty, m);
                        }
                        try
                        {
                            m.Job.OnLogWarranty(Database, m.FaultDescription, m.ManualProviderName, m.ManualProviderReference, m.OrganisationAddress, m.TechUser);
                            Database.SaveChanges();
                            return RedirectToAction(MVC.Job.Show(m.JobId));
                        }
                        catch (Exception ex)
                        {
                            m.Error = ex;
                            return View(Views.LogWarrantyError, m);
                            throw;
                        }
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
                                m.ProviderPropertiesJson = JsonConvert.SerializeObject(warrantyProviderProperties);
                            }
                            m.DiscloseProperties = p.SubmitJobDiscloseInfo(Database, m.Job, m.OrganisationAddress, m.TechUser, m.FaultDescription, warrantyProviderProperties);
                            return View(Views.LogWarrantyDisclose, m);
                        }
                    case "Submit":
                        try
                        {
                            m.Job.OnLogWarranty(Database, m.FaultDescription, m.PublishAttachments, m.WarrantyProvider, m.OrganisationAddress, m.TechUser, m.ProviderProperties());
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
            Models.Job.ProviderJobDetailsModel model = new Models.Job.ProviderJobDetailsModel();

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
                    model.JobDetailsNotSupportedMessage = string.Format("Warranty Provider '{0}' is not integrated with Disco ICT", job.JobMetaWarranty.ExternalName);
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

        #region Log Repair
        [DiscoAuthorize(Claims.Job.Actions.LogRepair)]
        public virtual ActionResult LogRepair(int id, string RepairProviderId, int? OrganisationAddressId)
        {
            var m = new Models.Job.LogRepairModel()
            {
                JobId = id,
                RepairProviderId = RepairProviderId,
                OrganisationAddressId = OrganisationAddressId
            };
            m.UpdateModel(Database, false);
            m.RepairDescription = m.Job.GenerateFaultDescription(Database);

            if (m.RepairProvider != null)
            {
                using (var rp = m.RepairProvider.CreateInstance<RepairProviderFeature>())
                {
                    m.RepairProviderSubmitJobBeginResult = rp.SubmitJobBegin(Database, this, m.Job, m.OrganisationAddress, m.TechUser);
                }
            }

            return View(m);
        }
        [HttpPost, ValidateAntiForgeryToken, DiscoAuthorize(Claims.Job.Actions.LogRepair), ValidateInput(false)]
        public virtual ActionResult LogRepair(Models.Job.LogRepairModel m, FormCollection form)
        {
            m.UpdateModel(Database, true);

            if (ModelState.IsValid)
            {
                switch (m.SubmissionAction)
                {
                    case "Update":
                        var updatedModel = new Models.Job.LogRepairModel()
                        {
                            JobId = m.JobId,
                            RepairProviderId = m.RepairProviderId,
                            OrganisationAddressId = m.OrganisationAddressId,
                            RepairDescription = m.RepairDescription,
                            PublishAttachmentIds = m.PublishAttachmentIds,
                            PublishAttachments = m.PublishAttachments
                        };
                        updatedModel.UpdateModel(Database, false);

                        if (updatedModel.RepairProvider != null)
                        {
                            using (var rp = m.RepairProvider.CreateInstance<RepairProviderFeature>())
                            {
                                updatedModel.RepairProviderSubmitJobBeginResult = rp.SubmitJobBegin(Database, this, updatedModel.Job, updatedModel.OrganisationAddress, updatedModel.TechUser);
                            }
                        }

                        return View(updatedModel);
                    case "Manual":
                        if (string.IsNullOrWhiteSpace(m.ManualProviderName))
                        {
                            ModelState.AddModelError("ManualProviderName", "The Repair Provider Name is required");
                            return View(Views.LogRepair, m);
                        }
                        try
                        {
                            m.Job.OnLogRepair(Database, m.RepairDescription, m.ManualProviderName, m.ManualProviderReference, m.OrganisationAddress, m.TechUser);
                            Database.SaveChanges();
                            return RedirectToAction(MVC.Job.Show(m.JobId));
                        }
                        catch (Exception ex)
                        {
                            m.Error = ex;
                            return View(Views.LogRepairError, m);
                            throw;
                        }
                    case "Disclose":
                        using (var p = m.RepairProvider.CreateInstance<RepairProviderFeature>())
                        {
                            Dictionary<string, string> warrantyProviderProperties;
                            try
                            {
                                warrantyProviderProperties = p.SubmitJobParseProperties(Database, form, this, m.Job, m.OrganisationAddress, m.TechUser, m.RepairDescription);
                            }
                            catch (Exception ex)
                            {
                                m.Error = ex;
                                return View(Views.LogRepairError, m);
                            }
                            if (!ModelState.IsValid)
                                return View(Views.LogRepair, m);

                            if (warrantyProviderProperties != null)
                            {
                                m.ProviderPropertiesJson = JsonConvert.SerializeObject(warrantyProviderProperties);
                            }
                            m.DiscloseProperties = p.SubmitJobDiscloseInfo(Database, m.Job, m.OrganisationAddress, m.TechUser, m.RepairDescription, warrantyProviderProperties);
                            return View(Views.LogRepairDisclose, m);
                        }
                    case "Submit":
                        try
                        {
                            m.Job.OnLogRepair(Database, m.RepairDescription, m.PublishAttachments, m.RepairProvider, m.OrganisationAddress, m.TechUser, m.ProviderProperties());
                            Database.SaveChanges();
                            return RedirectToAction(MVC.Job.Show(m.JobId));
                        }
                        catch (Exception ex)
                        {
                            m.Error = ex;
                            return View(Views.LogRepairError, m);
                            throw;
                        }
                    default:
                        return RedirectToAction(MVC.Job.Show(m.JobId));
                }

            }
            else
            {
                return View(Views.LogRepair, m);
            }
        }

        [DiscoAuthorize(Claims.Job.Properties.NonWarrantyProperties.RepairProviderDetails)]
        public virtual ActionResult RepairProviderJobDetails(int id)
        {
            Models.Job.ProviderJobDetailsModel model = new Models.Job.ProviderJobDetailsModel();

            Job job = Database.Jobs.Include("Device.DeviceModel").Include("JobMetaNonWarranty").Include("JobSubTypes").Where(j => j.Id == id).FirstOrDefault();
            if (job != null)
            {
                if (job.JobMetaNonWarranty != null && !string.IsNullOrEmpty(job.JobMetaNonWarranty.RepairerName))
                {
                    var providerDef = RepairProviderFeature.FindPluginFeature(job.JobMetaNonWarranty.RepairerName);

                    if (providerDef != null)
                    {
                        using (RepairProviderFeature providerInstance = providerDef.CreateInstance<RepairProviderFeature>())
                        {
                            if (providerInstance.JobDetailsSupported)
                            {
                                try
                                {
                                    Tuple<Type, dynamic> details = providerInstance.JobDetails(Database, this, job);

                                    model.JobDetailsSupported = true;
                                    model.ViewType = details.Item1;
                                    model.ViewModel = details.Item2;
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
                                model.JobDetailsNotSupportedMessage = string.Format("Plugin '{0} ({1})' (Repair Provider for '{2}') doesn't support Job Details", providerInstance.Manifest.Name, providerInstance.Manifest.Id, providerInstance.ProviderId);
                                return View(model);
                            }
                        }
                    }

                    model.JobDetailsSupported = false;
                    model.JobDetailsNotSupportedMessage = string.Format("Repair Provider '{0}' is not integrated with Disco ICT", job.JobMetaNonWarranty.RepairerName);
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

        #region Log Insurance
        [DiscoAuthorizeAny(Claims.Job.Properties.NonWarrantyProperties.InsuranceClaimFormSent, Claims.Job.Actions.LogInsurance)]
        public virtual ActionResult LogInsurance(int id, string providerId, int? organisationAddressId)
        {
            var m = new Models.Job.LogInsuranceModel()
            {
                JobId = id,
                ProviderId = providerId,
                OrganisationAddressId = organisationAddressId
            };
            m.UpdateModel(Database, false);

            if (m.Provider != null)
            {
                using (var rp = m.Provider.CreateInstance<InsuranceProviderFeature>())
                {
                    m.ProviderSubmitJobBeginResult = rp.SubmitJobBegin(Database, this, m.Job, m.OrganisationAddress, m.TechUser);
                }
            }

            return View(m);
        }
        [HttpPost, ValidateAntiForgeryToken, DiscoAuthorizeAny(Claims.Job.Properties.NonWarrantyProperties.InsuranceClaimFormSent, Claims.Job.Actions.LogInsurance), ValidateInput(false)]
        public virtual ActionResult LogInsurance(Models.Job.LogInsuranceModel m, FormCollection form)
        {
            m.UpdateModel(Database, true);

            if (ModelState.IsValid)
            {
                switch (m.SubmissionAction)
                {
                    case "Update":
                        var updatedModel = new Models.Job.LogInsuranceModel()
                        {
                            JobId = m.JobId,
                            ProviderId = m.ProviderId,
                            OrganisationAddressId = m.OrganisationAddressId,
                            AttachmentIds = m.AttachmentIds,
                            Attachments = m.Attachments
                        };
                        updatedModel.UpdateModel(Database, false);

                        if (updatedModel.Provider != null)
                        {
                            using (var ip = updatedModel.Provider.CreateInstance<InsuranceProviderFeature>())
                            {
                                updatedModel.ProviderSubmitJobBeginResult = ip.SubmitJobBegin(Database, this, updatedModel.Job, updatedModel.OrganisationAddress, updatedModel.TechUser);
                            }
                        }

                        return View(updatedModel);
                    case "Manual":
                        if (string.IsNullOrWhiteSpace(m.ManualProviderName))
                        {
                            ModelState.AddModelError("ManualProviderName", "The Provider Name is required");
                            return View(m);
                        }
                        try
                        {
                            m.Job.OnLogInsurance(Database, m.ManualProviderName, m.ManualProviderReference, m.OrganisationAddress, m.TechUser);
                            Database.SaveChanges();
                            return RedirectToAction(MVC.Job.Show(m.JobId));
                        }
                        catch (Exception ex)
                        {
                            m.Error = ex;
                            return View(Views.LogInsuranceError, m);
                            throw;
                        }
                    case "Disclose":
                        using (var p = m.Provider.CreateInstance<InsuranceProviderFeature>())
                        {
                            Dictionary<string, string> providerProperties;
                            try
                            {
                                providerProperties = p.SubmitJobParseProperties(Database, form, this, m.Job, m.OrganisationAddress, m.TechUser);
                            }
                            catch (Exception ex)
                            {
                                m.Error = ex;
                                return View(Views.LogInsuranceError, m);
                            }
                            if (!ModelState.IsValid)
                                return View(Views.LogInsurance, m);

                            if (providerProperties != null)
                            {
                                m.ProviderPropertiesJson = JsonConvert.SerializeObject(providerProperties);
                            }
                            m.DiscloseProperties = p.SubmitJobDiscloseInfo(Database, m.Job, m.OrganisationAddress, m.TechUser, providerProperties);
                            return View(Views.LogInsuranceDisclose, m);
                        }
                    case "Submit":
                        try
                        {
                            m.Job.OnLogInsurance(Database, m.Attachments, m.Provider, m.OrganisationAddress, m.TechUser, m.ProviderProperties());
                            Database.SaveChanges();
                            return RedirectToAction(MVC.Job.Show(m.JobId));
                        }
                        catch (Exception ex)
                        {
                            m.Error = ex;
                            return View(Views.LogInsuranceError, m);
                            throw;
                        }
                    default:
                        return RedirectToAction(MVC.Job.Show(m.JobId));
                }

            }
            else
            {
                return View(m);
            }
        }

        [DiscoAuthorize(Claims.Job.Properties.NonWarrantyProperties.InsuranceDetails)]
        public virtual ActionResult InsuranceProviderJobDetails(int id)
        {
            var model = new Models.Job.ProviderJobDetailsModel();

            Job job = Database.Jobs
                .Include(j => j.Device.DeviceModel)
                .Include(j => j.JobMetaNonWarranty)
                .Include(j => j.JobMetaInsurance)
                .Include(j => j.JobSubTypes)
                .Where(j => j.Id == id).FirstOrDefault();
            if (job != null)
            {
                if (job.JobMetaInsurance != null && !string.IsNullOrEmpty(job.JobMetaInsurance.Insurer))
                {
                    var providerDef = InsuranceProviderFeature.FindPluginFeature(job.JobMetaInsurance.Insurer);

                    if (providerDef != null)
                    {
                        using (var providerInstance = providerDef.CreateInstance<InsuranceProviderFeature>())
                        {
                            if (providerInstance.JobDetailsSupported)
                            {
                                try
                                {
                                    Tuple<Type, dynamic> details = providerInstance.JobDetails(Database, this, job);

                                    model.JobDetailsSupported = true;
                                    model.ViewType = details.Item1;
                                    model.ViewModel = details.Item2;
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
                                model.JobDetailsNotSupportedMessage = $"Plugin '{providerInstance.Manifest.Name} ({providerInstance.Manifest.Id})' (Insurance Provider for '{providerInstance.ProviderId}') doesn't support Job Details";
                                return View(model);
                            }
                        }
                    }

                    model.JobDetailsSupported = false;
                    model.JobDetailsNotSupportedMessage = $"Repair Provider '{job.JobMetaNonWarranty.RepairerName}' is not integrated with Disco ICT";
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

        #region Export

        [DiscoAuthorizeAny(Claims.Job.Actions.Export), HttpGet]
        public virtual ActionResult Export(Guid? exportId)
        {
            var m = new Models.Job.ExportModel()
            {
                Options = Database.DiscoConfiguration.JobPreferences.LastExportOptions,
                JobQueues = JobQueueService.GetQueues().Select(q => q.JobQueue).ToList(),
                JobTypes = Database.JobTypes.Include(t => t.JobSubTypes).ToList(),
                JobStatuses = Job.JobStatusIds.StatusDescriptions.ToList(),
            };

            m.Fields = ExportFieldsModel.Create(m.Options, JobExportOptions.DefaultOptions());
            m.Fields.AddCustomUserDetails(o => o.UserDetailsCustom, m.Fields.FieldGroups.FindIndex(g => g.Name == "User") + 1);

            if (Database.DiscoConfiguration.JobPreferences.LastExportDate.GetValueOrDefault() < DateTime.Today.AddDays(-1))
            {
                m.Options.FilterStartDate = new DateTime(DateTime.Today.Year, 1, 1);
                m.Options.FilterEndDate = null;
            }

            if (ExportTask.TryFromCache(exportId, out var context))
            {
                m.ExportId = context.Id;
                m.ExportResult = context.Result;
            }

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobExportModel>(ControllerContext, m);

            return View(m);
        }

        #endregion

    }
}