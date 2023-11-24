using Disco.Models.Repository;
using Disco.Services;
using Disco.Services.Authorization;
using Disco.Services.Devices.ManagedGroups;
using Disco.Services.Expressions;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Plugins;
using Disco.Services.Plugins.Features.CertificateAuthorityProvider;
using Disco.Services.Plugins.Features.CertificateProvider;
using Disco.Services.Plugins.Features.WirelessProfileProvider;
using Disco.Services.Tasks;
using Disco.Services.Users;
using Disco.Services.Web;
using Disco.Web.Areas.API.Models.DeviceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class DeviceProfileController : AuthorizedDatabaseController
    {
        const string pDescription = "description";
        const string pName = "name";
        const string pShortName = "shortname";
        const string pDistributionType = "distributiontype";
        const string pCertificateProviders = "certificateproviders";
        const string pCertificateAuthorityProviders = "certificateauthorityproviders";
        const string pWirelessProfileProviders = "wirelessprofileproviders";
        const string pOrganisationalUnit = "organisationalunit";
        const string pDefaultOrganisationAddress = "defaultorganisationaddress";
        const string pEnforceComputerNameConvention = "enforcecomputernameconvention";
        const string pEnforceOrganisationalUnit = "enforceorganisationalunit";
        const string pProvisionADAccount = "provisionadaccount";
        const string pAssignedUserLocalAdmin = "assigneduserlocaladmin";
        const string pAllowUntrustedReimageJobEnrolment = "allowuntrustedreimagejobrnrolment";
        const string pDevicesLinkedGroup = "deviceslinkedgroup";
        const string pAssignedUsersLinkedGroup = "assigneduserslinkedgroup";

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult Update(int id, string key, string value = null, bool? redirect = null)
        {
            Authorization.Require(Claims.Config.DeviceProfile.Configure);

            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("key");
                var deviceProfile = Database.DeviceProfiles.Find(id);
                if (deviceProfile != null)
                {
                    switch (key.ToLower())
                    {
                        case pDescription:
                            UpdateDescription(deviceProfile, value);
                            break;
                        case pName:
                            UpdateName(deviceProfile, value);
                            break;
                        case pShortName:
                            UpdateShortName(deviceProfile, value);
                            break;
                        case pDistributionType:
                            UpdateDistributionType(deviceProfile, value);
                            break;
                        case pCertificateProviders:
                            UpdateCertificateProviders(deviceProfile, value);
                            break;
                        case pCertificateAuthorityProviders:
                            UpdateCertificateAuthorityProviders(deviceProfile, value);
                            break;
                        case pWirelessProfileProviders:
                            UpdateWirelessProfileProviders(deviceProfile, value);
                            break;
                        case pOrganisationalUnit:
                            UpdateOrganisationalUnit(deviceProfile, value);
                            break;
                        case pDefaultOrganisationAddress:
                            UpdateDefaultOrganisationAddress(deviceProfile, value);
                            break;
                        case pEnforceComputerNameConvention:
                            UpdateEnforceComputerNameConvention(deviceProfile, value);
                            break;
                        case pEnforceOrganisationalUnit:
                            UpdateEnforceOrganisationalUnit(deviceProfile, value);
                            break;
                        case pProvisionADAccount:
                            UpdateProvisionADAccount(deviceProfile, value);
                            break;
                        case pAssignedUserLocalAdmin:
                            UpdateAssignedUserLocalAdmin(deviceProfile, value);
                            break;
                        case pAllowUntrustedReimageJobEnrolment:
                            UpdateAllowUntrustedReimageJobEnrolment(deviceProfile, value);
                            break;
                        case pDevicesLinkedGroup:
                            UpdateDevicesLinkedGroup(deviceProfile, value);
                            break;
                        case pAssignedUsersLinkedGroup:
                            UpdateAssignedUsersLinkedGroup(deviceProfile, value);
                            break;
                        default:
                            throw new Exception("Invalid Update Key");
                    }
                }
                else
                {
                    throw new Exception("Invalid Device Profile Number");
                }
                if (redirect.HasValue && redirect.Value)
                    return RedirectToAction(MVC.Config.DeviceProfile.Index(deviceProfile.Id));
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

        #region Update Shortcut Methods

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateDescription(int id, string Description = null, bool? redirect = null)
        {
            return Update(id, pDescription, Description, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateName(int id, string ProfileName = null, bool? redirect = null)
        {
            return Update(id, pName, ProfileName, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateShortName(int id, string ShortName = null, bool? redirect = null)
        {
            return Update(id, pShortName, ShortName, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateDistributionType(int id, string DistributionType = null, bool? redirect = null)
        {
            return Update(id, pDistributionType, DistributionType, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateCertificateProviders(int id, string CertificateProviders = null, bool? redirect = null)
        {
            return Update(id, pCertificateProviders, CertificateProviders, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateCertificateAuthorityProviders(int id, string CertificateAuthorityProviders = null, bool? redirect = null)
        {
            return Update(id, pCertificateAuthorityProviders, CertificateAuthorityProviders, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateWirelessProfileProviders(int id, string WirelessProfileProviders = null, bool? redirect = null)
        {
            return Update(id, pWirelessProfileProviders, WirelessProfileProviders, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateOrganisationalUnit(int id, string OrganisationalUnit = null, bool? redirect = null)
        {
            return Update(id, pOrganisationalUnit, OrganisationalUnit, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateDefaultOrganisationAddress(int id, string DefaultOrganisationAddress = null, bool? redirect = null)
        {
            return Update(id, pDefaultOrganisationAddress, DefaultOrganisationAddress, redirect);
        }

        [DiscoAuthorizeAll(Claims.Config.DeviceProfile.Configure, Claims.Config.DeviceProfile.ConfigureComputerNameTemplate)]
        [ValidateAntiForgeryToken]
        public virtual ActionResult UpdateComputerNameTemplate(int id, string ComputerNameTemplate = null, bool? redirect = null)
        {
            var deviceProfile = Database.DeviceProfiles.Find(id);

            if (deviceProfile == null)
                throw new ArgumentException("Invalid Device Profile Id", nameof(id));

            if (string.IsNullOrWhiteSpace(ComputerNameTemplate))
                throw new Exception("ComputerNameTemplate is Required");

            var expression = new EvaluateExpressionPart(ComputerNameTemplate);
            if (expression.ParseError)
            {
                return this.JsonStatusCode(400, expression.ParseErrorMessage);
            }

            deviceProfile.ComputerNameTemplate = ComputerNameTemplate;

            Database.SaveChanges();

            deviceProfile.ComputerNameInvalidateCache();

            if (redirect.GetValueOrDefault(false))
                return RedirectToAction(MVC.Config.DeviceProfile.Index(deviceProfile.Id));
            else
                return Json("OK");
        }

        [DiscoAuthorizeAll(Claims.Config.DeviceProfile.Configure, Claims.Config.DeviceProfile.ConfigureComputerNameTemplate)]
        [ValidateAntiForgeryToken]
        public virtual ActionResult TestComputerNameTemplate(int id, string ComputerNameTemplate = null, string UserSpecifiedDeviceSerialNumber = null)
        {
            Database.Configuration.LazyLoadingEnabled = true;
            var deviceProfile = Database.DeviceProfiles.Find(id);

            if (deviceProfile == null)
                throw new ArgumentException("Invalid Device Profile Id", nameof(id));

            if (string.IsNullOrWhiteSpace(ComputerNameTemplate))
                throw new Exception("ComputerNameTemplate is Required");

            var expression = Expression.TokenizeSingleDynamic(null, ComputerNameTemplate, 0);
            if (expression.First().ParseError)
            {
                return this.JsonStatusCode(400, expression.First().ParseErrorMessage);
            }

            var result = new TestComputerNameTemplateModel()
            {
                DeviceProfileId = deviceProfile.Id,
                ComputerNameTemplate = ComputerNameTemplate,
            };

            TestComputerNameTemplateModel.TestComputerNameTemplateResultModel evaluateDevice(Disco.Models.Repository.Device device)
            {
                var evaluatorVariables = Expression.StandardVariables(null, Database, UserService.CurrentUser, DateTime.Now, null, device);
                var deviceResult = new TestComputerNameTemplateModel.TestComputerNameTemplateResultModel()
                {
                    DeviceSerialNumber = device.SerialNumber,
                    DeviceComputerName = device.ComputerName,
                };
                try
                {
                    var rendered = expression.EvaluateFirst<string>(device, evaluatorVariables);
                    deviceResult.Url = Url.Action(MVC.Device.Show(device.SerialNumber));
                    deviceResult.RenderedComputerName = rendered;
                    deviceResult.Success = true;

                    if (string.IsNullOrWhiteSpace(rendered))
                    {
                        deviceResult.Success = false;
                        deviceResult.ErrorMessage = "Rendered computer name is null or blank";
                    }
                    else
                    {
                        if (rendered.Length > 15)
                        {
                            deviceResult.Success = false;
                            deviceResult.ErrorMessage = "Must be no more than 15 characters";
                        }
                        var invalidCharacters = Regex.Matches(rendered, @"[^a-z0-9\-]", RegexOptions.IgnoreCase);
                        if (invalidCharacters.Count > 0)
                        {
                            deviceResult.Success = false;
                            deviceResult.ErrorMessage = $"Invalid characters: {string.Join(" ", invalidCharacters.Cast<Match>().Select(m => m.Value.Replace(" ", "{space}")).Distinct())}";
                        }
                    }

                }
                catch (Exception ex)
                {
                    deviceResult.Success = false;
                    deviceResult.ErrorMessage = $"{ex.Message} [{ex.GetType().Name}]";
                }
                return deviceResult;
            }

            if (!string.IsNullOrWhiteSpace(UserSpecifiedDeviceSerialNumber))
            {
                var device = Database.Devices.FirstOrDefault(d => d.SerialNumber == UserSpecifiedDeviceSerialNumber);
                if (device == null)
                    return this.JsonStatusCode(400, "Invalid user-specified device serial number");
                result.UserSpecifiedResult = evaluateDevice(device);
            }

            result.RandomDeviceResults = Database.Devices
                .Where(d => d.DeviceProfileId == deviceProfile.Id && d.SerialNumber != UserSpecifiedDeviceSerialNumber)
                .OrderBy(d => Guid.NewGuid())
                .Take(6)
                .ToList()
                .Select(d => evaluateDevice(d))
                .ToList();

            if (result.UserSpecifiedResult == null && result.RandomDeviceResults.Count > 0)
            {
                result.UserSpecifiedResult = result.RandomDeviceResults.First();
                result.RandomDeviceResults.RemoveAt(0);
            }

            return Json(result);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateEnforceComputerNameConvention(int id, string EnforceComputerNameConvention = null, bool? redirect = null)
        {
            return Update(id, pEnforceComputerNameConvention, EnforceComputerNameConvention, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateEnforceOrganisationalUnit(int id, string EnforceOrganisationalUnit = null, bool? redirect = null)
        {
            return Update(id, pEnforceOrganisationalUnit, EnforceOrganisationalUnit, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateProvisionADAccount(int id, string ProvisionADAccount = null, bool? redirect = null)
        {
            return Update(id, pProvisionADAccount, ProvisionADAccount, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateAssignedUserLocalAdmin(int id, string AssignedUserLocalAdmin = null, bool? redirect = null)
        {
            return Update(id, pAssignedUserLocalAdmin, AssignedUserLocalAdmin, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateAllowUntrustedReimageJobEnrolment(int id, string AllowUntrustedReimageJobEnrolment = null, bool? redirect = null)
        {
            return Update(id, pAllowUntrustedReimageJobEnrolment, AllowUntrustedReimageJobEnrolment, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateDevicesLinkedGroup(int id, string GroupId = null, bool redirect = false)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");

                var deviceProfile = Database.DeviceProfiles.Find(id);
                if (deviceProfile == null)
                    throw new ArgumentException("Invalid Device Profile Id", "id");

                var syncTaskStatus = UpdateDevicesLinkedGroup(deviceProfile, GroupId);
                if (redirect)
                    if (syncTaskStatus == null)
                        return RedirectToAction(MVC.Config.DeviceProfile.Index(deviceProfile.Id));
                    else
                    {
                        syncTaskStatus.SetFinishedUrl(Url.Action(MVC.Config.DeviceProfile.Index(deviceProfile.Id)));
                        return RedirectToAction(MVC.Config.Logging.TaskStatus(syncTaskStatus.SessionId));
                    }
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
        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateAssignedUsersLinkedGroup(int id, string GroupId = null, bool redirect = false)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");

                var deviceProfile = Database.DeviceProfiles.Find(id);
                if (deviceProfile == null)
                    throw new ArgumentException("Invalid Device Profile Id", "id");

                var syncTaskStatus = UpdateAssignedUsersLinkedGroup(deviceProfile, GroupId);
                if (redirect)
                    if (syncTaskStatus == null)
                        return RedirectToAction(MVC.Config.DeviceProfile.Index(deviceProfile.Id));
                    else
                    {
                        syncTaskStatus.SetFinishedUrl(Url.Action(MVC.Config.DeviceProfile.Index(deviceProfile.Id)));
                        return RedirectToAction(MVC.Config.Logging.TaskStatus(syncTaskStatus.SessionId));
                    }
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
        #endregion

        #region Update Properties
        private void UpdateDescription(DeviceProfile deviceProfile, string Description)
        {
            if (string.IsNullOrWhiteSpace(Description))
                deviceProfile.Description = null;
            else
                deviceProfile.Description = Description;
            Database.SaveChanges();
        }

        private void UpdateName(DeviceProfile deviceProfile, string Name)
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new Exception("Profile name cannot be empty");
            else
                deviceProfile.Name = Name;
            Database.SaveChanges();
        }

        private void UpdateShortName(DeviceProfile deviceProfile, string ShortName)
        {
            if (string.IsNullOrWhiteSpace(ShortName))
                throw new Exception("Profile short name cannot be empty");
            else
                deviceProfile.ShortName = ShortName;
            Database.SaveChanges();
        }

        private void UpdateDistributionType(DeviceProfile deviceProfile, string DistributionType)
        {
            int iDt;
            if (int.TryParse(DistributionType, out iDt))
            {
                deviceProfile.DistributionType = (DeviceProfile.DistributionTypes)iDt;
                Database.SaveChanges();
                return;
            }
            throw new Exception("Invalid Distribution Type Number");
        }

        private void UpdateCertificateProviders(DeviceProfile deviceProfile, string CertificateProviderIds)
        {
            if (string.IsNullOrWhiteSpace(CertificateProviderIds))
            {
                deviceProfile.CertificateProviders = null;
            }
            else
            {
                // Validate
                var validatedProviders = new List<PluginFeatureManifest>();
                foreach (var certificateProviderId in CertificateProviderIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var featureManifest = Plugins.GetPluginFeature(certificateProviderId, typeof(CertificateProviderFeature));
                    if (featureManifest == null)
                    {
                        throw new Exception(string.Format("Invalid Certificate Provider Plugin Id: [{0}]", certificateProviderId));
                    }
                    else
                    {
                        validatedProviders.Add(featureManifest);
                    }
                }

                if (validatedProviders.Count > 0)
                {
                    deviceProfile.CertificateProviders = string.Join(",", validatedProviders.Select(p => p.Id));
                }
                else
                {
                    deviceProfile.CertificateProviders = null;
                }
            }

            Database.SaveChanges();
        }

        private void UpdateCertificateAuthorityProviders(DeviceProfile deviceProfile, string CertificateAuthorityProviderIds)
        {
            if (string.IsNullOrWhiteSpace(CertificateAuthorityProviderIds))
            {
                deviceProfile.CertificateAuthorityProviders = null;
            }
            else
            {
                // Validate
                var validatedProviders = new List<PluginFeatureManifest>();
                foreach (var certificateAuthorityProviderId in CertificateAuthorityProviderIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var featureManifest = Plugins.GetPluginFeature(certificateAuthorityProviderId, typeof(CertificateAuthorityProviderFeature));
                    if (featureManifest == null)
                    {
                        throw new Exception(string.Format("Invalid Certificate Authority Provider Plugin Id: [{0}]", certificateAuthorityProviderId));
                    }
                    else
                    {
                        validatedProviders.Add(featureManifest);
                    }
                }

                if (validatedProviders.Count > 0)
                {
                    deviceProfile.CertificateAuthorityProviders = string.Join(",", validatedProviders.Select(p => p.Id));
                }
                else
                {
                    deviceProfile.CertificateAuthorityProviders = null;
                }
            }

            Database.SaveChanges();
        }

        private void UpdateWirelessProfileProviders(DeviceProfile deviceProfile, string WirelessProfileProviderIds)
        {
            if (string.IsNullOrWhiteSpace(WirelessProfileProviderIds))
            {
                deviceProfile.WirelessProfileProviders = null;
            }
            else
            {
                // Validate
                var validatedProviders = new List<PluginFeatureManifest>();
                foreach (var wirelessProfileProviderId in WirelessProfileProviderIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var featureManifest = Plugins.GetPluginFeature(wirelessProfileProviderId, typeof(WirelessProfileProviderFeature));
                    if (featureManifest == null)
                    {
                        throw new Exception(string.Format("Invalid Wireless Profile Provider Plugin Id: [{0}]", wirelessProfileProviderId));
                    }
                    else
                    {
                        validatedProviders.Add(featureManifest);
                    }
                }

                if (validatedProviders.Count > 0)
                {
                    deviceProfile.WirelessProfileProviders = string.Join(",", validatedProviders.Select(p => p.Id));
                }
                else
                {
                    deviceProfile.WirelessProfileProviders = null;
                }
            }

            Database.SaveChanges();
        }

        private void UpdateOrganisationalUnit(DeviceProfile deviceProfile, string OrganisationalUnit)
        {
            if (string.IsNullOrWhiteSpace(OrganisationalUnit))
                OrganisationalUnit = ActiveDirectory.Context.PrimaryDomain.DefaultComputerContainer;

            if (OrganisationalUnit != deviceProfile.OrganisationalUnit)
            {
                deviceProfile.OrganisationalUnit = OrganisationalUnit;
                Database.SaveChanges();
            }
        }

        private void UpdateDefaultOrganisationAddress(DeviceProfile deviceProfile, string DefaultOrganisationAddress)
        {
            if (string.IsNullOrEmpty(DefaultOrganisationAddress))
            {
                deviceProfile.DefaultOrganisationAddress = null;
            }
            else
            {
                // Validate
                int daoId;
                if (int.TryParse(DefaultOrganisationAddress, out daoId))
                {
                    var oa = Database.DiscoConfiguration.OrganisationAddresses.GetAddress(daoId);
                    if (oa != null)
                    {
                        deviceProfile.DefaultOrganisationAddress = oa.Id;
                    }
                    else
                    {
                        throw new Exception("Invalid Default Organisation Address Id (No such Id)");
                    }
                }
                else
                {
                    throw new Exception("Invalid Default Organisation Address Id (Not Integer)");
                }
            }


            Database.SaveChanges();
        }

        private void UpdateEnforceComputerNameConvention(DeviceProfile deviceProfile, string EnforceComputerNameConvention)
        {
            bool bValue;
            if (bool.TryParse(EnforceComputerNameConvention, out bValue))
            {
                deviceProfile.EnforceComputerNameConvention = bValue;

                Database.SaveChanges();
                return;
            }
            throw new Exception("Invalid Boolean Value");
        }

        private void UpdateEnforceOrganisationalUnit(DeviceProfile deviceProfile, string EnforceOrganisationalUnit)
        {
            bool bValue;
            if (bool.TryParse(EnforceOrganisationalUnit, out bValue))
            {
                deviceProfile.EnforceOrganisationalUnit = bValue;

                Database.SaveChanges();
                return;
            }
            throw new Exception("Invalid Boolean Value");
        }

        private void UpdateProvisionADAccount(DeviceProfile deviceProfile, string ProvisionADAccount)
        {
            bool bValue;
            if (bool.TryParse(ProvisionADAccount, out bValue))
            {
                deviceProfile.ProvisionADAccount = bValue;

                Database.SaveChanges();
                return;
            }
            throw new Exception("Invalid Boolean Value");
        }

        private void UpdateAssignedUserLocalAdmin(DeviceProfile deviceProfile, string AssignedUserLocalAdmin)
        {
            bool bValue;
            if (bool.TryParse(AssignedUserLocalAdmin, out bValue))
            {
                deviceProfile.AssignedUserLocalAdmin = bValue;

                Database.SaveChanges();
                return;
            }
            throw new Exception("Invalid Boolean Value");
        }

        private void UpdateAllowUntrustedReimageJobEnrolment(DeviceProfile deviceProfile, string AllowUntrustedReimageJobEnrolment)
        {
            bool bValue;
            if (bool.TryParse(AllowUntrustedReimageJobEnrolment, out bValue))
            {
                deviceProfile.AllowUntrustedReimageJobEnrolment = bValue;

                Database.SaveChanges();
                return;
            }
            throw new Exception("Invalid Boolean Value");
        }

        private ScheduledTaskStatus UpdateDevicesLinkedGroup(DeviceProfile DeviceProfile, string DevicesLinkedGroup)
        {
            var configJson = ADManagedGroup.ValidConfigurationToJson(DeviceProfileDevicesManagedGroup.GetKey(DeviceProfile), DevicesLinkedGroup, null);

            if (DeviceProfile.DevicesLinkedGroup != configJson)
            {
                DeviceProfile.DevicesLinkedGroup = configJson;
                Database.SaveChanges();

                var managedGroup = DeviceProfileDevicesManagedGroup.Initialize(DeviceProfile);
                if (managedGroup != null) // Sync Group
                    return ADManagedGroupsSyncTask.ScheduleSync(managedGroup);
            }

            return null;
        }

        private ScheduledTaskStatus UpdateAssignedUsersLinkedGroup(DeviceProfile DeviceProfile, string AssignedUsersLinkedGroup)
        {
            var configJson = ADManagedGroup.ValidConfigurationToJson(DeviceProfileAssignedUsersManagedGroup.GetKey(DeviceProfile), AssignedUsersLinkedGroup, null);

            if (DeviceProfile.AssignedUsersLinkedGroup != configJson)
            {
                DeviceProfile.AssignedUsersLinkedGroup = configJson;
                Database.SaveChanges();

                var managedGroup = DeviceProfileAssignedUsersManagedGroup.Initialize(DeviceProfile);
                if (managedGroup != null) // Sync Group
                    return ADManagedGroupsSyncTask.ScheduleSync(managedGroup);
            }

            return null;
        }
        #endregion

        #region Actions

        [DiscoAuthorize(Claims.Config.DeviceProfile.Delete)]
        public virtual ActionResult Delete(int id, bool? redirect = false)
        {
            try
            {
                var dp = Database.DeviceProfiles.Find(id);
                if (dp != null)
                {
                    dp.Delete(Database);
                    Database.SaveChanges();
                    if (redirect.HasValue && redirect.Value)
                        return RedirectToAction(MVC.Config.DeviceProfile.Index(null));
                    else
                        return Json("OK", JsonRequestBehavior.AllowGet);
                }
                throw new Exception("Invalid Device Profile Number");
            }
            catch (Exception ex)
            {
                if (redirect.HasValue && redirect.Value)
                    throw;
                else
                    return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Defaults

        [DiscoAuthorize(Claims.Config.DeviceProfile.ConfigureDefaults)]
        public virtual ActionResult Default(int id, bool? redirect = null)
        {
            try
            {
                var dp = Database.DeviceProfiles.Find(id);
                if (dp != null)
                {
                    Database.DiscoConfiguration.DeviceProfiles.DefaultDeviceProfileId = dp.Id;
                    Database.SaveChanges();
                    if (redirect.HasValue && redirect.Value)
                        return RedirectToAction(MVC.Config.DeviceProfile.Index(id));
                    else
                        return Json("OK", JsonRequestBehavior.AllowGet);
                }
                throw new Exception("Invalid Device Profile Number");
            }
            catch (Exception ex)
            {
                if (redirect.HasValue && redirect.Value)
                    throw;
                else
                    return Json(string.Format("Error: {0}", ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.ConfigureDefaults)]
        public virtual ActionResult DefaultAddDeviceOffline(int id, bool? redirect = false)
        {
            try
            {
                int defaultValue = 0;
                if (id > 0)
                {
                    var dp = Database.DeviceProfiles.Find(id);
                    if (dp != null)
                    {
                        defaultValue = dp.Id;
                    }
                    else
                    {
                        throw new Exception("Invalid Device Profile Number");
                    }
                }
                Database.DiscoConfiguration.DeviceProfiles.DefaultAddDeviceOfflineDeviceProfileId = defaultValue;
                Database.SaveChanges();
                if (redirect.HasValue && redirect.Value)
                    return RedirectToAction(MVC.Config.DeviceProfile.Index(id));
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

        #endregion

    }
}
