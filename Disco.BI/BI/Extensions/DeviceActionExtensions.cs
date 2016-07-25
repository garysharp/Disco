﻿using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Users;
using System;
using System.Linq;

namespace Disco.BI.Extensions
{
    public static class DeviceActionExtensions
    {
        public static bool IsDecommissioned(this Device d)
        {
            return d.DecommissionedDate.HasValue;
        }

        public static bool CanCreateJob(this Device d)
        {
            if (!JobActionExtensions.CanCreate())
                return false;

            return !d.IsDecommissioned();
        }

        public static bool CanUpdateAssignment(this Device d)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Device.Actions.AssignUser))
                return false;

            return !d.IsDecommissioned();
        }

        public static bool CanUpdateDeviceProfile(this Device d)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Device.Properties.DeviceProfile))
                return false;

            return !d.IsDecommissioned();
        }

        public static bool CanUpdateDeviceBatch(this Device d)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Device.Properties.DeviceBatch))
                return false;

            return !d.IsDecommissioned();
        }

        public static bool CanUpdateTrustEnrol(this Device d)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Device.Actions.AllowUnauthenticatedEnrol))
                return false;

            return !d.IsDecommissioned() && !d.AllowUnauthenticatedEnrol;
        }
        public static bool CanUpdateUntrustEnrol(this Device d)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Device.Actions.AllowUnauthenticatedEnrol))
                return false;

            return !d.IsDecommissioned() && d.AllowUnauthenticatedEnrol;
        }

        #region Decommission
        public static bool CanDecommission(this Device d)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Device.Actions.Decommission))
                return false;

            if (d.DecommissionedDate.HasValue)
                return false; // Already Decommissioned

            if (d.AssignedUserId != null)
                return false; // User Assigned to Device

            if (d.Jobs.Count(j => !j.ClosedDate.HasValue) > 0)
                return false; // Device linked to > 0 Open Jobs

            return true;
        }
        public static void OnDecommission(this Device d, Disco.Models.Repository.DecommissionReasons Reason)
        {
            if (!d.CanDecommission())
                throw new InvalidOperationException("Decommission of Device is Denied");

            d.DecommissionedDate = DateTime.Now;
            d.DecommissionReason = Reason;

            // Disable AD Account
            if (ActiveDirectory.IsValidDomainAccountId(d.DeviceDomainId))
            {
                var adAccount = d.ActiveDirectoryAccount();
                if (adAccount != null && !adAccount.IsCriticalSystemObject)
                {
                    adAccount.DisableAccount();
					adAccount.SetDescription($"Decommissioned Date/Time: {DateTime.Now} - Decommissioned Reason: {Reason}");
                }
            }
        }
        #endregion
        #region Recommission
        public static bool CanRecommission(this Device d)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Device.Actions.Recommission))
                return false;

            return d.DecommissionedDate.HasValue;
        }
        public static void OnRecommission(this Device d)
        {
            if (!d.CanRecommission())
                throw new InvalidOperationException("Recommission of Device is Denied");

            d.DecommissionedDate = null;
            d.DecommissionReason = null;

            // Enable AD Account
            if (ActiveDirectory.IsValidDomainAccountId(d.DeviceDomainId))
            {
                var adAccount = d.ActiveDirectoryAccount();
                if (adAccount != null && !adAccount.IsCriticalSystemObject)
                {
                    adAccount.EnableAccount();
					adAccount.SetDescription(""); //Clear description on recommission
                }
            }
        }
        #endregion

        #region Delete
        public static bool CanDelete(this Device d)
        {
            if (!UserService.CurrentAuthorization.Has(Claims.Device.Actions.Delete))
                return false;

            return d.DecommissionedDate.HasValue;
        }
        public static void OnDelete(this Device d, DiscoDataContext Database)
        {
            // Delete Jobs
            foreach (Job j in Database.Jobs.Where(i => i.DeviceSerialNumber == d.SerialNumber))
            {
                if (j.UserId == null)
                { // No User associated, thus must Delete whole Job
                    if (j.CanDelete())
                        j.OnDelete(Database);
                    else
                        throw new InvalidOperationException(string.Format("Deletion of Device is Denied (See Job# {0})", j.Id));
                }
                else
                {
                    // User associated to Job, thus just remove Devices' association
                    j.DeviceSerialNumber = null;

                    // Write Job Log
                    JobLog jobLog = new JobLog()
                    {
                        JobId = j.Id,
                        TechUserId = UserService.CurrentUser.UserId,
                        Timestamp = DateTime.Now,
                        Comments = string.Format("# Device Deleted\r\n\r\nSerial Number: **{0}**\r\nComputer Name: **{1}**\r\nModel: **{2}**\r\nProfile: **{3}**",
                                                    d.SerialNumber, d.DeviceDomainId, d.DeviceModel, d.DeviceProfile)
                    };
                    Database.JobLogs.Add(jobLog);
                }
            }

            // Disable Wireless Certificates
            foreach (var wc in Database.DeviceCertificates.Where(i => i.DeviceSerialNumber == d.SerialNumber))
            {
                wc.DeviceSerialNumber = null;
                wc.Enabled = false;
            }
            // Delete Device Details
            foreach (var dd in Database.DeviceDetails.Where(i => i.DeviceSerialNumber == d.SerialNumber))
                Database.DeviceDetails.Remove(dd);
            // Delete Device Attachments
            foreach (var da in Database.DeviceAttachments.Where(i => i.DeviceSerialNumber == d.SerialNumber))
            {
                da.RepositoryDelete(Database);
                Database.DeviceAttachments.Remove(da);
            }
            // Delete Device User Assignments
            foreach (var dua in Database.DeviceUserAssignments.Where(i => i.DeviceSerialNumber == d.SerialNumber))
                Database.DeviceUserAssignments.Remove(dua);

            Database.Devices.Remove(d);
        }
        #endregion
    }
}
