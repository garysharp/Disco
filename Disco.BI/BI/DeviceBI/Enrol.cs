using System;
using System.Linq;
using System.Text.RegularExpressions;
using Disco.BI.Interop.ActiveDirectory;
using Disco.BI.Extensions;
using Disco.Data.Configuration.Modules;
using Disco.Data.Repository;
using Disco.Models.ClientServices;
using Disco.Models.Interop.ActiveDirectory;
using Disco.Models.Repository;
using Tamir.SharpSsh;
using Disco.Services.Plugins;
using Disco.Services.Plugins.Features.CertificateProvider;

namespace Disco.BI.DeviceBI
{
    public class DeviceEnrol
    {
        public enum EnrolmentTypes
        {
            Normal,
            Mac = 5,
            MacSecure
        }

        private static Regex SshPromptRegEx = new Regex("[\\$,\\#]", RegexOptions.Multiline);
        public static MacSecureEnrolResponse MacSecureEnrol(DiscoDataContext dbContext, string Host)
        {
            MacEnrol trustedRequest = new MacEnrol();
            string sessionId = System.Guid.NewGuid().ToString("B");
            MacSecureEnrolResponse MacSecureEnrol;
            try
            {
                EnrolmentLog.LogSessionStarting(sessionId, Host, EnrolmentTypes.MacSecure);
                EnrolmentLog.LogSessionProgress(sessionId, 0, string.Format("Connecting to '{0}' as '{1}'", Host, dbContext.DiscoConfiguration.Bootstrapper.MacSshUsername));
                SshShell shell = new SshShell(Host, dbContext.DiscoConfiguration.Bootstrapper.MacSshUsername, dbContext.DiscoConfiguration.Bootstrapper.MacSshPassword);
                try
                {
                    shell.ExpectPattern = "#";
                    shell.Connect();
                    EnrolmentLog.LogSessionProgress(sessionId, 10, "Connected, Authenticating");
                    var output = shell.Expect(SshPromptRegEx);
                    bool sessionElevated = false;
                    EnrolmentLog.LogSessionDiagnosticInformation(sessionId, output);
                    if (!output.TrimEnd(new char[0]).EndsWith("#"))
                    {
                        EnrolmentLog.LogSessionProgress(sessionId, 22, "Connected, Elevating Credentials");
                        shell.WriteLine("sudo -k");
                        System.Threading.Thread.Sleep(250);
                        output = shell.Expect(SshPromptRegEx);
                        EnrolmentLog.LogSessionProgress(sessionId, 25, "Connected, Elevating Credentials");
                        EnrolmentLog.LogSessionDiagnosticInformation(sessionId, output);
                        shell.WriteLine("sudo -s -S");
                        System.Threading.Thread.Sleep(250);
                        output = shell.Expect(":");
                        EnrolmentLog.LogSessionProgress(sessionId, 27, "Connected, Elevating Credentials");
                        EnrolmentLog.LogSessionDiagnosticInformation(sessionId, output);
                        shell.WriteLine(dbContext.DiscoConfiguration.Bootstrapper.MacSshPassword);
                        System.Threading.Thread.Sleep(250);
                        output = shell.Expect(SshPromptRegEx);
                        sessionElevated = true;
                        EnrolmentLog.LogSessionDiagnosticInformation(sessionId, output);
                    }
                    EnrolmentLog.LogSessionProgress(sessionId, 20, "Retrieving Serial Number");
                    trustedRequest.DeviceSerialNumber = ParseMacShellCommand(shell, "system_profiler SPHardwareDataType | grep \"Serial Number\" | cut -d \":\" -f 2-", sessionId);
                    EnrolmentLog.LogSessionDevice(sessionId, trustedRequest.DeviceSerialNumber, null);
                    EnrolmentLog.LogSessionProgress(sessionId, 30, "Retrieving Hardware UUID");
                    trustedRequest.DeviceUUID = ParseMacShellCommand(shell, "system_profiler SPHardwareDataType | grep \"Hardware UUID:\" | cut -d \":\" -f 2-", sessionId);
                    EnrolmentLog.LogSessionProgress(sessionId, 40, "Retrieving Computer Name");
                    trustedRequest.DeviceComputerName = ParseMacShellCommand(shell, "scutil --get ComputerName", sessionId);
                    EnrolmentLog.LogSessionProgress(sessionId, 50, "Retrieving Ethernet MAC Address");
                    string lanNicId = ParseMacShellCommand(shell, "system_profiler SPEthernetDataType | egrep -o \"en0|en1|en2|en3|en4|en5|en6\"", sessionId);
                    if (!string.IsNullOrWhiteSpace(lanNicId))
                    {
                        trustedRequest.DeviceLanMacAddress = ParseMacShellCommand(shell, string.Format("ifconfig {0} | grep ether | cut -d \" \" -f 2-", lanNicId), sessionId);
                    }
                    EnrolmentLog.LogSessionProgress(sessionId, 65, "Retrieving Wireless MAC Address");
                    string wlanNicId = ParseMacShellCommand(shell, "system_profiler SPAirPortDataType | egrep -o \"en0|en1|en2|en3|en4|en5|en6\"", sessionId);
                    if (!string.IsNullOrWhiteSpace(wlanNicId))
                    {
                        trustedRequest.DeviceWlanMacAddress = ParseMacShellCommand(shell, string.Format("ifconfig {0} | grep ether | cut -d \" \" -f 2-", wlanNicId), sessionId);
                    }
                    trustedRequest.DeviceManufacturer = "Apple Inc.";
                    EnrolmentLog.LogSessionProgress(sessionId, 80, "Retrieving Model");
                    trustedRequest.DeviceModel = ParseMacShellCommand(shell, "system_profiler SPHardwareDataType | grep \"Model Identifier:\" | cut -d \":\" -f 2-", sessionId);
                    EnrolmentLog.LogSessionProgress(sessionId, 90, "Retrieving Model Type");
                    trustedRequest.DeviceModelType = ParseMacModelType(ParseMacShellCommand(shell, "system_profiler SPHardwareDataType | grep \"Model Name:\" | cut -d \":\" -f 2-", sessionId));
                    EnrolmentLog.LogSessionProgress(sessionId, 99, "Disconnecting");
                    output = ParseMacModelType(ParseMacShellCommand(shell, "exit", sessionId));
                    if (sessionElevated)
                    {
                        output = ParseMacModelType(ParseMacShellCommand(shell, "exit", sessionId));
                    }
                    if (shell.Connected)
                    {
                        shell.Close();
                    }
                    EnrolmentLog.LogSessionProgress(sessionId, 100, "Disconnected, Starting Disco Enrolment");
                    MacSecureEnrolResponse response = MacSecureEnrolResponse.FromMacEnrolResponse(MacEnrol(dbContext, trustedRequest, true, sessionId));
                    EnrolmentLog.LogSessionFinished(sessionId);
                    MacSecureEnrol = response;
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (shell != null)
                    {
                        bool connected = shell.Connected;
                        if (connected)
                        {
                            shell.Close();
                        }
                    }
                }
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

        private static string ParseMacShellCommand(SshShell Shell, string Command, string LogSessionId)
        {
            Shell.WriteLine(Command);
            System.Threading.Thread.Sleep(250);
            string Response = Shell.Expect(SshPromptRegEx);
            Response = Response.Replace("\r", string.Empty);
            EnrolmentLog.LogSessionDiagnosticInformation(LogSessionId, Response);
            bool flag = Response.Contains("\n");
            string ParseMacShellCommand;
            if (flag)
            {
                string[] ResponseLines = Response.Split(new char[]
					{
						'\n'
					});
                switch (ResponseLines.Length)
                {
                    case 0:
                    case 1:
                        {
                            ParseMacShellCommand = string.Empty;
                            break;
                        }
                    case 2:
                    case 3:
                        {
                            ParseMacShellCommand = ResponseLines[1].Trim();
                            break;
                        }
                    default:
                        {
                            System.Text.StringBuilder ResponseBuilder = new System.Text.StringBuilder();
                            int num = ResponseLines.Length - 2;
                            int lineIndex = 1;
                            while (true)
                            {
                                int arg_111_0 = lineIndex;
                                int num2 = num;
                                if (arg_111_0 > num2)
                                {
                                    break;
                                }
                                ResponseBuilder.AppendLine(ResponseLines[lineIndex]);
                                lineIndex++;
                            }
                            ParseMacShellCommand = ResponseBuilder.ToString().Trim();
                            break;
                        }
                }
            }
            else
            {
                ParseMacShellCommand = Response;
            }
            return ParseMacShellCommand;
        }

        #endregion

        public static MacEnrolResponse MacEnrol(DiscoDataContext dbContext, MacEnrol Request, bool Trusted, string OpenSessionId = null)
        {
            string sessionId;
            if (OpenSessionId == null)
            {
                sessionId = System.Guid.NewGuid().ToString("B");
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
                EnrolmentLog.LogSessionProgress(sessionId, 10, "Querying Database");
                Device RepoDevice = dbContext.Devices.Include("AssignedUser").Include("DeviceProfile").Include("DeviceProfile").Where(d => d.SerialNumber == Request.DeviceSerialNumber).FirstOrDefault();
                if (!Trusted)
                {
                    if (RepoDevice == null)
                        throw new EnrolSafeException(string.Format("Unknown Device Serial Number (SN: '{0}')", Request.DeviceSerialNumber));
                    if (!RepoDevice.AllowUnauthenticatedEnrol)
                        throw new EnrolSafeException(string.Format("Device isn't allowed an Unauthenticated Enrolment (SN: '{0}')", Request.DeviceSerialNumber));
                }
                if (RepoDevice == null)
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 50, "New Device, Building Disco Instance");
                    EnrolmentLog.LogSessionTaskAddedDevice(sessionId, Request.DeviceSerialNumber);
                    DeviceProfile deviceProfile = dbContext.DeviceProfiles.Find(dbContext.DiscoConfiguration.DeviceProfiles.DefaultDeviceProfileId);

                    var deviceModelResult = dbContext.DeviceModels.GetOrCreateDeviceModel(Request.DeviceManufacturer.Trim(), Request.DeviceModel.Trim(), Request.DeviceModel.Trim());
                    DeviceModel deviceModel = deviceModelResult.Item1;
                    if (deviceModelResult.Item2)
                        EnrolmentLog.LogSessionTaskCreatedDeviceModel(sessionId, Request.DeviceSerialNumber, deviceModelResult.Item1.Manufacturer, deviceModelResult.Item1.Model);
                    else
                        EnrolmentLog.LogSessionDevice(sessionId, Request.DeviceSerialNumber, deviceModel.Id);

                    RepoDevice = new Device
                    {
                        SerialNumber = Request.DeviceSerialNumber,
                        ComputerName = Request.DeviceComputerName,
                        DeviceProfile = deviceProfile,
                        DeviceModel = deviceModel,
                        AllowUnauthenticatedEnrol = false,
                        Active = true,
                        CreatedDate = DateTime.Now,
                        EnrolledDate = DateTime.Now
                    };
                    dbContext.Devices.Add(RepoDevice);
                }
                else
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 50, "Existing Device, Updating Disco Instance");
                    EnrolmentLog.LogSessionTaskUpdatingDevice(sessionId, Request.DeviceSerialNumber);
                    if (!RepoDevice.DeviceModelId.HasValue || RepoDevice.DeviceModelId.Value == 1)
                    {
                        var deviceModelResult = dbContext.DeviceModels.GetOrCreateDeviceModel(Request.DeviceManufacturer.Trim(), Request.DeviceModel.Trim(), Request.DeviceModel.Trim());
                        DeviceModel deviceModel = deviceModelResult.Item1;
                        if (deviceModelResult.Item2)
                            EnrolmentLog.LogSessionTaskCreatedDeviceModel(sessionId, Request.DeviceSerialNumber, deviceModelResult.Item1.Manufacturer, deviceModelResult.Item1.Model);
                        else
                            EnrolmentLog.LogSessionDevice(sessionId, Request.DeviceSerialNumber, deviceModel.Id);

                        RepoDevice.DeviceModel = deviceModel;
                    }
                    else
                    {
                        EnrolmentLog.LogSessionDevice(sessionId, Request.DeviceSerialNumber, RepoDevice.DeviceModelId);
                    }
                    RepoDevice.ComputerName = Request.DeviceComputerName;
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
                    ActiveDirectoryUserAccount AssignedUserInfo = ActiveDirectory.GetUserAccount(RepoDevice.AssignedUser.Id);
                    EnrolmentLog.LogSessionTaskAssigningUser(sessionId, RepoDevice.SerialNumber, AssignedUserInfo.DisplayName, AssignedUserInfo.sAMAccountName, AssignedUserInfo.Domain, AssignedUserInfo.ObjectSid);
                    response.DeviceAssignedUserUsername = AssignedUserInfo.sAMAccountName;
                    response.DeviceAssignedUserDomain = AssignedUserInfo.Domain;
                    response.DeviceAssignedUserName = AssignedUserInfo.DisplayName;
                    response.DeviceAssignedUserSID = AssignedUserInfo.ObjectSid;
                }
                response.DeviceComputerName = RepoDevice.ComputerName;
                EnrolmentLog.LogSessionProgress(sessionId, 100, "Completed Successfully");
            }
            catch (EnrolSafeException ex)
            {
                EnrolmentLog.LogSessionError(sessionId, ex);
                return new MacEnrolResponse { ErrorMessage = ex.Message };
            }
            catch (System.Exception ex2)
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
        public static EnrolResponse Enrol(DiscoDataContext dbContext, string Username, Models.ClientServices.Enrol Request)
        {
            ActiveDirectoryMachineAccount MachineInfo = null;
            EnrolResponse response = new EnrolResponse();
            User authenticatedUser = null;
            bool isAuthenticated = false;
            string sessionId = System.Guid.NewGuid().ToString("B");
            response.SessionId = sessionId;
            EnrolmentLog.LogSessionStarting(sessionId, Request.DeviceSerialNumber, EnrolmentTypes.Normal);
            EnrolmentLog.LogSessionDeviceInfo(sessionId, Request);
            try
            {
                EnrolmentLog.LogSessionProgress(sessionId, 10, "Loading User Data");
                if (!string.IsNullOrWhiteSpace(Username))
                {
                    authenticatedUser = UserBI.UserCache.GetUser(Username, dbContext);
                    isAuthenticated = (authenticatedUser != null);
                }
                EnrolmentLog.LogSessionProgress(sessionId, 13, "Loading Device Data");

                Device RepoDevice = dbContext.Devices.Include("AssignedUser").Include("DeviceModel").Include("DeviceProfile").Where(d => d.SerialNumber == Request.DeviceSerialNumber).FirstOrDefault();
                EnrolmentLog.LogSessionProgress(sessionId, 15, "Discovering User/Device Disco Permissions");
                if (isAuthenticated)
                {
                    if (authenticatedUser.Type != "Admin")
                    {
                        if (authenticatedUser.Type != "Computer")
                            throw new EnrolSafeException(string.Format("Connection not correctly authenticated (SN: {0}; Auth User: {1}; User Type: {2})", Request.DeviceSerialNumber, authenticatedUser.Id, authenticatedUser.Type));
                        if (!authenticatedUser.Id.Equals(string.Format("{0}$", Request.DeviceComputerName), System.StringComparison.InvariantCultureIgnoreCase))
                            throw new EnrolSafeException(string.Format("Connection not correctly authenticated (SN: {0}; Auth User: {1}; User Type: {2})", Request.DeviceSerialNumber, authenticatedUser.Id, authenticatedUser.Type));
                    }
                }
                else
                {
                    if (RepoDevice == null)
                        throw new EnrolSafeException(string.Format("Unknown Device Serial Number (SN: '{0}')", Request.DeviceSerialNumber));
                    if (!RepoDevice.AllowUnauthenticatedEnrol)
                        throw new EnrolSafeException(string.Format("Device isn't allowed an Unauthenticated Enrolment (SN: '{0}')", Request.DeviceSerialNumber));
                }
                if (Request.DeviceIsPartOfDomain && !string.IsNullOrWhiteSpace(Request.DeviceComputerName))
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 20, "Loading Active Directory Computer Account");
                    System.Guid? uuidGuid = null;
                    System.Guid? macAddressGuid = null;
                    if (!string.IsNullOrEmpty(Request.DeviceUUID))
                        uuidGuid = ActiveDirectoryMachineAccountExtensions.NetbootGUIDFromUUID(Request.DeviceUUID);
                    if (!string.IsNullOrEmpty(Request.DeviceLanMacAddress))
                        macAddressGuid = ActiveDirectoryMachineAccountExtensions.NetbootGUIDFromMACAddress(Request.DeviceLanMacAddress);
                    MachineInfo = ActiveDirectory.GetMachineAccount(Request.DeviceComputerName, uuidGuid, macAddressGuid);
                }
                if (RepoDevice == null)
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 30, "New Device, Creating Disco Instance");
                    EnrolmentLog.LogSessionTaskAddedDevice(sessionId, Request.DeviceSerialNumber);
                    DeviceProfile deviceProfile = dbContext.DeviceProfiles.Find(dbContext.DiscoConfiguration.DeviceProfiles.DefaultDeviceProfileId);


                    var deviceModelResult = dbContext.DeviceModels.GetOrCreateDeviceModel(Request.DeviceManufacturer.Trim(), Request.DeviceModel.Trim(), Request.DeviceModel.Trim());
                    DeviceModel deviceModel = deviceModelResult.Item1;
                    if (deviceModelResult.Item2)
                        EnrolmentLog.LogSessionTaskCreatedDeviceModel(sessionId, Request.DeviceSerialNumber, deviceModelResult.Item1.Manufacturer, deviceModelResult.Item1.Model);
                    else
                        EnrolmentLog.LogSessionDevice(sessionId, Request.DeviceSerialNumber, deviceModel.Id);

                    RepoDevice = new Device
                    {
                        SerialNumber = Request.DeviceSerialNumber,
                        ComputerName = Request.DeviceComputerName,
                        DeviceProfile = deviceProfile,
                        DeviceModel = deviceModel,
                        AllowUnauthenticatedEnrol = false,
                        Active = true,
                        CreatedDate = DateTime.Now,
                        EnrolledDate = DateTime.Now,
                        LastEnrolDate = DateTime.Now
                    };
                    dbContext.Devices.Add(RepoDevice);
                }
                else
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 30, "Existing Device, Updating Disco Instance");
                    EnrolmentLog.LogSessionTaskUpdatingDevice(sessionId, Request.DeviceSerialNumber);

                    var deviceModelResult = dbContext.DeviceModels.GetOrCreateDeviceModel(Request.DeviceManufacturer.Trim(), Request.DeviceModel.Trim(), Request.DeviceModel.Trim());
                    DeviceModel deviceModel = deviceModelResult.Item1;
                    if (deviceModelResult.Item2)
                        EnrolmentLog.LogSessionTaskCreatedDeviceModel(sessionId, Request.DeviceSerialNumber, deviceModelResult.Item1.Manufacturer, deviceModelResult.Item1.Model);
                    else
                        EnrolmentLog.LogSessionDevice(sessionId, Request.DeviceSerialNumber, deviceModel.Id);

                    RepoDevice.DeviceModel = deviceModel;

                    if (!RepoDevice.EnrolledDate.HasValue)
                        RepoDevice.EnrolledDate = DateTime.Now;
                    RepoDevice.LastEnrolDate = DateTime.Now;
                }

                if (MachineInfo == null)
                {
                    if (isAuthenticated || RepoDevice.AllowUnauthenticatedEnrol)
                    {
                        if (RepoDevice.DeviceProfile.ProvisionADAccount)
                        {
                            EnrolmentLog.LogSessionProgress(sessionId, 50, "Provisioning an Active Directory Computer Account");
                            if (string.IsNullOrEmpty(RepoDevice.ComputerName) || RepoDevice.DeviceProfile.EnforceComputerNameConvention)
                                RepoDevice.ComputerName = RepoDevice.ComputerNameRender(dbContext);
                            EnrolmentLog.LogSessionTaskProvisioningADAccount(sessionId, RepoDevice.SerialNumber, RepoDevice.ComputerName);
                            MachineInfo = ActiveDirectory.GetMachineAccount(RepoDevice.ComputerName);
                            response.OfflineDomainJoin = ActiveDirectory.OfflineDomainJoinProvision(ref MachineInfo, RepoDevice.ComputerName, RepoDevice.DeviceProfile.OrganisationalUnit, sessionId);
                            response.RequireReboot = true;
                        }
                        if (MachineInfo != null)
                        {
                            response.DeviceComputerName = MachineInfo.Name;
                            response.DeviceDomainName = MachineInfo.Domain;
                        }
                        else
                        {
                            response.DeviceComputerName = RepoDevice.ComputerName;
                            response.DeviceDomainName = RepoDevice.ComputerName;
                        }
                    }
                    else
                    {
                        RepoDevice.ComputerName = Request.DeviceComputerName;
                        response.DeviceComputerName = Request.DeviceComputerName;
                        response.DeviceDomainName = RepoDevice.ComputerName;
                    }
                }
                else
                {
                    RepoDevice.ComputerName = MachineInfo.Name;
                    response.DeviceComputerName = MachineInfo.Name;
                    response.DeviceDomainName = MachineInfo.Domain;

                    // Enforce Computer Name Convention
                    if (RepoDevice.DeviceProfile.EnforceComputerNameConvention)
                    {
                        var calculatedComputerName = RepoDevice.ComputerNameRender(dbContext);
                        if (!Request.DeviceComputerName.Equals(calculatedComputerName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            EnrolmentLog.LogSessionProgress(sessionId, 50, string.Format("Renaming Device: {0} -> {1}", Request.DeviceComputerName, calculatedComputerName));
                            EnrolmentLog.LogSessionTaskRenamingDevice(sessionId, Request.DeviceComputerName, calculatedComputerName);

                            RepoDevice.ComputerName = calculatedComputerName;
                            response.DeviceComputerName = calculatedComputerName;

                            // Create New Account
                            response.OfflineDomainJoin = ActiveDirectory.OfflineDomainJoinProvision(ref MachineInfo, RepoDevice.ComputerName, RepoDevice.DeviceProfile.OrganisationalUnit, sessionId);
                            response.RequireReboot = true;
                        }
                    }

                    // Enforce Organisation Unit
                    if (response.OfflineDomainJoin == null && RepoDevice.DeviceProfile.EnforceOrganisationalUnit)
                    {
                        if ((RepoDevice.DeviceProfile.OrganisationalUnit == null && MachineInfo.ParentDistinguishedName.Equals("CN=Computers", StringComparison.InvariantCultureIgnoreCase)) // Null (Default) OU
                            || !MachineInfo.ParentDistinguishedName.Equals(RepoDevice.DeviceProfile.OrganisationalUnit, StringComparison.InvariantCultureIgnoreCase)) // Custom OU
                        {
                            string newOU = RepoDevice.DeviceProfile.OrganisationalUnit ?? "CN=Computers";

                            EnrolmentLog.LogSessionProgress(sessionId, 65, string.Format("Moving Device Organisation Unit: {0} -> {1}", MachineInfo.ParentDistinguishedName, newOU));
                            EnrolmentLog.LogSessionTaskMovingDeviceOrganisationUnit(sessionId, MachineInfo.ParentDistinguishedName, newOU);
                            MachineInfo.MoveOrganisationUnit(RepoDevice.DeviceProfile.OrganisationalUnit);
                            MachineInfo = ActiveDirectory.GetMachineAccount(MachineInfo.sAMAccountName);
                            response.RequireReboot = true;
                        }
                    }

                }
                if (MachineInfo != null)
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 75, "Updating Active Directory Computer Account Properties");
                    MachineInfo.UpdateNetbootGUID(Request.DeviceUUID, Request.DeviceLanMacAddress);
                    if (RepoDevice.AssignedUser != null)
                        MachineInfo.SetDescription(RepoDevice);
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
                        ActiveDirectoryUserAccount AssignedUserInfo = ActiveDirectory.GetUserAccount(RepoDevice.AssignedUser.Id);
                        EnrolmentLog.LogSessionTaskAssigningUser(sessionId, RepoDevice.SerialNumber, AssignedUserInfo.DisplayName, AssignedUserInfo.sAMAccountName, AssignedUserInfo.Domain, AssignedUserInfo.ObjectSid);
                        response.AllowBootstrapperUninstall = true;
                        response.DeviceAssignedUserUsername = AssignedUserInfo.sAMAccountName;
                        response.DeviceAssignedUserDomain = AssignedUserInfo.Domain;
                        response.DeviceAssignedUserName = AssignedUserInfo.DisplayName;
                        response.DeviceAssignedUserSID = AssignedUserInfo.ObjectSid;
                    }
                }
                else
                {
                    response.AllowBootstrapperUninstall = true;
                }
                if (!string.IsNullOrEmpty(Request.DeviceWlanMacAddress) && !string.IsNullOrEmpty(RepoDevice.DeviceProfile.CertificateProviderId))
                {
                    EnrolmentLog.LogSessionProgress(sessionId, 90, "Provisioning a Wireless Certificate");

                    var allocationResult = RepoDevice.AllocateCertificate(dbContext);
                    var deviceCertificate = allocationResult.Item1;
                    if (deviceCertificate != null)
                    {
                        bool certAlreadyInstalled = false;
                        if (Request.DeviceCertificates != null && Request.DeviceCertificates.Count > 0)
                        {
                            foreach (string existingCertName in Request.DeviceCertificates)
                            {
                                if (existingCertName.Contains(deviceCertificate.Name))
                                {
                                    certAlreadyInstalled = true;
                                    break;
                                }
                            }
                        }
                        if (!certAlreadyInstalled)
                        {
                            EnrolmentLog.LogSessionTaskProvisioningWirelessCertificate(sessionId, RepoDevice.SerialNumber, deviceCertificate.Name);
                            response.DeviceCertificate = System.Convert.ToBase64String(deviceCertificate.Content);
                        }
                    }
                    response.DeviceCertificateRemoveExisting = allocationResult.Item2;
                }

                // Reset 'AllowUnauthenticatedEnrol'
                if (RepoDevice.AllowUnauthenticatedEnrol)
                    RepoDevice.AllowUnauthenticatedEnrol = false;

                EnrolmentLog.LogSessionProgress(sessionId, 100, "Completed Successfully");
            }
            catch (EnrolSafeException ex)
            {
                EnrolmentLog.LogSessionError(sessionId, ex);
                return new EnrolResponse
                {
                    SessionId = sessionId,
                    ErrorMessage = ex.Message
                };
            }
            catch (System.Exception ex2)
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
