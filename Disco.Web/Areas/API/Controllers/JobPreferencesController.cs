using Disco.Models.Services.Job;
using Disco.Services.Authorization;
using Disco.Services.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class JobPreferencesController : AuthorizedDatabaseController
    {
        [DiscoAuthorize(Claims.Config.JobPreferences.Configure)]
        public virtual ActionResult UpdateLongRunningJobDaysThreshold(int LongRunningJobDaysThreshold, bool redirect = false)
        {
            Database.DiscoConfiguration.JobPreferences.LongRunningJobDaysThreshold = LongRunningJobDaysThreshold;
            Database.SaveChanges();

            if (redirect)
                return RedirectToAction(MVC.Config.JobPreferences.Index());
            else
                return Json("OK", JsonRequestBehavior.AllowGet);
        }

        [DiscoAuthorize(Claims.Config.JobPreferences.Configure)]
        public virtual ActionResult UpdateStaleJobMinutesThreshold(int StaleJobMinutesThreshold, bool redirect = false)
        {
            Database.DiscoConfiguration.JobPreferences.StaleJobMinutesThreshold = StaleJobMinutesThreshold;
            Database.SaveChanges();

            if (redirect)
                return RedirectToAction(MVC.Config.JobPreferences.Index());
            else
                return Json("OK", JsonRequestBehavior.AllowGet);
        }

        [DiscoAuthorize(Claims.Config.JobPreferences.Configure)]
        public virtual ActionResult UpdateDefaultNoticeboardTheme(string DefaultNoticeboardTheme, bool redirect = false)
        {
            Database.DiscoConfiguration.JobPreferences.DefaultNoticeboardTheme = DefaultNoticeboardTheme;
            Database.SaveChanges();

            Disco.Services.Jobs.Noticeboards.NoticeboardUpdatesHub.SetTheme(DefaultNoticeboardTheme);

            if (redirect)
                return RedirectToAction(MVC.Config.JobPreferences.Index());
            else
                return Json("OK", JsonRequestBehavior.AllowGet);
        }

        [DiscoAuthorize(Claims.Config.JobPreferences.Configure)]
        public virtual ActionResult UpdateLocationMode(LocationModes LocationMode, bool redirect = false)
        {
            Database.DiscoConfiguration.JobPreferences.LocationMode = LocationMode;
            Database.SaveChanges();

            if (redirect)
                return RedirectToAction(MVC.Config.JobPreferences.Index());
            else
                return Json("OK", JsonRequestBehavior.AllowGet);
        }

        [DiscoAuthorize(Claims.Config.JobPreferences.Configure)]
        public virtual ActionResult UpdateLocationList(string[] LocationList, bool redirect = false)
        {
            var list = LocationList
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .Select(i => i.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(i => i);

            Database.DiscoConfiguration.JobPreferences.LocationList = list.ToList();
            Database.SaveChanges();

            if (redirect)
                return RedirectToAction(MVC.Config.JobPreferences.Index());
            else
                return Json("OK", JsonRequestBehavior.AllowGet);
        }

        [DiscoAuthorize(Claims.Config.JobPreferences.Configure)]
        public virtual ActionResult ImportLocationList(string LocationList, bool AutomaticList = false, bool Override = false, bool redirect = false)
        {
            IEnumerable<string> list;

            if (AutomaticList == true)
            {
                var jobDateThreshold = DateTime.Now.AddYears(-1);
                list = Database.Jobs
                    .Where(j => (j.OpenedDate > jobDateThreshold || !j.ClosedDate.HasValue) && j.DeviceHeldLocation != null)
                    .Select(j => j.DeviceHeldLocation).Distinct().ToList();
            }
            else
            {
                list = LocationList
                    .Split(new string[] { Environment.NewLine, ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
            }

            if (!Override)
            {
                // Incorporate existing list
                list = list.Concat(Database.DiscoConfiguration.JobPreferences.LocationList);
            }

            // Remove duplicates & Order
            list = list
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => l.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(i => i);

            Database.DiscoConfiguration.JobPreferences.LocationList = list.ToList();
            Database.SaveChanges();

            if (redirect)
                return RedirectToAction(MVC.Config.JobPreferences.Index());
            else
                return Json("OK", JsonRequestBehavior.AllowGet);
        }
    }
}