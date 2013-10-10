using Disco.BI.Extensions;
using Disco.BI.Interop.ActiveDirectory;
using Disco.Services.Authorization;
using Disco.Services.Web;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class SystemController : AuthorizedDatabaseController
    {
        [DiscoAuthorize(Claims.Config.System.Show)]
        public virtual ActionResult UpdateLastNetworkLogonDates()
        {
            var taskStatus = ActiveDirectoryUpdateLastNetworkLogonDateJob.ScheduleImmediately();

            return RedirectToAction(MVC.Config.Logging.TaskStatus(taskStatus.SessionId));
        }

        [DiscoAuthorize(Claims.DiscoAdminAccount)]
        public virtual ActionResult UpdateAttachmentThumbnails()
        {
            // Device Attachments
            var das = Database.DeviceAttachments.Where(da => da.MimeType == "application/pdf");
            foreach (var da in das)
            {
                var fileName = da.RepositoryThumbnailFilename(Database);
                if (!System.IO.File.Exists(fileName))
                {
                    da.GenerateThumbnail(Database);
                }
            }

            // User Attachments
            var uas = Database.UserAttachments.Where(ua => ua.MimeType == "application/pdf");
            foreach (var ua in uas)
            {
                var fileName = ua.RepositoryThumbnailFilename(Database);
                if (!System.IO.File.Exists(fileName))
                {
                    ua.GenerateThumbnail(Database);
                }
            }

            // Job Attachments
            var jas = Database.JobAttachments.Where(ja => ja.MimeType == "application/pdf");
            foreach (var ja in jas)
            {
                var fileName = ja.RepositoryThumbnailFilename(Database);
                if (!System.IO.File.Exists(fileName))
                {
                    ja.GenerateThumbnail(Database);
                }
            }

            return Content("Done", "text/text");
        }

        [DiscoAuthorize(Claims.Config.System.Show)]
        public virtual ActionResult UpdateCheck()
        {
            var ts = Disco.BI.Interop.Community.UpdateCheckTask.ScheduleNow();
            ts.SetFinishedUrl(Url.Action(MVC.Config.SystemConfig.Index()));
            return RedirectToAction(MVC.Config.Logging.TaskStatus(ts.SessionId));
        }

        #region Organisation

        #region Organisation Name
        [DiscoAuthorize(Claims.Config.Organisation.ConfigureName)]
        public virtual ActionResult UpdateOrganisationName(string OrganisationName, bool redirect = false)
        {
            if (string.IsNullOrWhiteSpace(OrganisationName))
                Database.DiscoConfiguration.OrganisationName = null;
            else
                Database.DiscoConfiguration.OrganisationName = OrganisationName;

            Database.SaveChanges();

            DiscoApplication.OrganisationName = Database.DiscoConfiguration.OrganisationName;

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

            using (Stream logoStream = Database.DiscoConfiguration.OrganisationLogo)
            {
                using (Image logoBitmap = Bitmap.FromStream(logoStream))
                {
                    return File(logoBitmap.ResizeImage(Width, Height).SavePng(), "image/png");
                }
            }
        }
        [DiscoAuthorize(Claims.Config.Organisation.ConfigureLogo), HttpPost]
        public virtual ActionResult OrganisationLogo(bool redirect, HttpPostedFileBase Image, bool? ResetLogo = null)
        {
            if (ResetLogo.HasValue && ResetLogo.Value)
            {
                Database.DiscoConfiguration.OrganisationLogo = null;

                if (redirect)
                    return RedirectToAction(MVC.Config.Organisation.Index());
                else
                    return Json("OK", JsonRequestBehavior.AllowGet);
            }

            if (Image != null && Image.ContentLength > 0)
            {
                if (Image.ContentType.StartsWith("image/", StringComparison.InvariantCultureIgnoreCase))
                {
                    Database.DiscoConfiguration.OrganisationLogo = Image.InputStream;

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
        [DiscoAuthorize(Claims.Config.Organisation.ConfigureAddresses)]
        public virtual ActionResult UpdateOrganisationAddress(Disco.Models.BI.Config.OrganisationAddress organisationAddress, bool redirect = false)
        {
            if (organisationAddress == null)
            {
                ModelState.AddModelError("Address", "No address was supplied");
            }
            if (ModelState.IsValid)
            {
                Database.DiscoConfiguration.OrganisationAddresses.SetAddress(organisationAddress);
                Database.SaveChanges();
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
        [DiscoAuthorize(Claims.Config.Organisation.ConfigureAddresses)]
        public virtual ActionResult DeleteOrganisationAddress(int Id, bool redirect = false)
        {
            Database.DiscoConfiguration.OrganisationAddresses.RemoveAddress(Id);
            Database.SaveChanges();

            if (redirect)
                return RedirectToAction(MVC.Config.Organisation.Index());
            else
                return Json("OK", JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region MultiSiteMode

        [DiscoAuthorize(Claims.Config.Organisation.ConfigureMultiSiteMode)]
        public virtual ActionResult UpdateMultiSiteMode(bool MultiSiteMode, bool redirect = false)
        {
            Database.DiscoConfiguration.MultiSiteMode = MultiSiteMode;

            Database.SaveChanges();

            DiscoApplication.MultiSiteMode = Database.DiscoConfiguration.MultiSiteMode;

            if (redirect)
                return RedirectToAction(MVC.Config.Organisation.Index());
            else
                return Json("OK", JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion

    }
}
