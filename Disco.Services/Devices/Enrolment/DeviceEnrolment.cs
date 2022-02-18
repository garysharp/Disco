using Disco.Data.Repository;
using Disco.Models.ClientServices;
using Disco.Models.Repository;
using Disco.Services.Authorization;
using Disco.Services.Interop.ActiveDirectory;
using Disco.Services.Users;
using PListNet;
using PListNet.Nodes;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Disco.Services.Devices.Enrolment
{
    public static class DeviceEnrolment
    {

        public static MacSecureEnrolResponse MacSecureEnrol(DiscoDataContext Database, string Host)
        {
            MacEnrol trustedRequest = new MacEnrol();
            string sessionId = Guid.NewGuid().ToString("B");
            MacSecureEnrolResponse MacSecureEnrol;
            try
            {
                EnrolmentLog.LogSessionStarting(sessionId, Host, EnrolmentTypes.MacSecure);
                EnrolmentLog.LogSessionProgress(sessionId, 0, $"Connecting to '{Host}' as '{Database.DiscoConfiguration.Bootstrapper.MacSshUsername}'");

                var sshConnectionInfo = new KeyboardInteractiveConnectionInfo(Host, Database.DiscoConfiguration.Bootstrapper.MacSshUsername);
                sshConnectionInfo.AuthenticationPrompt += (sender, e) =>
                {
                    foreach (var prompt in e.Prompts)
                    {
                        if (prompt.Request.StartsWith("Password", StringComparison.OrdinalIgnoreCase))
                        {
                            EnrolmentLog.LogSessionProgress(sessionId, 10, $"Authenticating at '{Host}' as '{Database.DiscoConfiguration.Bootstrapper.MacSshUsername}'");
                            prompt.Response = Database.DiscoConfiguration.Bootstrapper.MacSshPassword;
                        }
                    }
                };

                using (var sshClient = new SshClient(sshConnectionInfo))
                {
                    sshClient.Connect();

                    try
                    {
                        EnrolmentLog.LogSessionProgress(sessionId, 30, "Retrieving System Profile Information");
                        var sshResult = sshClient.RunCommand("system_profiler -xml SPHardwareDataType SPNetworkDataType SPSoftwareDataType");

                        ArrayNode profilerData;

                        using (var sshResultStream = new MemoryStream())
                        {
                            using (var sshResultWriter = new StreamWriter(sshResultStream, Encoding.UTF8, 0x400, true))
                            {
                                sshResultWriter.Write(sshResult.Result);
                            }
                            sshResultStream.Position = 0;

                            profilerData = PList.Load(sshResultStream) as ArrayNode;
                        }

                        EnrolmentLog.LogSessionProgress(sessionId, 90, "Processing System Profile Information");

                        DictionaryNode profilerDataHardware = null;
                        ArrayNode profilerDataNetwork = null;
                        DictionaryNode profilerDataSoftware = null;

                        if (profilerData == null)
                            throw new InvalidOperationException("System Profiler didn't return the expected response");

                        foreach (var node in profilerData.OfType<DictionaryNode>())
                        {
                            var nodeItems = ((ArrayNode)node["_items"]);
                            PNode nodeDataType;

                            if (node.TryGetValue("_dataType", out nodeDataType) && nodeDataType is StringNode)
                            {
                                switch (((StringNode)nodeDataType).Value)
                                {
                                    case "SPHardwareDataType":
                                        profilerDataHardware = (DictionaryNode)nodeItems[0];
                                        break;
                                    case "SPNetworkDataType":
                                        profilerDataNetwork = nodeItems;
                                        break;
                                    case "SPSoftwareDataType":
                                        profilerDataSoftware = (DictionaryNode)nodeItems[0];
                                        break;
                                }
                            }

                        }

                        if (profilerDataHardware == null || profilerDataNetwork == null || profilerDataSoftware == null)
                            throw new InvalidOperationException("System Profiler didn't return information for a requested data type");

                        trustedRequest.DeviceSerialNumber = ((StringNode)profilerDataHardware["serial_number"]).Value;
                        trustedRequest.DeviceUUID = ((StringNode)profilerDataHardware["platform_UUID"]).Value;
                        trustedRequest.DeviceComputerName = ((StringNode)profilerDataSoftware["local_host_name"]).Value;

                        var profilerDataNetworkEthernet = profilerDataNetwork.OfType<DictionaryNode>().FirstOrDefault(e => ((StringNode)e["_name"]).Value == "Ethernet");
                        if (profilerDataNetworkEthernet != null)
                        {
                            trustedRequest.DeviceLanMacAddress = ((StringNode)((DictionaryNode)profilerDataNetworkEthernet["Ethernet"])["MAC Address"]).Value;
                        }

                        var profilerDataNetworkWiFi = profilerDataNetwork.OfType<DictionaryNode>().FirstOrDefault(e => ((StringNode)e["_name"]).Value == "Wi-Fi");
                        if (profilerDataNetworkWiFi != null)
                        {
                            trustedRequest.DeviceWlanMacAddress = ((StringNode)((DictionaryNode)profilerDataNetworkWiFi["Ethernet"])["MAC Address"]).Value;
                        }

                        trustedRequest.DeviceManufacturer = "Apple Inc.";
                        trustedRequest.DeviceModel = ((StringNode)profilerDataHardware["machine_model"]).Value;

                        trustedRequest.DeviceModelType = ParseMacModelType(((StringNode)profilerDataHardware["machine_name"]).Value);

                        EnrolmentLog.LogSessionProgress(sessionId, 99, "Disconnecting");

                        sshClient.Disconnect();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        if (sshClient != null)
                        {
                            bool connected = sshClient.IsConnected;
                            if (connected)
                            {
                                sshClient.Disconnect();
                            }
                        }
                    }
                }

                EnrolmentLog.LogSessionProgress(sessionId, 100, "Disconnected, Starting Disco Enrolment");
                MacSecureEnrolResponse response = MacSecureEnrolResponse.FromMacEnrolResponse(MacEnrol(Database, trustedRequest, true, sessionId));
                EnrolmentLog.LogSessionFinished(sessionId);
                MacSecureEnrol = response;
            }
            catch (System.Exception ex)
            {
                EnrolmentLog.LogSessionError(sessionId, ex);
                throw ex;
            }

            return MacSecureEnrol;
        }

        #region "Mac Enrol Helpers"

        private static string ParseMacModelType(string ModelName)
        {
            string ParseMacModelType;
            if (!string.IsNullOrWhiteSpace(ModelName))
            {
                string mn = ModelName.ToLower();
                if (mn.Contains("imac") || mn.Contains("mini"))
                {
                    ParseMacModelType = "Desktop";
                    return ParseMacModelType;
                }
                if (mn.Contains("macbook"))
                {
                    ParseMacModelType = "Mobile";
                    return ParseMacModelType;
                }
                if (mn.Contains("xserve"))
                {
                    ParseMacModelType = "Server";
                    return ParseMacModelType;
                }
            }
            ParseMacModelType = "Unknown";
            return ParseMacModelType;
        }

        #endregion

        public static MacEnrolResponse MacEnrol(DiscoDataContext Database, MacEnrol Request, bool Trusted, string OpenSessionId = null)
        {
            string sessionId;
            if (OpenSessionId == null)
            {
                sessionId = Guid.NewGuid().ToString("B");
                EnrolmentLog.LogSessionStarting(sessionId, Request.DeviceSerialNumber, EnrolmentTypes.Mac);
            }
            else
            {
                sessionId = OpenSessionId;
            }
            EnrolmentLog.LogSessionDeviceInfo(sessionId, Request);

            MacEnrolResponse response = new MacEnrolResponse();
            try
            {
                if (Request.DeviceSerialNumber.Contains("/") || Request.DeviceSerialNumber.Contains(@"\"))
                    throw new EnrolmentSafeException(@"The serial number cannot contain '/' or '\' characters.");

                EnrolmentLog.LogSessionProgress(sessionId, 10, "Querying Database");
                Device RepoDevice = Database.Devices.Include("AssignedUser").Include("DeviceProfile").Include("DeviceProfile").Where(d => d.SerialNumber == Request.DeviceSerialNumber).FirstOrDefault();
                if (!Trusted)
                {
                    if (RepoDevice == null)
                        throw new EnrolmentSafeException(string.Format("Unknown Device Serial Number (SN: '{0}')", Request.DeviceSerialNumber));
                    if (!RepoDevice.AllowUnauthenticatedEnrol)
                        throw new EnrolmentSafeException(string.Format("Device isn't allowed an Unauthenticated Enrolment (SN: '{0}')", Request.DeviceSerialNumber));
                }
                if (RepoDevice == null)
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 50, "New Device, Building Disco Instance");
                    EnrolmentLog.LogSessionTaskAddedDevice(sessionId, Request.DeviceSerialNumber);
                    DeviceProfile deviceProfile = Database.DeviceProfiles.Find(Database.DiscoConfiguration.DeviceProfiles.DefaultDeviceProfileId);

                    var deviceModelResult = Database.DeviceModels.GetOrCreateDeviceModel(Request.DeviceManufacturer, Request.DeviceModel, Request.DeviceModelType);
                    DeviceModel deviceModel = deviceModelResult.Item1;
                    if (deviceModelResult.Item2)
                        EnrolmentLog.LogSessionTaskCreatedDeviceModel(sessionId, Request.DeviceSerialNumber, deviceModelResult.Item1.Manufacturer, deviceModelResult.Item1.Model);
                    else
                        EnrolmentLog.LogSessionDevice(sessionId, Request.DeviceSerialNumber, deviceModel.Id);

                    RepoDevice = new Device
                    {
                        SerialNumber = Request.DeviceSerialNumber,
                        DeviceDomainId = Request.DeviceComputerName,
                        DeviceProfile = deviceProfile,
                        DeviceModel = deviceModel,
                        AllowUnauthenticatedEnrol = false,
                        CreatedDate = DateTime.Now,
                        EnrolledDate = DateTime.Now
                    };
                    Database.Devices.Add(RepoDevice);
                }
                else
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 50, "Existing Device, Updating Disco Instance");
                    EnrolmentLog.LogSessionTaskUpdatingDevice(sessionId, Request.DeviceSerialNumber);

                    var deviceModelResult = Database.DeviceModels.GetOrCreateDeviceModel(Request.DeviceManufacturer, Request.DeviceModel, Request.DeviceModelType);
                    DeviceModel deviceModel = deviceModelResult.Item1;
                    if (deviceModelResult.Item2)
                        EnrolmentLog.LogSessionTaskCreatedDeviceModel(sessionId, Request.DeviceSerialNumber, deviceModelResult.Item1.Manufacturer, deviceModelResult.Item1.Model);
                    else
                        EnrolmentLog.LogSessionDevice(sessionId, Request.DeviceSerialNumber, deviceModel.Id);

                    RepoDevice.DeviceModel = deviceModel;

                    RepoDevice.DeviceDomainId = Request.DeviceComputerName;
                    if (!RepoDevice.EnrolledDate.HasValue)
                    {
                        RepoDevice.EnrolledDate = DateTime.Now;
                    }
                }
                RepoDevice.LastEnrolDate = DateTime.Now;
                RepoDevice.AllowUnauthenticatedEnrol = false;
                // Removed 2012-06-14 G# - Properties moved to DeviceProfile model & DB Migrated in DBv3.
                //DeviceProfileConfiguration RepoDeviceProfileContext = RepoDevice.DeviceProfile.Configuration(Context);
                EnrolmentLog.LogSessionProgress(sessionId, 90, "Building Response");
                //if (RepoDeviceProfileContext.DistributionType == DeviceProfileConfiguration.DeviceProfileDistributionTypes.OneToOne && RepoDevice.AssignedUser != null)
                if (RepoDevice.DeviceProfile.DistributionType == DeviceProfile.DistributionTypes.OneToOne && RepoDevice.AssignedUser != null)
                {
                    ADUserAccount AssignedUserInfo = ActiveDirectory.RetrieveADUserAccount(RepoDevice.AssignedUser.UserId);
                    EnrolmentLog.LogSessionTaskAssigningUser(sessionId, RepoDevice.SerialNumber, AssignedUserInfo.DisplayName, AssignedUserInfo.SamAccountName, AssignedUserInfo.Domain.NetBiosName, AssignedUserInfo.SecurityIdentifier.ToString());
                    response.DeviceAssignedUserUsername = AssignedUserInfo.SamAccountName;
                    response.DeviceAssignedUserDomain = AssignedUserInfo.Domain.NetBiosName;
                    response.DeviceAssignedUserName = AssignedUserInfo.DisplayName;
                    response.DeviceAssignedUserSID = AssignedUserInfo.SecurityIdentifier.ToString();
                }
                response.DeviceComputerName = RepoDevice.DeviceDomainId;
                EnrolmentLog.LogSessionProgress(sessionId, 100, "Completed Successfully");
            }
            catch (EnrolmentSafeException ex)
            {
                EnrolmentLog.LogSessionError(sessionId, ex);
                return new MacEnrolResponse { ErrorMessage = ex.Message };
            }
            catch (Exception ex2)
            {
                EnrolmentLog.LogSessionError(sessionId, ex2);
                throw ex2;
            }
            finally
            {
                if (OpenSessionId == null)
                    EnrolmentLog.LogSessionFinished(sessionId);
            }
            return response;
        }
        public static EnrolResponse Enrol(DiscoDataContext Database, string Username, Enrol Request)
        {
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

            string sessionId = Guid.NewGuid().ToString("B");
            response.SessionId = sessionId;

            EnrolmentLog.LogSessionStarting(sessionId, Request.SerialNumber, EnrolmentTypes.Normal);
            EnrolmentLog.LogSessionDeviceInfo(sessionId, Request);

            try
            {
                if (Request.SerialNumber.Contains("/") || Request.SerialNumber.Contains(@"\"))
                    throw new EnrolmentSafeException(@"The serial number cannot contain '/' or '\' characters.");

                EnrolmentLog.LogSessionProgress(sessionId, 10, "Loading User Data");
                if (!string.IsNullOrWhiteSpace(Username))
                {
                    authenticatedToken = UserService.GetAuthorization(Username, Database);
                    isAuthenticated = (authenticatedToken != null);
                }
                EnrolmentLog.LogSessionProgress(sessionId, 13, "Loading Device Data");

                Device RepoDevice = Database.Devices.Include("AssignedUser").Include("DeviceModel").Include("DeviceProfile").Where(d => d.SerialNumber == Request.SerialNumber).FirstOrDefault();
                EnrolmentLog.LogSessionProgress(sessionId, 15, "Discovering User/Device Disco Permissions");
                if (isAuthenticated)
                {
                    if (!authenticatedToken.Has(Claims.Device.Actions.EnrolDevices))
                    {
                        if (!authenticatedToken.Has(Claims.ComputerAccount))
                            throw new EnrolmentSafeException(string.Format("Connection not correctly authenticated (SN: {0}; Auth User: {1})", Request.SerialNumber, authenticatedToken.User.UserId));

                        if (domain == null)
                            domain = ActiveDirectory.Context.GetDomainByName(Request.DNSDomainName);

                        if (!authenticatedToken.User.UserId.Equals(string.Format(@"{0}\{1}$", domain.NetBiosName, Request.ComputerName), StringComparison.OrdinalIgnoreCase))
                            throw new EnrolmentSafeException(string.Format("Connection not correctly authenticated (SN: {0}; Auth User: {1})", Request.SerialNumber, authenticatedToken.User.UserId));
                    }
                }
                else
                {
                    if (RepoDevice == null)
                    {
                        throw new EnrolmentSafeException(string.Format("Unknown Device Serial Number (SN: '{0}')", Request.SerialNumber));
                    }
                    if (!RepoDevice.AllowUnauthenticatedEnrol)
                    {
                        if (RepoDevice.DeviceProfile.AllowUntrustedReimageJobEnrolment)
                        {
                            if (Database.Jobs.Count(j => j.DeviceSerialNumber == RepoDevice.SerialNumber && j.JobTypeId == JobType.JobTypeIds.SImg && !j.ClosedDate.HasValue) == 0)
                            {
                                throw new EnrolmentSafeException(string.Format("Device has no open 'Software - Reimage' job (SN: '{0}')", Request.SerialNumber));
                            }
                        }
                        else
                        {
                            throw new EnrolmentSafeException(string.Format("Device isn't allowed an Unauthenticated Enrolment (SN: '{0}')", Request.SerialNumber));
                        }
                    }
                }
                if (Request.IsPartOfDomain && !string.IsNullOrWhiteSpace(Request.ComputerName))
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 20, "Loading Active Directory Computer Account");
                    System.Guid? uuidGuid = null;
                    System.Guid? macAddressGuid = null;
                    if (!string.IsNullOrEmpty(Request.Hardware.UUID))
                        uuidGuid = ADMachineAccount.NetbootGUIDFromUUID(Request.Hardware.UUID);

                    // Use non-Wlan Adapter with fastest speed
                    var macAddress = Request.Hardware?.NetworkAdapters?.Where(na => !na.IsWlanAdapter).OrderByDescending(na => na.Speed).Select(na => na.MACAddress).FirstOrDefault();
                    if (!string.IsNullOrEmpty(macAddress))
                        macAddressGuid = ADMachineAccount.NetbootGUIDFromMACAddress(macAddress);

                    if (domain == null)
                        domain = ActiveDirectory.Context.GetDomainByName(Request.DNSDomainName);

                    var requestDeviceId = string.Format(@"{0}\{1}", domain.NetBiosName, Request.ComputerName);

                    adMachineAccount = domainController.Value.RetrieveADMachineAccount(requestDeviceId, uuidGuid, macAddressGuid);
                }
                if (RepoDevice == null)
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

                    if (domain == null)
                        domain = ActiveDirectory.Context.GetDomainByName(Request.DNSDomainName);

                    RepoDevice = new Device
                    {
                        SerialNumber = Request.SerialNumber,
                        DeviceDomainId = string.Format(@"{0}\{1}", domain.NetBiosName, Request.ComputerName),
                        DeviceProfile = deviceProfile,
                        DeviceModel = deviceModel,
                        AllowUnauthenticatedEnrol = false,
                        CreatedDate = DateTime.Now,
                        EnrolledDate = DateTime.Now,
                        LastEnrolDate = DateTime.Now,
                        DeviceDetails = new List<DeviceDetail>()
                    };
                    Database.Devices.Add(RepoDevice);

                    var lanMacAddresses = string.Join("; ", Request.Hardware.NetworkAdapters?.Where(na => !na.IsWlanAdapter).Select(na => na.MACAddress));
                    var wlanMacAddresses = string.Join("; ", Request.Hardware.NetworkAdapters?.Where(na => na.IsWlanAdapter).Select(na => na.MACAddress));
                    if (!string.IsNullOrEmpty(lanMacAddresses))
                        RepoDevice.DeviceDetails.LanMacAddress(RepoDevice, lanMacAddresses);
                    if (!string.IsNullOrEmpty(wlanMacAddresses))
                        RepoDevice.DeviceDetails.WLanMacAddress(RepoDevice, wlanMacAddresses);
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

                    RepoDevice.DeviceModel = deviceModel;

                    var lanMacAddresses = string.Join("; ", Request.Hardware.NetworkAdapters?.Where(na => !na.IsWlanAdapter).Select(na => na.MACAddress));
                    var wlanMacAddresses = string.Join("; ", Request.Hardware.NetworkAdapters?.Where(na => na.IsWlanAdapter).Select(na => na.MACAddress));
                    if (!string.IsNullOrEmpty(lanMacAddresses))
                        RepoDevice.DeviceDetails.LanMacAddress(RepoDevice, lanMacAddresses);
                    if (!string.IsNullOrEmpty(wlanMacAddresses))
                        RepoDevice.DeviceDetails.WLanMacAddress(RepoDevice, wlanMacAddresses);

                    if (!RepoDevice.EnrolledDate.HasValue)
                        RepoDevice.EnrolledDate = DateTime.Now;
                    RepoDevice.LastEnrolDate = DateTime.Now;
                }

                // store hardware audit information
                if (Request.Hardware.Bios?.Count > 0)
                    RepoDevice.DeviceDetails.Bios(RepoDevice, Request.Hardware.Bios);
                if (Request.Hardware.BasebBoard?.Count > 0)
                    RepoDevice.DeviceDetails.BaseBoard(RepoDevice, Request.Hardware.BasebBoard);
                if (Request.Hardware.ComputerSystem?.Count > 0)
                    RepoDevice.DeviceDetails.ComputerSystem(RepoDevice, Request.Hardware.ComputerSystem);
                if (Request.Hardware.Processors?.Count > 0)
                    RepoDevice.DeviceDetails.Processors(RepoDevice, Request.Hardware.Processors);
                if (Request.Hardware.PhysicalMemory?.Count > 0)
                    RepoDevice.DeviceDetails.PhysicalMemory(RepoDevice, Request.Hardware.PhysicalMemory);
                if (Request.Hardware.DiskDrives?.Count > 0)
                    RepoDevice.DeviceDetails.DiskDrives(RepoDevice, Request.Hardware.DiskDrives);
                if (Request.Hardware.NetworkAdapters?.Count > 0)
                    RepoDevice.DeviceDetails.NetworkAdapters(RepoDevice, Request.Hardware.NetworkAdapters);
                if (Request.Hardware.Batteries?.Count > 0)
                    RepoDevice.DeviceDetails.Batteries(RepoDevice, Request.Hardware.Batteries);

                if (adMachineAccount == null)
                {
                    if (RepoDevice.DeviceProfile.ProvisionADAccount)
                    {
                        EnrolmentLog.LogSessionProgress(sessionId, 50, "Provisioning an Active Directory Computer Account");

                        if (string.IsNullOrWhiteSpace(RepoDevice.DeviceProfile.OrganisationalUnit))
                            throw new InvalidOperationException("No Organisational Unit has been set in the device profile");
                        if (domain == null)
                            domain = ActiveDirectory.Context.GetDomainFromDistinguishedName(RepoDevice.DeviceProfile.OrganisationalUnit);

                        if (string.IsNullOrEmpty(RepoDevice.DeviceDomainId) || RepoDevice.DeviceProfile.EnforceComputerNameConvention)
                            RepoDevice.DeviceDomainId = RepoDevice.ComputerNameRender(Database, domain);

                        string offlineProvisionDiagnosicInfo;
                        EnrolmentLog.LogSessionTaskProvisioningADAccount(sessionId, RepoDevice.SerialNumber, RepoDevice.DeviceDomainId);
                        adMachineAccount = domainController.Value.RetrieveADMachineAccount(RepoDevice.DeviceDomainId);

                        response.OfflineDomainJoinManifest = domainController.Value.OfflineDomainJoinProvision(RepoDevice.DeviceDomainId, RepoDevice.DeviceProfile.OrganisationalUnit, ref adMachineAccount, out offlineProvisionDiagnosicInfo);

                        EnrolmentLog.LogSessionDiagnosticInformation(sessionId, offlineProvisionDiagnosicInfo);

                        response.RequireReboot = true;
                    }
                    if (adMachineAccount != null)
                    {
                        response.ComputerName = adMachineAccount.Name;
                        response.DomainName = adMachineAccount.Domain.NetBiosName;
                    }
                    else if (ActiveDirectory.IsValidDomainAccountId(RepoDevice.DeviceDomainId))
                    {
                        string accountUsername;
                        ADDomain accountDomain;
                        ActiveDirectory.ParseDomainAccountId(RepoDevice.DeviceDomainId, out accountUsername, out accountDomain);

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
                    RepoDevice.DeviceDomainId = adMachineAccount.Id.Trim('$');
                    response.ComputerName = adMachineAccount.Name;
                    response.DomainName = adMachineAccount.Domain.NetBiosName;

                    // Enforce Computer Name Convention
                    if (!adMachineAccount.IsCriticalSystemObject && RepoDevice.DeviceProfile.EnforceComputerNameConvention)
                    {
                        if (string.IsNullOrWhiteSpace(RepoDevice.DeviceProfile.OrganisationalUnit))
                            throw new InvalidOperationException("No Organisational Unit has been set in the device profile");
                        if (domain == null)
                            domain = ActiveDirectory.Context.GetDomainFromDistinguishedName(RepoDevice.DeviceProfile.OrganisationalUnit);

                        var calculatedComputerName = RepoDevice.ComputerNameRender(Database, domain);
                        string calculatedAccountUsername;
                        ActiveDirectory.ParseDomainAccountId(calculatedComputerName, out calculatedAccountUsername);

                        if (!Request.ComputerName.Equals(calculatedAccountUsername, StringComparison.OrdinalIgnoreCase))
                        {
                            EnrolmentLog.LogSessionProgress(sessionId, 50, string.Format("Renaming Device: {0} -> {1}", Request.ComputerName, calculatedComputerName));
                            EnrolmentLog.LogSessionTaskRenamingDevice(sessionId, Request.ComputerName, calculatedComputerName);

                            RepoDevice.DeviceDomainId = calculatedComputerName;
                            response.DomainName = domain.NetBiosName;
                            response.ComputerName = calculatedAccountUsername;

                            // Create New Account
                            string offlineProvisionDiagnosicInfo;

                            response.OfflineDomainJoinManifest = domainController.Value.OfflineDomainJoinProvision(RepoDevice.DeviceDomainId, RepoDevice.DeviceProfile.OrganisationalUnit, ref adMachineAccount, out offlineProvisionDiagnosicInfo);

                            EnrolmentLog.LogSessionDiagnosticInformation(sessionId, offlineProvisionDiagnosicInfo);

                            response.RequireReboot = true;
                        }
                    }

                    // Enforce Organisational Unit
                    if (!adMachineAccount.IsCriticalSystemObject && response.OfflineDomainJoinManifest == null && RepoDevice.DeviceProfile.EnforceOrganisationalUnit)
                    {
                        var parentDistinguishedName = adMachineAccount.ParentDistinguishedName;
                        if (string.IsNullOrWhiteSpace(RepoDevice.DeviceProfile.OrganisationalUnit))
                            throw new InvalidOperationException(string.Format("The Organisational Unit for the Device Profile '{0}' [{1}] is not set.", RepoDevice.DeviceProfile.Name, RepoDevice.DeviceProfile.Id));

                        if (!parentDistinguishedName.Equals(RepoDevice.DeviceProfile.OrganisationalUnit, StringComparison.OrdinalIgnoreCase)) // Custom OU
                        {
                            var proposedDomain = ActiveDirectory.Context.GetDomainFromDistinguishedName(RepoDevice.DeviceProfile.OrganisationalUnit);
                            var currentDomain = ActiveDirectory.Context.GetDomainFromDistinguishedName(parentDistinguishedName);
                            if (currentDomain != proposedDomain)
                                throw new NotSupportedException("Unable to move the devices organisational unit when the source and destination domains are different.");
                            if (domain == null)
                                domain = proposedDomain;
                            else if (domain != proposedDomain)
                                throw new NotSupportedException("To many domains involved in this enrolment, contact support regarding your scenario.");

                            EnrolmentLog.LogSessionProgress(sessionId, 65, string.Format("Moving Device Organisational Unit: {0} -> {1}", parentDistinguishedName, RepoDevice.DeviceProfile.OrganisationalUnit));
                            EnrolmentLog.LogSessionTaskMovingDeviceOrganisationUnit(sessionId, parentDistinguishedName, RepoDevice.DeviceProfile.OrganisationalUnit);
                            adMachineAccount.MoveOrganisationalUnit(domainController.Value, RepoDevice.DeviceProfile.OrganisationalUnit);
                            response.RequireReboot = true;
                        }
                    }

                }
                if (adMachineAccount != null && !adMachineAccount.IsCriticalSystemObject)
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 75, "Updating Active Directory Computer Account Properties");
                    // Use non-Wlan Adapter with fastest speed
                    var macAddress = Request.Hardware?.NetworkAdapters?.Where(na => !na.IsWlanAdapter).OrderByDescending(na => na.Speed).Select(na => na.MACAddress).FirstOrDefault();
                    adMachineAccount.UpdateNetbootGUID(Request.Hardware.UUID, macAddress);
                    if (RepoDevice.AssignedUser != null)
                        adMachineAccount.SetDescription(RepoDevice);
                }
                if (RepoDevice.DeviceProfile.DistributionType == DeviceProfile.DistributionTypes.OneToOne)
                {
                    if (RepoDevice.AssignedUser == null)
                    {
                        response.AllowBootstrapperUninstall = false;
                    }
                    else
                    {
                        EnrolmentLog.LogSessionProgress(sessionId, 80, "Retrieving Active Directory Assigned User Account");
                        ADUserAccount AssignedUserInfo = ActiveDirectory.RetrieveADUserAccount(RepoDevice.AssignedUser.UserId);
                        EnrolmentLog.LogSessionTaskAssigningUser(sessionId, RepoDevice.SerialNumber, AssignedUserInfo.DisplayName, AssignedUserInfo.SamAccountName, AssignedUserInfo.Domain.NetBiosName, AssignedUserInfo.SecurityIdentifier.ToString());
                        response.AllowBootstrapperUninstall = true;
                        response.AssignedUserIsLocalAdmin = RepoDevice.DeviceProfile.AssignedUserLocalAdmin;
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
                if (!string.IsNullOrEmpty(RepoDevice.DeviceProfile.CertificateProviders) ||
                    !string.IsNullOrEmpty(RepoDevice.DeviceProfile.CertificateAuthorityProviders))
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 90, "Provisioning Certificates");

                    List<DeviceCertificate> provisionedCertificates;
                    var provisionResult = RepoDevice.ProvisionCertificates(Database, Request, out provisionedCertificates);

                    if (provisionedCertificates != null && provisionedCertificates.Count > 0)
                    {
                        foreach (var deviceCertificate in provisionedCertificates)
                        {
                            EnrolmentLog.LogSessionTaskProvisioningCertificate(sessionId, RepoDevice.SerialNumber, deviceCertificate.Name);
                        }
                    }

                    response.Certificates = provisionResult;
                }

                // Provision Wireless Profiles
                if (!string.IsNullOrEmpty(RepoDevice.DeviceProfile.WirelessProfileProviders))
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 95, "Provisioning Wireless Profiles");

                    var provisionResult = RepoDevice.ProvisionWirelessProfiles(Database, Request);

                    if (provisionResult != null && provisionResult.Profiles != null)
                    {
                        foreach (var wirelessProfiles in provisionResult.Profiles)
                        {
                            EnrolmentLog.LogSessionTaskProvisioningWirelessProfile(sessionId, RepoDevice.SerialNumber, wirelessProfiles.Name);
                        }
                    }

                    response.WirelessProfiles = provisionResult;
                }

                // Reset 'AllowUnauthenticatedEnrol'
                if (RepoDevice.AllowUnauthenticatedEnrol)
                    RepoDevice.AllowUnauthenticatedEnrol = false;

                EnrolmentLog.LogSessionProgress(sessionId, 100, "Completed Successfully");
            }
            catch (EnrolmentSafeException ex)
            {
                EnrolmentLog.LogSessionError(sessionId, ex);
                return new EnrolResponse
                {
                    SessionId = sessionId,
                    ErrorMessage = ex.Message
                };
            }
            catch (Exception ex2)
            {
                EnrolmentLog.LogSessionError(sessionId, ex2);
                throw ex2;
            }
            finally
            {
                EnrolmentLog.LogSessionFinished(sessionId);
            }
            return response;
        }

    }
}
