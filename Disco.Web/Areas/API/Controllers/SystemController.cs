using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.BI;
using Disco.BI.Extensions;
using System.IO;
using System.Drawing;
using System.Text;
using Disco.Services.Tasks;
using Disco.BI.Interop.ActiveDirectory;
using Disco.Models.Repository;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class SystemController : dbAdminController
    {

        public virtual ActionResult UpdateLastNetworkLogonDates()
        {
            var taskStatus = ActiveDirectoryUpdateLastNetworkLogonDateJob.ScheduleImmediately();

            return RedirectToAction(MVC.Config.Logging.TaskStatus(taskStatus.SessionId));
        }

        public virtual ActionResult UpdateAttachmentThumbnails()
        {
            // Device Attachments
            var das = dbContext.DeviceAttachments.Where(da => da.MimeType == "application/pdf");
            foreach (var da in das)
            {
                var fileName = da.RepositoryThumbnailFilename(dbContext);
                if (!System.IO.File.Exists(fileName))
                {
                    da.GenerateThumbnail(dbContext);
                }
            }

            // User Attachments
            var uas = dbContext.UserAttachments.Where(ua => ua.MimeType == "application/pdf");
            foreach (var ua in uas)
            {
                var fileName = ua.RepositoryThumbnailFilename(dbContext);
                if (!System.IO.File.Exists(fileName))
                {
                    ua.GenerateThumbnail(dbContext);
                }
            }

            // Job Attachments
            var jas = dbContext.JobAttachments.Where(ja => ja.MimeType == "application/pdf");
            foreach (var ja in jas)
            {
                var fileName = ja.RepositoryThumbnailFilename(dbContext);
                if (!System.IO.File.Exists(fileName))
                {
                    ja.GenerateThumbnail(dbContext);
                }
            }

            return Content("Done", "text/text");
        }

        public virtual ActionResult UpdateCheck()
        {
            var ts = Disco.BI.Interop.Community.UpdateCheckTask.ScheduleNow();
            ts.SetFinishedUrl(Url.Action(MVC.Config.SystemConfig.Index()));
            return RedirectToAction(MVC.Config.Logging.TaskStatus(ts.SessionId));
        }

        #region Organisation

        #region Organisation Name
        public virtual ActionResult UpdateOrganisationName(string OrganisationName, bool redirect = false)
        {
            if (string.IsNullOrWhiteSpace(OrganisationName))
                dbContext.DiscoConfiguration.OrganisationName = null;
            else
                dbContext.DiscoConfiguration.OrganisationName = OrganisationName;

            dbContext.SaveChanges();

            DiscoApplication.OrganisationName = dbContext.DiscoConfiguration.OrganisationName;

            if (redirect)
                return RedirectToAction(MVC.Config.Organisation.Index());
            else
                return Json("OK", JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Organisation Logo
        [OutputCache(Duration = 31536000, Location = System.Web.UI.OutputCacheLocation.Any, VaryByParam = "*")]
        public virtual ActionResult OrganisationLogo(int Width = 256, int Height = 256, string v = null)
        {
            if (Width < 1)
                throw new ArgumentOutOfRangeException("Width");
            if (Height < 1)
                throw new ArgumentOutOfRangeException("Height");

            using (Stream logoStream = dbContext.DiscoConfiguration.OrganisationLogo)
            {
                using (Image logoBitmap = Bitmap.FromStream(logoStream))
                {
                    return File(logoBitmap.ResizeImage(Width, Height).SavePng(), "image/png");
                }
            }
        }
        [HttpPost]
        public virtual ActionResult OrganisationLogo(bool redirect, HttpPostedFileBase Image, bool? ResetLogo = null)
        {
            if (ResetLogo.HasValue && ResetLogo.Value)
            {
                dbContext.DiscoConfiguration.OrganisationLogo = null;

                if (redirect)
                    return RedirectToAction(MVC.Config.Organisation.Index());
                else
                    return Json("OK", JsonRequestBehavior.AllowGet);
            }

            if (Image != null && Image.ContentLength > 0)
            {
                if (Image.ContentType.StartsWith("image/", StringComparison.InvariantCultureIgnoreCase))
                {
                    dbContext.DiscoConfiguration.OrganisationLogo = Image.InputStream;

                    if (redirect)
                        return RedirectToAction(MVC.Config.Organisation.Index());
                    else
                        return Json("OK", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (redirect)
                        return RedirectToAction(MVC.Config.Organisation.Index());
                    else
                        return Json("Invalid Content Type", JsonRequestBehavior.AllowGet);
                }
            }
            if (redirect)
                return RedirectToAction(MVC.Config.Organisation.Index());
            else
                return Json("No Image Supplied", JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Organisation Addresses

        public virtual ActionResult UpdateOrganisationAddress(Disco.Models.BI.Config.OrganisationAddress organisationAddress, bool redirect = false)
        {
            if (organisationAddress == null)
            {
                ModelState.AddModelError("Address", "No address was supplied");
            }
            if (ModelState.IsValid)
            {
                dbContext.DiscoConfiguration.OrganisationAddresses.SetAddress(organisationAddress);
                dbContext.SaveChanges();
                if (redirect)
                    return RedirectToAction(MVC.Config.Organisation.Index());
                else
                    return Json("OK", JsonRequestBehavior.AllowGet);
            }
            else
            {
                // Build Error Message
                var em = new StringBuilder();
                em.AppendLine("Error:");
                foreach (var item in ModelState)
                {
                    foreach (var errorItem in item.Value.Errors)
                    {
                        em.Append(item.Key);
                        em.Append(": ");
                        em.AppendLine(errorItem.ErrorMessage);
                    }
                }
                if (redirect)
                    throw new InvalidOperationException(em.ToString());
                else
                    return Json(em.ToString(), JsonRequestBehavior.AllowGet);
            }
        }
        public virtual ActionResult DeleteOrganisationAddress(int Id, bool redirect = false)
        {
            dbContext.DiscoConfiguration.OrganisationAddresses.RemoveAddress(Id);
            dbContext.SaveChanges();

            if (redirect)
                return RedirectToAction(MVC.Config.Organisation.Index());
            else
                return Json("OK", JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region MultiSiteMode

        public virtual ActionResult UpdateMultiSiteMode(bool MultiSiteMode, bool redirect = false)
        {
            dbContext.DiscoConfiguration.MultiSiteMode = MultiSiteMode;

            dbContext.SaveChanges();

            DiscoApplication.MultiSiteMode = dbContext.DiscoConfiguration.MultiSiteMode;

            if (redirect)
                return RedirectToAction(MVC.Config.Organisation.Index());
            else
                return Json("OK", JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion

    }
}
