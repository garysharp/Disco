using Disco.BI.Extensions;
using Disco.Data.Configuration;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Interop.DiscoServices;
using Disco.Services.Web;
using System;
using System.Collections.Generic;
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
            var taskStatus = Disco.Services.Interop.ActiveDirectory.ADNetworkLogonDatesUpdateTask.ScheduleImmediately();

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
            var ts = Disco.Services.Interop.DiscoServices.UpdateQueryTask.ScheduleNow();
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
                if (Image.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
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

        #region Active Directory

        [DiscoAuthorize(Claims.Config.System.ConfigureActiveDirectory)]
        public virtual ActionResult UpdateActiveDirectorySearchScope(List<string> Containers, bool redirect = false)
        {
            ActiveDirectory.Context.UpdateSearchContainers(Database, Containers);
            Database.SaveChanges();

            if (redirect)
                return RedirectToAction(MVC.Config.SystemConfig.Index());
            else
                return Json("OK", JsonRequestBehavior.AllowGet);
        }

        [DiscoAuthorize(Claims.Config.System.ConfigureActiveDirectory)]
        public virtual ActionResult UpdateActiveDirectorySearchAllForestServers(bool SearchAllForestServers, bool redirect = false)
        {
            try
            {
                var result = ActiveDirectory.Context.UpdateSearchAllForestServers(Database, SearchAllForestServers);
                
                Database.SaveChanges();

                if (!result)
                {
                    var forestServers = ActiveDirectory.Context.ForestServers;
                    if (forestServers.Count > ActiveDirectory.MaxForestServerSearch)
                        throw new InvalidOperationException(string.Format("This forest contains more than the Max Forest Server Search restriction ({0})", ActiveDirectory.MaxForestServerSearch));
                    else
                        throw new InvalidOperationException("Unable to change the 'SearchEntireForest' property for an unknown reason, please report this bug");
                }

                if (redirect)
                    return RedirectToAction(MVC.Config.SystemConfig.Index());
                else
                    return Json("OK", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (redirect)
                    throw;
                else
                    return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [DiscoAuthorizeAny(Claims.Config.System.ConfigureActiveDirectory, Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult DomainOrganisationalUnits()
        {
            var domainOUs = ActiveDirectory.RetrieveADOrganisationalUnitStructure()
                .Select(d => new Models.System.DomainOrganisationalUnitsModel() { Domain = d.Item1, OrganisationalUnits = d.Item2})
                .Select(ous => ous.ToFancyTreeNode()).ToList();

            return new JsonResult()
            {
                Data = domainOUs,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = int.MaxValue
            };
        }

        [DiscoAuthorizeAny(Claims.DiscoAdminAccount, Claims.Config.JobQueue.Configure)]
        public virtual ActionResult SearchSubjects(string term)
        {
            var groupResults = ActiveDirectory.SearchADGroups(term).Cast<IADObject>();
            var userResults = ActiveDirectory.SearchADUserAccounts(term, true).Cast<IADObject>();

            var results = groupResults.Concat(userResults).OrderBy(r => r.SamAccountName)
                .Select(r => Models.Shared.SubjectDescriptorModel.FromActiveDirectoryObject(r)).ToList();

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        [DiscoAuthorizeAny(Claims.Config.UserFlag.Configure)]
        public virtual ActionResult SearchGroupSubjects(string term)
        {
            var groupResults = ActiveDirectory.SearchADGroups(term).Cast<IADObject>();

            var results = groupResults.OrderBy(r => r.SamAccountName)
                .Select(r => Models.Shared.SubjectDescriptorModel.FromActiveDirectoryObject(r)).ToList();

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        [DiscoAuthorizeAny(Claims.DiscoAdminAccount, Claims.Config.JobQueue.Configure)]
        public virtual ActionResult Subject(string Id)
        {
            var subject = ActiveDirectory.RetrieveADObject(Id, Quick: true);

            if (subject == null)
                return Json(null, JsonRequestBehavior.AllowGet);
            else
                return Json(Models.Shared.SubjectDescriptorModel.FromActiveDirectoryObject(subject), JsonRequestBehavior.AllowGet);
        }

        [DiscoAuthorizeAny(Claims.Config.UserFlag.Configure)]
        public virtual ActionResult SyncActiveDirectoryManagedGroup(string id, string redirectUrl = null)
        {
            ADManagedGroup managedGroup;
            
            if (!ActiveDirectory.Context.ManagedGroups.TryGetValue(id, out managedGroup))
                throw new ArgumentException("Unknown Managed Group Key");

            var taskStatus = ADManagedGroupsSyncTask.ScheduleSync(managedGroup);
            
            if (redirectUrl != null)
                taskStatus.SetFinishedUrl(redirectUrl);

            return RedirectToAction(MVC.Config.Logging.TaskStatus(taskStatus.SessionId));
        }

        #endregion

        #region Proxy Settings

        [DiscoAuthorize(Claims.Config.System.ConfigureProxy)]
        public virtual ActionResult UpdateProxySettings(string ProxyAddress, int? ProxyPort, string ProxyUsername, string ProxyPassword, bool redirect = false)
        {
            // Default Proxy Port
            if (!ProxyPort.HasValue)
                ProxyPort = 8080;

            SystemConfiguration config = Database.DiscoConfiguration;
            //config.DataStoreLocation = DataStoreLocation;
            config.ProxyAddress = ProxyAddress;
            config.ProxyPort = ProxyPort.Value;
            config.ProxyUsername = ProxyUsername;
            config.ProxyPassword = ProxyPassword;
            DiscoApplication.SetGlobalProxy(ProxyAddress, ProxyPort.Value, ProxyUsername, ProxyPassword);

            Database.SaveChanges();

            // Try and check for updates if needed - After Proxy Changed
            if (Database.DiscoConfiguration.UpdateLastCheckResponse == null
                || Database.DiscoConfiguration.UpdateLastCheckResponse.UpdateResponseDate < DateTime.Now.AddDays(-1))
            {
                UpdateQueryTask.ScheduleNow();
            }

            if (redirect)
                return RedirectToAction(MVC.Config.SystemConfig.Index());
            else
                return Json("OK", JsonRequestBehavior.AllowGet);
        }

        #endregion

    }
}