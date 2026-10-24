using Disco.Data.Repository;
using Disco.Models.ClientServices;
using Disco.Models.Repository;
using Disco.Models.Services.Devices;
using Disco.Services.Authorization;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Users;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Web.UI.WebControls;

namespace Disco.Services.Devices.Enrolment
{
    public static class WindowsDeviceEnrolment
    {
        private static readonly string pendingIdentifierAlphabet = "23456789ABCDEFGHJKMNPQRSTWXYZ";
        private static readonly Random pendingIdentifierRng = new Random();
        private static readonly ConcurrentDictionary<string, EnrolResponse> pendingEnrolments = new ConcurrentDictionary<string, EnrolResponse>();
        private static readonly Dictionary<DeviceEnrolmentServerDiscoveryMethod, int> discoveryMethodStatistics = Enum.GetValues(typeof(DeviceEnrolmentServerDiscoveryMethod)).Cast<DeviceEnrolmentServerDiscoveryMethod>().ToDictionary(k => k, k => 0);

        public static string GetDnsServiceLocationRecordName()
            => $"_discoict._tcp.{ActiveDirectory.Context.PrimaryDomain.Name}";

        public static void IncrementDiscoveryMethod(DeviceEnrolmentServerDiscoveryMethod method)
        {
            discoveryMethodStatistics[method]++;
        }

        public static IEnumerable<KeyValuePair<DeviceEnrolmentServerDiscoveryMethod, int>> GetDiscoveryMethodStatistics()
            => discoveryMethodStatistics.AsEnumerable();

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

        public static void ResolvePendingEnrolment(string sessionId, bool approve, string username, int? deviceProfileId, int? deviceBatchId, string reason)
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
            enrolResponse.DeviceProfileId = deviceProfileId;
            enrolResponse.DeviceBatchId = deviceBatchId;
            enrolResponse.IsPending = false;
        }

        public static EnrolResponse Enrol(DiscoDataContext database, string username, Enrol request)
        {
            CleanupPendingEnrolments();

            var adMachineAccount = default(ADMachineAccount);

            var response = new EnrolResponse();

            var authenticatedToken = default(AuthorizationToken);
            var isAuthenticated = false;

            var domain = default(ADDomain);
            Lazy<ADDomainController> domainController = new Lazy<ADDomainController>(() =>
            {
                if (domain == null)
                    throw new InvalidOperationException("The [domain] variable must be initialized first");
                return domain.GetAvailableDomainController(RequireWritable: true);
            });

            try
            {
                string sessionId;
                var sessionApproved = false;
                if (!string.IsNullOrWhiteSpace(request.PendingSessionId))
                {
                    if (!pendingEnrolments.TryGetValue(request.PendingSessionId, out var pendingResponse))
                        throw new EnrolmentSafeException("The pending session is invalid or has expired");
                    if (pendingResponse.PendingTimeout < DateTimeOffset.Now)
                    {
                        pendingEnrolments.TryRemove(request.PendingSessionId, out _);
                        throw new EnrolmentSafeException("The pending session has expired");
                    }
                    if (!string.Equals(pendingResponse.PendingAuthorization, request.PendingAuthorization, StringComparison.Ordinal))
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

                    EnrolmentLog.LogSessionContinuing(sessionId, request.SerialNumber, EnrolmentTypes.Normal);
                }
                else
                {
                    sessionId = Guid.NewGuid().ToString("B");
                    response.SessionId = sessionId;

                    EnrolmentLog.LogSessionStarting(sessionId, request.SerialNumber, EnrolmentTypes.Normal);
                }

                EnrolmentLog.LogSessionDeviceInfo(sessionId, request);

                if (request.SerialNumber.Contains("/") || request.SerialNumber.Contains(@"\"))
                    throw new EnrolmentSafeException(@"The serial number cannot contain '/' or '\' characters.");

                EnrolmentLog.LogSessionProgress(sessionId, 10, "Loading User Data");
                if (!string.IsNullOrWhiteSpace(username))
                {
                    authenticatedToken = UserService.GetAuthorization(username, database);
                    isAuthenticated = authenticatedToken != null;
                }
                EnrolmentLog.LogSessionProgress(sessionId, 13, "Loading Device Data");

                Device device = database.Devices
                    .Include(d => d.AssignedUser)
                    .Include(d => d.DeviceModel)
                    .Include(d => d.DeviceProfile)
                    .Include(d => d.DeviceDetails)
                    .FirstOrDefault(d => d.SerialNumber == request.SerialNumber);
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
                                    throw new EnrolmentSafeException($"Connection not correctly authenticated (SN: {request.SerialNumber}; Auth User: {authenticatedToken.User.UserId})");
                                else if (!string.Equals($"{request.ComputerName}$", authenticatedToken.User.DomainUsername, StringComparison.OrdinalIgnoreCase))
                                    throw new InvalidOperationException($"Connection not correctly authenticated (SN: {request.SerialNumber}; Computer Name: {request.ComputerName}; Auth User: {authenticatedToken.User.UserId})");

                                if (domain == null && !ActiveDirectory.Context.TryGetDomainByName(request.DNSDomainName, out domain))
                                    throw new EnrolmentSafeException($"The specified domain name '{request.DNSDomainName}' is not recognized or reachable.");

                                if (!authenticatedToken.User.UserId.Equals($@"{domain.NetBiosName}\{request.ComputerName}$", StringComparison.OrdinalIgnoreCase))
                                    throw new EnrolmentSafeException($"Connection not correctly authenticated (SN: {request.SerialNumber}; Auth User: {authenticatedToken.User.UserId})");
                            }
                        }
                        else
                        {
                            if (device == null)
                            {
                                throw new EnrolmentSafeException($"Unknown Device Serial Number (SN: '{request.SerialNumber}')");
                            }
                            if (!device.AllowUnauthenticatedEnrol)
                            {
                                if (device.DeviceProfile.AllowUntrustedReimageJobEnrolment)
                                {
                                    if (!database.Jobs.Any(j => j.DeviceSerialNumber == device.SerialNumber && j.JobTypeId == JobType.JobTypeIds.SImg && !j.ClosedDate.HasValue))
                                    {
                                        throw new EnrolmentSafeException($"Device has no open 'Software - Reimage' job (SN: '{request.SerialNumber}')");
                                    }
                                }
                                else
                                {
                                    throw new EnrolmentSafeException($"Device isn't allowed an Unauthenticated Enrolment (SN: '{request.SerialNumber}')");
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
                        response.PendingTimeout = DateTimeOffset.Now.Add(database.DiscoConfiguration.Bootstrapper.PendingTimeout);
                        response.PendingIdentifier = GeneratePendingIdentifier();

                        EnrolmentLog.LogSessionPending(sessionId, request.SerialNumber, EnrolmentTypes.Normal, response.PendingReason, response.PendingIdentifier, device?.DeviceProfileId, device?.DeviceBatchId);

                        if (pendingEnrolments.TryAdd(sessionId, response))
                            return response;

                        throw;
                    }
                }

                int deviceProfileId;
                if (response.DeviceProfileId.HasValue)
                    deviceProfileId = response.DeviceProfileId.Value;
                else if (device != null)
                    deviceProfileId = device.DeviceProfileId;
                else
                    deviceProfileId = database.DiscoConfiguration.DeviceProfiles.DefaultDeviceProfileId;
                var deviceProfile = database.DeviceProfiles.Find(deviceProfileId)
                    ?? throw new InvalidOperationException($"Device profile {deviceProfileId} was not found, please check your default profile configuration");

                if (request.IsPartOfDomain && !string.IsNullOrWhiteSpace(request.ComputerName))
                {
                    if (ActiveDirectory.Context.TryGetDomainByName(request.DNSDomainName, out domain))
                    {
                        EnrolmentLog.LogSessionProgress(sessionId, 20, "Loading Active Directory Computer Account");
                        Guid? uuidGuid = null;
                        Guid? macAddressGuid = null;
                        if (!string.IsNullOrEmpty(request.Hardware.UUID))
                            uuidGuid = ADMachineAccount.NetbootGUIDFromUUID(request.Hardware.UUID);

                        // Use non-Wlan Adapter with fastest speed
                        var macAddress = request.Hardware?.NetworkAdapters?.Where(na => !na.IsWlanAdapter).OrderByDescending(na => na.Speed).Select(na => na.MACAddress).FirstOrDefault();
                        if (!string.IsNullOrEmpty(macAddress))
                            macAddressGuid = ADMachineAccount.NetbootGUIDFromMACAddress(macAddress);

                        var requestDeviceId = $@"{domain.NetBiosName}\{request.ComputerName}";

                        adMachineAccount = domainController.Value.RetrieveADMachineAccount(requestDeviceId, uuidGuid, macAddressGuid);
                    }
                    else if (!deviceProfile.ProvisionFromOtherDomain)
                    {
                        throw new EnrolmentSafeException($"The specified domain name '{request.DNSDomainName}' is not recognized or reachable.");
                    }
                }

                var dnsHostName = adMachineAccount?.DnsName ?? request.DnsHostName;
                if (string.IsNullOrWhiteSpace(dnsHostName))
                    dnsHostName = null;
                else if (dnsHostName.Contains("."))
                    dnsHostName = dnsHostName.Substring(0, dnsHostName.IndexOf("."));

                if (device == null)
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 30, "New Device, Creating Disco Instance");
                    EnrolmentLog.LogSessionTaskAddedDevice(sessionId, request.SerialNumber);

                    var deviceBatch = default(DeviceBatch);
                    if (response.DeviceBatchId.HasValue)
                        deviceBatch = database.DeviceBatches.Find(response.DeviceBatchId.Value);

                    var deviceModelResult = database.DeviceModels.GetOrCreateDeviceModel(request.Hardware.Manufacturer, request.Hardware.Model, request.Hardware.ModelType);
                    DeviceModel deviceModel = deviceModelResult.Item1;
                    if (deviceModelResult.Item2)
                        EnrolmentLog.LogSessionTaskCreatedDeviceModel(sessionId, request.SerialNumber, deviceModelResult.Item1.Manufacturer, deviceModelResult.Item1.Model);
                    else
                        EnrolmentLog.LogSessionDevice(sessionId, request.SerialNumber, deviceModel.Id);

                    device = new Device
                    {
                        SerialNumber = request.SerialNumber,
                        DeviceDomainId = domain == null ? request.ComputerName : $@"{domain.NetBiosName}\{request.ComputerName}",
                        DnsHostName = dnsHostName,
                        DeviceProfile = deviceProfile,
                        DeviceModel = deviceModel,
                        DeviceBatch = deviceBatch,
                        AllowUnauthenticatedEnrol = false,
                        CreatedDate = DateTime.Now,
                        EnrolledDate = DateTime.Now,
                        LastEnrolDate = DateTime.Now,
                        DeviceDetails = new List<DeviceDetail>()
                    };
                    database.Devices.Add(device);
                }
                else
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 30, "Existing Device, Updating Disco Instance");
                    EnrolmentLog.LogSessionTaskUpdatingDevice(sessionId, request.SerialNumber);

                    var deviceModelResult = database.DeviceModels.GetOrCreateDeviceModel(request.Hardware.Manufacturer, request.Hardware.Model, request.Hardware.ModelType);
                    DeviceModel deviceModel = deviceModelResult.Item1;
                    if (deviceModelResult.Item2)
                        EnrolmentLog.LogSessionTaskCreatedDeviceModel(sessionId, request.SerialNumber, deviceModelResult.Item1.Manufacturer, deviceModelResult.Item1.Model);
                    else
                        EnrolmentLog.LogSessionDevice(sessionId, request.SerialNumber, deviceModel.Id);

                    device.DeviceModel = deviceModel;

                    if (device.DeviceProfile.Id != deviceProfileId)
                    {
                        device.DeviceProfile = deviceProfile;
                        device.DeviceProfileId = deviceProfile.Id;
                    }

                    if (response.DeviceBatchId.HasValue && device.DeviceBatch?.Id != response.DeviceBatchId.Value)
                    {
                        var deviceBatch = database.DeviceBatches.Find(response.DeviceBatchId.Value);
                        if (deviceBatch != null)
                        {
                            device.DeviceBatch = deviceBatch;
                            device.DeviceBatchId = deviceBatch.Id;
                        }
                    }

                    device.DeviceDomainId = domain == null ? request.ComputerName : $@"{domain.NetBiosName}\{request.ComputerName}";
                    device.DnsHostName = dnsHostName;

                    if (!device.EnrolledDate.HasValue)
                        device.EnrolledDate = DateTime.Now;
                    device.LastEnrolDate = DateTime.Now;
                }

                // store hardware audit information
                var lanMacAddresses = string.Join("; ", request.Hardware.NetworkAdapters?.Where(na => !na.IsWlanAdapter).Select(na => na.MACAddress));
                var wlanMacAddresses = string.Join("; ", request.Hardware.NetworkAdapters?.Where(na => na.IsWlanAdapter).Select(na => na.MACAddress));
                if (!string.IsNullOrEmpty(lanMacAddresses))
                    device.DeviceDetails.LanMacAddress(device, lanMacAddresses);
                if (!string.IsNullOrEmpty(wlanMacAddresses))
                    device.DeviceDetails.WLanMacAddress(device, wlanMacAddresses);
                if (request.Hardware.Bios?.Count > 0)
                    device.DeviceDetails.Bios(device, request.Hardware.Bios);
                if (request.Hardware.BasebBoard?.Count > 0)
                    device.DeviceDetails.BaseBoard(device, request.Hardware.BasebBoard);
                if (request.Hardware.ComputerSystem?.Count > 0)
                    device.DeviceDetails.ComputerSystem(device, request.Hardware.ComputerSystem);
                if (request.Hardware.Processors?.Count > 0)
                    device.DeviceDetails.Processors(device, request.Hardware.Processors);
                if (request.Hardware.PhysicalMemory?.Count > 0)
                    device.DeviceDetails.PhysicalMemory(device, request.Hardware.PhysicalMemory);
                if (request.Hardware.DiskDrives?.Count > 0)
                    device.DeviceDetails.DiskDrives(device, request.Hardware.DiskDrives);
                if (request.Hardware.NetworkAdapters?.Count > 0)
                    device.DeviceDetails.NetworkAdapters(device, request.Hardware.NetworkAdapters);
                if (request.Hardware.Batteries?.Count > 0)
                    device.DeviceDetails.Batteries(device, request.Hardware.Batteries);
                if (!string.IsNullOrWhiteSpace(request.Hardware.MdmHardwareData))
                    device.DeviceDetails.MdmHardwareData(device, request.Hardware.MdmHardwareData);

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
                        {
                            var (renderedDeviceDomainId, renderedDnsHostName) = device.RenderComputerNames(database, domain);
                            device.DeviceDomainId = renderedDeviceDomainId;
                            device.DnsHostName = renderedDnsHostName;
                        }
                        else if (!ActiveDirectory.IsValidDomainAccountId(device.DeviceDomainId))
                            if (device.DeviceProfile.EnforceComputerNameConvention)
                            {
                                var (renderedDeviceDomainId, renderedDnsHostName) = device.RenderComputerNames(database, domain);
                                device.DeviceDomainId = renderedDeviceDomainId;
                                device.DnsHostName = renderedDnsHostName;
                            }
                            else
                            {
                                device.DeviceDomainId = $@"{domain.NetBiosName}\{request.ComputerName}";
                                device.DnsHostName = request.DnsHostName ?? device.ComputerName;
                            }

                        EnrolmentLog.LogSessionTaskProvisioningADAccount(sessionId, device.SerialNumber, device.DeviceDomainId);
                        adMachineAccount = domainController.Value.RetrieveADMachineAccount(device.DeviceDomainId);

                        if (device.DnsHostName.Length > 15)
                            EnrolmentLog.LogSessionWarning(sessionId, $"The sAMAccountName (Computer Name) is limited to 15 characters in Active Directory. The name will be shortened to \"{device.DnsHostName.Substring(0, 15)}\", which may cause conflicts or override existing names. The DNS Name \"{device.DnsHostName}\" will be used.");

                        response.OfflineDomainJoinManifest = domainController.Value.OfflineDomainJoinProvision(device.DnsHostName, device.DeviceProfile.OrganisationalUnit, ref adMachineAccount);

                        response.RequireReboot = true;
                    }
                    if (adMachineAccount != null)
                    {
                        response.ComputerName = adMachineAccount.Name;
                        response.DomainName = adMachineAccount.Domain.NetBiosName;
                    }
                    else if (ActiveDirectory.IsValidDomainAccountId(device.DeviceDomainId, out var accountUsername, out var accountDomain))
                    {
                        response.DomainName = accountDomain?.NetBiosName;
                        response.ComputerName = accountUsername;
                    }
                    else
                    {
                        response.DomainName = request.DNSDomainName;
                        response.ComputerName = request.ComputerName;
                    }
                }
                else
                {
                    device.DeviceDomainId = adMachineAccount.Id.Trim('$');
                    device.DnsHostName = dnsHostName;
                    response.ComputerName = adMachineAccount.Name;
                    response.DomainName = adMachineAccount.Domain.NetBiosName;

                    // Enforce Computer Name Convention
                    if (!adMachineAccount.IsCriticalSystemObject && device.DeviceProfile.EnforceComputerNameConvention)
                    {
                        if (string.IsNullOrWhiteSpace(device.DeviceProfile.OrganisationalUnit))
                            throw new InvalidOperationException("No Organisational Unit has been set in the device profile");
                        if (domain == null)
                            domain = ActiveDirectory.Context.GetDomainFromDistinguishedName(device.DeviceProfile.OrganisationalUnit);

                        var (renderedDeviceDomainId, renderedDnsHostName) = device.RenderComputerNames(database, domain);
                        ActiveDirectory.ParseDomainAccountId(renderedDeviceDomainId, out string calculatedAccountUsername);

                        if (!request.ComputerName.Equals(calculatedAccountUsername, StringComparison.OrdinalIgnoreCase))
                        {
                            EnrolmentLog.LogSessionProgress(sessionId, 50, $"Renaming Device: {request.ComputerName} -> {renderedDeviceDomainId}");
                            EnrolmentLog.LogSessionTaskRenamingDevice(sessionId, request.ComputerName, renderedDeviceDomainId);

                            if (renderedDnsHostName.Length > 15)
                                EnrolmentLog.LogSessionWarning(sessionId, $"The sAMAccountName (Computer Name) is limited to 15 characters in Active Directory. The name will be shortened to \"{renderedDnsHostName.Substring(0, 15)}\", which may cause conflicts or override existing names. The DNS Name \"{renderedDnsHostName}\" will be used.");

                            device.DeviceDomainId = renderedDeviceDomainId;
                            device.DnsHostName = renderedDnsHostName;
                            response.DomainName = domain.NetBiosName;
                            response.ComputerName = calculatedAccountUsername;

                            // Create New Account
                            response.OfflineDomainJoinManifest = domainController.Value.OfflineDomainJoinProvision(device.DnsHostName, device.DeviceProfile.OrganisationalUnit, ref adMachineAccount);

                            response.RequireReboot = true;
                        }
                    }

                    // Enforce Organisational Unit
                    if (!adMachineAccount.IsCriticalSystemObject && device.DeviceProfile.EnforceOrganisationalUnit)
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
                        var macAddress = request.Hardware?.NetworkAdapters?.Where(na => !na.IsWlanAdapter).OrderByDescending(na => na.Speed).Select(na => na.MACAddress).FirstOrDefault();
                        adMachineAccount.UpdateNetbootGUID(request.Hardware.UUID, macAddress);
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

                response.SetAssignedUserForLogon = device.DeviceProfile.SetAssignedUserForLogon;

                // Provision Certificates
                if (!string.IsNullOrEmpty(device.DeviceProfile.CertificateProviders) ||
                    !string.IsNullOrEmpty(device.DeviceProfile.CertificateAuthorityProviders))
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 90, "Provisioning Certificates");

                    var provisionResult = device.ProvisionCertificates(database, request, out var provisionedCertificates);

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

                    var provisionResult = device.ProvisionWirelessProfiles(database, request);

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
                throw;
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
