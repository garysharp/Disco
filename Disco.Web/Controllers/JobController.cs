using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.Data.Repository;
using Disco.BI;
using Disco.BI.Extensions;
using Disco.Models.Repository;
using System.Web.Script.Serialization;
using Disco.Services.Plugins.Features.WarrantyProvider;
using Disco.Services.Plugins;
using Disco.Models.UI.Job;
using Disco.Services.Plugins.Features.UIExtension;

namespace Disco.Web.Controllers
{
    public partial class JobController : dbAdminController
    {

        #region Index
        public virtual ActionResult Index()
        {
            var m = new Models.Job.IndexModel();

            //m.MyJobs = JobBI.SelectJobSearchResultItem((from j in dbContext.Jobs
            //                                              where j.OpenedTechUserId == DiscoApplication.CurrentUser.Id && j.ClosedDate == null && (j.DeviceHeld == null || j.DeviceReturnedDate != null || j.DeviceReadyForReturn == null)
            //                                              select j));
            //m.OpenJobs = JobBI.SelectJobSearchResultItem((from j in dbContext.Jobs
            //                                              where j.OpenedTechUserId != DiscoApplication.CurrentUser.Id && j.ClosedDate == null && (j.DeviceHeld == null || j.DeviceReturnedDate != null || j.DeviceReadyForReturn == null)
            //                                              select j));

            dbContext.Configuration.LazyLoadingEnabled = true;

            m.OpenJobs = new Disco.Models.BI.Job.JobTableModel() { ShowStatus = true };
            m.OpenJobs.Fill(dbContext, BI.JobBI.Searching.BuildJobTableModel(dbContext).Where(j => j.ClosedDate == null
                && !j.WaitingForUserAction.HasValue
                && !(j.JobTypeId == JobType.JobTypeIds.HNWar && (j.JobMetaNonWarranty.RepairerLoggedDate.HasValue && j.JobMetaNonWarranty.IsInsuranceClaim && !j.JobMetaInsurance.ClaimFormSentDate.HasValue))
                && !(j.JobTypeId == JobType.JobTypeIds.HNWar && (j.JobMetaNonWarranty.RepairerLoggedDate.HasValue && !j.JobMetaNonWarranty.RepairerCompletedDate.HasValue))
                && !(j.JobTypeId == JobType.JobTypeIds.HNWar && (j.JobMetaNonWarranty.AccountingChargeAddedDate.HasValue && !j.JobMetaNonWarranty.AccountingChargePaidDate.HasValue))
                && !(j.JobTypeId == JobType.JobTypeIds.HWar && (j.JobMetaWarranty.ExternalLoggedDate.HasValue && !j.JobMetaWarranty.ExternalCompletedDate.HasValue))
                && (j.DeviceHeld == null || j.DeviceReturnedDate != null || j.DeviceReadyForReturn == null)).OrderBy(j => j.Id));

            var longRunningThreshold = DateTime.Now.AddDays(-7);
            m.LongRunningJobs = new Disco.Models.BI.Job.JobTableModel() { ShowStatus = true };
            m.LongRunningJobs.Fill(dbContext, BI.JobBI.Searching.BuildJobTableModel(dbContext).Where(j => j.ClosedDate == null
                && j.OpenedDate < longRunningThreshold).OrderBy(j => j.Id));

            //m.WaitingForUserActionJobs = new Disco.Models.BI.Job.JobTableModel();
            //m.WaitingForUserActionJobs.Fill(Disco.BI.JobTableModelBI.BuildQuery(dbContext).Where(j => j.WaitingForUserAction.HasValue
            //                                                              && j.ClosedDate == null));

            //m.ReadyForReturnJobs = new Disco.Models.BI.Job.JobTableModel();
            //m.ReadyForReturnJobs.Fill(BI.JobTableModelBI.BuildQuery(dbContext).Where(j => !j.WaitingForUserAction.HasValue
            //                                                        && j.DeviceHeld != null && j.DeviceReturnedDate == null && j.DeviceReadyForReturn != null
            //                                                        && j.ClosedDate == null));

            //// 2 Days ago - Ignore Weekend
            //var dateTimeNow = DateTime.Now;
            //var closedThreshold = dateTimeNow.AddDays(-2);
            //if (dateTimeNow.DayOfWeek == DayOfWeek.Monday)
            //    closedThreshold = closedThreshold.AddDays(-2);
            //if (dateTimeNow.DayOfWeek == DayOfWeek.Tuesday)
            //    closedThreshold = closedThreshold.AddDays(-1);
            //m.RecentlyClosedJobs = new Disco.Models.BI.Job.JobTableModel();
            //m.RecentlyClosedJobs.Fill(BI.JobTableModelBI.BuildQuery(dbContext).Where(j => j.ClosedDate > closedThreshold));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobIndexModel>(this.ControllerContext, m);

            return View(m);
        }
        #endregion

        #region Lists
        public virtual ActionResult AllOpen()
        {
            dbContext.Configuration.LazyLoadingEnabled = true;
            var m = new Models.Job.ListModel() { Title = "All Open Jobs" };
            m.JobTable = new Disco.Models.BI.Job.JobTableModel() { ShowStatus = true };
            m.JobTable.Fill(dbContext, BI.JobBI.Searching.BuildJobTableModel(dbContext).Where(j => j.ClosedDate == null).OrderBy(j => j.Id));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }
        public virtual ActionResult DevicesReadyForReturn()
        {
            dbContext.Configuration.LazyLoadingEnabled = true;
            var m = new Models.Job.ListModel() { Title = "Jobs with Devices Ready for Return" };
            m.JobTable = new Disco.Models.BI.Job.JobTableModel() { ShowStatus = true };
            m.JobTable.Fill(dbContext, BI.JobBI.Searching.BuildJobTableModel(dbContext).Where(j => !j.WaitingForUserAction.HasValue
                && j.DeviceHeld != null && j.DeviceReturnedDate == null && j.DeviceReadyForReturn != null &&
                ((!j.JobMetaNonWarranty.AccountingChargeRequiredDate.HasValue && !j.JobMetaNonWarranty.AccountingChargeAddedDate.HasValue) || j.JobMetaNonWarranty.AccountingChargePaidDate.HasValue)
                && j.ClosedDate == null).OrderBy(j => j.Id));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }
        public virtual ActionResult DevicesAwaitingRepair()
        {
            dbContext.Configuration.LazyLoadingEnabled = true;
            var m = new Models.Job.ListModel() { Title = "Jobs with Devices Awaiting Repair" };
            m.JobTable = new Disco.Models.BI.Job.JobTableModel() { ShowStatus = true };
            m.JobTable.Fill(dbContext, BI.JobBI.Searching.BuildJobTableModel(dbContext).Where(j => j.ClosedDate == null &&
                (
                (j.JobMetaNonWarranty.RepairerLoggedDate != null && j.JobMetaNonWarranty.RepairerCompletedDate == null) ||
                (j.JobMetaWarranty.ExternalLoggedDate != null && j.JobMetaWarranty.ExternalCompletedDate == null)
                )).OrderBy(j => j.Id));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        #region "Finance Lists"
        public virtual ActionResult AwaitingFinance()
        {
            dbContext.Configuration.LazyLoadingEnabled = true;
            var m = new Models.Job.ListModel() { Title = "Jobs Awaiting Finance" };
            m.JobTable = new Disco.Models.BI.Job.JobTableModel() { ShowStatus = true };
            m.JobTable.Fill(dbContext, BI.JobBI.Searching.BuildJobTableModel(dbContext).Where(j => j.ClosedDate == null &&
                (
                (j.JobTypeId == JobType.JobTypeIds.HNWar && (j.JobMetaNonWarranty.IsInsuranceClaim && !j.JobMetaInsurance.ClaimFormSentDate.HasValue)) ||
                (j.JobTypeId == JobType.JobTypeIds.HNWar && (j.JobMetaNonWarranty.AccountingChargeRequiredDate.HasValue && (!j.JobMetaNonWarranty.AccountingChargeAddedDate.HasValue && !j.JobMetaNonWarranty.AccountingChargePaidDate.HasValue))) ||
                (j.JobTypeId == JobType.JobTypeIds.HNWar && (!j.JobMetaNonWarranty.AccountingChargeAddedDate.HasValue || !j.JobMetaNonWarranty.AccountingChargePaidDate.HasValue)) ||
                (j.JobTypeId == JobType.JobTypeIds.UMgmt && (long)Job.UserManagementFlags.Infringement_BreachFinancialAgreement == (j.Flags & (long)Job.UserManagementFlags.Infringement_BreachFinancialAgreement))
                )));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        public virtual ActionResult AwaitingFinanceCharge()
        {
            dbContext.Configuration.LazyLoadingEnabled = true;
            var m = new Models.Job.ListModel() { Title = "Jobs Awaiting Finance - Accounting Charge" };
            m.JobTable = new Disco.Models.BI.Job.JobTableModel() { ShowStatus = true };
            m.JobTable.Fill(dbContext, BI.JobBI.Searching.BuildJobTableModel(dbContext).Where(j => j.ClosedDate == null &&
                (j.JobTypeId == JobType.JobTypeIds.HNWar && (j.JobMetaNonWarranty.AccountingChargeRequiredDate.HasValue && (!j.JobMetaNonWarranty.AccountingChargeAddedDate.HasValue && !j.JobMetaNonWarranty.AccountingChargePaidDate.HasValue)))
                ).OrderBy(j => j.Id));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }
        public virtual ActionResult AwaitingFinancePayment()
        {
            dbContext.Configuration.LazyLoadingEnabled = true;
            var m = new Models.Job.ListModel() { Title = "Jobs Awaiting Finance - Accounting Payment" };
            m.JobTable = new Disco.Models.BI.Job.JobTableModel() { ShowStatus = true };
            m.JobTable.Fill(dbContext, BI.JobBI.Searching.BuildJobTableModel(dbContext).Where(j => j.ClosedDate == null &&
                (j.JobTypeId == JobType.JobTypeIds.HNWar && (!j.JobMetaNonWarranty.AccountingChargeAddedDate.HasValue || !j.JobMetaNonWarranty.AccountingChargePaidDate.HasValue))
                ).OrderBy(j => j.Id));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }
        public virtual ActionResult AwaitingFinanceInsuranceProcessing()
        {
            dbContext.Configuration.LazyLoadingEnabled = true;
            var m = new Models.Job.ListModel() { Title = "Jobs Awaiting Finance - Insurance Processing" };
            m.JobTable = new Disco.Models.BI.Job.JobTableModel() { ShowStatus = true };
            m.JobTable.Fill(dbContext, BI.JobBI.Searching.BuildJobTableModel(dbContext).Where(j => j.ClosedDate == null &&
                (j.JobTypeId == JobType.JobTypeIds.HNWar && (j.JobMetaNonWarranty.IsInsuranceClaim && !j.JobMetaInsurance.ClaimFormSentDate.HasValue))
                ).OrderBy(j => j.Id));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }
        public virtual ActionResult AwaitingFinanceAgreementBreach()
        {
            dbContext.Configuration.LazyLoadingEnabled = true;
            var m = new Models.Job.ListModel() { Title = "Jobs Awaiting Finance - Agreement Breach" };
            m.JobTable = new Disco.Models.BI.Job.JobTableModel() { ShowStatus = true };
            m.JobTable.Fill(dbContext, BI.JobBI.Searching.BuildJobTableModel(dbContext).Where(j => j.ClosedDate == null &&
                (j.JobTypeId == JobType.JobTypeIds.UMgmt && (long)Job.UserManagementFlags.Infringement_BreachFinancialAgreement == (j.Flags & (long)Job.UserManagementFlags.Infringement_BreachFinancialAgreement))
                ).OrderBy(j => j.Id));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        #endregion

        public virtual ActionResult AwaitingUserAction()
        {
            dbContext.Configuration.LazyLoadingEnabled = true;
            var m = new Models.Job.ListModel() { Title = "Jobs Awaiting User Action" };
            m.JobTable = new Disco.Models.BI.Job.JobTableModel() { ShowStatus = true };
            m.JobTable.Fill(dbContext, BI.JobBI.Searching.BuildJobTableModel(dbContext).Where(j => (j.WaitingForUserAction.HasValue || (j.JobMetaNonWarranty.AccountingChargeAddedDate != null && j.JobMetaNonWarranty.AccountingChargePaidDate == null))
                                                                          && j.ClosedDate == null).OrderBy(j => j.Id));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }
        public virtual ActionResult RecentlyClosed()
        {
            dbContext.Configuration.LazyLoadingEnabled = true;
            var m = new Models.Job.ListModel() { Title = "Recently Closed Jobs" };
            m.JobTable = new Disco.Models.BI.Job.JobTableModel() { ShowStatus = true };

            var dateTimeNow = DateTime.Now;
            var closedThreshold = dateTimeNow.AddDays(-2);
            if (dateTimeNow.DayOfWeek == DayOfWeek.Monday)
                closedThreshold = closedThreshold.AddDays(-2);
            if (dateTimeNow.DayOfWeek == DayOfWeek.Tuesday)
                closedThreshold = closedThreshold.AddDays(-1);
            m.JobTable.Fill(dbContext, BI.JobBI.Searching.BuildJobTableModel(dbContext).Where(j => j.ClosedDate > closedThreshold).OrderBy(j => j.Id));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }
        public virtual ActionResult Locations()
        {
            dbContext.Configuration.LazyLoadingEnabled = true;
            var m = new Models.Job.ListModel() { Title = "Held Device Locations" };
            m.JobTable = new Disco.Models.BI.Job.JobTableModel() { ShowStatus = true, ShowLocation = true, ShowTechnician = false, ShowType = false };
            m.JobTable.Fill(dbContext, BI.JobBI.Searching.BuildJobTableModel(dbContext).Where(j => j.ClosedDate == null && j.DeviceHeld.HasValue && !j.DeviceReturnedDate.HasValue).OrderBy(j => j.DeviceHeldLocation));

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobListModel>(this.ControllerContext, m);

            return View(Views.List, m);
        }

        #endregion

        #region Show
        public virtual ActionResult Show(int? id)
        {
            if (!id.HasValue)
                return RedirectToAction(MVC.Job.Index());

            dbContext.Configuration.LazyLoadingEnabled = true;

            var m = new Models.Job.ShowModel();

            m.Job = (from j in dbContext.Jobs.Include("Device.DeviceModel").Include("Device.DeviceBatch").Include("DeviceHeldTechUser").Include("DeviceReadyForReturnTechUser").Include("DeviceReturnedTechUser")
                         .Include("OpenedTechUser").Include("ClosedTechUser").Include("JobType").Include("JobSubTypes").Include("User").Include("JobLogs.TechUser")
                     where (j.Id == id.Value)
                     select j).FirstOrDefault();

            m.UpdatableJobSubTypes = m.Job.JobType.JobSubTypes.OrderBy(jst => jst.Description).ToList();

            m.AvailableDocumentTemplates = m.Job.AvailableDocumentTemplates(dbContext, DiscoApplication.CurrentUser, DateTime.Now);

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobShowModel>(this.ControllerContext, m);

            return View(m);
        }
        #endregion

        #region Create
        public virtual ActionResult Create(string DeviceSerialNumber, string UserId)
        {
            var m = new Models.Job.CreateModel()
            {
                DeviceSerialNumber = DeviceSerialNumber,
                UserId = UserId
            };
            m.UpdateModel(dbContext);

            // UI Extensions
            UIExtensions.ExecuteExtensions<JobCreateModel>(this.ControllerContext, m);

            return View(m);
        }
        [HttpPost]
        public virtual ActionResult Create(Models.Job.CreateModel m)
        {
            m.UpdateModel(dbContext);

            if (!ModelState.IsValid)
            {
                // UI Extensions
                UIExtensions.ExecuteExtensions<JobCreateModel>(this.ControllerContext, m);

                return View(m);
            }
            else
            {
                // Create New Job
                var currentUser = dbContext.Users.Find(DiscoApplication.CurrentUser.Id);
                var j = BI.JobBI.Utilities.Create(dbContext, m.Device, m.User, m.GetJobType, m.GetJobSubTypes, currentUser);

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
                    dbContext.JobLogs.Add(jl);
                }

                dbContext.SaveChanges();

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

        // Decommissioned 2012-11-28 G# - Moved to new infrastructure
        #region Create - Old
        //public virtual ActionResult Create(string DeviceSerialNumber, string UserId)
        //{
        //    var m = new Models.Job.CreateModel()
        //    {
        //        DeviceSerialNumber = DeviceSerialNumber,
        //        UserId = UserId
        //    };
        //    m.UpdateModel(dbContext);

        //    return View(m);
        //}
        //[HttpPost]
        //public virtual ActionResult Create(Models.Job.CreateModel m)
        //{
        //    m.UpdateModel(dbContext);

        //    if (!ModelState.IsValid)
        //    {
        //        return View(m);
        //    }
        //    else
        //    {
        //        // Create New Job
        //        var currentUser = dbContext.Users.Find(DiscoApplication.CurrentUser.Id);
        //        var j = BI.JobBI.Utilities.Create(dbContext, m.Device, m.User, m.GetJobType, m.GetJobSubTypes, currentUser);
        //        dbContext.SaveChanges();
        //        return RedirectToAction(MVC.Job.Show(j.Id));
        //    }
        //}
        #endregion
        // End Decommissioned 2012-11-28 G#

        #region Log Warranty
        public virtual ActionResult LogWarranty(int id, string WarrantyProviderId, int? OrganisationAddressId)
        {
            var m = new Models.Job.LogWarrantyModel() { JobId = id, WarrantyProviderId = WarrantyProviderId, OrganisationAddressId = OrganisationAddressId };
            m.UpdateModel(dbContext, false);
            m.FaultDescription = m.Job.GenerateFaultDescription(dbContext);

            if (m.WarrantyProvider != null)
            {
                using (var wp = m.WarrantyProvider.CreateInstance<WarrantyProviderFeature>())
                {
                    if (wp.SubmitJobViewType != null)
                    {
                        m.WarrantyProviderSubmitJobViewType = wp.SubmitJobViewType;
                        m.WarrantyProviderSubmitJobModel = wp.SubmitJobViewModel(dbContext, this, m.Job, m.OrganisationAddress, m.TechUser);
                    }
                }
            }

            return View(m);
        }
        [HttpPost]
        public virtual ActionResult LogWarranty(Models.Job.LogWarrantyModel m, FormCollection form)
        {
            m.UpdateModel(dbContext, true);

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
                                warrantyProviderProperties = p.SubmitJobParseProperties(dbContext, form, this, m.Job, m.OrganisationAddress, m.TechUser, m.FaultDescription);
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
                                JavaScriptSerializer j = new JavaScriptSerializer();
                                m.WarrantyProviderPropertiesJson = j.Serialize(warrantyProviderProperties);
                            }
                            m.DiscloseProperties = p.SubmitJobDiscloseInfo(dbContext, m.Job, m.OrganisationAddress, m.TechUser, m.FaultDescription, warrantyProviderProperties);
                            return View(Views.LogWarrantyDisclose, m);
                        }
                    case "Submit":
                        try
                        {
                            m.Job.OnLogWarranty(dbContext, m.FaultDescription, m.WarrantyProvider, m.OrganisationAddress, m.TechUser, m.WarrantyProviderProperties());
                            dbContext.SaveChanges();
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

        public virtual ActionResult WarrantyProviderJobDetails(int id)
        {
            Models.Job.WarrantyProviderJobDetailsModel model = new Models.Job.WarrantyProviderJobDetailsModel();

            Job job = dbContext.Jobs.Include("Device.DeviceModel").Include("JobMetaWarranty").Include("JobSubTypes").Where(j => j.Id == id).FirstOrDefault();
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
                                    object providerModel = providerInstance.JobDetailsViewModel(dbContext, this, job);

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
