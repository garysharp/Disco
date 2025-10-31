using Disco.Data.Configuration;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Interop.DiscoServices;
using Disco.Services.Messaging;
using Disco.Services.Users;
using Disco.Services.Web;
using Disco.Web.Areas.API.Models.Shared;
using Disco.Web.Models.Shared;
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
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateLastNetworkLogonDates()
        {
            var taskStatus = ADNetworkLogonDatesUpdateTask.ScheduleImmediately();

            return RedirectToAction(MVC.Config.Logging.TaskStatus(taskStatus.SessionId));
        }

        [DiscoAuthorize(Claims.DiscoAdminAccount)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateAttachmentThumbnails()
        {
            var ts = Disco.Services.Documents.AttachmentImport.ThumbnailUpdateTask.ScheduleImmediately();
            ts.SetFinishedUrl(Url.Action(MVC.Config.SystemConfig.Index()));
            return RedirectToAction(MVC.Config.Logging.TaskStatus(ts.SessionId));
        }

        [DiscoAuthorize(Claims.DiscoAdminAccount)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateADDeviceDescriptions()
        {
            var ts = ADDeviceDescriptionUpdateTask.ScheduleImmediately();
            ts.SetFinishedUrl(Url.Action(MVC.Config.SystemConfig.Index()));
            return RedirectToAction(MVC.Config.Logging.TaskStatus(ts.SessionId));
        }

        [DiscoAuthorize(Claims.Config.System.Show)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult LicenseCheck(string license)
        {
            if (string.IsNullOrWhiteSpace(license))
            {
                Database.DiscoConfiguration.LicenseKey = null;
                Database.DiscoConfiguration.LicenseExpiresOn = null;
                Database.DiscoConfiguration.LicenseError = null;
                Database.SaveChanges();
                return RedirectToAction(MVC.Config.SystemConfig.Index());
            }
            else
            {
                var ts = LicenseValidationTask.ScheduleNow(license);
                ts.SetFinishedUrl(Url.Action(MVC.Config.SystemConfig.Index()));
                return RedirectToAction(MVC.Config.Logging.TaskStatus(ts.SessionId));
            }
        }

        [DiscoAuthorize(Claims.Config.System.Show)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateCheck()
        {
            var ts = UpdateQueryTask.ScheduleNow();
            ts.SetFinishedUrl(Url.Action(MVC.Config.SystemConfig.Index()));
            return RedirectToAction(MVC.Config.Logging.TaskStatus(ts.SessionId));
        }

        [DiscoAuthorize(Claims.Config.System.Show)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult OnlineServicesConnectStart()
        {
            OnlineServicesConnect.QueueStart();

            return RedirectToAction(MVC.Config.SystemConfig.Index());
        }

        #region Organisation

        #region Organisation Name
        [DiscoAuthorize(Claims.Config.Organisation.ConfigureName)]
        [HttpPost, ValidateAntiForgeryToken]
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
                return Ok();
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
                using (Image logoBitmap = Image.FromStream(logoStream))
                {
                    return File(logoBitmap.ResizeImage(Width, Height).SavePng(), "image/png");
                }
            }
        }
        [DiscoAuthorize(Claims.Config.Organisation.ConfigureLogo)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult OrganisationLogo(bool redirect, HttpPostedFileBase Image, bool? ResetLogo = null)
        {
            if (ResetLogo.HasValue && ResetLogo.Value)
            {
                Database.DiscoConfiguration.OrganisationLogo = null;

                if (redirect)
                    return RedirectToAction(MVC.Config.Organisation.Index());
                else
                    return Ok();
            }

            if (Image != null && Image.ContentLength > 0)
            {
                if (Image.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    Database.DiscoConfiguration.OrganisationLogo = Image.InputStream;

                    if (redirect)
                        return RedirectToAction(MVC.Config.Organisation.Index());
                    else
                        return Ok();
                }
                else
                {
                    if (redirect)
                        return RedirectToAction(MVC.Config.Organisation.Index());
                    else
                        return BadRequest("Invalid Content Type");
                }
            }
            if (redirect)
                return RedirectToAction(MVC.Config.Organisation.Index());
            else
                return BadRequest("No Image Supplied");
        }
        #endregion

        #region Organisation Addresses
        [DiscoAuthorize(Claims.Config.Organisation.ConfigureAddresses)]
        [HttpPost, ValidateAntiForgeryToken]
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
                    return Ok();
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
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult DeleteOrganisationAddress(int id, bool redirect = false)
        {
            // Remove References in Device Profiles
            Database.DeviceProfiles
                .Where(dp => dp.DefaultOrganisationAddress == id).ToList()
                .ForEach(dp => dp.DefaultOrganisationAddress = null);

            Database.DiscoConfiguration.OrganisationAddresses.RemoveAddress(id);
            Database.SaveChanges();

            if (redirect)
                return RedirectToAction(MVC.Config.Organisation.Index());
            else
                return Ok();
        }

        #endregion

        #region MultiSiteMode

        [DiscoAuthorize(Claims.Config.Organisation.ConfigureMultiSiteMode)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateMultiSiteMode(bool MultiSiteMode, bool redirect = false)
        {
            Database.DiscoConfiguration.MultiSiteMode = MultiSiteMode;

            Database.SaveChanges();

            DiscoApplication.MultiSiteMode = Database.DiscoConfiguration.MultiSiteMode;

            if (redirect)
                return RedirectToAction(MVC.Config.Organisation.Index());
            else
                return Ok();
        }

        #endregion

        #endregion

        #region Active Directory

        [DiscoAuthorize(Claims.Config.System.ConfigureActiveDirectory)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateActiveDirectorySearchScope(List<string> Containers, bool redirect = false)
        {
            ActiveDirectory.Context.UpdateSearchContainers(Database, Containers);
            Database.SaveChanges();

            if (redirect)
                return RedirectToAction(MVC.Config.SystemConfig.Index());
            else
                return Ok();
        }

        [DiscoAuthorize(Claims.Config.System.ConfigureActiveDirectory)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateActiveDirectorySearchAllServers(bool SearchAllServers, bool redirect = false)
        {
            try
            {
                var result = ActiveDirectory.Context.UpdateSearchAllServers(Database, SearchAllServers);

                Database.SaveChanges();

                if (!result)
                {
                    var allServers = ActiveDirectory.Context.AllServers;
                    if (allServers.Count > ActiveDirectory.MaxAllServerSearch)
                        throw new InvalidOperationException($"This directory contains more than the Max Server Search restriction ({ActiveDirectory.MaxAllServerSearch})");
                    else
                        throw new InvalidOperationException("Unable to change the 'SearchAllServers' property for an unknown reason, please report this bug");
                }

                if (redirect)
                    return RedirectToAction(MVC.Config.SystemConfig.Index());
                else
                    return Ok();
            }
            catch (Exception ex)
            {
                if (redirect)
                    throw;
                else
                    return BadRequest(ex.Message);
            }
        }

        [DiscoAuthorize(Claims.Config.System.ConfigureActiveDirectory)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateActiveDirectorySearchWildcardSuffixOnly(bool SearchWildcardSuffixOnly, bool redirect = false)
        {
            ActiveDirectory.Context.UpdateWildcardSearchSuffixOnly(Database, SearchWildcardSuffixOnly);

            Database.SaveChanges();

            if (redirect)
                return RedirectToAction(MVC.Config.SystemConfig.Index());
            else
                return Ok();
        }

        [DiscoAuthorizeAny(Claims.Config.System.ConfigureActiveDirectory, Claims.Config.DeviceProfile.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult DomainOrganisationalUnitTree(string expandNode = null)
        {
            List<FancyTreeNode> nodes;

            nodes = ActiveDirectory.Context.Domains
                .Select(d => new FancyTreeNode()
                {
                    key = d.DistinguishedName,
                    title = d.NetBiosName,
                    folder = true,
                    tooltip = d.Name,
                    children = d.GetAvailableDomainController().RetrieveADOrganisationUnits()
                        .Select(ou => new FancyTreeNode()
                        {
                            key = ou.DistinguishedName,
                            title = ou.Name,
                            folder = true,
                            tooltip = ou.DistinguishedName,
                            unselectable = false,
                            expanded = false,
                            lazy = true,
                        }).ToArray(),
                    unselectable = true,
                    expanded = true,
                    lazy = false,
                }).ToList();
            if (!string.IsNullOrWhiteSpace(expandNode) && ActiveDirectory.Context.TryGetDomainFromDistinguishedName(expandNode, out var domain))
            {
                // domain node
                var node = nodes.FirstOrDefault(n => n.key.Equals(domain.DistinguishedName, StringComparison.OrdinalIgnoreCase));
                if (node != null)
                {
                    var domainController = domain.GetAvailableDomainController();
                    var ouIndex = expandNode.Length;
                    do
                    {
                        ouIndex = expandNode.LastIndexOf("OU=", ouIndex - 1, StringComparison.OrdinalIgnoreCase);
                        if (ouIndex >= 0)
                        {
                            var dn = expandNode.Substring(ouIndex);

                            node = node.children.FirstOrDefault(n => n.key.Equals(dn, StringComparison.OrdinalIgnoreCase));
                            if (node != null)
                            {
                                node.children = domainController.RetrieveADOrganisationUnits(dn).Select(ou => new FancyTreeNode()
                                {
                                    key = ou.DistinguishedName,
                                    title = ou.Name,
                                    folder = true,
                                    tooltip = ou.DistinguishedName,
                                    unselectable = false,
                                    expanded = false,
                                    lazy = true,
                                }).ToArray();
                                node.expanded = true;
                                node.lazy = false;
                            }
                        }
                    } while (node != null && ouIndex > 0);
                }
            }

            return new JsonResult()
            {
                Data = nodes,
                MaxJsonLength = int.MaxValue
            };
        }

        [DiscoAuthorizeAny(Claims.Config.System.ConfigureActiveDirectory, Claims.Config.DeviceProfile.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult DomainOrganisationalUnits(string node)
        {
            if (string.IsNullOrWhiteSpace(node))
                throw new ArgumentNullException("node");
            if (!ActiveDirectory.Context.TryGetDomainFromDistinguishedName(node, out var domain))
                throw new ArgumentException("Invalid node distinguished name", "node");

            var domainController = domain.GetAvailableDomainController();
            var nodes = domainController.RetrieveADOrganisationUnits(node).Select(ou => new FancyTreeNode()
            {
                key = ou.DistinguishedName,
                title = ou.Name,
                folder = true,
                tooltip = ou.DistinguishedName,
                unselectable = false,
                expanded = false,
                lazy = true,
            }).ToArray();

            return new JsonResult()
            {
                Data = nodes,
                MaxJsonLength = int.MaxValue
            };
        }

        [DiscoAuthorizeAny(Claims.DiscoAdminAccount, Claims.Config.JobQueue.Configure, Claims.Config.UserFlag.Configure, Claims.Config.DeviceFlag.Configure)]
        public virtual ActionResult SearchSubjects(string term, bool includeAuthorizationRoles = false)
        {
            var groupResults = ActiveDirectory.SearchADGroups(term).Select(r => SubjectDescriptorModel.FromActiveDirectoryObject(r));
            var userResults = ActiveDirectory.SearchADUserAccounts(term, true).Select(r => SubjectDescriptorModel.FromActiveDirectoryObject(r));

            IEnumerable<SubjectDescriptorModel> roleResults;
            if (includeAuthorizationRoles)
            {
                roleResults = Database.AuthorizationRoles.AsNoTracking().Where(r => r.Name.Contains(term))
                    .ToList()
                    .Select(r => SubjectDescriptorModel.FromAuthorizationRole(r));
            }
            else
                roleResults = Enumerable.Empty<SubjectDescriptorModel>();

            var results = groupResults.Concat(userResults).Concat(roleResults)
                .OrderBy(r => r.Id).ToList();

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        [DiscoAuthorizeAny(Claims.DiscoAdminAccount, Claims.Config.DeviceProfile.Configure, Claims.Config.DocumentTemplate.Configure, Claims.Config.Plugin.Configure, Claims.Config.UserFlag.Configure, Claims.Config.DeviceFlag.Configure)]
        public virtual ActionResult SearchGroupSubjects(string term)
        {
            var groupResults = ActiveDirectory.SearchADGroups(term).Cast<IADObject>();

            var results = groupResults.OrderBy(r => r.SamAccountName)
                .Select(r => SubjectDescriptorModel.FromActiveDirectoryObject(r)).ToList();

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        [DiscoAuthorizeAny(Claims.DiscoAdminAccount, Claims.Config.JobQueue.Configure, Claims.Config.UserFlag.Configure, Claims.Config.DeviceFlag.Configure)]
        public virtual ActionResult Subject(string Id, bool includeAuthorizationRoles = false)
        {
            if (string.IsNullOrWhiteSpace(Id))
                return Json(null, JsonRequestBehavior.AllowGet);

            if (Id.StartsWith("[", StringComparison.Ordinal))
            {
                if (includeAuthorizationRoles && int.TryParse(Id.Trim('[', ']'), out var roleId))
                {
                    var roleName = UserService.GetAuthorizationRoleName(roleId);
                    if (roleName != null)
                    {
                        return Json(SubjectDescriptorModel.FromAuthorizationRole(roleId, roleName), JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            var subject = ActiveDirectory.RetrieveADObject(Id, Quick: true);

            if (subject == null)
                return Json(null, JsonRequestBehavior.AllowGet);
            else
                return Json(SubjectDescriptorModel.FromActiveDirectoryObject(subject), JsonRequestBehavior.AllowGet);
        }

        [DiscoAuthorizeAny(Claims.Config.UserFlag.Configure, Claims.Config.DeviceFlag.Configure, Claims.Config.DeviceProfile.Configure, Claims.Config.DocumentTemplate.Configure)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult SyncActiveDirectoryManagedGroup(string id, string redirectUrl = null)
        {

            if (!ActiveDirectory.Context.ManagedGroups.TryGetValue(id, out var managedGroup))
                throw new ArgumentException("Unknown Managed Group Key");

            var taskStatus = ADManagedGroupsSyncTask.ScheduleSync(managedGroup);

            if (redirectUrl != null)
                taskStatus.SetFinishedUrl(redirectUrl);

            return RedirectToAction(MVC.Config.Logging.TaskStatus(taskStatus.SessionId));
        }

        #endregion

        #region Proxy Settings

        [DiscoAuthorize(Claims.Config.System.ConfigureProxy)]
        [HttpPost, ValidateAntiForgeryToken]
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
                return Ok();
        }

        #endregion

        #region Email Settings

        [DiscoAuthorize(Claims.Config.System.ConfigureEmail), ValidateInput(false)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult UpdateEmailSettings(string SmtpServer, int? SmtpPort, string FromAddress, string ReplyToAddress, bool EnableSsl, string Username, string Password, bool redirect = false)
        {
            // Default Port
            if (!SmtpPort.HasValue)
                SmtpPort = 25;

            EmailService.ValidateConfiguration(SmtpServer, SmtpPort.Value, FromAddress, ReplyToAddress, EnableSsl, Username, Password);

            SystemConfiguration config = Database.DiscoConfiguration;
            config.EmailSmtpServer = SmtpServer;
            config.EmailSmtpPort = SmtpPort.Value;
            config.EmailFromAddress = FromAddress;
            config.EmailReplyToAddress = ReplyToAddress;
            config.EmailEnableSsl = EnableSsl;
            config.EmailUsername = Username;
            config.EmailPassword = Password;

            EmailService.Update(config);

            Database.SaveChanges();

            if (redirect)
                return RedirectToAction(MVC.Config.SystemConfig.Index());
            else
                return Ok();
        }

        [DiscoAuthorize(Claims.Config.System.ConfigureEmail)]
        [HttpPost, ValidateAntiForgeryToken]
        public virtual ActionResult SendTestEmail(string Recipient, bool redirect = false)
        {
            if (string.IsNullOrWhiteSpace(Recipient))
                throw new ArgumentNullException(nameof(Recipient));

            EmailService.SendTestEmail(Recipient);

            if (redirect)
                return RedirectToAction(MVC.Config.SystemConfig.Index());
            else
                return Ok();
        }

        #endregion

    }
}