using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.BI.Extensions;
using Disco.Web.Extensions;
using Disco.Models.Repository;
using Disco.Data.Configuration;
using System.IO;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class JobController : dbAdminController
    {

        #region Property Constants
        const string pExpectedClosedDate = "expectedcloseddate";
        const string pDeviceHeldLocation = "deviceheldlocation";
        const string pFlags = "flags";

        const string pNonWarrantyAccountingChargeRequired = "nonwarrantyaccountingchargerequired";
        const string pNonWarrantyAccountingChargeAdded = "nonwarrantyaccountingchargeadded";
        const string pNonWarrantyAccountingChargePaid = "nonwarrantyaccountingchargepaid";
        const string pNonWarrantyPurchaseOrderRaised = "nonwarrantypurchaseorderraised";
        const string pNonWarrantyPurchaseOrderReference = "nonwarrantypurchaseorderreference";
        const string pNonWarrantyPurchaseOrderSent = "nonwarrantypurchaseordersent";
        const string pNonWarrantyInvoiceReceived = "nonwarrantyinvoicereceived";
        const string pNonWarrantyRepairerName = "nonwarrantyrepairername";
        const string pNonWarrantyRepairerLoggedDate = "nonwarrantyrepairerloggeddate";
        const string pNonWarrantyRepairerReference = "nonwarrantyrepairerreference";
        const string pNonWarrantyRepairerCompletedDate = "nonwarrantyrepairercompleteddate";
        const string pNonWarrantyIsInsuranceClaim = "nonwarrantyinsuranceisinsuranceclaim";

        const string pInsuranceLossOrDamageDate = "insurancelossordamagedate";
        const string pInsuranceEventLocation = "insuranceeventlocation";
        const string pInsuranceDescription = "insurancedescription";
        const string pInsuranceThirdPartyCaused = "insurancethirdpartycaused";
        const string pInsuranceThirdPartyCausedName = "insurancethirdpartycausedname";
        const string pInsuranceThirdPartyCausedWhy = "insurancethirdpartycausedwhy";
        const string pInsuranceWitnessesNamesAddresses = "insurancewitnessesnamesaddresses";
        const string pInsuranceBurglaryTheftMethodOfEntry = "insuranceburglarytheftmethodofentry";
        const string pInsurancePropertyLastSeenDate = "insurancepropertylastseendate";
        const string pInsurancePoliceNotified = "insurancepolicenotified";
        const string pInsurancePoliceNotifiedStation = "insurancepolicenotifiedstation";
        const string pInsurancePoliceNotifiedDate = "insurancepolicenotifieddate";
        const string pInsurancePoliceNotifiedCrimeReportNo = "insurancepolicenotifiedcrimereportno";
        const string pInsuranceRecoverReduceAction = "insurancerecoverreduceaction";
        const string pInsuranceOtherInterestedParties = "insuranceotherinterestedparties";
        const string pInsuranceDateOfPurchase = "insurancedateofpurchase";
        const string pInsuranceClaimFormSentDate = "insuranceclaimformsentdate";
        const string pInsuranceClaimFormSentUserId = "insuranceclaimformsentuserid";

        const string pWarrantyExternalName = "warrantyexternalname";
        const string pWarrantyExternalLoggedDate = "warrantyexternalloggeddate";
        const string pWarrantyExternalReference = "warrantyexternalreference";
        const string pWarrantyExternalCompletedDate = "warrantyexternalcompleteddate";

        const string pJobDetailsTabResources = "jobDetailTab-Resources";
        const string pJobDetailsTabComponents = "jobDetailTab-Components";
        const string pJobDetailsTabNonWarrantyFinance = "jobDetailTab-NonWarrantyFinance";
        const string pJobDetailsTabNonWarrantyRepairs = "jobDetailTab-NonWarrantyRepairs";
        const string pJobDetailsTabNonWarrantyInsurance = "jobDetailTab-NonWarrantyInsurance";
        const string pJobDetailsTabWarranty = "jobDetailTab-Warranty";
        const string pJobDetailsTabFlags = "jobDetailTab-Flags";

        #endregion

        public virtual ActionResult Update(int id, string key, string value = null, Nullable<bool> redirect = null)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("key");

                dbContext.Configuration.LazyLoadingEnabled = true;
                var job = dbContext.Jobs.Find(id);

                object resultData = null;
                string resultUrlFragment = null;

                if (job != null)
                {
                    switch (key.ToLower())
                    {
                        case pExpectedClosedDate:
                            UpdateExpectedClosedDate(job, value);
                            break;
                        case pDeviceHeldLocation:
                            UpdateDeviceHeldLocation(job, value);
                            break;
                        case pFlags:
                            UpdateFlags(job, value);
                            resultUrlFragment = pJobDetailsTabFlags;
                            break;
                        case pNonWarrantyAccountingChargeRequired:
                            resultData = UpdateNonWarrantyAccountingChargeRequired(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyFinance;
                            break;
                        case pNonWarrantyAccountingChargeAdded:
                            resultData = UpdateNonWarrantyAccountingChargeAdded(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyFinance;
                            break;
                        case pNonWarrantyAccountingChargePaid:
                            resultData = UpdateNonWarrantyAccountingChargePaid(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyFinance;
                            break;
                        case pNonWarrantyPurchaseOrderRaised:
                            resultData = UpdateNonWarrantyPurchaseOrderRaised(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyFinance;
                            break;
                        case pNonWarrantyPurchaseOrderReference:
                            UpdateNonWarrantyPurchaseOrderReference(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyFinance;
                            break;
                        case pNonWarrantyPurchaseOrderSent:
                            resultData = UpdateNonWarrantyPurchaseOrderSent(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyFinance;
                            break;
                        case pNonWarrantyInvoiceReceived:
                            resultData = UpdateNonWarrantyInvoiceReceived(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyFinance;
                            break;
                        case pNonWarrantyRepairerName:
                            UpdateNonWarrantyRepairerName(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyRepairs;
                            break;
                        case pNonWarrantyRepairerLoggedDate:
                            UpdateNonWarrantyRepairerLoggedDate(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyRepairs;
                            break;
                        case pNonWarrantyRepairerReference:
                            UpdateNonWarrantyRepairerReference(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyRepairs;
                            break;
                        case pNonWarrantyRepairerCompletedDate:
                            UpdateNonWarrantyRepairerCompletedDate(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyRepairs;
                            break;
                        case pNonWarrantyIsInsuranceClaim:
                            UpdateNonWarrantyIsInsuranceClaim(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyInsurance;
                            break;
                        case pInsuranceLossOrDamageDate:
                            UpdateInsuranceLossOrDamageDate(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyInsurance;
                            break;
                        case pInsuranceEventLocation:
                            UpdateInsuranceEventLocation(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyInsurance;
                            break;
                        case pInsuranceDescription:
                            UpdateInsuranceDescription(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyInsurance;
                            break;
                        case pInsuranceThirdPartyCaused:
                            UpdateInsuranceThirdPartyCaused(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyInsurance;
                            break;
                        case pInsuranceThirdPartyCausedName:
                            UpdateInsuranceThirdPartyCausedName(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyInsurance;
                            break;
                        case pInsuranceThirdPartyCausedWhy:
                            UpdateInsuranceThirdPartyCausedWhy(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyInsurance;
                            break;
                        case pInsuranceWitnessesNamesAddresses:
                            UpdateInsuranceWitnessesNamesAddresses(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyInsurance;
                            break;
                        case pInsuranceBurglaryTheftMethodOfEntry:
                            UpdateInsuranceBurglaryTheftMethodOfEntry(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyInsurance;
                            break;
                        case pInsurancePropertyLastSeenDate:
                            UpdateInsurancePropertyLastSeenDate(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyInsurance;
                            break;
                        case pInsurancePoliceNotified:
                            UpdateInsurancePoliceNotified(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyInsurance;
                            break;
                        case pInsurancePoliceNotifiedStation:
                            UpdateInsurancePoliceNotifiedStation(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyInsurance;
                            break;
                        case pInsurancePoliceNotifiedDate:
                            UpdateInsurancePoliceNotifiedDate(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyInsurance;
                            break;
                        case pInsurancePoliceNotifiedCrimeReportNo:
                            UpdateInsurancePoliceNotifiedCrimeReportNo(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyInsurance;
                            break;
                        case pInsuranceRecoverReduceAction:
                            UpdateInsuranceRecoverReduceAction(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyInsurance;
                            break;
                        case pInsuranceOtherInterestedParties:
                            UpdateInsuranceOtherInterestedParties(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyInsurance;
                            break;
                        case pInsuranceDateOfPurchase:
                            UpdateInsuranceDateOfPurchase(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyInsurance;
                            break;
                        case pInsuranceClaimFormSentDate:
                            resultData = UpdateInsuranceClaimFormSentDate(job, value);
                            resultUrlFragment = pJobDetailsTabNonWarrantyInsurance;
                            break;
                        case pWarrantyExternalName:
                            UpdateWarrantyExternalName(job, value);
                            resultUrlFragment = pJobDetailsTabWarranty;
                            break;
                        case pWarrantyExternalLoggedDate:
                            UpdateWarrantyExternalLoggedDate(job, value);
                            resultUrlFragment = pJobDetailsTabWarranty;
                            break;
                        case pWarrantyExternalReference:
                            UpdateWarrantyExternalReference(job, value);
                            resultUrlFragment = pJobDetailsTabWarranty;
                            break;
                        case pWarrantyExternalCompletedDate:
                            UpdateWarrantyExternalCompletedDate(job, value);
                            resultUrlFragment = pJobDetailsTabWarranty;
                            break;
                        default:
                            throw new Exception("Invalid Update Key");
                    }
                }
                else
                {
                    throw new Exception("Invalid Job Id");
                }
                if (redirect.HasValue && redirect.Value)
                    return this.RedirectToAction(MVC.Job.Show(job.Id), resultUrlFragment);
                    //return RedirectToAction(MVC.Job.Show(job.Id));
                else
                {
                    if (resultData != null)
                    {
                        return Json(resultData, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("OK", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                if (redirect.HasValue && redirect.Value)
                    throw;
                else
                    return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        #region Update Shortcut Methods
        public virtual ActionResult UpdateExpectedClosedDate(int id, string ExpectedClosedDate, Nullable<bool> redirect = null)
        {
            return Update(id, pExpectedClosedDate, ExpectedClosedDate, redirect);
        }
        public virtual ActionResult UpdateDeviceHeldLocation(int id, string DeviceHeldLocation, Nullable<bool> redirect = null)
        {
            return Update(id, pDeviceHeldLocation, DeviceHeldLocation, redirect);
        }
        public virtual ActionResult UpdateFlags(int id, string Flags, Nullable<bool> redirect = null)
        {
            return Update(id, pFlags, Flags, redirect);
        }

        #region NonWarranty
        public virtual ActionResult UpdateNonWarrantyAccountingChargeRequired(int id, string AccountingChargeRequiredDate, Nullable<bool> redirect = null)
        {
            return Update(id, pNonWarrantyAccountingChargeRequired, AccountingChargeRequiredDate, redirect);
        }
        public virtual ActionResult UpdateNonWarrantyAccountingChargeAdded(int id, string AccountingChargeAddedDate, Nullable<bool> redirect = null)
        {
            return Update(id, pNonWarrantyAccountingChargeAdded, AccountingChargeAddedDate, redirect);
        }
        public virtual ActionResult UpdateNonWarrantyAccountingChargePaid(int id, string AccountingChargePaidDate, Nullable<bool> redirect = null)
        {
            return Update(id, pNonWarrantyAccountingChargePaid, AccountingChargePaidDate, redirect);
        }
        public virtual ActionResult UpdateNonWarrantyPurchaseOrderRaised(int id, string PurchaseOrderRaisedDate, Nullable<bool> redirect = null)
        {
            return Update(id, pNonWarrantyPurchaseOrderRaised, PurchaseOrderRaisedDate, redirect);
        }
        public virtual ActionResult UpdateNonWarrantyPurchaseOrderReference(int id, string PurchaseOrderReference, Nullable<bool> redirect = null)
        {
            return Update(id, pNonWarrantyPurchaseOrderReference, PurchaseOrderReference, redirect);
        }
        public virtual ActionResult UpdateNonWarrantyPurchaseOrderSent(int id, string PurchaseOrderSentDate, Nullable<bool> redirect = null)
        {
            return Update(id, pNonWarrantyPurchaseOrderSent, PurchaseOrderSentDate, redirect);
        }
        public virtual ActionResult UpdateNonWarrantyInvoiceReceived(int id, string InvoiceReceivedDate, Nullable<bool> redirect = null)
        {
            return Update(id, pNonWarrantyInvoiceReceived, InvoiceReceivedDate, redirect);
        }
        public virtual ActionResult UpdateNonWarrantyRepairerName(int id, string RepairerName, Nullable<bool> redirect = null)
        {
            return Update(id, pNonWarrantyRepairerName, RepairerName, redirect);
        }
        public virtual ActionResult UpdateNonWarrantyRepairerLoggedDate(int id, string RepairerLoggedDate, Nullable<bool> redirect = null)
        {
            return Update(id, pNonWarrantyRepairerLoggedDate, RepairerLoggedDate, redirect);
        }
        public virtual ActionResult UpdateNonWarrantyRepairerReference(int id, string RepairerReference, Nullable<bool> redirect = null)
        {
            return Update(id, pNonWarrantyRepairerReference, RepairerReference, redirect);
        }
        public virtual ActionResult UpdateNonWarrantyRepairerCompletedDate(int id, string RepairerCompletedDate, Nullable<bool> redirect = null)
        {
            return Update(id, pNonWarrantyRepairerCompletedDate, RepairerCompletedDate, redirect);
        }
        public virtual ActionResult UpdateNonWarrantyIsInsuranceClaim(int id, bool IsInsuranceClaim, Nullable<bool> redirect = null)
        {
            return Update(id, pNonWarrantyIsInsuranceClaim, IsInsuranceClaim.ToString(), redirect);
        }
        #endregion

        #region Insurance

        public virtual ActionResult UpdateInsuranceLossOrDamageDate(int id, string LossOrDamageDate, Nullable<bool> redirect = null)
        {
            return Update(id, pInsuranceLossOrDamageDate, LossOrDamageDate, redirect);
        }
        public virtual ActionResult UpdateInsuranceEventLocation(int id, string EventLocation, Nullable<bool> redirect = null)
        {
            return Update(id, pInsuranceEventLocation, EventLocation, redirect);
        }
        public virtual ActionResult UpdateInsuranceDescription(int id, string Description, Nullable<bool> redirect = null)
        {
            return Update(id, pInsuranceDescription, Description, redirect);
        }
        public virtual ActionResult UpdateInsuranceThirdPartyCaused(int id, string ThirdPartyCaused, Nullable<bool> redirect = null)
        {
            return Update(id, pInsuranceThirdPartyCaused, ThirdPartyCaused, redirect);
        }
        public virtual ActionResult UpdateInsuranceThirdPartyCausedName(int id, string ThirdPartyCausedName, Nullable<bool> redirect = null)
        {
            return Update(id, pInsuranceThirdPartyCausedName, ThirdPartyCausedName, redirect);
        }
        public virtual ActionResult UpdateInsuranceThirdPartyCausedWhy(int id, string ThirdPartyCausedWhy, Nullable<bool> redirect = null)
        {
            return Update(id, pInsuranceThirdPartyCausedWhy, ThirdPartyCausedWhy, redirect);
        }
        public virtual ActionResult UpdateInsuranceWitnessesNamesAddresses(int id, string WitnessesNamesAddresses, Nullable<bool> redirect = null)
        {
            return Update(id, pInsuranceWitnessesNamesAddresses, WitnessesNamesAddresses, redirect);
        }
        public virtual ActionResult UpdateInsuranceBurglaryTheftMethodOfEntry(int id, string BurglaryTheftMethodOfEntry, Nullable<bool> redirect = null)
        {
            return Update(id, pInsuranceBurglaryTheftMethodOfEntry, BurglaryTheftMethodOfEntry, redirect);
        }
        public virtual ActionResult UpdateInsurancePropertyLastSeenDate(int id, string PropertyLastSeenDate, Nullable<bool> redirect = null)
        {
            return Update(id, pInsurancePropertyLastSeenDate, PropertyLastSeenDate, redirect);
        }
        public virtual ActionResult UpdateInsurancePoliceNotified(int id, string PoliceNotified, Nullable<bool> redirect = null)
        {
            return Update(id, pInsurancePoliceNotified, PoliceNotified, redirect);
        }
        public virtual ActionResult UpdateInsurancePoliceNotifiedStation(int id, string PoliceNotifiedStation, Nullable<bool> redirect = null)
        {
            return Update(id, pInsurancePoliceNotifiedStation, PoliceNotifiedStation, redirect);
        }
        public virtual ActionResult UpdateInsurancePoliceNotifiedDate(int id, string PoliceNotifiedDate, Nullable<bool> redirect = null)
        {
            return Update(id, pInsurancePoliceNotifiedDate, PoliceNotifiedDate, redirect);
        }
        public virtual ActionResult UpdateInsurancePoliceNotifiedCrimeReportNo(int id, string PoliceNotifiedCrimeReportNo, Nullable<bool> redirect = null)
        {
            return Update(id, pInsurancePoliceNotifiedCrimeReportNo, PoliceNotifiedCrimeReportNo, redirect);
        }
        public virtual ActionResult UpdateInsuranceRecoverReduceAction(int id, string RecoverReduceAction, Nullable<bool> redirect = null)
        {
            return Update(id, pInsuranceRecoverReduceAction, RecoverReduceAction, redirect);
        }
        public virtual ActionResult UpdateInsuranceOtherInterestedParties(int id, string OtherInterestedParties, Nullable<bool> redirect = null)
        {
            return Update(id, pInsuranceOtherInterestedParties, OtherInterestedParties, redirect);
        }
        public virtual ActionResult UpdateInsuranceDateOfPurchase(int id, string DateOfPurchase, Nullable<bool> redirect = null)
        {
            return Update(id, pInsuranceDateOfPurchase, DateOfPurchase, redirect);
        }
        public virtual ActionResult UpdateInsuranceClaimFormSentDate(int id, string ClaimFormSentDate, Nullable<bool> redirect = null)
        {
            return Update(id, pInsuranceClaimFormSentDate, ClaimFormSentDate, redirect);
        }
        public virtual ActionResult UpdateInsuranceClaimFormSentUserId(int id, string ClaimFormSentUserId, Nullable<bool> redirect = null)
        {
            return Update(id, pInsuranceClaimFormSentUserId, ClaimFormSentUserId, redirect);
        }

        #endregion

        #region Warranty
        public virtual ActionResult UpdateWarrantyExternalName(int id, string ExternalName, Nullable<bool> redirect = null)
        {
            return Update(id, pWarrantyExternalName, ExternalName, redirect);
        }
        public virtual ActionResult UpdateWarrantyExternalLoggedDate(int id, string ExternalLoggedDate, Nullable<bool> redirect = null)
        {
            return Update(id, pWarrantyExternalLoggedDate, ExternalLoggedDate, redirect);
        }
        public virtual ActionResult UpdateWarrantyExternalReference(int id, string ExternalReference, Nullable<bool> redirect = null)
        {
            return Update(id, pWarrantyExternalReference, ExternalReference, redirect);
        }
        public virtual ActionResult UpdateWarrantyExternalCompletedDate(int id, string ExternalCompletedDate, Nullable<bool> redirect = null)
        {
            return Update(id, pWarrantyExternalCompletedDate, ExternalCompletedDate, redirect);
        }
        #endregion

        #endregion

        #region Update Properties
        private void UpdateExpectedClosedDate(Job job, string ExpectedClosedDate)
        {
            if (!string.IsNullOrEmpty(ExpectedClosedDate))
            {
                DateTime ecd;
                if (DateTime.TryParse(ExpectedClosedDate, out ecd))
                {
                    ecd = job.ValidateDateAfterOpened(ecd);
                    job.ExpectedClosedDate = ecd;
                }
                else
                {
                    throw new Exception("Invalid Date Format");
                }
            }
            else
            {
                job.ExpectedClosedDate = null;
            }
            dbContext.SaveChanges();
            job.BroadcastUpdate();
        }
        private void UpdateDeviceHeldLocation(Job job, string DeviceHeldLocation)
        {
            if (string.IsNullOrWhiteSpace(DeviceHeldLocation))
                job.DeviceHeldLocation = null;
            else
                job.DeviceHeldLocation = DeviceHeldLocation;

            dbContext.SaveChanges();
        }
        private void UpdateFlags(Job job, string Flags)
        {
            // Only User Management Job Supports Flags at the moment
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.UMgmt, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for User Management Jobs");
            }

            if (string.IsNullOrWhiteSpace(Flags))
            {
                if (!job.Flags.HasValue)
                {
                    job.Flags = null;
                    dbContext.SaveChanges();
                }
            }
            else
            {
                long flags;
                if (Flags.Contains(','))
                {
                    flags = 0;
                    foreach (var fs in Flags.Split(','))
                    {
                        long fi;
                        if (!long.TryParse(fs, out fi))
                            throw new Exception("Invalid Int64 Format");
                        else
                            flags = flags | fi;
                    }
                }
                else
                {
                    if (!long.TryParse(Flags, out flags))
                        throw new Exception("Invalid Int64 Format");
                }
                if (flags == 0)
                {
                    if (job.Flags.HasValue)
                    {
                        job.Flags = null;
                        dbContext.SaveChanges();
                    }
                }
                else
                {
                    if (!job.Flags.HasValue || job.Flags.Value != flags)
                    {
                        job.Flags = flags;
                        dbContext.SaveChanges();
                    }
                }
            }
        }

        #region Job NonWarranty

        private void UpdateNonWarrantyIsInsuranceClaim(Job job, string IsInsuranceClaim)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }
            bool bIsInsuranceClaim;
            if (string.IsNullOrEmpty(IsInsuranceClaim) || !bool.TryParse(IsInsuranceClaim, out bIsInsuranceClaim))
            {
                throw new Exception("Invalid IsInsuranceClaim Value");
            }

            job.JobMetaNonWarranty.IsInsuranceClaim = bIsInsuranceClaim;
            if (job.JobMetaInsurance == null)
            {
                var jmi = new Disco.Models.Repository.JobMetaInsurance();
                jmi.JobId = job.Id;

                if (job.Device.DeviceBatch != null)
                {
                    jmi.DateOfPurchase = job.Device.DeviceBatch.PurchaseDate;
                }
                else
                {
                    if (!string.IsNullOrEmpty(job.DeviceSerialNumber))
                    {
                        jmi.DateOfPurchase = job.Device.DeviceModel.DefaultPurchaseDate;
                    }
                }
                if (job.User != null)
                    jmi.OtherInterestedParties = job.User.DisplayName;

                job.JobMetaInsurance = jmi;
                dbContext.JobMetaInsurances.Add(jmi);
            }
            dbContext.SaveChanges();
        }

        private Models.Job._DateChangeModel UpdateNonWarrantyAccountingChargeRequired(Job job, string AccountingChargeRequiredDate)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            if (string.IsNullOrEmpty(AccountingChargeRequiredDate))
            {
                job.JobMetaNonWarranty.AccountingChargeRequiredDate = null;
            }
            else
            {
                DateTime d;
                if (DateTime.TryParse(AccountingChargeRequiredDate, out d))
                {
                    d = job.ValidateDateAfterOpened(d);
                    job.JobMetaNonWarranty.AccountingChargeRequiredDate = d;
                }
                else
                {
                    throw new Exception("Invalid Date Format");
                }
            }
            job.JobMetaNonWarranty.AccountingChargeRequiredUserId = DiscoApplication.CurrentUser.Id;
            dbContext.SaveChanges();
            job.BroadcastUpdate();
            return new Models.Job._DateChangeModel()
            {
                Id = job.Id,
                Result = "OK",
                UserDescription = DiscoApplication.CurrentUser.ToString()
            }.SetDateTime(job.JobMetaNonWarranty.AccountingChargeRequiredDate);
        }
        private Models.Job._DateChangeModel UpdateNonWarrantyAccountingChargeAdded(Job job, string AccountingChargeAddedDate)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            if (string.IsNullOrEmpty(AccountingChargeAddedDate))
            {
                job.JobMetaNonWarranty.AccountingChargeAddedDate = null;
            }
            else
            {
                DateTime d;
                if (DateTime.TryParse(AccountingChargeAddedDate, out d))
                {
                    d = job.ValidateDateAfterOpened(d);
                    job.JobMetaNonWarranty.AccountingChargeAddedDate = d;
                }
                else
                {
                    throw new Exception("Invalid Date Format");
                }
            }
            job.JobMetaNonWarranty.AccountingChargeAddedUserId = DiscoApplication.CurrentUser.Id;
            dbContext.SaveChanges();
            job.BroadcastUpdate();
            return new Models.Job._DateChangeModel()
            {
                Id = job.Id,
                Result = "OK",
                UserDescription = DiscoApplication.CurrentUser.ToString()
            }.SetDateTime(job.JobMetaNonWarranty.AccountingChargeAddedDate);
        }
        private Models.Job._DateChangeModel UpdateNonWarrantyAccountingChargePaid(Job job, string AccountingChargePaidDate)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            if (string.IsNullOrEmpty(AccountingChargePaidDate))
            {
                job.JobMetaNonWarranty.AccountingChargePaidDate = null;
            }
            else
            {
                DateTime d;
                if (DateTime.TryParse(AccountingChargePaidDate, out d))
                {
                    d = job.ValidateDateAfterOpened(d);
                    job.JobMetaNonWarranty.AccountingChargePaidDate = d;
                }
                else
                {
                    throw new Exception("Invalid Date Format");
                }
            }
            job.JobMetaNonWarranty.AccountingChargePaidUserId = DiscoApplication.CurrentUser.Id;
            dbContext.SaveChanges();
            job.BroadcastUpdate();
            return new Models.Job._DateChangeModel()
            {
                Id = job.Id,
                Result = "OK",
                UserDescription = DiscoApplication.CurrentUser.ToString()
            }.SetDateTime(job.JobMetaNonWarranty.AccountingChargePaidDate);
        }
        private Models.Job._DateChangeModel UpdateNonWarrantyPurchaseOrderRaised(Job job, string PurchaseOrderRaisedDate)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            if (string.IsNullOrEmpty(PurchaseOrderRaisedDate))
            {
                job.JobMetaNonWarranty.PurchaseOrderRaisedDate = null;
            }
            else
            {
                DateTime d;
                if (DateTime.TryParse(PurchaseOrderRaisedDate, out d))
                {
                    d = job.ValidateDateAfterOpened(d);
                    job.JobMetaNonWarranty.PurchaseOrderRaisedDate = d;
                }
                else
                {
                    throw new Exception("Invalid Date Format");
                }
            }
            job.JobMetaNonWarranty.PurchaseOrderRaisedUserId = DiscoApplication.CurrentUser.Id;
            dbContext.SaveChanges();
            return new Models.Job._DateChangeModel()
            {
                Id = job.Id,
                Result = "OK",
                UserDescription = DiscoApplication.CurrentUser.ToString()
            }.SetDateTime(job.JobMetaNonWarranty.PurchaseOrderRaisedDate);
        }
        private void UpdateNonWarrantyPurchaseOrderReference(Job job, string PurchaseOrderReference)
        {
            if (string.IsNullOrWhiteSpace(PurchaseOrderReference))
                job.JobMetaNonWarranty.PurchaseOrderReference = null;
            else
                job.JobMetaNonWarranty.PurchaseOrderReference = PurchaseOrderReference;

            dbContext.SaveChanges();
        }
        private Models.Job._DateChangeModel UpdateNonWarrantyPurchaseOrderSent(Job job, string PurchaseOrderSentDate)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            if (string.IsNullOrEmpty(PurchaseOrderSentDate))
            {
                job.JobMetaNonWarranty.PurchaseOrderSentDate = null;
            }
            else
            {
                DateTime d;
                if (DateTime.TryParse(PurchaseOrderSentDate, out d))
                {
                    d = job.ValidateDateAfterOpened(d);
                    job.JobMetaNonWarranty.PurchaseOrderSentDate = d;
                }
                else
                {
                    throw new Exception("Invalid Date Format");
                }
            }
            job.JobMetaNonWarranty.PurchaseOrderSentUserId = DiscoApplication.CurrentUser.Id;
            dbContext.SaveChanges();
            return new Models.Job._DateChangeModel()
            {
                Id = job.Id,
                Result = "OK",
                UserDescription = DiscoApplication.CurrentUser.ToString()
            }.SetDateTime(job.JobMetaNonWarranty.PurchaseOrderSentDate);
        }
        private Models.Job._DateChangeModel UpdateNonWarrantyInvoiceReceived(Job job, string InvoiceReceivedDate)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            if (string.IsNullOrEmpty(InvoiceReceivedDate))
            {
                job.JobMetaNonWarranty.InvoiceReceivedDate = null;
            }
            else
            {
                DateTime d;
                if (DateTime.TryParse(InvoiceReceivedDate, out d))
                {
                    d = job.ValidateDateAfterOpened(d);
                    job.JobMetaNonWarranty.InvoiceReceivedDate = d;
                }
                else
                {
                    throw new Exception("Invalid Date Format");
                }
            }
            job.JobMetaNonWarranty.InvoiceReceivedUserId = DiscoApplication.CurrentUser.Id;
            dbContext.SaveChanges();
            return new Models.Job._DateChangeModel()
            {
                Id = job.Id,
                Result = "OK",
                UserDescription = DiscoApplication.CurrentUser.ToString()
            }.SetDateTime(job.JobMetaNonWarranty.InvoiceReceivedDate);
        }

        private void UpdateNonWarrantyRepairerName(Job job, string RepairerName)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            if (string.IsNullOrWhiteSpace(RepairerName))
            {
                job.JobMetaNonWarranty.RepairerName = null;
            }
            else
            {
                job.JobMetaNonWarranty.RepairerName = RepairerName.Trim();
            }
            dbContext.SaveChanges();
        }
        private void UpdateNonWarrantyRepairerLoggedDate(Job job, string RepairerLoggedDate)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            if (string.IsNullOrEmpty(RepairerLoggedDate))
            {
                job.JobMetaNonWarranty.RepairerLoggedDate = null;
            }
            else
            {
                if (RepairerLoggedDate.Equals("Now", StringComparison.InvariantCultureIgnoreCase))
                {
                    job.JobMetaNonWarranty.RepairerLoggedDate = DateTime.Now;
                }
                else
                {
                    DateTime d;
                    if (DateTime.TryParse(RepairerLoggedDate, out d))
                    {
                        job.JobMetaNonWarranty.RepairerLoggedDate = d;
                    }
                    else
                    {
                        throw new Exception("Invalid Date Format");
                    }
                }
            }
            dbContext.SaveChanges();
        }
        private void UpdateNonWarrantyRepairerReference(Job job, string RepairerReference)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            if (string.IsNullOrWhiteSpace(RepairerReference))
            {
                job.JobMetaNonWarranty.RepairerReference = null;
            }
            else
            {
                job.JobMetaNonWarranty.RepairerReference = RepairerReference.Trim();
            }
            dbContext.SaveChanges();
        }
        private void UpdateNonWarrantyRepairerCompletedDate(Job job, string RepairerCompletedDate)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            if (string.IsNullOrEmpty(RepairerCompletedDate))
            {
                job.JobMetaNonWarranty.RepairerCompletedDate = null;
            }
            else
            {
                if (RepairerCompletedDate.Equals("Now", StringComparison.InvariantCultureIgnoreCase))
                {
                    job.JobMetaNonWarranty.RepairerCompletedDate = DateTime.Now;
                }
                else
                {
                    DateTime d;
                    if (DateTime.TryParse(RepairerCompletedDate, out d))
                    {
                        job.JobMetaNonWarranty.RepairerCompletedDate = d;
                    }
                    else
                    {
                        throw new Exception("Invalid Date Format");
                    }
                }
            }
            dbContext.SaveChanges();
        }

        #endregion

        #region Job Insurance

        private Models.Job._DateChangeModel UpdateInsuranceClaimFormSentDate(Job job, string ClaimFormSentDate)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            if (string.IsNullOrEmpty(ClaimFormSentDate))
            {
                job.JobMetaInsurance.ClaimFormSentDate = null;
            }
            else
            {
                if (ClaimFormSentDate.Equals("Now", StringComparison.InvariantCultureIgnoreCase))
                {
                    job.JobMetaInsurance.ClaimFormSentDate = DateTime.Now;
                }
                else
                {
                    DateTime d;
                    if (DateTime.TryParse(ClaimFormSentDate, out d))
                    {
                        d = job.ValidateDateAfterOpened(d);
                        job.JobMetaInsurance.ClaimFormSentDate = d;
                    }
                    else
                    {
                        throw new Exception("Invalid Date Format");
                    }
                }
            }
            job.JobMetaInsurance.ClaimFormSentUserId = DiscoApplication.CurrentUser.Id;
            dbContext.SaveChanges();
            return new Models.Job._DateChangeModel()
            {
                Id = job.Id,
                Result = "OK",
                UserDescription = DiscoApplication.CurrentUser.ToString()
            }.SetDateTime(job.JobMetaInsurance.ClaimFormSentDate);
        }

        private void UpdateInsuranceDateOfPurchase(Job job, string DateOfPurchase)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            if (string.IsNullOrEmpty(DateOfPurchase))
            {
                job.JobMetaInsurance.DateOfPurchase = null;
            }
            else
            {
                DateTime dt;
                if (!DateTime.TryParse(DateOfPurchase, out dt))
                {
                    throw new Exception("Invalid DateTime Value");
                }
                job.JobMetaInsurance.DateOfPurchase = dt;
            }
            dbContext.SaveChanges();
        }

        private void UpdateInsuranceOtherInterestedParties(Job job, string OtherInterestedParties)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            if (string.IsNullOrWhiteSpace(OtherInterestedParties))
            {
                job.JobMetaInsurance.OtherInterestedParties = null;
            }
            else
            {
                job.JobMetaInsurance.OtherInterestedParties = OtherInterestedParties.Trim();
            }
            dbContext.SaveChanges();
        }

        private void UpdateInsuranceRecoverReduceAction(Job job, string RecoverReduceAction)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            if (string.IsNullOrWhiteSpace(RecoverReduceAction))
            {
                job.JobMetaInsurance.RecoverReduceAction = null;
            }
            else
            {
                job.JobMetaInsurance.RecoverReduceAction = RecoverReduceAction.Trim();
            }
            dbContext.SaveChanges();
        }

        private void UpdateInsurancePoliceNotifiedCrimeReportNo(Job job, string PoliceNotifiedCrimeReportNo)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            if (string.IsNullOrWhiteSpace(PoliceNotifiedCrimeReportNo))
            {
                job.JobMetaInsurance.PoliceNotifiedCrimeReportNo = null;
            }
            else
            {
                job.JobMetaInsurance.PoliceNotifiedCrimeReportNo = PoliceNotifiedCrimeReportNo.Trim();
            }
            dbContext.SaveChanges();
        }

        private void UpdateInsurancePoliceNotifiedDate(Job job, string PoliceNotifiedDate)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            if (string.IsNullOrEmpty(PoliceNotifiedDate))
            {
                job.JobMetaInsurance.PoliceNotifiedDate = null;
            }
            else
            {
                DateTime dt;
                if (!DateTime.TryParse(PoliceNotifiedDate, out dt))
                {
                    throw new Exception("Invalid DateTime Value");
                }
                job.JobMetaInsurance.PoliceNotifiedDate = dt;
            }
            dbContext.SaveChanges();
        }

        private void UpdateInsurancePoliceNotifiedStation(Job job, string PoliceNotifiedStation)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            if (string.IsNullOrWhiteSpace(PoliceNotifiedStation))
            {
                job.JobMetaInsurance.PoliceNotifiedStation = null;
            }
            else
            {
                job.JobMetaInsurance.PoliceNotifiedStation = PoliceNotifiedStation.Trim();
            }
            dbContext.SaveChanges();
        }

        private void UpdateInsurancePoliceNotified(Job job, string PoliceNotified)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            bool b;
            if (string.IsNullOrEmpty(PoliceNotified) || !bool.TryParse(PoliceNotified, out b))
            {
                throw new Exception("Invalid Boolean Value");
            }

            job.JobMetaInsurance.PoliceNotified = b;
            dbContext.SaveChanges();
        }

        private void UpdateInsurancePropertyLastSeenDate(Job job, string PropertyLastSeenDate)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            if (string.IsNullOrEmpty(PropertyLastSeenDate))
            {
                job.JobMetaInsurance.PropertyLastSeenDate = null;
            }
            else
            {
                DateTime d;
                if (DateTime.TryParse(PropertyLastSeenDate, out d))
                {
                    job.JobMetaInsurance.PropertyLastSeenDate = d;
                }
                else
                {
                    throw new Exception("Invalid Date Format");
                }
            }
            dbContext.SaveChanges();
        }

        private void UpdateInsuranceBurglaryTheftMethodOfEntry(Job job, string BurglaryTheftMethodOfEntry)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            if (string.IsNullOrWhiteSpace(BurglaryTheftMethodOfEntry))
            {
                job.JobMetaInsurance.BurglaryTheftMethodOfEntry = null;
            }
            else
            {
                job.JobMetaInsurance.BurglaryTheftMethodOfEntry = BurglaryTheftMethodOfEntry.Trim();
            }
            dbContext.SaveChanges();
        }

        private void UpdateInsuranceWitnessesNamesAddresses(Job job, string WitnessesNamesAddresses)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            if (string.IsNullOrWhiteSpace(WitnessesNamesAddresses))
            {
                job.JobMetaInsurance.WitnessesNamesAddresses = null;
            }
            else
            {
                job.JobMetaInsurance.WitnessesNamesAddresses = WitnessesNamesAddresses.Trim();
            }
            dbContext.SaveChanges();
        }

        private void UpdateInsuranceThirdPartyCausedWhy(Job job, string ThirdPartyCausedWhy)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            if (string.IsNullOrWhiteSpace(ThirdPartyCausedWhy))
            {
                job.JobMetaInsurance.ThirdPartyCausedWhy = null;
            }
            else
            {
                job.JobMetaInsurance.ThirdPartyCausedWhy = ThirdPartyCausedWhy.Trim();
            }
            dbContext.SaveChanges();
        }

        private void UpdateInsuranceThirdPartyCausedName(Job job, string ThirdPartyCausedName)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            if (string.IsNullOrWhiteSpace(ThirdPartyCausedName))
            {
                job.JobMetaInsurance.ThirdPartyCausedName = null;
            }
            else
            {
                job.JobMetaInsurance.ThirdPartyCausedName = ThirdPartyCausedName.Trim();
            }
            dbContext.SaveChanges();
        }

        private void UpdateInsuranceThirdPartyCaused(Job job, string ThirdPartyCaused)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            bool b;
            if (string.IsNullOrEmpty(ThirdPartyCaused) || !bool.TryParse(ThirdPartyCaused, out b))
            {
                throw new Exception("Invalid Boolean Value");
            }

            job.JobMetaInsurance.ThirdPartyCaused = b;
            dbContext.SaveChanges();
        }

        private void UpdateInsuranceDescription(Job job, string Description)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                job.JobMetaInsurance.Description = null;
            }
            else
            {
                job.JobMetaInsurance.Description = Description.Trim();
            }
            dbContext.SaveChanges();
        }

        private void UpdateInsuranceEventLocation(Job job, string EventLocation)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            if (string.IsNullOrWhiteSpace(EventLocation))
            {
                job.JobMetaInsurance.EventLocation = null;
            }
            else
            {
                job.JobMetaInsurance.EventLocation = EventLocation.Trim();
            }
            dbContext.SaveChanges();
        }

        private void UpdateInsuranceLossOrDamageDate(Job job, string LossOrDamageDate)
        {
            // Validate Is NonWarranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HNWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware NonWarranty Jobs");
            }

            if (string.IsNullOrEmpty(LossOrDamageDate))
            {
                job.JobMetaInsurance.LossOrDamageDate = null;
            }
            else
            {
                DateTime d;
                if (DateTime.TryParse(LossOrDamageDate, out d))
                {
                    job.JobMetaInsurance.LossOrDamageDate = d;
                }
                else
                {
                    throw new Exception("Invalid Date Format");
                }
            }
            dbContext.SaveChanges();
        }
        #endregion

        #region Job Warranty
        private void UpdateWarrantyExternalName(Job job, string ExternalName)
        {
            // Validate Is Warranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware Warranty Jobs");
            }

            if (string.IsNullOrWhiteSpace(ExternalName))
            {
                job.JobMetaWarranty.ExternalName = null;
            }
            else
            {
                job.JobMetaWarranty.ExternalName = ExternalName.Trim();
            }
            dbContext.SaveChanges();
        }

        private void UpdateWarrantyExternalLoggedDate(Job job, string ExternalLoggedDate)
        {
            // Validate Is Warranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware Warranty Jobs");
            }

            if (string.IsNullOrEmpty(ExternalLoggedDate))
            {
                job.JobMetaWarranty.ExternalLoggedDate = null;
            }
            else
            {
                DateTime d;
                if (DateTime.TryParse(ExternalLoggedDate, out d))
                {
                    job.JobMetaWarranty.ExternalLoggedDate = d;
                }
                else
                {
                    throw new Exception("Invalid Date Format");
                }
            }
            dbContext.SaveChanges();
        }

        private void UpdateWarrantyExternalReference(Job job, string ExternalReference)
        {
            // Validate Is Warranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware Warranty Jobs");
            }

            if (string.IsNullOrWhiteSpace(ExternalReference))
            {
                job.JobMetaWarranty.ExternalReference = null;
            }
            else
            {
                job.JobMetaWarranty.ExternalReference = ExternalReference.Trim();
            }
            dbContext.SaveChanges();
        }
        private void UpdateWarrantyExternalCompletedDate(Job job, string ExternalCompletedDate)
        {
            // Validate Is Warranty Job
            if (!job.JobTypeId.Equals(JobType.JobTypeIds.HWar, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("This property can only be set for Hardware Warranty Jobs");
            }

            if (string.IsNullOrEmpty(ExternalCompletedDate))
            {
                job.JobMetaWarranty.ExternalCompletedDate = null;
            }
            else
            {
                if (ExternalCompletedDate.Equals("Now", StringComparison.InvariantCultureIgnoreCase))
                {
                    job.JobMetaWarranty.ExternalCompletedDate = DateTime.Now;
                }
                else
                {
                    DateTime d;
                    if (DateTime.TryParse(ExternalCompletedDate, out d))
                    {
                        job.JobMetaWarranty.ExternalCompletedDate = d;
                    }
                    else
                    {
                        throw new Exception("Invalid Date Format");
                    }
                }
            }
            dbContext.SaveChanges();
        }
        #endregion

        #endregion

        #region Job Actions
        public virtual ActionResult UpdateSubTypes(int id, List<string> SubTypes = null, Nullable<bool> AddComponents = null, Nullable<bool> redirect = null)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");
                if (SubTypes == null)
                    throw new ArgumentNullException("SubTypes");
                if (SubTypes.Count == 0)
                    throw new ArgumentException("The job must contain at least one Sub Type", "SubTypes");
                if (AddComponents == null)
                    AddComponents = false;

                dbContext.Configuration.LazyLoadingEnabled = true;
                var job = dbContext.Jobs.Include("JobSubTypes").Where(j => j.Id == id).FirstOrDefault();
                if (job == null)
                    throw new Exception("Invalid Job Id");

                var subTypes = dbContext.JobSubTypes.Where(jst => SubTypes.Contains(jst.JobTypeId + "_" + jst.Id)).ToList();
                job.UpdateSubTypes(dbContext, subTypes, AddComponents.Value, DiscoApplication.CurrentUser);
                dbContext.SaveChanges();
                if (redirect.HasValue && redirect.Value)
                    return RedirectToAction(MVC.Job.Show(job.Id));
                else
                    return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (redirect.HasValue && redirect.Value)
                    throw;
                else
                    return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
            }
        }
        public virtual ActionResult UpdateFlag(int id, long? Flag, string Reason, Nullable<bool> redirect = null)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");
                if (!Flag.HasValue || Flag.Value == 0)
                    throw new ArgumentNullException("Flag");

                var job = dbContext.Jobs.Include("JobSubTypes").Where(j => j.Id == id).FirstOrDefault();
                if (job == null)
                    throw new Exception("Invalid Job Id");

                long flag = Flag.Value;
                var validFlags = job.ValidFlags();
                Tuple<string, bool> flagStatus;
                if (validFlags.TryGetValue((flag < 0 ? flag * -1 : flag), out flagStatus))
                {
                    if (flag < 0)
                    { // Remove Flag
                        if (flagStatus.Item2)
                        {
                            job.Flags = (job.Flags ?? 0) ^ (flag * -1);
                            dbContext.SaveChanges();
                        }
                    }
                    else
                    { // Add Flag
                        if (!flagStatus.Item2)
                        {
                            job.Flags = (job.Flags ?? 0) | flag;
                        }
                        // Write Reason
                        JobLog jobLog = new JobLog()
                        {
                            JobId = job.Id,
                            TechUserId = DiscoApplication.CurrentUser.Id,
                            Timestamp = DateTime.Now,
                            Comments = string.Format("Added Flag: {0}{1}Reason: {2}", flagStatus.Item1, Environment.NewLine, Reason)
                        };
                        dbContext.JobLogs.Add(jobLog);

                        dbContext.SaveChanges();
                    }


                    if (redirect.HasValue && redirect.Value)
                        return RedirectToAction(MVC.Job.Show(job.Id));
                    else
                        return Json("OK", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Flag", "Invalid Flag");
                }
            }
            catch (Exception ex)
            {
                if (redirect.HasValue && redirect.Value)
                    throw;
                else
                    return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
            }
        }
        public virtual ActionResult WaitingForUserAction(int id, string Reason, Nullable<bool> redirect = null)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");

                dbContext.Configuration.LazyLoadingEnabled = true;
                var job = dbContext.Jobs.Where(j => j.Id == id).FirstOrDefault();
                if (job == null)
                    throw new Exception("Invalid Job Id");
                if (!job.CanWaitingForUserAction())
                    throw new InvalidOperationException("Unable to set Waiting For User Action");

                job.OnWaitingForUserAction(dbContext, DiscoApplication.CurrentUser, Reason);
                dbContext.SaveChanges();
                job.BroadcastUpdate();
                if (redirect.HasValue && redirect.Value)
                    return RedirectToAction(MVC.Job.Show(job.Id));
                else
                    return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (redirect.HasValue && redirect.Value)
                    throw;
                else
                    return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
            }
        }
        public virtual ActionResult NotWaitingForUserAction(int id, string Resolution, Nullable<bool> redirect = null)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");

                dbContext.Configuration.LazyLoadingEnabled = true;
                var job = dbContext.Jobs.Where(j => j.Id == id).FirstOrDefault();
                if (job == null)
                    throw new Exception("Invalid Job Id");
                if (!job.CanNotWaitingForUserAction())
                    throw new InvalidOperationException("Unable to set Waiting For User Action");

                job.OnNotWaitingForUserAction(dbContext, DiscoApplication.CurrentUser, Resolution);
                dbContext.SaveChanges();
                job.BroadcastUpdate();
                if (redirect.HasValue && redirect.Value)
                    return RedirectToAction(MVC.Job.Show(job.Id));
                else
                    return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (redirect.HasValue && redirect.Value)
                    throw;
                else
                    return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        public virtual ActionResult LogRepair(int id, string RepairerName, string RepairerReference, bool? redirect = null)
        {
            var j = dbContext.Jobs.Include("JobMetaNonWarranty").Where(job => job.Id == id).FirstOrDefault();
            if (j != null)
            {
                if (j.CanLogRepair())
                {
                    j.OnLogRepair(RepairerName, RepairerReference);

                    dbContext.SaveChanges();

                    if (redirect.HasValue && redirect.Value)
                        return RedirectToAction(MVC.Job.Show(id));
                    else
                        return Json("OK", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Job's state doesn't allow this action", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Invalid Job Number", JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult DeviceReadyForReturn(int id, bool redirect)
        {
            var j = dbContext.Jobs.Find(id);
            if (j != null)
            {
                if (j.CanDeviceReadyForReturn())
                {
                    j.OnDeviceReadyForReturn(DiscoApplication.CurrentUser);

                    dbContext.SaveChanges();
                    j.BroadcastUpdate();
                    if (redirect)
                        return RedirectToAction(MVC.Job.Show(id));
                    else
                        return Json("OK", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Job's state doesn't allow this action", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Invalid Job Number", JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult DeviceHeld(int id, bool redirect)
        {
            var j = dbContext.Jobs.Find(id);
            if (j != null)
            {
                if (j.CanDeviceHeld())
                {
                    j.OnDeviceHeld(DiscoApplication.CurrentUser);

                    dbContext.SaveChanges();
                    j.BroadcastUpdate();
                    if (redirect)
                        return RedirectToAction(MVC.Job.Show(id));
                    else
                        return Json("OK", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Job's state doesn't allow this action", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Invalid Job Number", JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult DeviceReturned(int id, bool redirect)
        {
            var j = dbContext.Jobs.Find(id);
            if (j != null)
            {
                if (j.CanDeviceReturned())
                {
                    j.OnDeviceReturned(DiscoApplication.CurrentUser);

                    dbContext.SaveChanges();
                    j.BroadcastUpdate();
                    if (redirect)
                        return RedirectToAction(MVC.Job.Show(id));
                    else
                        return Json("OK", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Job's state doesn't allow this action", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Invalid Job Number", JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult ForceClose(int id, string Reason, Nullable<bool> redirect = null)
        {
            var j = dbContext.Jobs.Find(id);
            dbContext.Configuration.LazyLoadingEnabled = true;
            if (j != null)
            {
                if (j.CanForceClose())
                {
                    j.OnForceClose(dbContext, DiscoApplication.CurrentUser, Reason);

                    dbContext.SaveChanges();
                    if (redirect.HasValue && redirect.Value)
                        return RedirectToAction(MVC.Job.Show(id));
                    else
                        return Json("OK", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Job's state doesn't allow this action", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Invalid Job Number", JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult Close(int id, bool redirect)
        {
            var j = dbContext.Jobs.Find(id);
            dbContext.Configuration.LazyLoadingEnabled = true;
            if (j != null)
            {
                if (j.CanClose())
                {
                    j.OnClose(DiscoApplication.CurrentUser);

                    dbContext.SaveChanges();
                    if (redirect)
                        return RedirectToAction(MVC.Job.Show(id));
                    else
                        return Json("OK", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Job's state doesn't allow this action", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Invalid Job Number", JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult Reopen(int id, bool redirect)
        {
            var j = dbContext.Jobs.Find(id);
            if (j != null)
            {
                if (j.CanReopen())
                {
                    j.OnReopen();

                    dbContext.SaveChanges();
                    j.BroadcastUpdate();
                    if (redirect)
                        return RedirectToAction(MVC.Job.Show(id));
                    else
                        return Json("OK", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Job's state doesn't allow this action", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Invalid Job Number", JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult Delete(int id, bool redirect)
        {
            var j = dbContext.Jobs.Find(id);
            dbContext.Configuration.LazyLoadingEnabled = true;
            if (j != null)
            {
                if (j.CanDelete())
                {
                    j.OnDelete(dbContext);

                    dbContext.SaveChanges();
                    if (redirect)
                        return RedirectToAction(MVC.Job.Index());
                    else
                        return Json("OK", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Job's state doesn't allow this action", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Invalid Job Number", JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult ConvertHWarToHNWar(int id, bool redirect)
        {
            var j = dbContext.Jobs.Find(id);
            dbContext.Configuration.LazyLoadingEnabled = true;
            if (j != null)
            {
                if (j.CanConvertHWarToHNWar())
                {
                    j.OnConvertHWarToHNWar(dbContext);

                    dbContext.SaveChanges();
                    if (redirect)
                        return RedirectToAction(MVC.Job.Show(j.Id));
                    else
                        return Json("OK", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Job's state doesn't allow this action", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Invalid Job Number", JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Job Comments
        public virtual ActionResult Comments(int id)
        {
            var j = dbContext.Jobs.Include("JobLogs.TechUser").Where(m => m.Id == id).FirstOrDefault();
            if (j != null)
            {
                var c = new Models.Job.CommentsModel() { Result = "OK" };
                c.Comments = j.JobLogs.OrderByDescending(m => m.Timestamp).Select(jl => Models.Job._CommentModel.FromJobLog(jl)).ToList();

                return Json(c, JsonRequestBehavior.AllowGet);
            }
            return Json(new Models.Job.CommentsModel() { Result = "Invalid Job Number" }, JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult CommentPost(int id, string comment)
        {
            var j = dbContext.Jobs.Find(id);
            if (j != null)
            {
                var jl = new Disco.Models.Repository.JobLog()
                {
                    JobId = j.Id,
                    TechUserId = DiscoApplication.CurrentUser.Id,
                    Timestamp = DateTime.Now,
                    Comments = comment
                };
                dbContext.JobLogs.Add(jl);
                dbContext.SaveChanges();

                jl = dbContext.JobLogs.Include("TechUser").Where(m => m.Id == jl.Id).FirstOrDefault();
                return Json(new Models.Job.CommentPostModel() { Result = "OK", Comment = Models.Job._CommentModel.FromJobLog(jl) }, JsonRequestBehavior.AllowGet);
            }
            return Json(new Models.Job.CommentPostModel() { Result = "Invalid Job Number" }, JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult CommentRemove(int id)
        {
            var jl = dbContext.JobLogs.Find(id);
            if (jl != null)
            {
                // 2012-02-17 G# Remove - 'Delete Own Comments' policy
                // Only Delete Own Comments
                //if (jl.TechUserId == DiscoApplication.CurrentUser.Id)
                //{
                dbContext.JobLogs.Remove(jl);
                dbContext.SaveChanges();
                return Json("OK", JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    return Json("You can only delete your own comments.", JsonRequestBehavior.AllowGet);
                //}
            }
            // Doesn't Exist/Already Deleted - OK
            return Json("OK", JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Job Attachements

        [OutputCache(Location = System.Web.UI.OutputCacheLocation.Client, Duration = 172800)]
        public virtual ActionResult AttachmentDownload(int id)
        {
            var ja = dbContext.JobAttachments.Find(id);
            if (ja != null)
            {
                var filePath = ja.RepositoryFilename(dbContext);
                if (System.IO.File.Exists(filePath))
                {
                    return File(filePath, ja.MimeType, ja.Filename);
                }
                else
                {
                    return HttpNotFound("Attachment reference exists, but file not found");
                }
            }
            return HttpNotFound("Invalid Attachment Number");
        }
        [OutputCache(Location = System.Web.UI.OutputCacheLocation.Client, Duration = 172800)]
        public virtual ActionResult AttachmentThumbnail(int id)
        {
            var ja = dbContext.JobAttachments.Find(id);
            if (ja != null)
            {
                var thumbPath = ja.RepositoryThumbnailFilename(dbContext);
                var thumbFileInfo = new FileInfo(thumbPath);
                if (thumbFileInfo.Exists && thumbFileInfo.Length > 0)
                {
                    if (thumbPath.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                        return File(thumbPath, "image/png");
                    else
                        return File(thumbPath, "image/jpg");
                }
                else
                    return File(ClientSource.Style.Images.AttachmentTypes.MimeTypeIcons.Icon(ja.MimeType), "image/png");
            }
            return HttpNotFound("Invalid Attachment Number");
        }
        public virtual ActionResult AttachmentUpload(int id, string Comments)
        {
            var j = dbContext.Jobs.Find(id);
            if (j != null)
            {
                if (Request.Files.Count > 0)
                {
                    var file = Request.Files.Get(0);
                    if (file.ContentLength > 0)
                    {
                        var contentType = file.ContentType;
                        if (string.IsNullOrEmpty(contentType) || contentType.Equals("unknown/unknown", StringComparison.InvariantCultureIgnoreCase))
                            contentType = BI.Interop.MimeTypes.ResolveMimeType(file.FileName);

                        var ja = new Disco.Models.Repository.JobAttachment()
                        {
                            JobId = j.Id,
                            TechUserId = DiscoApplication.CurrentUser.Id,
                            Filename = file.FileName,
                            MimeType = contentType,
                            Timestamp = DateTime.Now,
                            Comments = Comments
                        };
                        dbContext.JobAttachments.Add(ja);
                        dbContext.SaveChanges();

                        ja.SaveAttachment(dbContext, file.InputStream);

                        ja.GenerateThumbnail(dbContext);

                        return Json(ja.Id, JsonRequestBehavior.AllowGet);
                    }
                }
                throw new Exception("No Attachment Uploaded");
            }
            throw new Exception("Invalid Job Number");
        }
        public virtual ActionResult Attachment(int id)
        {
            var ja = dbContext.JobAttachments.Include("TechUser").Where(m => m.Id == id).FirstOrDefault();
            if (ja != null)
            {

                var m = new Models.Attachment.AttachmentModel()
                {
                    Attachment = Models.Attachment._AttachmentModel.FromAttachment(ja),
                    Result = "OK"
                };

                return Json(m, JsonRequestBehavior.AllowGet);
            }
            return Json(new Models.Attachment.AttachmentModel() { Result = "Invalid Attachment Number" }, JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult Attachments(int id)
        {
            var j = dbContext.Jobs.Include("JobAttachments.TechUser").Where(m => m.Id == id).FirstOrDefault();
            if (j != null)
            {
                var m = new Models.Attachment.AttachmentsModel()
                {
                    Attachments = j.JobAttachments.Select(ja => Models.Attachment._AttachmentModel.FromAttachment(ja)).ToList(),
                    Result = "OK"
                };

                return Json(m, JsonRequestBehavior.AllowGet);
            }
            return Json(new Models.Attachment.AttachmentsModel() { Result = "Invalid Attachment Number" }, JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult AttachmentRemove(int id)
        {
            var ja = dbContext.JobAttachments.Include("TechUser").Where(m => m.Id == id).FirstOrDefault();
            if (ja != null)
            {
                // 2012-02-17 G# Remove - 'Delete Own Comments' policy
                //if (ja.TechUserId == DiscoApplication.CurrentUser.Id)
                //{
                ja.OnDelete(dbContext);
                dbContext.SaveChanges();
                return Json("OK", JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    return Json("You can only delete your own attachments.", JsonRequestBehavior.AllowGet);
                //}
            }
            return Json("Invalid Attachment Number", JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Job Components
        public virtual ActionResult ComponentAdd(int id, string Description, string Cost)
        {
            var j = dbContext.Jobs.Find(id);
            if (j != null)
            {
                decimal cost = 0;
                if (string.IsNullOrEmpty(Description))
                    Description = "?";
                if (!string.IsNullOrEmpty(Cost) && Cost.Contains("$"))
                    Cost = Cost.Substring(Cost.IndexOf("$") + 1);
                decimal.TryParse(Cost, out cost);

                var jc = new Disco.Models.Repository.JobComponent()
                {
                    JobId = j.Id,
                    Description = Description,
                    Cost = cost,
                    TechUserId = DiscoApplication.CurrentUser.Id
                };
                dbContext.JobComponents.Add(jc);
                dbContext.SaveChanges();

                return Json(new Models.Job.ComponentModel { Result = "OK", Component = Models.Job._ComponentModel.FromJobComponent(jc) }, JsonRequestBehavior.AllowGet);
            }
            return Json(new Models.Job.ComponentModel { Result = "Invalid Job Number" }, JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult ComponentUpdate(int id, string Description, string Cost)
        {
            var jc = dbContext.JobComponents.Find(id);
            if (jc != null)
            {
                decimal cost = 0;

                if (string.IsNullOrEmpty(Description))
                    Description = "?";
                if (!string.IsNullOrEmpty(Cost) && Cost.Contains("$"))
                    Cost = Cost.Substring(Cost.IndexOf("$") + 1);
                decimal.TryParse(Cost, out cost);

                jc.Description = Description;
                jc.Cost = cost;
                dbContext.SaveChanges();

                return Json(new Models.Job.ComponentModel { Result = "OK", Component = Models.Job._ComponentModel.FromJobComponent(jc) }, JsonRequestBehavior.AllowGet);
            }
            return Json(new Models.Job.ComponentModel { Result = "Invalid Job Component Number" }, JsonRequestBehavior.AllowGet);
        }
        public virtual ActionResult ComponentRemove(int id)
        {
            var jc = dbContext.JobComponents.Find(id);
            if (jc != null)
            {
                dbContext.JobComponents.Remove(jc);
                dbContext.SaveChanges();
                return Json("OK", JsonRequestBehavior.AllowGet);
            }
            return Json("Invalid Job Component Number", JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Job Statistics

        public virtual ActionResult StatisticsDailyOpenedClosed()
        {
            var result = BI.JobBI.Statistics.DailyOpenedClosed.Data(dbContext, true);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public virtual ActionResult GeneratePdf(string id, string DocumentTemplateId)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");
            if (string.IsNullOrEmpty(DocumentTemplateId))
                throw new ArgumentNullException("AttachmentTypeId");
            var job = dbContext.Jobs.Find(int.Parse(id));
            if (job != null)
            {
                var documentTemplate = dbContext.DocumentTemplates.Find(DocumentTemplateId);
                if (documentTemplate != null)
                {
                    var timeStamp = DateTime.Now;
                    Stream pdf;
                    using (var generationState = Disco.Models.BI.DocumentTemplates.DocumentState.DefaultState())
                    {
                        pdf = documentTemplate.GeneratePdf(dbContext, job, DiscoApplication.CurrentUser, timeStamp, generationState);
                    }
                    dbContext.SaveChanges();
                    return File(pdf, "application/pdf", string.Format("{0}_{1}_{2:yyyyMMdd-HHmmss}.pdf", documentTemplate.Id, job.Id, timeStamp));
                }
                else
                {
                    throw new ArgumentException("Invalid Document Template Id", "id");
                }
            }
            else
            {
                throw new ArgumentException("Invalid Job Id", "id");
            }
        }

        public virtual ActionResult OrganisationAddress(int id)
        {
            var address = dbContext.DiscoConfiguration.OrganisationAddresses.GetAddress(id);
            return Json(address, JsonRequestBehavior.AllowGet);
        }
    }
}
