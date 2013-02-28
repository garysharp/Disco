using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disco.Models.Repository;
using Disco.Data.Repository;
using Disco.BI.Interop.ActiveDirectory;

namespace Disco.BI.Extensions
{
    public static class DeviceActionExtensions
    {

        public static bool CanCreateJob(this Device d)
        {
            return !d.DecommissionedDate.HasValue;
        }

        #region Decommission
        public static bool CanDecommission(this Device d)
        {
            if (d.DecommissionedDate.HasValue)
                return false; // Already Decommissioned

            if (d.AssignedUserId != null)
                return false; // User Assigned to Device

            if (d.Jobs.Count(j => !j.ClosedDate.HasValue) > 0)
                return false; // Device linked to > 0 Open Jobs

            return true;
        }
        public static void OnDecommission(this Device d)
        {
            if (!d.CanDecommission())
                throw new InvalidOperationException("Decommission of Device is Denied");

            d.DecommissionedDate = DateTime.Now;

            // Disable AD Account
            if (d.ComputerName != null)
            {
                var adAccount = d.ActiveDirectoryAccount();
                if (adAccount != null)
                {
                    adAccount.DisableAccount();
                }
            }
        }
        #endregion
        #region Recommission
        public static bool CanRecommission(this Device d)
        {
            return d.DecommissionedDate.HasValue;
        }
        public static void OnRecommission(this Device d)
        {
            if (!d.CanRecommission())
                throw new InvalidOperationException("Recommission of Device is Denied");

            d.DecommissionedDate = null;

            // Enable AD Account
            if (d.ComputerName != null)
            {
                var adAccount = d.ActiveDirectoryAccount();
                if (adAccount != null)
                {
                    adAccount.EnableAccount();
                }
            }
        }
        #endregion

        #region Delete
        public static bool CanDelete(this Device d)
        {
            return d.DecommissionedDate.HasValue;
        }
        public static void OnDelete(this Device d, DiscoDataContext dbContext)
        {
            // Delete Jobs
            foreach (Job j in dbContext.Jobs.Where(i => i.DeviceSerialNumber == d.SerialNumber))
            {
                if (j.UserId == null)
                { // No User associated, thus must Delete whole Job
                    if (j.CanDelete())
                        j.OnDelete(dbContext);
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
                        TechUserId = UserBI.UserCache.CurrentUser.Id,
                        Timestamp = DateTime.Now,
                        Comments = string.Format("Device Deleted{0}{0}Serial Number: {1}{0}Computer Name: {2}{0}Model: {3}{0}Profile: {4}",
                                                    Environment.NewLine, d.SerialNumber, d.ComputerName, d.DeviceModel, d.DeviceProfile)
                    };
                    dbContext.JobLogs.Add(jobLog);
                }
            }

            // Disable Wireless Certificates
            foreach (var wc in dbContext.DeviceCertificates.Where(i => i.DeviceSerialNumber == d.SerialNumber))
            {
                wc.DeviceSerialNumber = null;
                wc.Enabled = false;
            }
            // Delete Device Details
            foreach (var dd in dbContext.DeviceDetails.Where(i => i.DeviceSerialNumber == d.SerialNumber))
                dbContext.DeviceDetails.Remove(dd);
            // Delete Device Attachments
            foreach (var da in dbContext.DeviceAttachments.Where(i => i.DeviceSerialNumber == d.SerialNumber))
            {
                da.RepositoryDelete(dbContext);
                dbContext.DeviceAttachments.Remove(da);
            }
            // Delete Device User Assignments
            foreach (var dua in dbContext.DeviceUserAssignments.Where(i => i.DeviceSerialNumber == d.SerialNumber))
                dbContext.DeviceUserAssignments.Remove(dua);

            dbContext.Devices.Remove(d);
        }
        #endregion
    }
}
