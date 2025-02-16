using Disco.Data.Repository;
using Disco.Models.ClientServices;
using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Users;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;

namespace Disco.Services.Devices.Enrolment
{
    public static class WindowsDeviceEnrolment
    {
        private static readonly string pendingIdentifierAlphabet = "23456789ABCDEFGHJKMNPQRSTWXYZ";
        private static readonly Random pendingIdentifierRng = new Random();
        private static readonly ConcurrentDictionary<string, EnrolResponse> pendingEnrolments = new ConcurrentDictionary<string, EnrolResponse>();

        private static void CleanupPendingEnrolments()
        {
            var now = DateTimeOffset.Now;
            var expiredEnrolments = pendingEnrolments
                .Where(e => e.Value.PendingTimeout < now)
                .Select(kvp => kvp.Key).ToList();
            foreach (var expiredEnrolment in expiredEnrolments)
                pendingEnrolments.TryRemove(expiredEnrolment, out _);
        }

        private static string GeneratePendingIdentifier()
        {
            var identifier = default(string);
            var chars = new char[4];
            var retryAllowed = 100;
            while (--retryAllowed > 0)
            {
                lock (pendingIdentifierRng)
                {
                    for (var i = 0; i < chars.Length; i++)
                    {
                        chars[i] = pendingIdentifierAlphabet[pendingIdentifierRng.Next(pendingIdentifierAlphabet.Length)];
                    }
                }
                identifier = new string(chars);

                if (!GetPendingEnrolments().Any(e => string.Equals(e.PendingIdentifier, identifier, StringComparison.Ordinal)))
                    break;
            }
            return identifier;
        }

        public static List<EnrolResponse> GetPendingEnrolments()
        {
            var now = DateTimeOffset.Now;
            return pendingEnrolments.Values
                .Where(e => e.PendingTimeout > now && e.IsPending)
                .ToList();
        }

        public static void ResolvePendingEnrolment(string sessionId, bool approve, string username, string reason)
        {
            if (!pendingEnrolments.TryGetValue(sessionId, out var enrolResponse))
                throw new InvalidOperationException("The pending session is invalid or has expired");
            if (enrolResponse.PendingTimeout < DateTimeOffset.Now)
            {
                pendingEnrolments.TryRemove(sessionId, out _);
                throw new InvalidOperationException("The pending session has expired");
            }
            if (!enrolResponse.IsPending)
                return;

            enrolResponse.IsPending = false;
            if (approve)
            {
                enrolResponse.ErrorMessage = null;
                EnrolmentLog.LogSessionPendingApproved(sessionId, username, reason);
            }
            else
            {
                enrolResponse.ErrorMessage = $"Enrolment rejected";
                EnrolmentLog.LogSessionPendingRejected(sessionId, username, reason);
            }
        }

        public static EnrolResponse Enrol(DiscoDataContext Database, string Username, Enrol Request)
        {
            CleanupPendingEnrolments();

            ADMachineAccount adMachineAccount = null;

            EnrolResponse response = new EnrolResponse();

            AuthorizationToken authenticatedToken = null;
            bool isAuthenticated = false;

            ADDomain domain = null;
            Lazy<ADDomainController> domainController = new Lazy<ADDomainController>(() =>
            {
                if (domain == null)
                    throw new InvalidOperationException("The [domain] variable must be initialized first");
                return domain.GetAvailableDomainController(RequireWritable: true);
            });

            try
            {
                string sessionId;
                bool sessionApproved = false;
                if (!string.IsNullOrWhiteSpace(Request.PendingSessionId))
                {
                    if (!pendingEnrolments.TryGetValue(Request.PendingSessionId, out var pendingResponse))
                        throw new EnrolmentSafeException("The pending session is invalid or has expired");
                    if (pendingResponse.PendingTimeout < DateTimeOffset.Now)
                    {
                        pendingEnrolments.TryRemove(Request.PendingSessionId, out _);
                        throw new EnrolmentSafeException("The pending session has expired");
                    }
                    if (!string.Equals(pendingResponse.PendingAuthorization, Request.PendingAuthorization, StringComparison.Ordinal))
                        throw new EnrolmentSafeException("The pending session authorization is incorrect");

                    sessionId = pendingResponse.SessionId;
                    response = pendingResponse;

                    // still pending?
                    if (response.IsPending)
                        return response;

                    // session rejected?
                    if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
                        throw new EnrolmentSafeException(response.ErrorMessage);

                    // session approved; continue
                    sessionApproved = true;

                    EnrolmentLog.LogSessionContinuing(sessionId, Request.SerialNumber, EnrolmentTypes.Normal);
                }
                else
                {
                    sessionId = Guid.NewGuid().ToString("B");
                    response.SessionId = sessionId;

                    EnrolmentLog.LogSessionStarting(sessionId, Request.SerialNumber, EnrolmentTypes.Normal);
                }

                EnrolmentLog.LogSessionDeviceInfo(sessionId, Request);

                if (Request.SerialNumber.Contains("/") || Request.SerialNumber.Contains(@"\"))
                    throw new EnrolmentSafeException(@"The serial number cannot contain '/' or '\' characters.");

                EnrolmentLog.LogSessionProgress(sessionId, 10, "Loading User Data");
                if (!string.IsNullOrWhiteSpace(Username))
                {
                    authenticatedToken = UserService.GetAuthorization(Username, Database);
                    isAuthenticated = (authenticatedToken != null);
                }
                EnrolmentLog.LogSessionProgress(sessionId, 13, "Loading Device Data");

                Device device = Database.Devices
                    .Include(d => d.AssignedUser)
                    .Include(d => d.DeviceModel)
                    .Include(d => d.DeviceProfile)
                    .Include(d => d.DeviceDetails)
                    .Where(d => d.SerialNumber == Request.SerialNumber).FirstOrDefault();
                EnrolmentLog.LogSessionProgress(sessionId, 15, "Discovering User/Device Disco ICT Permissions");
                if (!sessionApproved)
                {
                    try
                    {
                        if (isAuthenticated)
                        {
                            if (!authenticatedToken.Has(Claims.Device.Actions.EnrolDevices))
                            {
                                if (!authenticatedToken.Has(Claims.ComputerAccount))
                                    throw new EnrolmentSafeException($"Connection not correctly authenticated (SN: {Request.SerialNumber}; Auth User: {authenticatedToken.User.UserId})");

                                if (domain == null)
                                    domain = ActiveDirectory.Context.GetDomainByName(Request.DNSDomainName);

                                if (!authenticatedToken.User.UserId.Equals($@"{domain.NetBiosName}\{Request.ComputerName}$", StringComparison.OrdinalIgnoreCase))
                                    throw new EnrolmentSafeException($"Connection not correctly authenticated (SN: {Request.SerialNumber}; Auth User: {authenticatedToken.User.UserId})");
                            }
                        }
                        else
                        {
                            if (device == null)
                            {
                                throw new EnrolmentSafeException($"Unknown Device Serial Number (SN: '{Request.SerialNumber}')");
                            }
                            if (!device.AllowUnauthenticatedEnrol)
                            {
                                if (device.DeviceProfile.AllowUntrustedReimageJobEnrolment)
                                {
                                    if (Database.Jobs.Count(j => j.DeviceSerialNumber == device.SerialNumber && j.JobTypeId == JobType.JobTypeIds.SImg && !j.ClosedDate.HasValue) == 0)
                                    {
                                        throw new EnrolmentSafeException($"Device has no open 'Software - Reimage' job (SN: '{Request.SerialNumber}')");
                                    }
                                }
                                else
                                {
                                    throw new EnrolmentSafeException($"Device isn't allowed an Unauthenticated Enrolment (SN: '{Request.SerialNumber}')");
                                }
                            }
                        }
                    }
                    catch (EnrolmentSafeException ex)
                    {
                        response.IsPending = true;
                        response.PendingReason = ex.Message;
                        using (var rng = new RNGCryptoServiceProvider())
                        {
                            var authBytes = new byte[33]; // 264 bits (base64-aligned)
                            rng.GetBytes(authBytes);
                            response.PendingAuthorization = Convert.ToBase64String(authBytes);
                        }
                        response.PendingTimeout = DateTimeOffset.Now.Add(Database.DiscoConfiguration.Bootstrapper.PendingTimeout);
                        response.PendingIdentifier = GeneratePendingIdentifier();

                        EnrolmentLog.LogSessionPending(sessionId, Request.SerialNumber, EnrolmentTypes.Normal, response.PendingReason, response.PendingIdentifier);

                        if (pendingEnrolments.TryAdd(sessionId, response))
                            return response;

                        throw;
                    }
                }
                if (Request.IsPartOfDomain && !string.IsNullOrWhiteSpace(Request.ComputerName))
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 20, "Loading Active Directory Computer Account");
                    Guid? uuidGuid = null;
                    Guid? macAddressGuid = null;
                    if (!string.IsNullOrEmpty(Request.Hardware.UUID))
                        uuidGuid = ADMachineAccount.NetbootGUIDFromUUID(Request.Hardware.UUID);

                    // Use non-Wlan Adapter with fastest speed
                    var macAddress = Request.Hardware?.NetworkAdapters?.Where(na => !na.IsWlanAdapter).OrderByDescending(na => na.Speed).Select(na => na.MACAddress).FirstOrDefault();
                    if (!string.IsNullOrEmpty(macAddress))
                        macAddressGuid = ADMachineAccount.NetbootGUIDFromMACAddress(macAddress);

                    if (domain == null)
                        domain = ActiveDirectory.Context.GetDomainByName(Request.DNSDomainName);

                    var requestDeviceId = $@"{domain.NetBiosName}\{Request.ComputerName}";

                    adMachineAccount = domainController.Value.RetrieveADMachineAccount(requestDeviceId, uuidGuid, macAddressGuid);
                }
                if (device == null)
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 30, "New Device, Creating Disco Instance");
                    EnrolmentLog.LogSessionTaskAddedDevice(sessionId, Request.SerialNumber);
                    DeviceProfile deviceProfile = Database.DeviceProfiles.Find(Database.DiscoConfiguration.DeviceProfiles.DefaultDeviceProfileId);

                    var deviceModelResult = Database.DeviceModels.GetOrCreateDeviceModel(Request.Hardware.Manufacturer, Request.Hardware.Model, Request.Hardware.ModelType);
                    DeviceModel deviceModel = deviceModelResult.Item1;
                    if (deviceModelResult.Item2)
                        EnrolmentLog.LogSessionTaskCreatedDeviceModel(sessionId, Request.SerialNumber, deviceModelResult.Item1.Manufacturer, deviceModelResult.Item1.Model);
                    else
                        EnrolmentLog.LogSessionDevice(sessionId, Request.SerialNumber, deviceModel.Id);

                    device = new Device
                    {
                        SerialNumber = Request.SerialNumber,
                        DeviceDomainId = domain == null ? Request.ComputerName : $@"{domain.NetBiosName}\{Request.ComputerName}",
                        DeviceProfile = deviceProfile,
                        DeviceModel = deviceModel,
                        AllowUnauthenticatedEnrol = false,
                        CreatedDate = DateTime.Now,
                        EnrolledDate = DateTime.Now,
                        LastEnrolDate = DateTime.Now,
                        DeviceDetails = new List<DeviceDetail>()
                    };
                    Database.Devices.Add(device);
                }
                else
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 30, "Existing Device, Updating Disco Instance");
                    EnrolmentLog.LogSessionTaskUpdatingDevice(sessionId, Request.SerialNumber);

                    var deviceModelResult = Database.DeviceModels.GetOrCreateDeviceModel(Request.Hardware.Manufacturer, Request.Hardware.Model, Request.Hardware.ModelType);
                    DeviceModel deviceModel = deviceModelResult.Item1;
                    if (deviceModelResult.Item2)
                        EnrolmentLog.LogSessionTaskCreatedDeviceModel(sessionId, Request.SerialNumber, deviceModelResult.Item1.Manufacturer, deviceModelResult.Item1.Model);
                    else
                        EnrolmentLog.LogSessionDevice(sessionId, Request.SerialNumber, deviceModel.Id);

                    device.DeviceModel = deviceModel;

                    var deviceDomainId = domain == null ? Request.ComputerName : $@"{domain.NetBiosName}\{Request.ComputerName}";
                    if (!string.Equals(device.DeviceDomainId, deviceDomainId, StringComparison.Ordinal))
                        device.DeviceDomainId = deviceDomainId;                    

                    if (!device.EnrolledDate.HasValue)
                        device.EnrolledDate = DateTime.Now;
                    device.LastEnrolDate = DateTime.Now;
                }

                // store hardware audit information
                var lanMacAddresses = string.Join("; ", Request.Hardware.NetworkAdapters?.Where(na => !na.IsWlanAdapter).Select(na => na.MACAddress));
                var wlanMacAddresses = string.Join("; ", Request.Hardware.NetworkAdapters?.Where(na => na.IsWlanAdapter).Select(na => na.MACAddress));
                if (!string.IsNullOrEmpty(lanMacAddresses))
                    device.DeviceDetails.LanMacAddress(device, lanMacAddresses);
                if (!string.IsNullOrEmpty(wlanMacAddresses))
                    device.DeviceDetails.WLanMacAddress(device, wlanMacAddresses);
                if (Request.Hardware.Bios?.Count > 0)
                    device.DeviceDetails.Bios(device, Request.Hardware.Bios);
                if (Request.Hardware.BasebBoard?.Count > 0)
                    device.DeviceDetails.BaseBoard(device, Request.Hardware.BasebBoard);
                if (Request.Hardware.ComputerSystem?.Count > 0)
                    device.DeviceDetails.ComputerSystem(device, Request.Hardware.ComputerSystem);
                if (Request.Hardware.Processors?.Count > 0)
                    device.DeviceDetails.Processors(device, Request.Hardware.Processors);
                if (Request.Hardware.PhysicalMemory?.Count > 0)
                    device.DeviceDetails.PhysicalMemory(device, Request.Hardware.PhysicalMemory);
                if (Request.Hardware.DiskDrives?.Count > 0)
                    device.DeviceDetails.DiskDrives(device, Request.Hardware.DiskDrives);
                if (Request.Hardware.NetworkAdapters?.Count > 0)
                    device.DeviceDetails.NetworkAdapters(device, Request.Hardware.NetworkAdapters);
                if (Request.Hardware.Batteries?.Count > 0)
                    device.DeviceDetails.Batteries(device, Request.Hardware.Batteries);
                if (!string.IsNullOrWhiteSpace(Request.Hardware.MdmHardwareData))
                    device.DeviceDetails.MdmHardwareData(device, Request.Hardware.MdmHardwareData);

                if (adMachineAccount == null)
                {
                    if (device.DeviceProfile.ProvisionADAccount)
                    {
                        EnrolmentLog.LogSessionProgress(sessionId, 50, "Provisioning an Active Directory Computer Account");

                        if (string.IsNullOrWhiteSpace(device.DeviceProfile.OrganisationalUnit))
                            throw new InvalidOperationException("No Organisational Unit has been set in the device profile");
                        if (domain == null)
                            domain = ActiveDirectory.Context.GetDomainFromDistinguishedName(device.DeviceProfile.OrganisationalUnit);

                        if (string.IsNullOrEmpty(device.DeviceDomainId) || device.DeviceProfile.EnforceComputerNameConvention)
                            device.DeviceDomainId = device.ComputerNameRender(Database, domain);
                        else if (!ActiveDirectory.IsValidDomainAccountId(device.DeviceDomainId))
                            if (device.DeviceProfile.EnforceComputerNameConvention)
                                device.DeviceDomainId = device.ComputerNameRender(Database, domain);
                            else
                                device.DeviceDomainId = $@"{domain.NetBiosName}\{Request.ComputerName}";

                        string offlineProvisionDiagnosicInfo;
                        EnrolmentLog.LogSessionTaskProvisioningADAccount(sessionId, device.SerialNumber, device.DeviceDomainId);
                        adMachineAccount = domainController.Value.RetrieveADMachineAccount(device.DeviceDomainId);

                        response.OfflineDomainJoinManifest = domainController.Value.OfflineDomainJoinProvision(device.DeviceDomainId, device.DeviceProfile.OrganisationalUnit, ref adMachineAccount, out offlineProvisionDiagnosicInfo);

                        EnrolmentLog.LogSessionDiagnosticInformation(sessionId, offlineProvisionDiagnosicInfo);

                        response.RequireReboot = true;
                    }
                    if (adMachineAccount != null)
                    {
                        response.ComputerName = adMachineAccount.Name;
                        response.DomainName = adMachineAccount.Domain.NetBiosName;
                    }
                    else if (ActiveDirectory.IsValidDomainAccountId(device.DeviceDomainId, out var accountUsername, out var accountDomain))
                    {
                        response.DomainName = accountDomain == null ? null : accountDomain.NetBiosName;
                        response.ComputerName = accountUsername;
                    }
                    else
                    {
                        response.DomainName = Request.DNSDomainName;
                        response.ComputerName = Request.ComputerName;
                    }
                }
                else
                {
                    device.DeviceDomainId = adMachineAccount.Id.Trim('$');
                    response.ComputerName = adMachineAccount.Name;
                    response.DomainName = adMachineAccount.Domain.NetBiosName;

                    // Enforce Computer Name Convention
                    if (!adMachineAccount.IsCriticalSystemObject && device.DeviceProfile.EnforceComputerNameConvention)
                    {
                        if (string.IsNullOrWhiteSpace(device.DeviceProfile.OrganisationalUnit))
                            throw new InvalidOperationException("No Organisational Unit has been set in the device profile");
                        if (domain == null)
                            domain = ActiveDirectory.Context.GetDomainFromDistinguishedName(device.DeviceProfile.OrganisationalUnit);

                        var calculatedComputerName = device.ComputerNameRender(Database, domain);
                        string calculatedAccountUsername;
                        ActiveDirectory.ParseDomainAccountId(calculatedComputerName, out calculatedAccountUsername);

                        if (!Request.ComputerName.Equals(calculatedAccountUsername, StringComparison.OrdinalIgnoreCase))
                        {
                            EnrolmentLog.LogSessionProgress(sessionId, 50, $"Renaming Device: {Request.ComputerName} -> {calculatedComputerName}");
                            EnrolmentLog.LogSessionTaskRenamingDevice(sessionId, Request.ComputerName, calculatedComputerName);

                            device.DeviceDomainId = calculatedComputerName;
                            response.DomainName = domain.NetBiosName;
                            response.ComputerName = calculatedAccountUsername;

                            // Create New Account
                            string offlineProvisionDiagnosicInfo;

                            response.OfflineDomainJoinManifest = domainController.Value.OfflineDomainJoinProvision(device.DeviceDomainId, device.DeviceProfile.OrganisationalUnit, ref adMachineAccount, out offlineProvisionDiagnosicInfo);

                            EnrolmentLog.LogSessionDiagnosticInformation(sessionId, offlineProvisionDiagnosicInfo);

                            response.RequireReboot = true;
                        }
                    }

                    // Enforce Organisational Unit
                    if (!adMachineAccount.IsCriticalSystemObject && response.OfflineDomainJoinManifest == null && device.DeviceProfile.EnforceOrganisationalUnit)
                    {
                        var parentDistinguishedName = adMachineAccount.ParentDistinguishedName;
                        if (string.IsNullOrWhiteSpace(device.DeviceProfile.OrganisationalUnit))
                            throw new InvalidOperationException($"The Organisational Unit for the Device Profile '{device.DeviceProfile.Name}' [{device.DeviceProfile.Id}] is not set.");

                        if (!parentDistinguishedName.Equals(device.DeviceProfile.OrganisationalUnit, StringComparison.OrdinalIgnoreCase)) // Custom OU
                        {
                            var proposedDomain = ActiveDirectory.Context.GetDomainFromDistinguishedName(device.DeviceProfile.OrganisationalUnit);
                            var currentDomain = ActiveDirectory.Context.GetDomainFromDistinguishedName(parentDistinguishedName);
                            if (currentDomain != proposedDomain)
                                throw new NotSupportedException("Unable to move the devices organisational unit when the source and destination domains are different.");
                            if (domain == null)
                                domain = proposedDomain;
                            else if (domain != proposedDomain)
                                throw new NotSupportedException("To many domains involved in this enrolment, contact support regarding your scenario.");

                            EnrolmentLog.LogSessionProgress(sessionId, 65, $"Moving Device Organisational Unit: {parentDistinguishedName} -> {device.DeviceProfile.OrganisationalUnit}");
                            EnrolmentLog.LogSessionTaskMovingDeviceOrganisationUnit(sessionId, parentDistinguishedName, device.DeviceProfile.OrganisationalUnit);
                            adMachineAccount.MoveOrganisationalUnit(domainController.Value, device.DeviceProfile.OrganisationalUnit);
                            response.RequireReboot = true;
                        }
                    }

                }
                if (adMachineAccount != null && !adMachineAccount.IsCriticalSystemObject)
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 75, "Updating Active Directory Computer Account Properties");
                    try
                    {
                        // Use non-Wlan Adapter with fastest speed
                        var macAddress = Request.Hardware?.NetworkAdapters?.Where(na => !na.IsWlanAdapter).OrderByDescending(na => na.Speed).Select(na => na.MACAddress).FirstOrDefault();
                        adMachineAccount.UpdateNetbootGUID(Request.Hardware.UUID, macAddress);
                        if (device.AssignedUser != null)
                            adMachineAccount.SetDescription(device);
                    }
                    catch (Exception ex)
                    {
                        EnrolmentLog.LogSessionWarning(sessionId, $"Unable to update AD Machine Account attributes: {ex.Message}");
                    }
                }
                if (device.DeviceProfile.DistributionType == DeviceProfile.DistributionTypes.OneToOne)
                {
                    if (device.AssignedUser == null)
                    {
                        response.AllowBootstrapperUninstall = false;
                    }
                    else
                    {
                        EnrolmentLog.LogSessionProgress(sessionId, 80, "Retrieving Active Directory Assigned User Account");
                        ADUserAccount AssignedUserInfo = ActiveDirectory.RetrieveADUserAccount(device.AssignedUser.UserId);
                        EnrolmentLog.LogSessionTaskAssigningUser(sessionId, device.SerialNumber, AssignedUserInfo.DisplayName, AssignedUserInfo.SamAccountName, AssignedUserInfo.Domain.NetBiosName, AssignedUserInfo.SecurityIdentifier.ToString());
                        response.AllowBootstrapperUninstall = true;
                        response.AssignedUserIsLocalAdmin = device.DeviceProfile.AssignedUserLocalAdmin;
                        response.AssignedUserUsername = AssignedUserInfo.SamAccountName;
                        response.AssignedUserDomain = AssignedUserInfo.Domain.NetBiosName;
                        response.AssignedUserDescription = AssignedUserInfo.DisplayName;
                        response.AssignedUserSID = AssignedUserInfo.SecurityIdentifier.ToString();
                    }
                }
                else
                {
                    response.AllowBootstrapperUninstall = true;
                }

                // Provision Certificates
                if (!string.IsNullOrEmpty(device.DeviceProfile.CertificateProviders) ||
                    !string.IsNullOrEmpty(device.DeviceProfile.CertificateAuthorityProviders))
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 90, "Provisioning Certificates");

                    List<DeviceCertificate> provisionedCertificates;
                    var provisionResult = device.ProvisionCertificates(Database, Request, out provisionedCertificates);

                    if (provisionedCertificates != null && provisionedCertificates.Count > 0)
                    {
                        foreach (var deviceCertificate in provisionedCertificates)
                        {
                            EnrolmentLog.LogSessionTaskProvisioningCertificate(sessionId, device.SerialNumber, deviceCertificate.Name);
                        }
                    }

                    response.Certificates = provisionResult;
                }

                // Provision Wireless Profiles
                if (!string.IsNullOrEmpty(device.DeviceProfile.WirelessProfileProviders))
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 95, "Provisioning Wireless Profiles");

                    var provisionResult = device.ProvisionWirelessProfiles(Database, Request);

                    if (provisionResult != null && provisionResult.Profiles != null)
                    {
                        foreach (var wirelessProfiles in provisionResult.Profiles)
                        {
                            EnrolmentLog.LogSessionTaskProvisioningWirelessProfile(sessionId, device.SerialNumber, wirelessProfiles.Name);
                        }
                    }

                    response.WirelessProfiles = provisionResult;
                }

                // Reset 'AllowUnauthenticatedEnrol'
                if (device.AllowUnauthenticatedEnrol)
                    device.AllowUnauthenticatedEnrol = false;

                EnrolmentLog.LogSessionProgress(sessionId, 100, "Completed Successfully");
            }
            catch (EnrolmentSafeException ex)
            {
                EnrolmentLog.LogSessionError(response.SessionId, ex);
                return new EnrolResponse
                {
                    SessionId = response.SessionId,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex2)
            {
                EnrolmentLog.LogSessionError(response.SessionId, ex2);
                throw ex2;
            }
            finally
            {
                if (!response.IsPending)
                    EnrolmentLog.LogSessionFinished(response.SessionId);
            }
            return response;
        }


    }
}
