using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.BI;
using Disco.BI.Extensions;
using Disco.Data.Configuration.Modules;

namespace Disco.Web.Areas.API.Controllers
{
    public partial class DeviceProfileController : dbAdminController
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

        public virtual ActionResult Update(int id, string key, string value = null, Nullable<bool> redirect = null)
        {
            try
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("id");
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException("key");
                var deviceProfile = dbContext.DeviceProfiles.Find(id);
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
        public virtual ActionResult UpdateDescription(int id, string Description = null, Nullable<bool> redirect = null)
        {
            return Update(id, pDescription, Description, redirect);
        }
        public virtual ActionResult UpdateName(int id, string ProfileName = null, Nullable<bool> redirect = null)
        {
            return Update(id, pName, ProfileName, redirect);
        }
        public virtual ActionResult UpdateShortName(int id, string ShortName = null, Nullable<bool> redirect = null)
        {
            return Update(id, pShortName, ShortName, redirect);
        }
        public virtual ActionResult UpdateDistributionType(int id, string DistributionType = null, Nullable<bool> redirect = null)
        {
            return Update(id, pDistributionType, DistributionType, redirect);
        }
        public virtual ActionResult UpdateCertificateProviderId(int id, string CertificateProviderId = null, Nullable<bool> redirect = null)
        {
            return Update(id, pCertificateProviderId, CertificateProviderId, redirect);
        }
        public virtual ActionResult UpdateOrganisationalUnit(int id, string OrganisationalUnit = null, Nullable<bool> redirect = null)
        {
            return Update(id, pOrganisationalUnit, OrganisationalUnit, redirect);
        }
        public virtual ActionResult UpdateDefaultOrganisationAddress(int id, string DefaultOrganisationAddress = null, Nullable<bool> redirect = null)
        {
            return Update(id, pDefaultOrganisationAddress, DefaultOrganisationAddress, redirect);
        }
        public virtual ActionResult UpdateComputerNameTemplate(int id, string ComputerNameTemplate = null, Nullable<bool> redirect = null)
        {
            return Update(id, pComputerNameTemplate, ComputerNameTemplate, redirect);
        }
        // Added 2012-06-14 G#
        public virtual ActionResult UpdateEnforceComputerNameConvention(int id, string EnforceComputerNameConvention = null, Nullable<bool> redirect = null)
        {
            return Update(id, pEnforceComputerNameConvention, EnforceComputerNameConvention, redirect);
        }
        // Added 2012-06-14 G#
        public virtual ActionResult UpdateEnforceOrganisationalUnit(int id, string EnforceOrganisationalUnit = null, Nullable<bool> redirect = null)
        {
            return Update(id, pEnforceOrganisationalUnit, EnforceOrganisationalUnit, redirect);
        }
        // Added 2012-06-28 G#
        public virtual ActionResult UpdateProvisionADAccount(int id, string ProvisionADAccount = null, Nullable<bool> redirect = null)
        {
            return Update(id, pProvisionADAccount, ProvisionADAccount, redirect);
        }
        #endregion

        #region Update Properties
        private void UpdateDescription(Disco.Models.Repository.DeviceProfile deviceProfile, string Description)
        {
            if (string.IsNullOrWhiteSpace(Description))
                deviceProfile.Description = null;
            else
                deviceProfile.Description = Description;
            dbContext.SaveChanges();
        }

        private void UpdateName(Disco.Models.Repository.DeviceProfile deviceProfile, string Name)
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new Exception("Profile name cannot be empty");
            else
                deviceProfile.Name = Name;
            dbContext.SaveChanges();
        }

        private void UpdateShortName(Disco.Models.Repository.DeviceProfile deviceProfile, string ShortName)
        {
            if (string.IsNullOrWhiteSpace(ShortName))
                throw new Exception("Profile short name cannot be empty");
            else
                deviceProfile.ShortName = ShortName;
            dbContext.SaveChanges();
        }

        private void UpdateDistributionType(Disco.Models.Repository.DeviceProfile deviceProfile, string DistributionType)
        {
            int iDt;
            if (int.TryParse(DistributionType, out iDt))
            {
                // Removed 2012-06-14 G# - Properties moved to DeviceProfile model & DB Migrated in DBv3.
                //deviceProfile.Configuration(dbContext).DistributionType = (DeviceProfileConfiguration.DeviceProfileDistributionTypes)iDt;
                deviceProfile.DistributionType = (Disco.Models.Repository.DeviceProfile.DistributionTypes)iDt;
                dbContext.SaveChanges();
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
            dbContext.SaveChanges();
        }

        private void UpdateOrganisationalUnit(Disco.Models.Repository.DeviceProfile deviceProfile, string OrganisationalUnit)
        {
            if (string.IsNullOrWhiteSpace(OrganisationalUnit))
                OrganisationalUnit = null;
            // Removed 2012-06-14 G# - Properties moved to DeviceProfile model & DB Migrated in DBv3.
            //deviceProfile.Configuration(dbContext).OrganisationalUnit = OrganisationalUnit;
            deviceProfile.OrganisationalUnit = OrganisationalUnit;

            dbContext.SaveChanges();
        }
        private void UpdateComputerNameTemplate(Disco.Models.Repository.DeviceProfile deviceProfile, string ComputerNameTemplate)
        {
            if (string.IsNullOrWhiteSpace(ComputerNameTemplate))
                throw new Exception("ComputerNameTemplate is Required");
            // Removed 2012-06-14 G# - Properties moved to DeviceProfile model & DB Migrated in DBv3.
            //deviceProfile.Configuration(dbContext).ComputerNameTemplate = ComputerNameTemplate;
            deviceProfile.ComputerNameTemplate = ComputerNameTemplate;

            dbContext.SaveChanges();
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
                    var oa = dbContext.DiscoConfiguration.OrganisationAddresses.GetAddress(daoId);
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


            dbContext.SaveChanges();
        }

        // Added 2012-06-14 G#
        private void UpdateEnforceComputerNameConvention(Disco.Models.Repository.DeviceProfile deviceProfile, string EnforceComputerNameConvention)
        {
            bool bValue;
            if (bool.TryParse(EnforceComputerNameConvention, out bValue))
            {
                deviceProfile.EnforceComputerNameConvention = bValue;

                dbContext.SaveChanges();
                return;
            }
            throw new Exception("Invalid Boolean Value");
        }
        // Added 2012-06-14 G#
        private void UpdateEnforceOrganisationalUnit(Disco.Models.Repository.DeviceProfile deviceProfile, string EnforceOrganisationalUnit)
        {
            bool bValue;
            if (bool.TryParse(EnforceOrganisationalUnit, out bValue))
            {
                deviceProfile.EnforceOrganisationalUnit = bValue;

                dbContext.SaveChanges();
                return;
            }
            throw new Exception("Invalid Boolean Value");
        }
        // Added 2012-06-28 G#
        private void UpdateProvisionADAccount(Disco.Models.Repository.DeviceProfile deviceProfile, string ProvisionADAccount)
        {
            bool bValue;
            if (bool.TryParse(ProvisionADAccount, out bValue))
            {
                deviceProfile.ProvisionADAccount = bValue;

                dbContext.SaveChanges();
                return;
            }
            throw new Exception("Invalid Boolean Value");
        }
        #endregion

        public virtual ActionResult OrganisationalUnits()
        {
            var OUs = BI.Interop.ActiveDirectory.ActiveDirectory.GetOrganisationalUnitStructure();
            return Json(OUs, JsonRequestBehavior.AllowGet);
        }

        #region Actions

        public virtual ActionResult Delete(int id, Nullable<bool> redirect = false)
        {
            try
            {
                var dp = dbContext.DeviceProfiles.Find(id);
                if (dp != null)
                {
                    dp.Delete(dbContext);
                    dbContext.SaveChanges();
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
        public virtual ActionResult Default(int id, Nullable<bool> redirect = null)
        {
            try
            {
                var dp = dbContext.DeviceProfiles.Find(id);
                if (dp != null)
                {
                    dbContext.DiscoConfiguration.DeviceProfiles.DefaultDeviceProfileId = dp.Id;
                    dbContext.SaveChanges();
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
        public virtual ActionResult DefaultAddDeviceOffline(int id, Nullable<bool> redirect = false)
        {
            try
            {
                int defaultValue = 0;
                if (id > 0)
                {
                    var dp = dbContext.DeviceProfiles.Find(id);
                    if (dp != null)
                    {
                        defaultValue = dp.Id;
                    }
                    else
                    {
                        throw new Exception("Invalid Device Profile Number");
                    }
                }
                dbContext.DiscoConfiguration.DeviceProfiles.DefaultAddDeviceOfflineDeviceProfileId = defaultValue;
                dbContext.SaveChanges();
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
