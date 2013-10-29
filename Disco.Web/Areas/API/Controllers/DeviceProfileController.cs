using Disco.BI.Extensions;
using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Web;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class DeviceProfileController : AuthorizedDatabaseController
    {

        const string pDescription = "description";
        const string pName = "name";
        const string pShortName = "shortname";
        const string pDistributionType = "distributiontype";
        const string pCertificateProviderId = "certificateproviderid";
        const string pOrganisationalUnit = "organisationalunit";
        const string pDefaultOrganisationAddress = "defaultorganisationaddress";
        const string pComputerNameTemplate = "computernametemplate";
        const string pEnforceComputerNameConvention = "enforcecomputernameconvention";
        const string pEnforceOrganisationalUnit = "enforceorganisationalunit";
        const string pProvisionADAccount = "provisionadaccount";
        const string pAssignedUserLocalAdmin = "assigneduserlocaladmin";
        const string pAllowUntrustedReimageJobEnrolment = "allowuntrustedreimagejobrnrolment";

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult Update(int id, string key, string value = null, Nullable<bool> redirect = null)
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
                        case pCertificateProviderId:
                            UpdateCertificateProviderId(deviceProfile, value);
                            break;
                        case pOrganisationalUnit:
                            UpdateOrganisationalUnit(deviceProfile, value);
                            break;
                        case pDefaultOrganisationAddress:
                            UpdateDefaultOrganisationAddress(deviceProfile, value);
                            break;
                        case pComputerNameTemplate:
                            Authorization.Require(Claims.Config.DeviceProfile.ConfigureComputerNameTemplate);
                            UpdateComputerNameTemplate(deviceProfile, value);
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
                        default:
                            throw new Exception("Invalid Update Key");
                    }
                }
                else
                {
                    throw new Exception("Invalid Device Profile Number");
                }
                if (redirect.HasValue && redirect.Value)
                    return RedirectToAction(MVC.Config.DeviceModel.Index(deviceProfile.Id));
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
        public virtual ActionResult UpdateDescription(int id, string Description = null, Nullable<bool> redirect = null)
        {
            return Update(id, pDescription, Description, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateName(int id, string ProfileName = null, Nullable<bool> redirect = null)
        {
            return Update(id, pName, ProfileName, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateShortName(int id, string ShortName = null, Nullable<bool> redirect = null)
        {
            return Update(id, pShortName, ShortName, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateDistributionType(int id, string DistributionType = null, Nullable<bool> redirect = null)
        {
            return Update(id, pDistributionType, DistributionType, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateCertificateProviderId(int id, string CertificateProviderId = null, Nullable<bool> redirect = null)
        {
            return Update(id, pCertificateProviderId, CertificateProviderId, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateOrganisationalUnit(int id, string OrganisationalUnit = null, Nullable<bool> redirect = null)
        {
            return Update(id, pOrganisationalUnit, OrganisationalUnit, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateDefaultOrganisationAddress(int id, string DefaultOrganisationAddress = null, Nullable<bool> redirect = null)
        {
            return Update(id, pDefaultOrganisationAddress, DefaultOrganisationAddress, redirect);
        }

        [DiscoAuthorizeAll(Claims.Config.DeviceProfile.Configure, Claims.Config.DeviceProfile.ConfigureComputerNameTemplate)]
        public virtual ActionResult UpdateComputerNameTemplate(int id, string ComputerNameTemplate = null, Nullable<bool> redirect = null)
        {
            return Update(id, pComputerNameTemplate, ComputerNameTemplate, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateEnforceComputerNameConvention(int id, string EnforceComputerNameConvention = null, Nullable<bool> redirect = null)
        {
            return Update(id, pEnforceComputerNameConvention, EnforceComputerNameConvention, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateEnforceOrganisationalUnit(int id, string EnforceOrganisationalUnit = null, Nullable<bool> redirect = null)
        {
            return Update(id, pEnforceOrganisationalUnit, EnforceOrganisationalUnit, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateProvisionADAccount(int id, string ProvisionADAccount = null, Nullable<bool> redirect = null)
        {
            return Update(id, pProvisionADAccount, ProvisionADAccount, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateAssignedUserLocalAdmin(int id, string AssignedUserLocalAdmin = null, Nullable<bool> redirect = null)
        {
            return Update(id, pAssignedUserLocalAdmin, AssignedUserLocalAdmin, redirect);
        }

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult UpdateAllowUntrustedReimageJobEnrolment(int id, string AllowUntrustedReimageJobEnrolment = null, Nullable<bool> redirect = null)
        {
            return Update(id, pAllowUntrustedReimageJobEnrolment, AllowUntrustedReimageJobEnrolment, redirect);
        }
        #endregion

        #region Update Properties
        private void UpdateDescription(Disco.Models.Repository.DeviceProfile deviceProfile, string Description)
        {
            if (string.IsNullOrWhiteSpace(Description))
                deviceProfile.Description = null;
            else
                deviceProfile.Description = Description;
            Database.SaveChanges();
        }

        private void UpdateName(Disco.Models.Repository.DeviceProfile deviceProfile, string Name)
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new Exception("Profile name cannot be empty");
            else
                deviceProfile.Name = Name;
            Database.SaveChanges();
        }

        private void UpdateShortName(Disco.Models.Repository.DeviceProfile deviceProfile, string ShortName)
        {
            if (string.IsNullOrWhiteSpace(ShortName))
                throw new Exception("Profile short name cannot be empty");
            else
                deviceProfile.ShortName = ShortName;
            Database.SaveChanges();
        }

        private void UpdateDistributionType(Disco.Models.Repository.DeviceProfile deviceProfile, string DistributionType)
        {
            int iDt;
            if (int.TryParse(DistributionType, out iDt))
            {
                deviceProfile.DistributionType = (Disco.Models.Repository.DeviceProfile.DistributionTypes)iDt;
                Database.SaveChanges();
                return;
            }
            throw new Exception("Invalid Distribution Type Number");
        }

        private void UpdateCertificateProviderId(Disco.Models.Repository.DeviceProfile deviceProfile, string CertificateProviderId)
        {
            if (string.IsNullOrWhiteSpace(CertificateProviderId))
            {
                deviceProfile.CertificateProviderId = null;
            }
            else
            {
                // Validate
                var featureManifest = Disco.Services.Plugins.Plugins.GetPluginFeature(CertificateProviderId, typeof(Disco.Services.Plugins.Features.CertificateProvider.CertificateProviderFeature));
                if (featureManifest == null)
                    throw new Exception(string.Format("Invalid Certificate Provider Plugin Id: [{0}]", CertificateProviderId));
                else
                    deviceProfile.CertificateProviderId = featureManifest.Id;
            }
            Database.SaveChanges();
        }

        private void UpdateOrganisationalUnit(Disco.Models.Repository.DeviceProfile deviceProfile, string OrganisationalUnit)
        {
            if (string.IsNullOrWhiteSpace(OrganisationalUnit))
                OrganisationalUnit = null;

            deviceProfile.OrganisationalUnit = OrganisationalUnit;

            Database.SaveChanges();
        }

        private void UpdateComputerNameTemplate(Disco.Models.Repository.DeviceProfile deviceProfile, string ComputerNameTemplate)
        {
            Authorization.Require(Claims.Config.DeviceProfile.ConfigureComputerNameTemplate);

            if (string.IsNullOrWhiteSpace(ComputerNameTemplate))
                throw new Exception("ComputerNameTemplate is Required");

            deviceProfile.ComputerNameTemplate = ComputerNameTemplate;

            Database.SaveChanges();

            deviceProfile.ComputerNameInvalidateCache();
        }

        private void UpdateDefaultOrganisationAddress(Disco.Models.Repository.DeviceProfile deviceProfile, string DefaultOrganisationAddress)
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

        private void UpdateEnforceComputerNameConvention(Disco.Models.Repository.DeviceProfile deviceProfile, string EnforceComputerNameConvention)
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

        private void UpdateEnforceOrganisationalUnit(Disco.Models.Repository.DeviceProfile deviceProfile, string EnforceOrganisationalUnit)
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

        private void UpdateProvisionADAccount(Disco.Models.Repository.DeviceProfile deviceProfile, string ProvisionADAccount)
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
        #endregion

        [DiscoAuthorize(Claims.Config.DeviceProfile.Configure)]
        public virtual ActionResult OrganisationalUnits()
        {
            var OUs = BI.Interop.ActiveDirectory.ActiveDirectory.GetOrganisationalUnitStructure();
            return Json(OUs, JsonRequestBehavior.AllowGet);
        }

        #region Actions

        [DiscoAuthorize(Claims.Config.DeviceProfile.Delete)]
        public virtual ActionResult Delete(int id, Nullable<bool> redirect = false)
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
        public virtual ActionResult Default(int id, Nullable<bool> redirect = null)
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
        public virtual ActionResult DefaultAddDeviceOffline(int id, Nullable<bool> redirect = false)
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

        #region Exporting
        [DiscoAuthorizeAll(Claims.Config.DeviceProfile.Show, Claims.Device.Actions.Export)]
        public virtual ActionResult ExportDevices(int id)
        {
            DeviceProfile dp = Database.DeviceProfiles.Find(id);
            if (dp == null)
                throw new ArgumentNullException("id", "Invalid Device Profile Id");

            var devices = Database.Devices.Where(d => !d.DecommissionedDate.HasValue && d.DeviceProfileId == dp.Id);

            var export = BI.DeviceBI.Importing.Export.GenerateExport(devices);

            var filename = string.Format("DiscoDeviceExport-Profile_{0}-{1:yyyyMMdd-HHmmss}.csv", dp.Id, DateTime.Now);

            return File(export, "text/csv", filename);
        }
        #endregion

    }
}
